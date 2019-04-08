using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_Library.Classes
{
    public static class AppDataManager
    {
        public static string Folder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "osu!Library");

        public static void Save(object file, string localPath)
        {
            CreateFolder();

            Type _fileType = file.GetType();
            string _pathToFile = Path.Combine(Folder, localPath);
            StreamWriter writer;

            switch (_fileType.FullName)
            {
                case "System.String[]":
                    using (writer = new StreamWriter(_pathToFile, false))
                    {
                        foreach (string line in file as string[])
                            writer.WriteLine(line);
                    }
                    return;

                case "System.String":
                    using (writer = new StreamWriter(_pathToFile, false))
                    {
                        writer.WriteLine(file as string);
                    }
                    return;

                default:
                    throw new Exception($"AppDataManager: Input file type ({_fileType.FullName}) does not supported.");
            }

        }

        public static string[] Load(string localPath)
        {
            string _pathToFile = Path.Combine(Folder, localPath);
            string[] result;

            if (!File.Exists(_pathToFile))
                return null;

            try
            {
                result = File.ReadAllLines(_pathToFile);

                return result;
            }
            catch (Exception e)
            {
                throw new Exception($"AppDataManager: Error occured during loading text file. {e.Message}");
            }
        }

        public static bool Exists(string localPath)
        {
            string _pathToFile = Path.Combine(Folder, localPath);

            if (File.Exists(_pathToFile))
                return true;
            else
                return false;
        }

        private static void CreateFolder()
        {
            if (!Directory.Exists(Folder))
                Directory.CreateDirectory(Folder);
        }
    }
}
