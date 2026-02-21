using CommonLibrary.Extensions;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace CommonLibrary.Utils
{
    public static class CsvUtil
    {
        public static Encoding Enc { get; set; } = Encoding.GetEncoding("Shift_JIS");

        /// <summary>
        /// CSV書き込み
        /// </summary>
        /// <param name="filePath">出力ファイル名パス</param>
        /// <param name="records">出力レコード</param>
        /// <param name="isAppend">追加書き込みモード  false:上書き true:追加　省略時はfalse</param>
        /// <remarks>外側のListは行単位、内側のListは列単位を想定（可変列がある際に使用する）</remarks>
        public static void Write(string filePath, List<List<string>> records, bool isAppend = false)
        {
            using var sw = new StreamWriter(filePath, isAppend, Enc);

            records.ForEach(record =>
            {
                var wr = DoubleQuote(record).Aggregate((a, b) => a + "," + b);
                sw.WriteLine(wr);
            });
        }

        /// <summary>
        /// CSV書き込み
        /// </summary>
        /// <param name="filePath">出力ファイル名パス</param>
        /// <param name="records">出力レコード</param>
        /// <remarks>外側のListは行単位、内側のListは列単位を想定（可変列がある際に使用する）</remarks>
        public static byte[] Write(List<List<string>> records)
        {
            using (var stream = new MemoryStream())
            using (TextWriter sw = new StreamWriter(stream, Enc) { AutoFlush = true })
            {
                records.ForEach(record =>
                {
                    var wr = DoubleQuote(record).Aggregate((a, b) => a + "," + b);
                    sw.WriteLine(wr);
                });
                return stream.ToArray();
            }
        }

        public static byte[] Write(List<List<object>> records)
        {
            using (var stream = new MemoryStream())
            using (TextWriter sw = new StreamWriter(stream, Enc) { AutoFlush = true })
            {
                records.ForEach(record =>
                {
                    var wr = DoubleQuote(record).Aggregate((a, b) => a + "," + b);
                    sw.WriteLine(wr);
                });
                return stream.ToArray();
            }
        }

        public static void Write(string filePath, Action<CsvWriter> action)
            => Write(filePath, action, false, false);

        public static void WriteWithHeader(string filePath, Action<CsvWriter> action)
            => Write(filePath, action, true, false);

        public static void WriteWithQuote(string filePath, Action<CsvWriter> action)
            => Write(filePath, action, false, true);

        public static void WriteWithHeaderWithQuote(string filePath, Action<CsvWriter> action)
            => Write(filePath, action, true, true);

        /// <summary>
        /// CSV書き込み
        /// </summary>
        /// <param name="filePath">出力パス</param>
        /// <param name="action">アクション</param>
        /// <param name="hasHeaderRecord">ヘッダーレコード出力判定　True：出力する　False：出力しない</param>
        public static void Write(string filePath, Action<CsvWriter> action, bool hasHeaderRecord, bool hasQuote)
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                //ヘッダを出力しないように指定
                HasHeaderRecord = hasHeaderRecord,
                ShouldQuote = (content) => hasQuote,
            };
            using (TextWriter sw = new StreamWriter(filePath, false, Enc) { AutoFlush = true })
            using (var csv = new CsvWriter(sw, config))
            {
                action(csv);
            }
        }

        public static byte[] Write(Action<CsvWriter> action)
            => Write(action, false);

        public static byte[] WriteWithHeader(Action<CsvWriter> action)
            => Write(action, true);

        public static byte[] Write<TRecord, TCsvClassMap>(Action<CsvWriter> action) where TCsvClassMap : ClassMap<TRecord>
            => Write<TRecord, TCsvClassMap>(action, false);

        public static byte[] WriteWithHeader<TRecord, TCsvClassMap>(Action<CsvWriter> action) where TCsvClassMap : ClassMap<TRecord>
            => Write<TRecord, TCsvClassMap>(action, true);

        private static byte[] Write(Action<CsvWriter> action, bool hasHeaderRecord)
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                //ヘッダを出力しないように指定
                HasHeaderRecord = hasHeaderRecord,
            };
            using (var stream = new MemoryStream())
            using (TextWriter sw = new StreamWriter(stream, Enc) { AutoFlush = true })
            using (var csv = new CsvWriter(sw, config))
            {
                action(csv);
                return stream.ToArray();
            }
        }

        private static byte[] Write<TRecord, TCsvClassMap>(Action<CsvWriter> action, bool hasHeaderRecord) where TCsvClassMap : ClassMap<TRecord>
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                //ヘッダを出力しないように指定
                HasHeaderRecord = hasHeaderRecord,
            };
            using (var stream = new MemoryStream())
            using (TextWriter sw = new StreamWriter(stream, Enc) { AutoFlush = true })
            using (var csv = new CsvWriter(sw, config))
            {
                csv.Context.RegisterClassMap<TCsvClassMap>();
                action(csv);
                return stream.ToArray();
            }
        }

        public static IEnumerable<string> DoubleQuote(IEnumerable<string> items)
            => items.Select(i => string.IsNullOrWhiteSpace(i) ? string.Empty : i.Replace("\"", "\"\""))
                .Select(i => string.IsNullOrWhiteSpace(i) ? string.Empty : (i.Contains(",") || i.Contains("\n") ? "\"" + i + "\"" : i));

        public static IEnumerable<string> DoubleQuote(IEnumerable<object> items)
        {
            return items.Select(i => (i?.ToString() ?? string.Empty).Replace("\"", "\"\""))
           .Select(i => string.IsNullOrWhiteSpace(i) ? string.Empty : (i.Contains(",") || i.Contains("\n") ? "\"" + i + "\"" : i));
        }

        /// <summary>
        /// CSVの読み込み
        /// </summary>
        /// <typeparam name="TRecord">CSV定義クラス</typeparam>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="hasHeaderRecord">ヘッダがあるか</param>
        /// <returns></returns>
        public static (List<TRecord>, List<string>) Read<TRecord>(string filePath, bool hasHeaderRecord, MissingFieldFound? missingFieldFound = null)
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                //ヘッダを出力しないように指定
                HasHeaderRecord = hasHeaderRecord,
            };
            if (missingFieldFound != null)
            {
                config.MissingFieldFound = missingFieldFound;
            }
            return Read<TRecord>(new StreamReader(filePath, Enc), config);
        }

        /// <summary>
        /// CSVの読み込み
        /// </summary>
        /// <typeparam name="TRecord">CSV定義クラス</typeparam>
        /// <typeparam name="TCsvClassMap">CSVMappingクラス</typeparam>
        /// <param name="filePath">ファイルパス</param>
        /// <param name="hasHeaderRecord">ヘッダがあるか</param>
        /// <returns></returns>
        public static (List<TRecord>, List<string>) Read<TRecord, TCsvClassMap>(string filePath, bool hasHeaderRecord, MissingFieldFound? missingFieldFound = null) where TCsvClassMap : ClassMap<TRecord>
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                //ヘッダを出力しないように指定
                HasHeaderRecord = hasHeaderRecord,
            };
            if (missingFieldFound != null)
            {
                config.MissingFieldFound = missingFieldFound;
            }
            return Read<TRecord, TCsvClassMap>(new StreamReader(filePath, Enc), config);
        }

        /// <summary>
        /// CSVの読み込み
        /// </summary>
        /// <typeparam name="TRecord">CSV定義クラス</typeparam>
        /// <param name="file">ファイル</param>
        /// <param name="hasHeaderRecord">ヘッダがあるか</param>
        /// <returns></returns>
        public static (List<TRecord>, List<string>) Read<TRecord>(IFormFile file, bool hasHeaderRecord, MissingFieldFound? missingFieldFound = null)
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                //ヘッダを出力しないように指定
                HasHeaderRecord = hasHeaderRecord,
            };
            if (missingFieldFound != null)
            {
                config.MissingFieldFound = missingFieldFound;
            }
            return Read<TRecord>(new StreamReader(file.OpenReadStream(), Enc), config);
        }

        /// <summary>
        /// CSVの読み込み
        /// </summary>
        /// <typeparam name="TRecord">CSV定義クラス</typeparam>
        /// <typeparam name="TCsvClassMap">CSVMappingクラス</typeparam>
        /// <param name="file">ファイル</param>
        /// <param name="hasHeaderRecord">ヘッダがあるか</param>
        /// <returns></returns>
        public static (List<TRecord>, List<string>) Read<TRecord, TCsvClassMap>(IFormFile file, bool hasHeaderRecord, MissingFieldFound? missingFieldFound = null) where TCsvClassMap : ClassMap<TRecord>
        {
            var config = new CsvConfiguration(CultureInfo.CurrentCulture)
            {
                //ヘッダを出力しないように指定
                HasHeaderRecord = hasHeaderRecord,
            };
            if (missingFieldFound != null)
            {
                config.MissingFieldFound = missingFieldFound;
            }
            return Read<TRecord, TCsvClassMap>(new StreamReader(file.OpenReadStream(), Enc), config);
        }

        /// <summary>
        /// CSVの読み込み
        /// </summary>
        /// <returns></returns>
        public static (List<TRecord>, List<string>) Read<TRecord>(StreamReader sr, CsvConfiguration config)
        {
            var errors = new List<string>();
            var records = new List<TRecord>();
            try
            {
                using (var csv = new CsvReader(sr, config))
                {
                    records = csv.GetRecords<TRecord>().ToList();
                };
            }
            catch (BadDataException ex)
            {
                errors.Add($"{ex.Context?.Parser?.Row}行目のデータが不正です。");
            }
            catch (CsvHelper.MissingFieldException ex)
            {
                errors.Add($"{ex.Context?.Parser?.Row}行目の列数が不足しています。");
            }
            return (records, errors);
        }

        /// <summary>
        /// CSVの読み込み
        /// </summary>
        /// <returns></returns>
        public static (List<TRecord>, List<string>) Read<TRecord, TCsvClassMap>(StreamReader sr, CsvConfiguration config) where TCsvClassMap : ClassMap<TRecord>
        {
            var errors = new List<string>();
            var records = new List<TRecord>();
            try
            {
                using (var csv = new CsvReader(sr, config))
                {
                    csv.Context.RegisterClassMap<TCsvClassMap>();
                    records = csv.GetRecords<TRecord>().ToList();

                    // ヘッダー列数超過は Exception 発生しないため、ここでチェックする。
                    if (csv.HeaderRecord?.Length > typeof(TRecord).GetProperties().Length)
                    {
                        errors.Add("ヘッダーの列数が超過しています。");
                    }

                    // ヘッダー列の順序チェック
                    var headers = csv.Context.Reader?.HeaderRecord;
                    var indexNameDct = GetCSVPropIndexNameDct<TRecord>();
                    var diffOrderIndexes = headers?.ZipWithIndex()
                        .Where(x => x.Item1 != indexNameDct.Get(x.Item2).IfNone(() => string.Empty))
                        .Select(x => x.Item2 + 1).ToList();

                    if (diffOrderIndexes.NotEmpty())
                    {
                        errors.Add($"ヘッダー列{diffOrderIndexes?.Join()}の順序が不正です。");
                    }
                };
            }
            catch (BadDataException ex)
            {
                errors.Add($"{ex.Context?.Parser?.Row}行目のデータが不正です。");
            }
            catch (TypeConverterException e)
            {
                errors.Add($"{e.Context?.Parser?.Row}行目のデータの型が不正です。");
            }
            catch (CsvHelper.MissingFieldException ex)
            {
                errors.Add($"{ex.Context?.Parser?.Row}行目の列数が不足しています。");
            }
            catch (HeaderValidationException)
            {
                errors.Add($"タイトル行が正しくありません。");
            }
            return (records, errors);
        }

        /// <summary>
        /// CSVHelper のカラムインデックス取得
        /// </summary>
        /// <typeparam name="T">CSVクラス</typeparam>
        /// <param name="propName">カラム名</param>
        /// <param name="diffIndex">インデックスと実際の列数の差分</param>
        public static int GetCSVPropIndex<T>(string propName, int diffIndex = 1)
        {
            var propInfo = typeof(T).GetProperty(propName);

            if (propInfo == null)
            {
                return 0 + diffIndex;
            }

            var indexAttribute = Attribute.GetCustomAttribute(propInfo, typeof(IndexAttribute)) as IndexAttribute;
            return (indexAttribute?.Index ?? 0) + diffIndex;
        }

        /// <summary>
        /// CSVHelper のカラムインデックス一覧取得
        /// </summary>
        /// <typeparam name="T">CSVクラス</typeparam>
        /// <param name="diffIndex">インデックスと実際の列数の差分</param>
        public static Dictionary<string, int> GetCSVPropIndexDct<T>(int diffIndex = 1)
            => typeof(T).GetProperties()
                .ToDictionary(
                    x => x.Name,
                    x => GetCSVPropIndex<T>(x.Name, diffIndex)
                );

        /// <summary>
        /// CSVHelper のカラム名取得
        /// </summary>
        /// <typeparam name="T">CSVクラス</typeparam>
        /// <param name="propName">カラム名</param>
        public static string GetCSVPropName<T>(string propName)
        {
            var propInfo = typeof(T).GetProperty(propName);
            if (propInfo == null)
            {
                return string.Empty;
            }

            var nameAttribute = Attribute.GetCustomAttribute(propInfo, typeof(NameAttribute)) as NameAttribute;
            return nameAttribute?.Names.FirstOption()
                .IfNone(() => string.Empty) ?? string.Empty;
        }

        /// <summary>
        /// CSVHelper のカラム名一覧取得
        /// </summary>
        /// <typeparam name="T">CSVクラス</typeparam>
        public static Dictionary<string, string> GetCSVPropNameDct<T>()
            => typeof(T).GetProperties()
                .ToDictionary(
                    x => x.Name,
                    x => GetCSVPropName<T>(x.Name)
                );

        /// <summary>
        /// CSVHelper の列番からカラム名一覧取得
        /// </summary>
        /// <typeparam name="T">CSVクラス</typeparam>
        public static Dictionary<int, string> GetCSVPropIndexNameDct<T>()
        {
            var indexDct = GetCSVPropIndexDct<T>(0);
            var nameDct = GetCSVPropNameDct<T>();
            return indexDct
                .Select(index => new
                {
                    Index = index.Value,
                    Name = nameDct.Get(index.Key).IfNone(() => string.Empty),
                })
                .OrderBy(x => x.Index)
                .ToDictionary(x => x.Index, x => x.Name);
        }
    }
}
