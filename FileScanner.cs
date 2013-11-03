using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace com.wibblr
{
    public class DirectoryContents
    {
        public DirectoryContents(DirectoryInfo directory, IEnumerable<FileSystemInfo> entries)
        {
            Directory = directory;
            Entries = entries;
        }

        public DirectoryInfo Directory { get; set; }
        public IEnumerable<FileSystemInfo> Entries { get; set; }
        
        IEnumerable<FileInfo> fileInfos {
            get { return Entries.OfType<FileInfo>(); }
        }

        IEnumerable<DirectoryInfo> directoryInfos
        {
            get { return Entries.OfType<DirectoryInfo>(); }
        }
    }

    public class FileScanner
    {
        public IEnumerable<DirectoryContents> ScanDirectory(string directory, 
                                                            Func<DirectoryInfo, Boolean> directoryFilter = null,
                                                            Func<FileInfo, Boolean> fileFilter = null)
        {
            if (directoryFilter == null) directoryFilter = x => { return true; };
            if (fileFilter == null) fileFilter = x => { return true; };

            return ScanDirectory(new DirectoryInfo(directory), directoryFilter, fileFilter);
        }

        private IEnumerable<DirectoryContents> ScanDirectory(DirectoryInfo searchDirectory,
                                   Func<DirectoryInfo, Boolean> directoryFilter,
                                   Func<FileInfo, Boolean> fileFilter)
        {
            var entries = searchDirectory.EnumerateFileSystemInfos();

            var files = entries.OfType<FileInfo>().Where(fileFilter);
            var directories = entries.OfType<DirectoryInfo>().Where(directoryFilter);

            yield return new DirectoryContents(searchDirectory, directories.Concat<FileSystemInfo>(files));

            foreach (var directory in directories) 
            {
                foreach (var directoryContents in ScanDirectory(directory, directoryFilter, fileFilter))
                {
                    yield return directoryContents;
                }
            }
        }
    }
}
