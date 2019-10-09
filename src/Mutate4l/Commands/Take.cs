using System;
using Mutate4l.Cli;
using Mutate4l.Core;

namespace Mutate4l.Commands
{
    public class TakeOptions
    {
        [OptionInfo(type: OptionType.Default)]
        public int[] TakeCounts { get; set; } = {2};
    }
    
    public static class Take
    {
        // Create a new clip based on taking every # note from another clip
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            var (success, msg) = OptionParser.TryParseOptions(command, out TakeOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(TakeOptions options, params Clip[] clips)
        {
            var resultClips = new Clip[clips.Length];

            // Normalize take values (typical input range: 1 - N, while 0 - N is used internally)
            for (var ix = 0; ix < options.TakeCounts.Length; ix++)
            {
                ref var takeCount = ref options.TakeCounts[ix];
                if (takeCount < 1) takeCount = 1;
                takeCount--;
            }

            var i = 0;
            foreach (var clip in clips)
            {
                var resultClip = new Clip(clips[i].Length, clips[i].IsLooping);
                decimal currentPos = 0;
                var noteIx = 0;
                var currentTake = options.TakeCounts[0];
                var takeIx = 0;
                // We want to keep the length of the newly created clip approximately equal to the original, therefore we keep
                // going until we have filled at least the same length as the original clip
                while (currentPos < resultClip.Length)
                {
                    if (currentTake == 0)
                    {
                        if (noteIx >= clip.Count) noteIx = 0;
                        var note = new NoteEvent(clip.Notes[noteIx]) {Start = currentPos};
                        currentPos += clip.DurationUntilNextNote(noteIx);
                        resultClip.Add(note);
                        currentTake = options.TakeCounts[++takeIx % options.TakeCounts.Length];
                    }
                    else
                    {
                        currentTake--;
                    }
                    noteIx++;
                }
                resultClips[i] = resultClip;
                i++;
            }
            return new ProcessResultArray<Clip>(resultClips);
        }
    }
}