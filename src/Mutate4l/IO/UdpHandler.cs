using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;
using Mutate4l.Cli;
using Mutate4l.Core;
using Mutate4l.State;
using Mutate4l.Utility;

namespace Mutate4l.IO
{
    public static class UdpHandler
    {
        private static async IAsyncEnumerable<byte[]> ReceiveUdpDataAsync(UdpClient udpClient)
        {
            while (true)
            {
                UdpReceiveResult result;
                try
                {
                    result = await udpClient.ReceiveAsync();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                yield return result.Buffer;
            }
        }

        public static async Task ProcessUdpDataAsync(UdpClient udpClient, ChannelWriter<InternalCommand> writer)
        {
            await foreach (byte[] values in ReceiveUdpDataAsync(udpClient).ConfigureAwait(false))
            {
                // this should trigger some event that notifies the state of mutateful and possibly triggers a re-evaluation of any formulas (should take into account whether the received data is for a complete clip or just partial)
                Console.WriteLine($"Received datagram of size {values.Length}");
                //var result = processFunction(values);
                if (Decoder.IsTypedCommand(values))
                {
                    // new logic for handling input
                }
                else // old logic
                {
                    var result = CliHandler.HandleInput(values);
                    if (result != ClipSlot.Empty)
                        await writer.WriteAsync(new InternalCommand(InternalCommandType.SetClipSlot, result, new[] { result.Clip.ClipReference }));
                }
            }
        }

        public static async Task SendUdpDataAsync(UdpClient udpClient, ChannelReader<InternalCommand> reader)
        {
            await foreach (var internalCommand in reader.ReadAllAsync().ConfigureAwait(false))
            {
                var clipData = IOUtilities.GetClipAsBytes(internalCommand.ClipSlot.Id, internalCommand.ClipSlot.Clip).ToArray();
                Console.WriteLine($"Received data to send, with length {clipData.Length}");
                try
                {
                    udpClient.Send(clipData, clipData.Length, "127.0.0.1", 8023);
                }
                catch (Exception)
                {
                    Console.WriteLine("Exception occurred while sending UDP data");
                }

                await Task.Delay(10);
            }
        }
    }
}