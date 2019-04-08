using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu_Library.Classes
{
    public class Beatmap
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Creator { get; set; }
        public int ID { get; set; }

        public string PathToAudio { get; set; }
        public string PathToBackground { get; set; }

        private readonly string[] imageFormats = new string[] { ".jpg", ".png", ".jpeg" };

        public Beatmap()
        {

        }

        public Beatmap(string path)
        {
            string workingDirectory = Path.GetDirectoryName(path);

            List<string> info = File.ReadAllLines(path).ToList();

            string id = info.Where(x => x.Contains("BeatmapID:")).FirstOrDefault();

            if (id != null)
            {
                try
                {
                    ID = Convert.ToInt32(id.Split(':')[1]);
                }
                catch
                {
                    ID = -1;
                }
            }

            Artist = info.Where(x => x.Contains("Artist:")).FirstOrDefault().Split(':')[1];
            Title = info.Where(x => x.Contains("Title:")).FirstOrDefault().Split(':')[1];
            Creator = info.Where(x => x.Contains("Creator:")).FirstOrDefault().Split(':')[1];
            PathToAudio = Path.Combine(workingDirectory, info.Where(x => x.Contains("AudioFilename:")).FirstOrDefault().Split(':')[1].Remove(0, 1));

            foreach (string format in imageFormats)
            {
                string data = info.Where(x => x.ToLower().Contains(format)).FirstOrDefault();

                if (data != null)
                {
                    PathToBackground = Path.Combine(workingDirectory, data.Split(',').Where(x => x.ToLower().Contains(format)).First().Trim('"'));
                    break;
                }
            }
        }

        public override string ToString()
        {
            return $"{Artist} - {Title} // {Creator}";
        }
    }
}
