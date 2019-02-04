﻿using Mutate4l.Dto;
using Mutate4l.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Mutate4l.IO
{
    public static class UdpConnector
    {
        public static int ReceivePort = 8022;
        public static int SendPort = 8023;

        public static Clip GetClip(int channel, int clip)
        {
            return GetClipData("/mu4l/clip/get", channel, clip);
        }

        // currently unused
        public static Clip GetSelectedClip()
        {
            return GetClipData("/mu4l/selectedclip/get", 0, 0);
        }

        public static Clip GetClipData(string address, int channel, int clip)
        {
            byte[] message = OscHandler.CreateOscMessage(address, channel, clip);
            byte[] result;
            var endPoint = new IPEndPoint(IPAddress.Any, ReceivePort);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
                result = udpClient.Receive(ref endPoint);
            }
            var data = Encoding.ASCII.GetString(result);
            var noteData = OscHandler.GetOscStringValue(data);
            return IOUtilities.StringToClip(noteData);
        }

        // currently unused
        public static void SetClips(int trackNo, int startingClipNo, Clip[] clips)
        {
            var i = 0;
            foreach (var clip in clips)
            {
                SetClip(trackNo, startingClipNo + i++, clip);
            }
        }

        // currently unused
        public static void SetClip(int trackNo, int clipNo, Clip clip)
        {
            string data = IOUtilities.ClipToString(clip);
            byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/set", trackNo, clipNo, data);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
            }
        }

        public static void SetClipById(string id, Clip clip)
        {
            string data = IOUtilities.ClipToString(clip);
            byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/setbyid", int.Parse(id), 0, data);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
            }
        }

        // currently unused
        public static void SetSelectedClip(Clip clip)
        {
            string data = IOUtilities.ClipToString(clip);
            byte[] message = OscHandler.CreateOscMessage("/mu4l/selectedclip/set", 0, 0, data);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
            }
        }

        public static bool TestCommunication()
        {
            byte[] message = OscHandler.CreateOscMessage("/mu4l/hello", 0, 0);
            byte[] result;
            var endPoint = new IPEndPoint(IPAddress.Any, ReceivePort);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
                result = udpClient.Receive(ref endPoint);
            }
            string data = Encoding.ASCII.GetString(result);
            return data.Contains("/mu4l/out/hello");
        }

        // currently unused
        public static void EnumerateClips()
        {
            byte[] message = OscHandler.CreateOscMessage("/mu4l/enum", 0, 0);

            using (var udpClient = new UdpClient(ReceivePort))
            {
                udpClient.Send(message, message.Length, "localhost", SendPort);
            }
        }

        public static string WaitForData()
        {
            byte[] result;
            var endPoint = new IPEndPoint(IPAddress.Any, ReceivePort);

            using (var udpClient = new UdpClient(ReceivePort))
            {
//                udpClient.Send(message, message.Length, "localhost", SendPort);
                result = udpClient.Receive(ref endPoint);
            }
            DecodeData(result);
            string rawData = Encoding.ASCII.GetString(result);
            for (var i = 0; i < result.Length; i++) { Console.Write(result[i] + ", "); }
            string data = OscHandler.GetOscStringValue(rawData);
            return data;
        }

        public static void DecodeData(byte[] data)
        {
            var clips = new List<Clip>();
            ushort id = BitConverter.ToUInt16(data, 0);
            byte trackNo = data[2];
            byte numClips = data[3];

            int dataOffset = 4;

            byte currentClip = 0;
            //            while (currentClip < numClips)
            //            {
            //if (BitConverter.IsLittleEndian) lengthData = lengthData.Reverse().ToArray();
            ClipReference clipReference = new ClipReference(data[dataOffset], data[dataOffset + 1]);
            float length = BitConverter.ToSingle(data, dataOffset + 2);
            bool isLooping = data[dataOffset + 6] == 1;
//            Clip clip = new Clip((decimal)length, isLooping);
            ushort numNotes = BitConverter.ToUInt16(data, dataOffset + 7);

//            }

        }
    }
}
