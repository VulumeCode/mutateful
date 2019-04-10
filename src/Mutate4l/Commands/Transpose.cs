﻿using Mutate4l.Core;
using Mutate4l.Utility;
using System.Linq;
using Mutate4l.Cli;

namespace Mutate4l.Commands
{
    public enum TransposeMode
    {
        Absolute, // pitch is transposed relative to an absolute base pitch of 60 (C4)
        Relative, // pitch is transposed relative to the first note in the transposing clip
        Overwrite // pitch is set to the notes in the transposing clip
    }

    public class TransposeOptions
    {
        public Clip By { get; set; } = new Clip(4, true); // Allows syntax like a1 transpose -by a2 -mode relative. This syntax makes it much clearer which clip is being affected, and which is used as the source.

        public TransposeMode Mode { get; set; } = TransposeMode.Absolute;

        [OptionInfo(type: OptionType.Default)]
        public int[] TransposeValues { get; set; } = new int[0];
    }

    // also needed: a transpose function (rangetranspose?) transposing all notes contained within the bounds of the respective note in the control clip
    public class Transpose
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out TransposeOptions options);
            return !success ? new ProcessResultArray<Clip>(msg) : Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(TransposeOptions options, params Clip[] clips)
        {
            if (options.By.Count == 0 && options.TransposeValues.Length == 0)
            {
                return new ProcessResultArray<Clip>("No -by clip or transpose values specified.");
            }
            ClipUtilities.Monophonize(options.By);
            int basePitch = 60;
            if (options.By.Count > 0)
            {
                basePitch = options.By.Notes[0].Pitch;
            }
            if (options.Mode == TransposeMode.Absolute)
            {
                basePitch -= basePitch % 12;
            }

            if (options.Mode == TransposeMode.Relative && options.By.Notes.Count > 0)
            {
                basePitch = options.By.Notes[0].Pitch;
            }

            int[] transposeValues;
            if (options.TransposeValues.Length > 0)
            {
                transposeValues = options.TransposeValues;
            } else
            {
                transposeValues = options.By.Notes.Select(x => 
                    options.Mode == TransposeMode.Overwrite ? 
                        x.Pitch : 
                        x.Pitch - basePitch
                ).ToArray();
            }

            foreach (var clip in clips)
            {
                clip.GroupSimultaneousNotes();
                if (options.Mode == TransposeMode.Overwrite)
                {
                    for (var i = 0; i < clip.Count; i++)
                    {
                        clip.Notes[i].Pitch = transposeValues[i % transposeValues.Length];
                    }
                }
                else
                {
                    for (var i = 0; i < clip.Count; i++)
                    {
                        clip.Notes[i].Pitch += transposeValues[i % transposeValues.Length];
                    }
                }
                clip.Flatten();
            }
            return new ProcessResultArray<Clip>(clips); // currently destructive
        }
    }
}
