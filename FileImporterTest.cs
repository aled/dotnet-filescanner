using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace com.wibblr
{
    class FileImporterTest
    {
        public static void Main(string[] args)
        {
            var importer = new FileImporter(new InMemoryDatabase());

            importer.ImportChangedFiles(@"z:", fi => Console.WriteLine(fi.FullName));

            // Change a file here...
            var a = 0;

            importer.ImportChangedFiles(@"z:", fi => Console.WriteLine(fi.FullName));
        }

        public class InMemoryDatabase : FileInfoDatabase
        {
            // Dictionary of Directory => (File => LastModified)
            private Dictionary<string, Dictionary<string, DateTime>> lastModifiedData = new Dictionary<string, Dictionary<string, DateTime>>();

            // Dictionary of Directory => (File => LastImported)
            private Dictionary<string, Dictionary<string, DateTime>> lastImportedData = new Dictionary<string, Dictionary<string, DateTime>>();

            public void SetDirectoryContents(string directory, IEnumerable<FileSystemInfo> entries)
            {
                if (!lastModifiedData.ContainsKey(directory))
                    lastModifiedData[directory] = new Dictionary<string, DateTime>();

                lastModifiedData[directory] = entries.Select(x => new { x.Name, x.LastWriteTimeUtc }).ToDictionary(x => x.Name, x => x.LastWriteTimeUtc);

                if (lastImportedData.ContainsKey(directory))
                    foreach (var filename in lastImportedData[directory].Keys)
                        if (!lastModifiedData[directory].ContainsKey(filename))
                            lastImportedData.Remove(filename);
            }

            public void SetImported(string directory, string file)
            {
                if (!lastImportedData.ContainsKey(directory))
                    lastImportedData[directory] = new Dictionary<string, DateTime>();

                lastImportedData[directory][file] = DateTime.UtcNow;
            }

            public IEnumerable<FileInfo> GetChangedFiles(string directory)
            {
                if (lastModifiedData.ContainsKey(directory))
                {
                    foreach (var file in lastModifiedData[directory].Keys)
                    {
                        var lastModified = lastModifiedData[directory][file];

                        if (lastImportedData.ContainsKey(directory) && lastImportedData[directory].ContainsKey(file))
                        {
                            var lastImported = lastImportedData[directory][file];
                            if (lastModified <= lastImported)
                                continue;
                        }

                        yield return new FileInfo(Path.GetFullPath(Path.Combine(directory, file)));
                    }
                }
            }
        }
    }
}
