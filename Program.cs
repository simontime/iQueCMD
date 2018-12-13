using System;
using System.IO;
using System.Text;
using static Image;
using static Rights;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: iQueCMD.exe file.cmd");
            return;
        }

        string titleName = null;

        using (var strm  = File.OpenRead(args[0]))
        using (var rdr   = new BinaryReader(strm))
        using (var desc  = new MemoryStream())
        using (var desw  = new StreamWriter(desc, Encoding.Unicode))
        using (var meta  = new MemoryStream())
        using (var metw  = new StreamWriter(meta, Encoding.Unicode))
        using (var thumb = new MemoryStream())
        using (var thwrt = new BinaryWriter(thumb))
        using (var thstm = new MemoryStream())
        using (var thrdr = new BinaryReader(thstm))
        using (var title = new MemoryStream())
        using (var tiwrt = new BinaryWriter(title))
        using (var tistm = new MemoryStream())
        using (var tirdr = new BinaryReader(tistm))
        {
            var separator = new string('-', 32);

            void DualPrintDesc(string format, object arg0 = null)
            {
                Console.WriteLine(format, arg0);
                desw.WriteLine(format, arg0);
            }

            void DualPrintMeta(string format, object arg0 = null)
            {
                Console.WriteLine(format, arg0);
                metw.WriteLine(format, arg0);
            }

            void PrintMulti(params string[] input)
            {
                foreach (var str in input)
                    Console.WriteLine(str);
            }

            PrintMulti(separator, "contentDesc:", separator);

            DualPrintDesc("EEPROM RDRAM location: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("EEPROM size: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("Flash RDRAM location: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("Flash size: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("SRAM RDRAM location: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("SRAM size: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("Controller Pak 0 location: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("Controller Pak 1 location: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("Controller Pak 2 location: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("Controller Pak 3 location: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("Controller Pak size: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("osRomBase: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("osTvType: 0x{0:x}", rdr.ReadBU32());
            DualPrintDesc("osMemSize: 0x{0:x}", rdr.ReadBU32());

            strm.Position += sizeof(ulong);

            DualPrintDesc("Magic: {0}", rdr.ReadASCII(3));
            DualPrintDesc("Number of .u0x files: {0}", rdr.ReadByte());

            Console.WriteLine(separator);

            ushort thumbLen = rdr.ReadBU16(),
                   titleLen = rdr.ReadBU16();

            rdr.ReadBytes(thumbLen).Decompress().CopyTo(thstm);
            rdr.ReadBytes(titleLen).Decompress().CopyTo(tistm);

            thstm.Position = 0;
            tistm.Position = 0;

            for (int i = 0; i < thstm.Length; i += sizeof(ushort))
                thwrt.Write(_5551to8888(thrdr.ReadBU16()));

            for (int i = 0; i < tistm.Length; i += sizeof(ushort))
                tiwrt.Write(_5551to8888(tirdr.ReadBU16()));

            titleName = rdr.ReadGB2312Z();

            DualPrintDesc("Title name: {0}", titleName);

            PrintMulti(separator, "BbContentMetaDataHead:", separator);

            strm.Position = 0x2804;

            DualPrintMeta("CA Crl Version: 0x{0:x}", rdr.ReadBU32());
            DualPrintMeta("CP Crl Version: 0x{0:x}", rdr.ReadBU32());
            DualPrintMeta("Size: 0x{0:x}", rdr.ReadBU32());
            DualPrintMeta("Desc flags: 0x{0:x}", rdr.ReadBU32());
            DualPrintMeta("Titlekey IV: {0}", rdr.ReadBytes(0x10).Hex());
            DualPrintMeta("SHA1 Hash: {0}", rdr.ReadBytes(0x14).Hex());
            DualPrintMeta("Content IV: {0}", rdr.ReadBytes(0x10).Hex());
            DualPrintMeta("Exec flags: 0x{0:x}", rdr.ReadBU32());
            DualPrintMeta("HW access rights: {0}", Convert.ToString(rdr.ReadBU32(), 2).PadLeft(9, '0'));
            DualPrintMeta("Secure kernel rights: {0}", GetCalls(rdr.ReadBU32()));
            DualPrintMeta("BBID: 0x{0:x}", rdr.ReadBU32());
            DualPrintMeta("Issuer: {0}", rdr.ReadASCII(64).TrimEnd('\0'));
            DualPrintMeta("Content ID: 0x{0:x}", rdr.ReadBU32());
            DualPrintMeta("Titlekey: {0}", rdr.ReadBytes(0x10).Hex());
            DualPrintMeta("Signature:");

            foreach (var hex in rdr.ReadBytes(0x100).Hex().Chop(0x20))
                DualPrintMeta(hex);

            Console.WriteLine(separator);

            desw.Flush();
            metw.Flush();

            Directory.CreateDirectory(titleName);

            File.WriteAllBytes($"{titleName}/contentDesc.txt", desc.ToArray());
            File.WriteAllBytes($"{titleName}/BbContentMetaDataHead.txt", meta.ToArray());

            SaveImage(thumb.ToArray(), titleName, true);
            SaveImage(title.ToArray(), titleName, false);

            if (strm.Length > 0x29AC)
                File.WriteAllBytes($"{titleName}/ticket.dat", rdr.ReadBytes((int)(strm.Length - strm.Position)));
        }
    }
}