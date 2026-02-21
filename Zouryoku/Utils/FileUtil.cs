namespace Zouryoku.Utils
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

    }
}
