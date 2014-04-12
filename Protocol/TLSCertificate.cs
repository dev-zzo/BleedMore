
namespace BleedMore.Protocol
{
    public class TLSCertificate
    {
        public byte[] X509Data;
    }

    public static class TLSCertificateSerializationExtensions
    {
        public static TLSCertificate Read(this BufferStream stream, TLSCertificate o)
        {
            int length = (int)stream.ReadUInt24();
            o.X509Data = new byte[length];
            stream.Read(o.X509Data, 0, length);
            return o;
        }

        public static void Write(this BufferStream stream, TLSCertificate o)
        {
            stream.WriteUInt24((uint)o.X509Data.Length);
            stream.Write(o.X509Data, 0, o.X509Data.Length);
        }
    }
}
