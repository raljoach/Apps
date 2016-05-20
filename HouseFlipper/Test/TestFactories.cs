using Common.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.ModelBased;

namespace HouseFlipper.Test
{
    public class DataFolderFactory : ParameterValueFactory
    {
        private static string fooPath;
        private static string barPath;
        private static string quotedFooPath;

        static DataFolderFactory()
        {
            fooPath = Path.Combine(DirectoryIO.GetProjectDir(), "foo");
            quotedFooPath = "\"" + fooPath + "\"";
            barPath = Path.Combine(fooPath, "bar.csv");
        }

        public FolderPath Create(DataFolder dataFolder)
        {
            FolderPath folderPath = null;
            switch (dataFolder)
            {
                case DataFolder.Default:
                    break;
                case DataFolder.NotExistent:
                    folderPath = DeleteFooFolder();                    
                    break;
                case DataFolder.Exists:
                    folderPath = CreateFooFolder();                    
                    break;
                default:
                    throw new InvalidOperationException(string.Format(@"Error: Unhandled \data parameter value {0}", dataFolder));
            }

            return folderPath;
        }

        private FolderPath CreateFooFolder()
        {
            string folder = fooPath;
            new FolderFactory().Create(folder);
            return new FolderPath(folder, quotedFooPath);
        }

        private FolderPath DeleteFooFolder()
        {
            string folder = fooPath;
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }

            return new FolderPath(folder, quotedFooPath);
        }
    }

    public class FolderFactory : ParameterValueFactory
    {
        public void Create(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }
    }

    public class FilesFactory : ParameterValueFactory
    {
        public void Create(FolderPath folderPath, Files files)
        {
            switch (files)
            {
                case Files.UnusedParameter:
                    break;
                case Files.NotExistent:
                    MakeFolderEmpty(folderPath);
                    break;
                case Files.Exists:
                    break;
                default:
                    throw new InvalidOperationException(string.Format(@"Error: Unhandled \files parameter value {0}", files));
            }
        }

        public void MakeFolderEmpty(FolderPath folderPath)
        {
            var folder = folderPath.Raw;
            DeleteAllFiles(folder);
            DeleteSubDirectories(folder);
        }

        public void CreateFile(FolderPath folderPath, string fileName)
        {
            var folder = folderPath.Raw;
            var filePath = Path.Combine(folder, fileName);
            using (var f = File.Create(filePath))
            {
            }
        }

        public IEnumerable<string> GetFiles(FolderPath folderPath)
        {
            var folder = folderPath.Raw;
            foreach (var f in Directory.GetFiles(folder))
            {
                yield return f;
            }
        }

        private void DeleteSubDirectories(string folder)
        {
            foreach (var s in Directory.GetDirectories(folder))
            {
                Directory.Delete(s);
            }
        }

        private void DeleteAllFiles(string folder)
        {
            foreach (var f in Directory.GetFiles(folder))
            {
                File.Delete(f);
            }
        }
    }

    public class FilesCountFactory
    {
        public void Create(FolderPath folderPath, FileCount filesCount)
        {
            switch (filesCount)
            {
                case FileCount.UnusedParameter:
                    break;
                case FileCount.Single:
                    new FilesFactory().MakeFolderEmpty(folderPath);
                    new FilesFactory().CreateFile(folderPath, "bar.csv");
                    break;
                default:
                    throw new InvalidOperationException(string.Format(@"Error: Unhandled \fileCount parameter value {0}", filesCount));
            }
        }
    }

    public class HeaderCountFactory
    {
        private Random random = new Random();
        public void Create(FolderPath folderPath, HeaderCount headerCount)
        {
            switch (headerCount)
            {
                case HeaderCount.UnusedParameter:
                    break;
                case HeaderCount.None:
                    foreach (var f in new FilesFactory().GetFiles(folderPath))
                    {
                        FileIO.Write(f, string.Empty);
                    }
                    break;
                case HeaderCount.Single:
                    foreach (var f in new FilesFactory().GetFiles(folderPath))
                    {
                        var sb = new StringBuilder();
                        var strLength = random.Next() % 21;
                        for (var k = 0; k < strLength; k++)
                        {
                            var choice = random.Next() % 3;
                            switch (choice)
                            {
                                case 0: //lower alpha
                                    sb.Append(new CharacterFactory().LowerAlpha());
                                    break;
                                case 1: //upper alpha
                                    sb.Append(new CharacterFactory().UpperAlpha());
                                    break;
                                case 2: //numeric
                                    sb.Append(new CharacterFactory().Digit());
                                    break;
                                default:
                                    throw new InvalidOperationException(string.Format(@"Error: Unhandled choice {0}", choice));
                            }

                            FileIO.Write(f, sb.ToString());
                        }
                    }
                    break;
                default:
                    throw new InvalidOperationException(string.Format(@"Error: Unhandled \fieldCount parameter value {0}", headerCount));
            }
        }
    }

    public class CharacterFactory : ParameterValueFactory
    {
        private Random random = new Random();

        public int Digit()
        {
            return random.Next(0, 10);
        }

        public char UpperAlpha()
        {
            int start = 'A', end = 'Z';
            char ch = (char)random.Next(start, end + 1);
            return ch;
        }

        public char LowerAlpha()
        {
            int start = 'a', end = 'z';
            char ch = (char)random.Next(start, end + 1);
            return ch;
        }
    }

    public class DataRowCountFactory : ParameterValueFactory
    {
        public void Create(DataRowCount dataRowCount)
        {
            switch (dataRowCount)
            {
                case DataRowCount.UnusedParameter:
                    break;
                case DataRowCount.None:
                    break;
                default:
                    throw new InvalidOperationException(string.Format(@"Error: Unhandled \dataRowCount parameter value {0}", dataRowCount));
            }
        }
    }
}
