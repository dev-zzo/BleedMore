using System;
using System.Text;

namespace BleedMore
{
    public static class HexPrinter
    {
        public static void Print(byte[] data, int offset = 0, int length = -1)
        {
            if (length == -1)
            {
                length = data.Length;
            }
            else
            {
                length += offset;
            }

            for (int i = offset; i < length; i += 16)
            {
                Console.WriteLine(FormatLine(data, i));
            }
        }

        static string FormatLine(byte[] data, int offset)
        {
            int length = Math.Min(data.Length - offset, 16);
            StringBuilder b = new StringBuilder(8 + 1 + 16 * 3 + 1 + 16 * 3 + 1 + 16);
            b.AppendFormat("{0:X8} ", offset);
            for (int i = 0; i < 16; i++)
            {
                if (i == 8)
                {
                    b.Append(" ");
                }

                if (i < length)
                {
                    b.AppendFormat("{0:X2} ", data[offset + i]);
                }
                else
                {
                    b.Append("   ");
                }
            }
            for (int i = 0; i < 16; i++)
            {
                if (i < length)
                {
                    byte x = data[offset + i];
                    if (x < 0x20)
                    {
                        b.Append('.');
                    }
                    else
                    {
                        b.Append((char)x);
                    }
                }
                else
                {
                    b.Append(' ');
                }
            }
            return b.ToString();
        }
    }
}
