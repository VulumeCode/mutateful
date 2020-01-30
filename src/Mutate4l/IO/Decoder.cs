using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mutate4l.Core;

namespace Mutate4l.IO
{
    public static class Decoder
    {
        private const byte TypedDataFirstByte = 127;
        private const byte TypedDataSecondByte = 126;
        private const byte TypedDataThirdByte = 125;
        private const byte StringData = 124;
        private const byte SetAndEvaluateClipSlot = 255;
        private const byte SetClipSlot = 254;
        private const byte EvaluateClipSlots = 253;

        public static bool IsString(byte[] result)
        {
            return result.Length > 4 && result[0] == TypedDataFirstByte && result[1] == TypedDataSecondByte && result[2] == TypedDataThirdByte && result[3] == StringData;
        }

        public static bool IsSetAndEvaluateClipSlotCommand(byte[] result)
        {
            return result.Length > 4 && result[0] == TypedDataFirstByte && result[1] == TypedDataSecondByte && result[2] == TypedDataThirdByte && result[3] == SetAndEvaluateClipSlot;
        }

        public static bool IsSetClipSlotCommand(byte[] result)
        {
            return result.Length > 4 && result[0] == TypedDataFirstByte && result[1] == TypedDataSecondByte && result[2] == TypedDataThirdByte && result[3] == SetClipSlot;
        }

        public static bool IsEvaluateClipSlotsCommand(byte[] result)
        {
            return result.Length > 4 && result[0] == TypedDataFirstByte && result[1] == TypedDataSecondByte && result[2] == TypedDataThirdByte && result[3] == EvaluateClipSlots;
        }
        
        public static bool IsTypedCommand(byte[] result)
        {
            return result.Length > 4 && result[0] == TypedDataFirstByte && result[1] == TypedDataSecondByte && result[2] == TypedDataThirdByte;
        }
        
        public static string GetText(byte[] data)
        {
            if (data.Length < 5) return "";
            return Encoding.UTF8.GetString(data.Skip(4).ToArray());
        }

        public static (List<Clip> Clips, string Formula, ushort Id, byte TrackNo) DecodeData(byte[] data)
        {
            var clips = new List<Clip>();
            ushort id = BitConverter.ToUInt16(data, 0);
            byte trackNo = data[2];
            byte numClips = data[3];
            int dataOffset = 4;

            // Decode clipdata
            while (clips.Count < numClips)
            {
                ClipReference clipReference = new ClipReference(data[dataOffset], data[dataOffset += 1]);
                decimal length = (decimal)BitConverter.ToSingle(data, dataOffset += 1);
                bool isLooping = data[dataOffset += 4] == 1;
                var clip = new Clip(length, isLooping) {
                    ClipReference = clipReference
                };
                ushort numNotes = BitConverter.ToUInt16(data, dataOffset += 1);
                dataOffset += 2;
                for (var i = 0; i < numNotes; i++)
                {
                    clip.Notes.Add(new NoteEvent(
                        data[dataOffset], 
                        (decimal)BitConverter.ToSingle(data, dataOffset += 1), 
                        (decimal)BitConverter.ToSingle(data, dataOffset += 4), 
                        data[dataOffset += 4])
                    );
                    dataOffset++;
                }
                clips.Add(clip);
            }
            // Convert remaining bytes to text containing the formula
            string formula = Encoding.ASCII.GetString(data, dataOffset, data.Length - dataOffset);

            return (clips, formula, id, trackNo);
        }
    }
}