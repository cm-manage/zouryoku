using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace CommonLibrary.Utils
{
    public static class FileUtil
    {
        public const string ExcelContextType = "application/msexcel";
        public const string XlsContextType = "application/vnd.ms-excel";
        public const string XlsxContextType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string PdfContextType = "application/pdf";
        public const string WordContextType = "application/msword";
        public const string PptContextType = "application/mspowerpoint";
        public const string ExeContextType = "application/octet-stream";
        public const string TextContextType = "text/plain";
        public const string XmlContextType = "text/xml";
        public const string CsvContextType = "text/csv";
        public const string TsvContextType = "text/tab-separated-values";
        public const string HtmlContextType = "text/html";
        public const string JpegContextType = "image/jpeg";
        public const string BmpContextType = "image/bmp";
        public const string GifContextType = "image/gif";
        public const string PngContextType = "image/png";
        public const string ZipContextType = "application/zip";
        public const string LzhContextType = "application/lha";
        public const string CssContextType = "text/css";
        public const string JsContextType = "text/javascript";
        public const string VbsContextType = "text/vbscript";

        /// <summary>
        /// アップロードするファイル名を生成する
        /// IEの場合だと、フルパスで格納されるので、
        /// ディレクトリまでのインデックスを取得し切り取ってアップロードのファイル名とする
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string CreateUploadFileName(IFormFile file)
        {
            // \マークまでのインデックスを取得
            var fileNameIdx = file.FileName.LastIndexOf("\\") + 1;
            if (fileNameIdx > -1)
            {
                return file.FileName.Substring(fileNameIdx);
            }
            else
            {
                return file.FileName;
            }
        }

        /// <summary>
        /// ファイルのサイズを読みやすい形に成型する
        /// </summary>
        /// <param name="fileSize"></param>
        /// <returns></returns>
        public static string AjustFileSizeName(long fileSize)
        {
            var fs = (decimal)fileSize;
            if (fs < 1024)
            {
                return string.Format("{0:n2}{1}", fs, "B");
            }
            else if ((fs / 1024) < 1024)
            {
                return string.Format("{0:n2}{1}", (fs / 1024), "KB");
            }
            else
            {
                return string.Format("{0:n2}{1}", (fs / (1024 * 1024)), "MB");
            }
        }

        public static byte[] FileToByte(string filePath)
        {
            byte[] data;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                data = FileToByte(fs);
            }
            return data;
        }

        public static byte[] FileToByte(Stream stream)
        {
            byte[] data;
            using (var br = new BinaryReader(stream))
            {
                var fileSize = stream.Length;
                data = new byte[fileSize];
                for (int i = 0; i < fileSize; i++)
                {
                    data[i] += br.ReadByte();
                }
            }
            return data;
        }

        public static byte[] FileToByte(IFormFile file)
        {
            byte[] data;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                data = ms.ToArray();
            }
            return data;
        }

        public static string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            var contentType = string.Empty;
            switch (ext)
            {
                case ".txt":
                    contentType = TextContextType;
                    break;
                case ".htm":
                case ".html":
                    contentType = HtmlContextType;
                    break;
                case ".csv":
                    contentType = CsvContextType;
                    break;
                case ".tsv":
                    contentType = TsvContextType;
                    break;
                case ".xml":
                    contentType = XmlContextType;
                    break;
                case ".doc":
                case ".docx":
                    contentType = WordContextType;
                    break;
                case ".xls":
                    contentType = XlsContextType;
                    break;
                case ".xlsx":
                    contentType = XlsxContextType;
                    break;
                case ".ppt":
                case ".pptx":
                    contentType = PptContextType;
                    break;
                case ".pdf":
                    contentType = PdfContextType;
                    break;
                case ".bmp":
                    contentType = BmpContextType;
                    break;
                case ".gif":
                    contentType = GifContextType;
                    break;
                case ".jpg":
                    contentType = JpegContextType;
                    break;
                case ".png":
                    contentType = PngContextType;
                    break;
                case ".zip":
                    contentType = ZipContextType;
                    break;
                case ".lzh":
                    contentType = LzhContextType;
                    break;
                case ".css":
                    contentType = CssContextType;
                    break;
                case ".js":
                    contentType = JsContextType;
                    break;
                case ".vbs":
                    contentType = VbsContextType;
                    break;
                case ".exe":
                    contentType = ExeContextType;
                    break;
            }
            return contentType;
        }

        public static string GetExtension(string contentType)
        {
            var ext = string.Empty;
            switch (contentType)
            {
                case TextContextType:
                    ext = ".txt";
                    break;
                case HtmlContextType:
                    ext = ".html";
                    break;
                case CsvContextType:
                    ext = ".csv";
                    break;
                case TsvContextType:
                    ext = ".tsv";
                    break;
                case XmlContextType:
                    ext = ".xml";
                    break;
                case WordContextType:
                    ext = ".docx";
                    break;
                case XlsContextType:
                    ext = ".xls";
                    break;
                case XlsxContextType:
                    ext = ".xlsx";
                    break;
                case PptContextType:
                    ext = ".pptx";
                    break;
                case PdfContextType:
                    ext = ".pdf";
                    break;
                case BmpContextType:
                    ext = ".bmp";
                    break;
                case GifContextType:
                    ext = ".gif";
                    break;
                case JpegContextType:
                    ext = ".jpg";
                    break;
                case PngContextType:
                    ext = ".png";
                    break;
                case ZipContextType:
                    ext = ".zip";
                    break;
                case LzhContextType:
                    ext = ".lzh";
                    break;
                case CssContextType:
                    ext = ".css";
                    break;
                case JsContextType:
                    contentType = ".js";
                    break;
                case VbsContextType:
                    ext = ".vbs";
                    break;
                case ExeContextType:
                    ext = ".exe";
                    break;
            }
            return ext;
        }

        /// <summary>
        /// ファイルに拡張子を付けて返す
        /// </summary>
        /// <param name="baseFileName">拡張子を取得する基礎となるファイル</param>
        /// <param name="targetFileName">対象のファイル</param>
        /// <returns></returns>
        public static string GetFileNameExt(string baseFileName, string targetFileName)
        {
            var name = targetFileName;
            var baseExt = Path.GetExtension(baseFileName);
            var ext = Path.GetExtension(targetFileName);
            if (ext != baseExt)
            {
                var length = baseExt.Length + name.Length;
                if (length > 100)
                {
                    name = name.Substring(0, name.Length - (length - 100));
                }
                name += baseExt;
            }
            return name;
        }

        /// <summary>
        /// ファイル名の使用禁止文字を[_]に置換
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <param name="replaceChar">置換文字</param>
        /// <returns></returns>
        public static string EncodeInvalidChar(string fileName, char replaceChar = '_')
        {
            var replaceChars = Path.GetInvalidFileNameChars().ToList();
            replaceChars.ForEach(x => fileName = fileName.Replace(x, replaceChar));
            return fileName;
        }

        /// <summary>
        /// フォルダが存在していない場合、作成
        /// </summary>
        public static void NotExistsToCreateDirectory(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }
    }
}
