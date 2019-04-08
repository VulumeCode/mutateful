﻿using Mutate4l.Core;
using Mutate4l.Utility;
using System.Collections.Generic;
using Mutate4l.Cli;

namespace Mutate4l.Commands
{
    public class SliceOptions
    {
        [OptionInfo(type: OptionType.Default)]
        public decimal[] Lengths { get; set; } = new decimal[] { .25m };
    }

    public class Slice
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            (var success, var msg) = OptionParser.TryParseOptions(command, out SliceOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(SliceOptions options, params Clip[] clips)
        {
            var processedClips = new List<Clip>();
            foreach (var clip in clips)
            {
                processedClips.Add(ClipUtilities.SplitNotesAtEvery(clip, options.Lengths));
            }
            return new ProcessResultArray<Clip>(processedClips.ToArray());
        }
    }
}
