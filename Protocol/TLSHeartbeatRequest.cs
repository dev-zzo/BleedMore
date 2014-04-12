using System;

// http://tools.ietf.org/html/rfc6520

namespace BleedMore.Protocol
{
    public class TLSHeartbeatRequest
    {
        //00000000 18 03 02 00 06  01 00 00 02 40 00
        //00000000 18 03 02 00 03  01 40 00
        public ushort PayloadLength;
        public byte[] Payload;
        public byte[] Padding;
    }

    static class TLSHeartbeatRequestSerializationExtensions
    {
        public static TLSHeartbeatRequest Read(this BufferStream stream, int Length, TLSHeartbeatRequest o)
        {
            throw new NotImplementedException();
        }

        public static void Write(this BufferStream stream, TLSHeartbeatRequest o)
        {
            stream.WriteUInt16(o.PayloadLength);
            stream.Write(o.Payload, 0, o.Payload.Length);
            if (o.Padding != null)
            {
                stream.Write(o.Padding, 0, o.Padding.Length);
            }
        }

        public static int EstimateLength(this BufferStream stream, TLSHeartbeatRequest o)
        {
            return 2 + o.Payload.Length + (o.Padding != null ? o.Padding.Length : 0);
        }
    }
}
