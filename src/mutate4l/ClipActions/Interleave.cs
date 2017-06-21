﻿using Mutate4l.Dto;
using System;
using System.Collections.Generic;
using static Mutate4l.ClipActions.InterleaveMode;

namespace Mutate4l.ClipActions
{
    public enum InterleaveMode
    {
        EventCount,
        TimeRange
    }

    public class Interleave
    {
        public static InterleaveMode Mode { get; set; } = TimeRange;
        public static List<int> EventCounts { get; set; }
        public static decimal EventRangeA { get; set; } = 1; // todo: support list of any number of ranges instead
        public static decimal EventRangeB { get; set; } = 1; // todo: support list of any number of ranges instead

        private static decimal AddNextNote(List<Note> noteSrc, decimal position, int ix, Clip a, Clip b, Clip resultClip)
        {
            decimal pos = position;
            var noteToAdd = new Note(noteSrc[ix % noteSrc.Count])
            {
                Start = pos
            };
            resultClip.Notes.Add(noteToAdd);
            var note = noteSrc[ix % noteSrc.Count];
            var nextNote = noteSrc[(ix + 1) % noteSrc.Count];
            if ((ix + 1) % noteSrc.Count == 0 && ix > 0)
            {
                pos = pos + a.Length - note.Start;
                pos = pos + nextNote.Start;
            }
            else
            {
                pos = pos + nextNote.Start - note.Start;
            }
            return pos;
        }

        public static Clip Apply(Clip a, Clip b) // todo: Expand to interleave any list of two clips or more
        {
            Clip resultClip = new Clip(a.Length + b.Length, true);
            decimal position = 0;

            switch (Mode)
            {
                case EventCount:
                    int i = 0, nix = 0;
                    while (i < b.Notes.Count + a.Notes.Count)
                    {

                        if (i == 0)
                        {
                            position = position + a.Notes[nix % a.Notes.Count].Start;
                        }
                        if (i % 2 == 0)
                        {
                            position = AddNextNote(a.Notes, position, nix, a, b, resultClip);
                        }
                        if (i % 2 == 1)
                        {
                            position = AddNextNote(b.Notes, position, nix, a, b, resultClip);
                        }
                        i++;
                        nix = i / 2;
                    }
                    break;
                case TimeRange:
                    decimal srcPositionA = 0,
                        srcPositionB = 0;
                    a.Notes = Utility.SplitNotesAtEvery(a.Notes, EventRangeA, b.Length);
                    b.Notes = Utility.SplitNotesAtEvery(b.Notes, EventRangeB, a.Length);

                    while (position < resultClip.Length)
                    {
                        resultClip.Notes.AddRange(Utility.GetNotesInRangeAtPosition(srcPositionA, srcPositionA + EventRangeA, a.Notes, position));
                        position += EventRangeA;
                        srcPositionA += EventRangeA;

                        resultClip.Notes.AddRange(Utility.GetNotesInRangeAtPosition(srcPositionB, srcPositionB + EventRangeB, b.Notes, position));
                        position += EventRangeB;
                        srcPositionB += EventRangeB;
                    }
                    break;
            }
            return resultClip;
        }
    }
}