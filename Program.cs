using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JakDaxterVAGWAD
{
    internal class Program
    {
        static void Main(string[] args)
        {
           //Extract("VAGDIR.AYB", "VAGWAD.ENG");
           //Rebuild("VAGWAD");
           if (args.Length == 0)
            {
                Console.WriteLine("Rebuild: tool.exe dir\nExtract: FILE.AYB FILE.ENG");
            }
           if (args.Length == 1)
            {
                Rebuild(args[0]);
            }
            else
            {
                Extract(args[0], args[1]);
            }
        }
        public static void Extract(string toc, string arc)
        {
            var reader = new BinaryReader(File.OpenRead(toc));
            string dir = Path.GetFileNameWithoutExtension(arc) + "\\";
            Directory.CreateDirectory(Path.GetFileNameWithoutExtension(arc));
            int count = reader.ReadInt32();
            string[] names = new string[count];
            int[] offset = new int[count];
            for (int i = 0; i < count; i++)
            {
                names[i] = Encoding.UTF8.GetString(reader.ReadBytes(8));
                offset[i] = reader.ReadInt32() * 2048;
            }
            reader.Close();
            var arcreader = new BinaryReader(File.OpenRead(arc));
            for (int i = 0; i < count; i++)
            {
                arcreader.BaseStream.Position = offset[i];
                if (i == count - 1)
                {
                    byte[] bytes = arcreader.ReadBytes((int)arcreader.BaseStream.Length - offset[i]);
                    File.WriteAllBytes(dir + i + "_" + names[i] + ".vag", bytes);
                }
                else
                {
                    byte[] bytes = arcreader.ReadBytes(offset[i + 1] - offset[i]);
                    File.WriteAllBytes(dir + i + "_" + names[i] + ".vag", bytes);
                }
            }
        }
        public static void Rebuild(string dir)
        {
            string[] files = Directory.GetFiles(dir);
            files = SortNamesByNumber(files);
            string[] names = new string[files.Length];
            int[] offset = new int[files.Length];
            using (BinaryWriter arcwriter = new BinaryWriter(File.Create(dir + ".ARC")))
            using (BinaryWriter tocwriter = new BinaryWriter(File.Create("VAGDIR.AYB")))
            {
                tocwriter.Write((int)files.Length);
                for (int i = 0; i < files.Length; i++)
                {
                    byte[] bytes = File.ReadAllBytes(files[i]);
                    if (i == 0)
                    {
                        offset[i] = (int)arcwriter.BaseStream.Position;
                    }
                    else
                    {
                        offset[i] = (int)arcwriter.BaseStream.Position / 2048;
                    }
                    arcwriter.Write(bytes);
                    names[i] = GetFileNameWithoutExtension(files[i]);
                    tocwriter.Write(Encoding.UTF8.GetBytes(names[i]));
                    tocwriter.Write(offset[i]);
                }
                tocwriter.Write(new byte[10432 - tocwriter.BaseStream.Length]);
            }
        }
        static string[] SortNamesByNumber(string[] names)
        {
            return names.OrderBy(name =>
            {
                int underscoreIndex = name.LastIndexOf('_');
                if (underscoreIndex != -1)
                {
                    string numberString = name.Substring(name.LastIndexOf('\\') + 1, underscoreIndex - name.LastIndexOf('\\') - 1);
                    bool success = int.TryParse(numberString, out int number);
                    return success ? number : int.MaxValue;
                }
                else
                {
                    return int.MaxValue;
                }
            }).ToArray();
        }
        static string GetFileNameWithoutExtension(string filePath)
        {
            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
            int indexOfUnderscore = fileName.IndexOf('_');

            if (indexOfUnderscore >= 0)
            {
                fileName = fileName.Substring(indexOfUnderscore + 1);
            }

            return fileName;
        }
    }
}
