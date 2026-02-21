using System;
using System.IO;
using System.Text;
using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Http;
using NPOI.XSSF.UserModel;

namespace CommonLibrary.Utils
{
    public static class ExcelUtil
    {
        public static Encoding Enc = Encoding.GetEncoding("Shift_JIS");

        public static byte[] Write(Action<XSSFWorkbook> action, string? tmplateFileName = null)
        {
            // エクセルファイルをオープン
            var book = new XSSFWorkbook();
            FileStream? infile = null;
            if (!string.IsNullOrEmpty(tmplateFileName))
            {
                // 他プロセスがエクセルを開いていても例外とならないようにFileShare.ReadWriteを設定
                infile = new FileStream(tmplateFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                // エクセルファイルを開いて内容を取得（ワークブックオブジェクトを作成）
                book = new XSSFWorkbook(infile);
            }

            action?.Invoke(book);

            if (!string.IsNullOrEmpty(tmplateFileName))
            {
                infile?.Dispose();
            }
            return book.GetBytes();
        }

        public static void Read(IFormFile file, Action<XSSFWorkbook> action)
        {
            using (var book = new XSSFWorkbook(file.OpenReadStream()))
            {
                action?.Invoke(book);
            }
        }

        public static T Read<T>(IFormFile file, Func<XSSFWorkbook, T> func)
        {
            using (var book = new XSSFWorkbook(file.OpenReadStream()))
            {
                var result = func.Invoke(book);
                return result;
            }
        }
    }
}
