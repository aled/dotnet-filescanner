using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace com.wibblr
{   
    public interface FileInfoDatabase
    {
        void SetDirectoryContents(string directory, IEnumerable<FileSystemInfo> entries);
        void SetImported(string directory, string file);
        IEnumerable<FileInfo> GetChangedFiles(string directory);
    }

    public class FileImporter
    {
        private FileInfoDatabase database;

        public FileImporter(FileInfoDatabase d)
        {
            database = d;
        }

        public void ImportChangedFiles(string rootDirectory, Action<FileInfo> Import)
        {
            foreach (var contents in new FileScanner().ScanDirectory(rootDirectory))
            {
                var directory = contents.Directory;
                database.SetDirectoryContents(directory.FullName, contents.Entries);

                foreach (var file in database.GetChangedFiles(directory.FullName))
                {
                    Import(file);
                    database.SetImported(directory.FullName, file.Name);
                }
            }
        }
    }
}
