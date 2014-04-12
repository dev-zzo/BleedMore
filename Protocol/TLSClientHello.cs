using System;

namespace BleedMore.Protocol
{
    public class TLSClientHello
    {
        public TLSVersion ProtocolVersion;
        public TLSRandom Random;
        public TLSSessionId SessionId;
        public ushort[] CipherSuites;
        public byte[] Compressions;
        public TLSExtension[] Extensions; // Since 1.2
    }

    public static class TLSClientHelloSerializationExtensions
    {
        public static TLSClientHello Read(this BufferStream stream, int Length, TLSClientHello o)
        {
            throw new NotImplementedException();
        }

        public static void Write(this BufferStream stream, TLSClientHello o)
        {
            stream.Write(o.ProtocolVersion);
            stream.Write(o.Random);
            stream.Write(o.SessionId);
            stream.WriteUInt16((ushort)(o.CipherSuites.Length * 2));
            for (int i = 0; i < o.CipherSuites.Length; i++)
            {
                stream.WriteUInt16(o.CipherSuites[i]);
            }
            stream.WriteUInt8((byte)o.Compressions.Length);
            for (int i = 0; i < o.Compressions.Length; i++)
            {
                stream.WriteUInt8(o.Compressions[i]);
            }
            if (o.Extensions != null)
            {
                // TODO
            }
        }

        public static int EstimateLength(this BufferStream stream, TLSClientHello o)
        {
            int length = 0;
            length += stream.EstimateLength(o.ProtocolVersion);
            length += stream.EstimateLength(o.Random);
            length += stream.EstimateLength(o.SessionId);
            length += 2 + o.CipherSuites.Length * 2;
            length += 1 + o.Compressions.Length;
            if (o.Extensions != null)
            {
                // TODO
            }
            return length;
        }
    }
}
