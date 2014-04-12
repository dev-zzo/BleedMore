using System;

namespace BleedMore.Protocol
{
    public class TLSSessionId
    {
        public byte[] Data;
    }

    static class TLSSessionIdSerializationExtensions
    {
        public static TLSSessionId Read(this BufferStream stream, TLSSessionId o)
        {
            byte length = stream.ReadUInt8();
            o.Data = new byte[length];
            stream.Read(o.Data, 0, length);
            return o;
        }

        public static void Write(this BufferStream stream, TLSSessionId o)
        {
            if (o != null)
            {
                stream.WriteUInt8((byte)o.Data.Length);
                stream.Write(o.Data, 0, o.Data.Length);
            }
            else
            {
                stream.WriteUInt8(0);
            }
        }

        public static int EstimateLength(this BufferStream stream, TLSSessionId o)
        {
            return o != null ? 1 + o.Data.Length : 1;
        }
    }
}
