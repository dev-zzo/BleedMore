using System;

namespace BleedMore.Protocol
{
    public sealed class TLSVersion
    {
        public TLSVersion()
            : this(3,2)
        {
        }

        public TLSVersion(byte major, byte minor)
        {
            this.Major = major;
            this.Minor = minor;
        }

        public byte Major;
        public byte Minor;

        public static TLSVersion TLS10 = new TLSVersion(3, 1);
        public static TLSVersion TLS11 = new TLSVersion(3, 2);
        public static TLSVersion TLS12 = new TLSVersion(3, 3);

        public override string ToString()
        {
            if (this.Major == 3)
            {
                switch (this.Minor)
                {
                    case 0:
                        return "SSL 3.0";
                    case 1:
                        return "TLS 1.0";
                    case 2:
                        return "TLS 1.1";
                    case 3:
                        return "TLS 1.2";
                }
            }
            return String.Format("Unknown ver: {0}.{1}", this.Major, this.Minor);
        }
    }

    static class TLSVersionSerializationExtensions
    {
        public static TLSVersion Read(this BufferStream stream, TLSVersion o)
        {
            o.Major = stream.ReadUInt8();
            o.Minor = stream.ReadUInt8();
            return o;
        }

        public static void Write(this BufferStream stream, TLSVersion o)
        {
            stream.WriteUInt8(o.Major);
            stream.WriteUInt8(o.Minor);
        }

        public static int EstimateLength(this BufferStream stream, TLSVersion o)
        {
            return 2;
        }
    }
}
