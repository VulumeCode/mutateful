﻿using Mutate4l.IO;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mutate4l.Core;

namespace Mutate4l.Cli
{
    internal static class CliHandler
    {
        private static readonly string UnitTestDirective = " test";
        private static readonly string SvgDocDirective = " doc";
        
        public static void Start()
        {
            while (true)
            {
                var generateUnitTest = false;
                var generateSvgDoc = false;
                var result = UdpConnector.WaitForData();
                if (UdpConnector.IsString(result))
                {
                    string text = UdpConnector.GetText(result);
                    Console.WriteLine(text);
                    continue;
                }
                
                (List<Clip> clips, string formula, ushort id, byte trackNo) = UdpConnector.DecodeData(result);
                Console.WriteLine($"Received {clips.Count} clips and formula: {formula}");
                if (formula.EndsWith(UnitTestDirective))
                {
                    Console.WriteLine($"Saving autogenerated unit test to {Path.Join(Environment.CurrentDirectory, "GeneratedUnitTests.txt")}");
                    formula = formula.Substring(0, formula.Length - UnitTestDirective.Length);
                    generateUnitTest = true;
                }
                if (formula.EndsWith(SvgDocDirective))
                {
                    Console.WriteLine($"Saving autogenerated SVG documentation for this formula to {Path.Join(Environment.CurrentDirectory, "GeneratedDocs.svg")}");
                    formula = formula.Substring(0, formula.Length - SvgDocDirective.Length);
                    generateSvgDoc = true;
                }
                
                var chainedCommandWrapper = Parser.ParseFormulaToChainedCommand(formula, clips, new ClipMetaData(id, trackNo));
                if (!chainedCommandWrapper.Success)
                {
                    Console.WriteLine(chainedCommandWrapper.ErrorMessage);
                    continue;
                }

                var processedClipWrapper = ClipProcessor.ProcessChainedCommand(chainedCommandWrapper.Result);

                if (processedClipWrapper.WarningMessage.Length > 0)
                {
                    Console.WriteLine($"Warnings were generated:{System.Environment.NewLine}" +
                                      $"{processedClipWrapper.WarningMessage}");
                }

                if (processedClipWrapper.Success && processedClipWrapper.Result.Length > 0)
                {
                    var processedClip = processedClipWrapper.Result[0];
                    byte[] clipData = IOUtilities.GetClipAsBytes(chainedCommandWrapper.Result.TargetMetaData.Id, processedClip).ToArray();
                    if (generateUnitTest)
                    {
                        AppendUnitTest(formula, result, clipData);
                    }

                    if (generateSvgDoc)
                    {
                        GenerateSvgDoc(formula, clips, processedClip, 882, 300);
                    }
                    UdpConnector.SetClipAsBytesById(clipData);
                }
                else
                    Console.WriteLine($"Error applying formula: {processedClipWrapper.ErrorMessage}");
            }
        }

        private static void GenerateSvgDoc(string formula, List<Clip> clips, Clip resultClip, int width, int height)
        {
            var padding = 10;
            var resultClipWidth = width / 2 - padding;
            var sourceClipWidth = resultClipWidth;
            var sourceClipHeight = height / 2;

            if (clips.Count > 2)
            {
                sourceClipWidth = resultClipWidth / 2;
            }
            
            var output = $"<svg version=\"1.1\" baseProfile=\"full\" width=\"{width}\" height=\"{height}\" " +
                         "xmlns=\"http://www.w3.org/2000/svg\">" + Environment.NewLine;
            var x = 0;
            var y = 0;
            var highestNote = (clips.Max(c => c.Notes.Max(d => d.Pitch)) + 4) & 0x7F; // Leave 3 notes on each side
            var lowestNote = (clips.Min(c => c.Notes.Min(d => d.Pitch)) - 3) & 0x7F; // as padding, and clamp to 0-127 range
            var numNotes = highestNote - lowestNote + 1;
            
            for (var i = 0; i < Math.Min(4, clips.Count); i++)
            {
                var clip = clips[i];
                output += SvgUtilities.ClipToSvg(clip, x, y, sourceClipWidth, sourceClipHeight, numNotes, lowestNote, highestNote);
                y += sourceClipHeight;
                if (i == 1)
                {
                    x += sourceClipWidth;
                    y = 0;
                }
            }

            y = 0;
            highestNote = (resultClip.Notes.Max(c => c.Pitch) + 4) & 0x7F; 
            lowestNote = (resultClip.Notes.Min(c => c.Pitch) - 3) & 0x7F; 
            numNotes = highestNote - lowestNote + 1;
            output += SvgUtilities.ClipToSvg(resultClip, width - resultClipWidth, y, resultClipWidth, height, numNotes, lowestNote, highestNote);
            output += "</svg>";

            using (var file = File.AppendText($"Generated{DateTime.Now.Ticks}-clip.svg"))
            {
                file.Write(output);
            }
        }

        private static void AppendUnitTest(string formula, byte[] inputData, byte[] outputData)
        {
            using (var file = File.AppendText(@"GeneratedUnitTests.txt"))
            {
                file.Write($"// Test generated by mutate4l from formula: {formula}{Environment.NewLine}" +
                           $"[TestMethod]{Environment.NewLine}" +
                           $"public void NameThisMethod(){Environment.NewLine}" +
                           $"{{{Environment.NewLine}" +
                           $"    byte[] input = {{ {Utilities.GetByteArrayContentsAsString(inputData.Take(inputData.Length - UnitTestDirective.Length).ToArray())} }};{Environment.NewLine}" +
                           $"    byte[] output = {{ {Utilities.GetByteArrayContentsAsString(outputData)} }};{Environment.NewLine}" +
                           $"{Environment.NewLine}" +
                           $"    TestUtilities.InputShouldProduceGivenOutput(input, output);{Environment.NewLine}" +
                           $"}}{Environment.NewLine}{Environment.NewLine}");
            }
        }

        public static void DoDump(string command)
        {
            var arguments = command.Split(' ').Skip(1);
            foreach (var arg in arguments)
            {
                var (channelNo, clipNo) = Parser.ResolveClipReference(arg);
                var clip = UdpConnector.GetClip(channelNo, clipNo);
                Console.WriteLine($"Clip length {clip.Length}");
                Console.WriteLine(Utility.IOUtilities.ClipToString(clip));
            }
        }

/*
        private static void DoSvg(string command, IEnumerable<Clip> clips)
        {
            var arguments = command.Split(' ').Skip(1);
            var options = arguments.Where(x => x.StartsWith("-"));
            int numNotes = 24; // 2 octaves default
            int startNote = 60; // C3
            foreach (var option in options)
            {
                if (option.StartsWith("-numnotes:"))
                {
                    numNotes = int.Parse(option.Substring(option.IndexOf(':') + 1));
                }
                if (option.StartsWith("-startnote:"))
                {
                    startNote = int.Parse(option.Substring(option.IndexOf(':') + 1));
                }
                //Console.WriteLine($"option: {option}");
            }
            Console.WriteLine($"start: {startNote}");
            Console.WriteLine($"number of notes: {numNotes}");
            foreach (var clip in clips)
            {
                Console.WriteLine(SvgUtilities.SvgFromClip(clip, 0, 0, 370, 300, numNotes, startNote));
            }
        }
*/
    }
}
