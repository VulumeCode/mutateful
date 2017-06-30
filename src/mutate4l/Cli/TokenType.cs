﻿namespace Mutate4l.Cli
{
    public enum TokenType
    {
        _CommandsBegin,
        Interleave,
        Constrain,
        Slice,
        Explode,
        _CommandsEnd,
        _OptionsBegin,
        Start,
        Pitch,
        Range,
        Count,
        _OptionsEnd,
        Colon,
        Destination,
        _ValuesBegin,
        ClipReference,
        Number,
        MusicalDivision,
        _ValuesEnd
    }
}
