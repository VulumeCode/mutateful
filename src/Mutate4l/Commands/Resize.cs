﻿using Mutate4l.Cli;
using Mutate4l.Core;

namespace Mutate4l.Commands
{
    public class ResizeOptions
    {
        [OptionInfo(type: OptionType.Default, noImplicitCast: true)]
        public decimal Factor { get; set; } = 1.0m;
    }

    public static class Resize
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            (var success, var msg) = OptionParser.TryParseOptions(command, out ResizeOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(ResizeOptions options, params Clip[] clips)
        {
            foreach (var clip in clips)
            {
                foreach (var note in clip.Notes)
                {
                    note.Duration *= options.Factor;
                    note.Start *= options.Factor;
                }
                clip.Length *= options.Factor;
            }

            return new ProcessResultArray<Clip>(clips);
        }
    }
}
