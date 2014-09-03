using System.IO;

namespace GitBranchDiff
{
    public class TempDirectory
    {
        private DirectoryInfo info;

        ~TempDirectory()
        {
            if (info != null)
            {
                foreach (FileInfo file in info.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch
                    {
                    }
                }

                info.Delete();
            }
        }

        public DirectoryInfo Info
        {
            get
            {
                if (info == null)
                {
                    var name = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    info = Directory.CreateDirectory(name);
                }

                return info;
            }
        }  
    }
}