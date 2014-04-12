using System;
using BleedMore.Protocol;

namespace BleedMore
{
    public sealed class TLSHandshakeStream
    {
        public delegate void TLSServerHelloDelegate(TLSServerHello msg);
        public delegate void TLSCertificateDelegate(TLSCertificate msg);
        public delegate void TLSServerHelloDoneDelegate();

        public TLSHandshakeStream(TLSStream stream)
        {
            this.stream = stream;
            this.stream.HandshakeMessageEvent += new TLSStream.HandshakeMessageDelegate(HandleHandshakeMessage);
        }

        public event TLSServerHelloDelegate TLSServerHelloEvent;
        public event TLSCertificateDelegate TLSCertificateEvent;
        public event TLSServerHelloDoneDelegate TLSServerHelloDoneEvent;

        public void Send(TLSClientHello o)
        {
            BufferStream bs = new BufferStream();
            bs.WriteUInt8(1);
            bs.PushWritePosition();
            bs.WriteUInt24(0);
            bs.Write(o);
            int length = bs.BytesWritten - 4;
            bs.PopWritePosition();
            bs.WriteUInt24((uint)length);
            stream.SendHandshakeContent(o.ProtocolVersion, bs.Bytes, length + 4);
        }

        private TLSStream stream;

        private void HandleHandshakeMessage(TLSStream unused, byte Type, int Length, BufferStream Data)
        {
            Data.Flush();
            switch (Type)
            {
                case 2:
                    HandleServerHello(Length, Data);
                    break;
#if false
                case 12:
                    break;
#endif

                case 11:
                    HandleCertificate(Length, Data);
                    break;

                case 14:
                    HandleServerHelloDone();
                    break;

                default:
                    Console.WriteLine("[.] Handshake message type {0}, length {1}.", Type, Length);
                    Data.SkipRead(Length);
                    //HexPrinter.Print(message.Content);
                    break;
            }
        }

        private void HandleServerHello(int Length, BufferStream Data)
        {
            Console.WriteLine("[.] ServerHello, length {0}.", Length);
            TLSServerHello hello = Data.Read(Length, new TLSServerHello());
            stream.DefaultVersion = hello.ProtocolVersion; // HACK
            if (this.TLSServerHelloEvent != null)
            {
                this.TLSServerHelloEvent.Invoke(hello);
            }
        }

        private void HandleCertificate(int Length, BufferStream Data)
        {
            Console.WriteLine("[.] Certificate, length {0}.", Length);
            TLSCertificate cert = Data.Read(new TLSCertificate());
            if (this.TLSCertificateEvent != null)
            {
                this.TLSCertificateEvent.Invoke(cert);
            }
        }

        private void HandleServerHelloDone()
        {
            Console.WriteLine("[.] ServerHelloDone.");
            if (this.TLSServerHelloDoneEvent != null)
            {
                this.TLSServerHelloDoneEvent.Invoke();
            }
        }

    }
}
