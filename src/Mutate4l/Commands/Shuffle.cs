﻿using Mutate4l.Dto;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Mutate4l.Core;

namespace Mutate4l.Commands
{
    public class ShuffleOptions
    {
        public Clip By { get; set; }
//        public bool RelativeToNearestBoundary { get; set; } = false;
    }

    public class Shuffle
    {
        public static ProcessResultArray<Clip> Apply(Command command, params Clip[] clips)
        {
            (var success, var msg) = OptionParser.TryParseOptions(command, out ShuffleOptions options);
            if (!success)
            {
                return new ProcessResultArray<Clip>(msg);
            }
            return Apply(options, clips);
        }

        public static ProcessResultArray<Clip> Apply(ShuffleOptions options, params Clip[] clips)
        {
            if (options.By == null || options.By.Notes.Count == 0) options.By = clips[0];
            ClipUtilities.Monophonize(options.By);
            var targetClips = new Clip[clips.Length];

            var c = 0;
            foreach (var clip in clips) // we only support one generated clip since these are tied to a specific clip slot. Maybe support multiple clips under the hood, but discard any additional clips when sending the output is the most flexible approach.
            {
                clip.GroupSimultaneousNotes();
                targetClips[c] = new Clip(clip.Length, clip.IsLooping);
                int minPitch = options.By.Notes.Min(x => x.Pitch);
                var numShuffleIndexes = options.By.Notes.Count;
                if (numShuffleIndexes < clip.Notes.Count) numShuffleIndexes = clip.Notes.Count;
                var indexes = new int[numShuffleIndexes];

                for (var i = 0; i < numShuffleIndexes; i++)
                {
                    // Calc shuffle indexes as long as there are notes in the source clip. If the clip to be shuffled contains more events than the source, add zero-indexes so that the rest of the sequence is produced sequentially.
                    if (i < options.By.Notes.Count)
                    {
                        indexes[i] = (int)Math.Floor(((options.By.Notes[i].Pitch - minPitch - 0f) / clip.Notes.Count) * clip.Notes.Count);
                    } else
                    {
                        indexes[i] = 0;
                    }
                }

                // preserve original durations until next note
                var durationUntilNextNote = new List<decimal>(clip.Notes.Count);
                for (var i = 0; i < clip.Notes.Count; i++)
                {
                    durationUntilNextNote.Add(clip.DurationUntilNextNote(i));
                }

                // do shuffle
                var j = 0;
                decimal pos = 0m;
                while (clip.Notes.Count > 0)
                {
                    int currentIx = indexes[j++] % clip.Notes.Count;
                    targetClips[c].Notes.Add(
                        new NoteEvent(clip.Notes[currentIx]) {
                            Start = pos
                        }
                    );
                    pos += durationUntilNextNote[currentIx];
                    durationUntilNextNote.RemoveAt(currentIx);
                    clip.Notes.RemoveAt(currentIx);
                }
                targetClips[c].Flatten();
                c++;
            }

            return new ProcessResultArray<Clip>(targetClips);
        }
    }
}
