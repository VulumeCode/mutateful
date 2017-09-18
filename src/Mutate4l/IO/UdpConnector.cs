﻿using Mutate4l.ClipActions;
using Mutate4l.Core;
using Mutate4l.Dto;
using Mutate4l.Utility;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Mutate4l.IO
{
    public class UdpConnector
    {
        private UdpClient Listener;
        private UdpClient Sender;
        private IPEndPoint GroupEP;

        public void Open()
        {
            Listener = new UdpClient(8008);
            Sender = new UdpClient();
            GroupEP = new IPEndPoint(IPAddress.Any, 8008);
        }

        public void Close()
        {
            Listener.Close();
            Sender.Close();
        }

        public Clip GetClip(int channel, int clip)
        {
            var notes = new SortedList<Note>();
            string noteData = "";
            try
            {
                byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/get", channel, clip);
                Sender.Send(message, message.Length, "localhost", 8009);
//                Console.WriteLine("Waiting for broadcast");
                byte[] bytes = Listener.Receive(ref GroupEP);
                string data = Encoding.ASCII.GetString(bytes);
//                Console.WriteLine($"[{OscHandler.GetOscStringKey(data)}] : [{OscHandler.GetOscStringValue(data)}]");

                noteData = OscHandler.GetOscStringValue(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return IOUtilities.StringToClip(noteData);
        }

        public void SetClips(int trackNo, int startingClipNo, Clip[] clips)
        {
            var i = 0;
            foreach (var clip in clips)
            {
                SetClip(trackNo, startingClipNo + i++, clip);
            }
        }

        public void SetClip(int trackNo, int clipNo, Clip clip)
        {
            string data = IOUtilities.ClipToString(clip);
            byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/set", trackNo, clipNo, data);
            Sender.Send(message, message.Length, "localhost", 8009);
        }

        public async Task<bool> TestCommunication()
        {
            byte[] message = OscHandler.CreateOscMessage("/mu4l/hello", 0, 0);

            UdpReceiveResult result;
            using (var udpClient = new UdpClient(8022))
            {
//                udpClient.Connect(new IPEndPoint(IPAddress.Any, 8009));
                Console.WriteLine($"SendAsync returned {await udpClient.SendAsync(message, message.Length, "localhost", 8023)}");
                result = await udpClient.ReceiveAsync();
            }
            string data = Encoding.ASCII.GetString(result.Buffer);
            Console.WriteLine($"ReceiveAsync returned {data}");
            return data.Contains("/mu4l/out/hello");
        }

        /*
         * 
         * 
        
        public async Task<Clip> GetClip(int channel, int clip)
        {
            var notes = new SortedList<Note>();
            string noteData = "";
            try
            {
                byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/get", channel, clip);

                using (var udpClient = new UdpClient())
                {
                    udpClient.Connect(new IPEndPoint(IPAddress.Any, 8009));
                    Console.WriteLine($"SendAsync returned {await udpClient.SendAsync(message, message.Length)}";
                }
//                Sender.Send(message, message.Length, "localhost", 8009);
//                Console.WriteLine("Waiting for broadcast");
//                byte[] bytes = Listener.Receive(ref GroupEP);
                UdpReceiveResult result;
                using (var udpClient = new UdpClient(8008))
                {
                    result = await udpClient.ReceiveAsync();
                }
                string data = Encoding.ASCII.GetString(result.Buffer);
                Console.WriteLine($"[{OscHandler.GetOscStringKey(data)}] : [{OscHandler.GetOscStringValue(data)}]");

                noteData = OscHandler.GetOscStringValue(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return IOUtilities.StringToClip(noteData);
        }

        public void SetClips(int trackNo, int startingClipNo, Clip[] clips)
        {
            var i = 0;
            foreach (var clip in clips)
            {
                SetClip(trackNo, startingClipNo + i++, clip);
            }
        }

        public void SetClip(int trackNo, int clipNo, Clip clip)
        {
            string data = IOUtilities.ClipToString(clip);
            byte[] message = OscHandler.CreateOscMessage("/mu4l/clip/set", trackNo, clipNo, data);
            Sender.Send(message, message.Length, "localhost", 8009);
        }

        public async Task<bool> TestCommunication()
        {
            byte[] message = OscHandler.CreateOscMessage("/mu4l/hello", 0, 0);

            using (var udpClient = new UdpClient())
            {
                udpClient.Connect(new IPEndPoint(IPAddress.Any, 8009));
                Console.WriteLine($"SendAsync returned {await udpClient.SendAsync(message, message.Length)}";
            }
            //                Sender.Send(message, message.Length, "localhost", 8009);
            //                Console.WriteLine("Waiting for broadcast");
            //                byte[] bytes = Listener.Receive(ref GroupEP);
            UdpReceiveResult result;
            using (var udpClient = new UdpClient(8008))
            {
                result = await udpClient.ReceiveAsync();
            }
            string data = Encoding.ASCII.GetString(result.Buffer);


            Sender.Send(message, message.Length, "localhost", 8009);
            byte[] bytes = Listener.Receive(ref GroupEP);
            string data = Encoding.ASCII.GetString(bytes);
            return data.Contains("/mu4l/hello");
        }
         * */
    }
}
