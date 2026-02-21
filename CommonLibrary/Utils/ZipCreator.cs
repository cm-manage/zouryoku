using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace CommonLibrary.Utils
{
    public class ZipCreator
    {
        public static byte[] Create(Action<ZipCreator> create)
        {
            using var ms = new MemoryStream();
            using (var ac = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                var c = new ZipCreator(ac);

                create(c);
            }
            // ToArray前にZipArchiveを閉じないと、破損したzipファイルになる
            return ms.ToArray();
        }
        
        private ZipArchive archive;

        private ZipCreator(ZipArchive archive) 
        {
            this.archive = archive;
        }

        /// <summary>
        /// zipにEntryを追加します(fileNameに/を含む場合は、_に置換されます)
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="data">ファイルデータ</param>
        public void AddEntry(string fileName, byte[] data)
        {
            AddEntryWithFolder(fileName.Replace('/', '_'), data);
        }

        /// <summary>
        /// zipにEntryを追加します(fileNameに/がある場合は、それ以前をフォルダとして扱います)
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="data">ファイルデータ</param>
        public void AddEntryWithFolder(string fileName, byte[] data)
        {
            using var entryStream = archive.CreateEntry(fileName).Open();
            entryStream.Write(data, 0, data.Length);
        }
    }
}
