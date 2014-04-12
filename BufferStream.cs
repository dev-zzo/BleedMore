using System;
using System.Collections.Generic;

namespace BleedMore
{
    public sealed class BufferStream
    {
        public BufferStream(int preallocation = 8192)
        {
            this.Buffer = new byte[preallocation];
            this.ReadPosition = 0;
            this.WritePosition = 0;
        }

        public BufferStream(byte[] buffer)
        {
            this.Buffer = buffer;
            this.ReadPosition = 0;
            this.WritePosition = this.Buffer.Length;
        }

        public int BytesRead { get { return this.ReadPosition; } }

        public int BytesWritten { get { return this.WritePosition; } }

        public int Available
        {
            get { return WritePosition - ReadPosition; }
        }

        public byte[] Bytes { get { return this.Buffer; } }

        public void SkipRead(int length)
        {
            ReadPosition = Math.Min(ReadPosition + length, WritePosition);
        }

        public int Read(byte[] dest, int offset, int length)
        {
            int amount = WritePosition - ReadPosition;
            if (amount > length)
            {
                amount = length;
            }

            if (amount > 0)
            {
                Array.Copy(Buffer, ReadPosition, dest, offset, amount);
                ReadPosition += amount;
            }

            return amount;
        }

        public byte ReadUInt8()
        {
            int count = Read(TempBuffer, 0, 1);
            if (count <= 0)
            {
                throw new InvalidOperationException();
            }
            return TempBuffer[0];
        }

        public ushort ReadUInt16()
        {
            int count = Read(TempBuffer, 0, 2);
            if (count <= 0)
            {
                throw new InvalidOperationException();
            }
            return (ushort)((TempBuffer[0] << 8) | (TempBuffer[1]));
        }

        public uint ReadUInt24()
        {
            int count = Read(TempBuffer, 0, 3);
            if (count <= 0)
            {
                throw new InvalidOperationException();
            }
            return (uint)((TempBuffer[0] << 16) | (TempBuffer[1] << 8) | (TempBuffer[2]));
        }

        public uint ReadUInt32()
        {
            int count = Read(TempBuffer, 0, 4);
            if (count <= 0)
            {
                throw new InvalidOperationException();
            }
            return (uint)((TempBuffer[0] << 24) | (TempBuffer[1] << 16) | (TempBuffer[2] << 8) | (TempBuffer[3]));
        }

        public void Write(byte[] src, int offset, int length)
        {
            if (length > 0)
            {
                if (WritePosition + length > Buffer.Length)
                {
                    Array.Resize(ref Buffer, WritePosition + length + 256);
                }

                Array.Copy(src, offset, Buffer, WritePosition, length);
                WritePosition += length;
            }
        }

        public void WriteUInt8(byte data)
        {
            TempBuffer[0] = data;
            Write(TempBuffer, 0, 1);
        }

        public void WriteUInt16(ushort data)
        {
            WriteUInt8((byte)(data >> 8));
            WriteUInt8((byte)(data & 0xFF));
        }

        public void WriteUInt24(uint data)
        {
            WriteUInt8((byte)(data >> 16));
            WriteUInt8((byte)(data >> 8));
            WriteUInt8((byte)(data & 0xFF));
        }

        public void WriteUInt32(uint data)
        {
            WriteUInt8((byte)(data >> 24));
            WriteUInt8((byte)(data >> 16));
            WriteUInt8((byte)(data >> 8));
            WriteUInt8((byte)(data & 0xFF));
        }

        public void PushWritePosition()
        {
            WritePositionStack.Push(WritePosition);
        }

        public void PopWritePosition()
        {
            WritePosition = WritePositionStack.Pop();
        }

        public void Flush()
        {
            if (ReadPosition > 0)
            {
                Array.Copy(Buffer, ReadPosition, Buffer, 0, WritePosition - ReadPosition);
                WritePosition -= ReadPosition;
                ReadPosition = 0;
            }
        }

        private byte[] Buffer;
        private byte[] TempBuffer = new byte[4];
        private int ReadPosition;
        private int WritePosition;
        private Stack<int> WritePositionStack = new Stack<int>();
    }
}
