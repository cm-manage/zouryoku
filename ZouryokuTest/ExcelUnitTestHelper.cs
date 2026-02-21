using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace ZouryokuTest
{
    public static class ExcelTestHelper
    {
        public static IWorkbook CreateExcelFile(string sheetName, List<List<string>> rows)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet(sheetName);

            for (int i = 0; i < rows.Count; i++)
            {
                var row = sheet.CreateRow(i);
                for (int j = 0; j < rows[i].Count; j++)
                {
                    row.CreateCell(j).SetCellValue(rows[i][j]);
                }
            }

            return workbook;
        }

        public static IFormFile ConvertToFormFile(IWorkbook workbook, string fileName)
        {
            using var originalStream = new MemoryStream();
            workbook.Write(originalStream);

            // コピー用のストリームを作成
            var copiedStream = new MemoryStream(originalStream.ToArray());
            copiedStream.Position = 0;

            return new FormFile(copiedStream, 0, copiedStream.Length, "file", fileName);
        }


        private class TestFormFile : IFormFile
        {
            private readonly Stream _stream;
            private readonly string _fileName;

            public TestFormFile(Stream stream, string fileName)
            {
                _stream = stream;
                _fileName = fileName;
            }

            public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            public string ContentDisposition => $"inline; filename={_fileName}";
            public IHeaderDictionary Headers => new HeaderDictionary();
            public long Length => _stream.Length;
            public string Name => "file";
            public string FileName => _fileName;

            public void CopyTo(Stream target) => _stream.CopyTo(target);
            public Task CopyToAsync(Stream target, CancellationToken cancellationToken = default) => _stream.CopyToAsync(target, cancellationToken);
            public Stream OpenReadStream() => _stream;
        }
    }

}
