﻿using Mutate4l.Core;
using Mutate4l.Dto;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mutate4l.Commands
{
    // Extracts a region of the current clips, effectively cropping them. If two params, start - duration is used, otherwise 0 - duration.
    public class TakeOptions
    {
        [OptionInfo(type: OptionType.Default)]
        public decimal[] Lengths { get; set; } = new decimal[] { 2 };
    }

    public class Take
    {
        public static ProcessResultArray<Clip> Apply(TakeOptions options, params Clip[] clips)
        {
            var processedClips = new Clip[clips.Length];
            var start = options.Lengths.Length > 1 ? options.Lengths[0] : 0;
            var duration = options.Lengths.Length > 1 ? options.Lengths[1] : options.Lengths[0];
            var i = 0;

            foreach (var clip in clips)
            {
                var processedClip = new Clip(duration - start, clip.IsLooping);
                processedClip.Notes.AddRange(ClipUtilities.GetSplitNotesInRangeAtPosition(start, start + duration, clip.Notes, 0));
                processedClips[i++] = processedClip;
            }
            return new ProcessResultArray<Clip>(processedClips);
        }
    }
}
