using System;

namespace BleedMore.Protocol
{
    public sealed class TLSRandom
    {
        public TLSRandom()
        {
            this.GMTUnixTime = 0;
            this.RandomBytes = new byte[28];
        }

        public uint GMTUnixTime;
        public byte[] RandomBytes;
    }

    static class TLSRandomSerializationExtensions
    {
        public static TLSRandom Read(this BufferStream stream, TLSRandom o)
        {
            o.GMTUnixTime = stream.ReadUInt32();
            o.RandomBytes = new byte[28];
            stream.Read(o.RandomBytes, 0, 28);
            return o;
        }

        public static void Write(this BufferStream stream, TLSRandom o)
        {
            stream.WriteUInt32(o.GMTUnixTime);
            stream.Write(o.RandomBytes, 0, o.RandomBytes.Length);
        }

        public static int EstimateLength(this BufferStream stream, TLSRandom o)
        {
            return 32;
        }
    }
}
