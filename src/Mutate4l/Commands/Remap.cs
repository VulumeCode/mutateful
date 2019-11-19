using System;
using System.Collections.Generic;
using System.Linq;
using Mutate4l.Cli;
using Mutate4l.Core;

namespace Mutate4l.Commands
{
    public class RemapOptions
    {
        public Clip To { get; set; } = Clip.Empty;
    }

    // # desc: Remaps a set of pitches to another set of pitches
    public static class Remap
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out RemapOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(RemapOptions options, params Clip[] clips)
        {
            foreach (var clip in clips)
            {
                var sourcePitches = clip.Notes.Select(x => x.Pitch).Distinct().OrderBy(x => x).ToList();
                var destPitches = options.To.Count > 0 ? 
                    options.To.Notes.Select(x => x.Pitch).Distinct().OrderBy(x => x).ToList() : 
                    Enumerable.Range(36, Math.Min(sourcePitches.Count, 128 - 36)).ToList();
                var inc = 1f;
                
                if (destPitches.Count < sourcePitches.Count)
                {
                    inc = (float) destPitches.Count / sourcePitches.Count;
                }

                var map = new Dictionary<int, int>();
                var destIx = 0f;
                foreach (var sourcePitch in sourcePitches)
                {
                    // future improvement: any notes being stuck together on the same pitch should first shorten existing notes if overlaps occur
                    map[sourcePitch] = destPitches[(int)Math.Floor(destIx)];
                    destIx += inc;
                }

                foreach (var note in clip.Notes)
                {
                    note.Pitch = map[note.Pitch];
                }
            }

            return new ProcessResultArray<Clip>(clips);
        }
    }
}