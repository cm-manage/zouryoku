using CommonLibrary.Utils;
using CsvHelper.Configuration;

namespace Zouryoku.Models
{
    public record FileModel(byte[] File, string FileName, string ContentType)
    {
        /// <summary>
        /// Pdfmodel取得（ファイル名に.pdfを付与）
        /// </summary>
        public static FileModel GetPdfModel(byte[] data, string fileName)
            => new FileModel
            (
                File: data,
                FileName: $"{fileName}.pdf",
                ContentType: FileUtil.PdfContextType
            );

        /// <summary>
        /// Excelmodel取得（ファイル名に.xlsxを付与）
        /// </summary>
        public static FileModel GetExcelModel(byte[] records, string fileName)
            => new FileModel
            (
                File: records,
                FileName: $"{fileName}.xlsx",
                ContentType: FileUtil.ExcelContextType
            );
        /// <summary>
        /// CSVmodel取得（ファイル名に.csvを付与）
        /// </summary>
        public static FileModel GetCsvModel<A>(List<A> records, string fileName, bool withHeader = true)
            => new FileModel
            (
                File: withHeader ? CsvUtil.WriteWithHeader(x => x.WriteRecords(records)) : CsvUtil.Write(x => x.WriteRecords(records)),
                FileName: $"{fileName}.csv",
                ContentType: FileUtil.CsvContextType
            );

        /// <summary>
        /// CSVmodel取得（ファイル名に.csvを付与）
        /// </summary>
        public static FileModel GetCsvModel<TRecord, TCsvClassMap>(List<TRecord> records, string fileName, bool withHeader = true) where TCsvClassMap : ClassMap<TRecord>
            => new FileModel
            (
                File: withHeader ? CsvUtil.WriteWithHeader<TRecord, TCsvClassMap>(x => x.WriteRecords(records)) : CsvUtil.Write<TRecord, TCsvClassMap>(x => x.WriteRecords(records)),
                FileName: $"{fileName}.csv",
                ContentType: FileUtil.CsvContextType
            );
    }

    public class FileBindModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Size { get; set; }
        public IFormFile Data { get; set; } = null!;
    }

}
