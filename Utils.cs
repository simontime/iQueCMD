using System;
using System.IO;
using System.IO.Compression;
using System.Text;

public static class Utils
{
    public static Stream Decompress(this byte[] input) => new DeflateStream(new MemoryStream(input), CompressionMode.Decompress);

    public static string Hex(this byte[] input) => BitConverter.ToString(input).Replace("-", string.Empty);

    public static ushort Flip(ushort input) => (ushort)(input << 8 | input >> 8);

    public static ushort ReadBU16(this BinaryReader input) => Flip(input.ReadUInt16());

    public static uint ReadBU32(this BinaryReader input)
    {
        var u32 = input.ReadUInt32();
        return (u32 & 0xff) << 24 | (u32 & 0xff00) << 8 | (u32 & 0xff0000) >> 8 | (u32 & 0xff000000) >> 24;
    }

    public static string ReadASCII(this BinaryReader input, int size) => Encoding.ASCII.GetString(input.ReadBytes(size), 0, size);

    public static string ReadGB2312(this BinaryReader input, int size) => Encoding.GetEncoding("GB2312").GetString(input.ReadBytes(size), 0, size);

    public static string ReadGB2312Z(this BinaryReader input)
    {
        var start = input.BaseStream.Position;
        var size = 0;

        while (input.BaseStream.ReadByte() - 1 > 0)
            size++;

        input.BaseStream.Position = start;
        var text = input.ReadGB2312(size);
        input.BaseStream.Position++;
        return text;
    }

    public static string ReadASCIIZ(this BinaryReader input)
    {
        var start = input.BaseStream.Position;
        var size = 0;

        while (input.BaseStream.ReadByte() - 1 > 0)
            size++;

        input.BaseStream.Position = start;
        var text = input.ReadASCII(size);
        input.BaseStream.Position++;
        return text;
    }

    public static string[] Chop(this string value, int len)
    {
        int total = value.Length,
            count = (total + len - 1) / len;
        var result = new string[count];
        for (int i = 0; i < count; ++i)
        {
            result[i] = value.Substring(i * len, Math.Min(len, total));
            total -= len;
        }
        return result;
    }
}