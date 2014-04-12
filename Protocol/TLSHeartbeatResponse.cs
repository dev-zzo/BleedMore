using System;

// http://tools.ietf.org/html/rfc6520

namespace BleedMore.Protocol
{
    public sealed class TLSHeartbeatResponse
    {
        public ushort PayloadLength;
        public byte[] Payload;
        public byte[] Padding;
    }

    static class TLSHeartbeatResponseSerializationExtensions
    {
        public static TLSHeartbeatResponse Read(this BufferStream stream, int Length, TLSHeartbeatResponse o)
        {
            o.PayloadLength = (ushort)Length;
            o.Payload = new byte[Length];
            int count = stream.Read(o.Payload, 0, o.PayloadLength);
            if (stream.Available > 0)
            {
                stream.SkipRead(stream.Available);
            }
            //int paddingLength = Length - o.PayloadLength - 2;
            //if (paddingLength > 0)
            //{
            //    o.Padding = new byte[paddingLength];
            //    stream.Read(o.Padding, 0, paddingLength);
            //}
            return o;
        }

        public static void Write(this BufferStream stream, TLSHeartbeatResponse o)
        {
            throw new NotImplementedException();
        }

        public static int EstimateLength(this BufferStream stream, TLSHeartbeatResponse o)
        {
            throw new NotImplementedException();
        }
    }
}
