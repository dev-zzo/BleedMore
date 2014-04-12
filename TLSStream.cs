using System;
using System.Net.Sockets;
using BleedMore.Protocol;

namespace BleedMore
{
    public sealed class TLSStream
    {
        public delegate void HandshakeMessageDelegate(TLSStream Stream, byte Type, int Length, BufferStream Data);
        public delegate void HeartbeatMessageDelegate(TLSStream Stream, byte Type, int Length, BufferStream Data);

        public TLSStream(NetworkStream stream)
        {
            this.stream = stream;
        }

        public event HandshakeMessageDelegate HandshakeMessageEvent;
        public event HeartbeatMessageDelegate HeartbeatMessageEvent;

        public TLSVersion DefaultVersion;

        public void DispatchLoop()
        {
            byte[] Header = new byte[5];
            TLSVersion Version = new TLSVersion();
            byte[] Content = new byte[16384 + 1024];

            while (true)
            {
                // Read the next record
                Console.WriteLine("[.] Reading next TLS record...");
                ReadBuffer(Header, Header.Length);
                BufferStream HeaderStream = new BufferStream(Header);

                // Parse the header data
                byte Type = HeaderStream.ReadUInt8();
                HeaderStream.Read(Version);
                int Length = HeaderStream.ReadUInt16();
                Console.WriteLine("[.] Record header: type {0}, version {1}, length {2}", Type, Version, Length);

                if (Length < Content.Length)
                {
                    ReadBuffer(Content, Length);
                    DispatchContent(Type, Version, Length, Content);
                }
            }
        }

        public void SendHandshakeContent(TLSVersion Version, byte[] Content, int Length)
        {
            SendContent(Version, 22, Content, Length);
        }

        public void SendHeartbeatContent(TLSVersion Version, byte[] Content, int Length)
        {
            SendContent(Version, 24, Content, Length);
        }

        public void SendContent(TLSVersion Version, byte Type, byte[] Content, int Length)
        {
            BufferStream b = new BufferStream(5 + Length);
            b.WriteUInt8(Type);
            b.Write(Version);
            b.WriteUInt16((ushort)Length);
            b.Write(Content, 0, Length);
            Console.WriteLine("[.] Sending TLS record:");
            HexPrinter.Print(b.Bytes);
            stream.Write(b.Bytes, 0, b.BytesWritten);
        }

        private NetworkStream stream;
        private readonly BufferStream ChangeCipherSpecStream = new BufferStream(128);
        private readonly BufferStream AlertStream = new BufferStream(128);
        private readonly BufferStream HandshakeStream = new BufferStream();
        private int HandshakeParserState = 0;
        private byte HandshakeMessageType;
        private int HandshakeMessageLength;
        private readonly BufferStream ApplicationStream = new BufferStream();
        private readonly BufferStream HeartbeatStream = new BufferStream(128);
        private int HeartbeatParserState = 0;
        private byte HeartbeatMessageType;
        private int HeartbeatMessageLength;

        private void ParseHandshakeContent()
        {
            bool redo;

            do
            {
                redo = false;
                switch (HandshakeParserState)
                {
                    case 0:
                        // Waiting for msg type/length
                        if (HandshakeStream.Available < 4)
                            break;

                        HandshakeMessageType = (byte)HandshakeStream.ReadUInt8();
                        HandshakeMessageLength = (int)HandshakeStream.ReadUInt24();
                        if (HandshakeMessageLength < 0x4000)
                        {
                            HandshakeStream.Flush();
                            HandshakeParserState = 1;
                            redo = true;
                        }
                        else
                        {
                            // Fail.
                        }

                        break;

                    case 1:
                        // Waiting for content
                        if (HandshakeStream.Available < HandshakeMessageLength)
                            break;

                        if (HandshakeMessageEvent != null)
                        {
                            HandshakeMessageEvent.Invoke(this, HandshakeMessageType, HandshakeMessageLength, HandshakeStream);
                        }

                        HandshakeStream.Flush();
                        HandshakeParserState = 0;
                        redo = true;
                        break;
                }
            } while (redo);

        }

        private void ParseHeartbeatContent()
        {
            bool redo;

            do
            {
                redo = false;
                switch (HeartbeatParserState)
                {
                    case 0:
                        // Waiting for msg type/length
                        if (HeartbeatStream.Available < 4)
                            break;

                        HeartbeatMessageType = (byte)HeartbeatStream.ReadUInt8();
                        HeartbeatMessageLength = (int)HeartbeatStream.ReadUInt16();
                        Console.WriteLine("[.] Heartbeat message: type {0}, length {1}", HeartbeatMessageType, HeartbeatMessageLength);
                        if (HeartbeatMessageLength <= 0xFFFF)
                        {
                            HeartbeatStream.Flush();
                            HeartbeatParserState = 1;
                            redo = true;
                        }
                        else
                        {
                            // Fail.
                        }

                        break;

                    case 1:
                        // Waiting for content
                        if (HeartbeatStream.Available < HeartbeatMessageLength)
                        {
                            Console.WriteLine("[.] Heartbeat message: length {0}, available {1}", HeartbeatMessageLength, HeartbeatStream.Available);
                            break;
                        }

                        if (HeartbeatMessageEvent != null)
                        {
                            HeartbeatMessageEvent.Invoke(this, HeartbeatMessageType, HeartbeatMessageLength, HeartbeatStream);
                        }

                        HeartbeatStream.Flush();
                        HeartbeatParserState = 0;
                        redo = true;
                        break;
                }
            } while (redo);
        }

        private void DispatchContent(byte Type, TLSVersion Version, int Length, byte[] Content)
        {
            switch (Type)
            {
                case 20:
                    ChangeCipherSpecStream.Write(Content, 0, Length);
                    break;
                case 21:
                    // Alert protocol
                    AlertStream.Write(Content, 0, Length);
                    break;
                case 22:
                    // Handshake protocol
                    HandshakeStream.Write(Content, 0, Length);
                    ParseHandshakeContent();
                    break;
                case 23:
                    // Application protocol
                    ApplicationStream.Write(Content, 0, Length);
                    break;
                case 24:
                    // Heartbeat protocol
                    HeartbeatStream.Write(Content, 0, Length);
                    ParseHeartbeatContent();
                    break;
                default:
                    Console.WriteLine("[!] Received data of unknown content type.");
                    break;
            }
        }

        /// <summary>
        /// Read the given number of bytes from the net stream.
        /// </summary>
        /// <param name="Buffer">Buffer to read into</param>
        /// <param name="Length">Amount</param>
        private void ReadBuffer(byte[] Buffer, int Length)
        {
            int Offset = 0;
            while (Length > 0)
            {
                int count = stream.Read(Buffer, Offset, Length);
                Offset += count;
                Length -= count;
            }
        }
    }
}
