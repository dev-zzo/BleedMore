using System;

namespace BleedMore.Protocol
{
    public class TLSServerHello
    {
        public TLSVersion ProtocolVersion;
        public TLSRandom Random;
        public TLSSessionId SessionId;
        public ushort CipherSuite;
        public byte Compression;
        public TLSExtension[] Extensions; // Since 1.2
    }

    public static class TLSServerHelloSerializationExtensions
    {
        public static TLSServerHello Read(this BufferStream stream, int Length, TLSServerHello o)
        {
            o.ProtocolVersion = stream.Read(new TLSVersion());
            o.Random = stream.Read(new TLSRandom());
            o.SessionId = stream.Read(new TLSSessionId());
            o.CipherSuite = stream.ReadUInt16();
            o.Compression = stream.ReadUInt8();
            if (stream.BytesRead < Length)
            {
                // Extensions
                stream.SkipRead(Length - stream.BytesRead);
            }
            return o;
        }

        public static void Write(this BufferStream stream, TLSServerHello o)
        {
            throw new NotImplementedException();
        }
    }
}
