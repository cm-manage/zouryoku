using LanguageExt;
using LanguageExt.TypeClasses;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.IO;
using System.Linq;
using static LanguageExt.Prelude;

namespace CommonLibrary.Extensions
{
    public static class NPOIExpand
    {
        public static void SetStyle(this IRow row, int colIndex, ICellStyle style)
        {
            row.GetOrCreateCell(colIndex).CellStyle = style;
        }

        //書式変更
        public static void SetStyle(this ISheet sheet, int columnIndex, int rowIndex, ICellStyle style)
        {
            sheet.GetCell(columnIndex, rowIndex).CellStyle = style;
        }

        /// <summary>
        /// 書式設定
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="columnStart">列開始インデックス</param>
        /// <param name="columnEndExclude">列終了インデックス(自身は含まない)</param>
        /// <param name="rowStart">行開始インデックス</param>
        /// <param name="rowEndExclude">行終了インデックス(自身は含まない)</param>
        /// <param name="style">設定するスタイル</param>
        public static void SetStyle(this ISheet sheet, int columnStart, int columnEndExclude, int rowStart, int rowEndExclude, ICellStyle style)
        {
            Enumerable.Range(columnStart, columnEndExclude - columnStart).ForEach(colIndex =>
            {
                Enumerable.Range(rowStart, rowEndExclude - rowStart).ForEach(rowIndex => sheet.SetStyle(colIndex, rowIndex, style));
                // 幅調整
                sheet.AutoSizeColumn(colIndex);
            });
        }

        /// <summary>
        /// セルに値とスタイルをセット
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="colIndex">列インデクス</param>
        /// <param name="value">値(string)</param>
        /// <param name="style">設定するスタイル</param>
        public static void SetValue(this IRow row, int colIndex, string value, ICellStyle? style = null)
        {
            var cell = row.GetOrCreateCell(colIndex);
            cell.SetCellValue(value);
            if (style != null)
            {
                cell.CellStyle = style;
            }
        }

        /// <summary>
        /// セルに値とスタイルをセット
        /// </summary>
        /// <param name="row">行</param>
        /// <param name="colIndex">列インデクス</param>
        /// <param name="value">値(double)</param>
        /// <param name="style">設定するスタイル</param>
        public static void SetValue(this ISheet sheet, int colIndex, int rowIndex, double value, ICellStyle? style = null)
        {
            var cell = sheet.GetCell(colIndex, rowIndex);
            cell.SetCellValue(value);
            if (style != null)
            {
                cell.CellStyle = style;
            }
        }

        public static ICell GetOrCreateCell(this IRow row, int colIndex)
             => row.GetCell(colIndex) ?? row.CreateCell(colIndex);

        public static void SetBorder(this ICellStyle style, BorderStyle borderStyle, short borderColor)
        {
            style.BorderTop = borderStyle;
            style.BorderRight = borderStyle;
            style.BorderBottom = borderStyle;
            style.BorderLeft = borderStyle;
            style.TopBorderColor = borderColor;
            style.RightBorderColor = borderColor;
            style.BottomBorderColor = borderColor;
            style.LeftBorderColor = borderColor;
        }

        /// <summary>
        /// ヘッダラベル用の背景色、文字色、罫線が設定されたSytleを返します。
        /// </summary>
        /// <param name="book"></param>
        /// <returns></returns>
        public static ICellStyle GetHeaderLabelStyle(this IWorkbook book)
        {
            var style = book.CreateCellStyle();
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            style.FillForegroundColor = IndexedColors.RoyalBlue.Index;
            style.FillPattern = FillPattern.SolidForeground;

            var font = book.CreateFont();
            font.Color = IndexedColors.White.Index;
            //フォントサイズを設定（しないとExcelがつぶれる）
            font.FontHeightInPoints = style.GetFont(book).FontHeightInPoints;
            style.SetFont(font);
            style.SetBorder(BorderStyle.Thin, IndexedColors.Grey50Percent.Index);

            return style;
        }

        /// <summary>
        /// 指定セルにヘッダラベル用Styleを設定します。
        /// </summary>
        public static void SetHeaderStyle(this ISheet sheet, int rowIndex, int startColIndex, int columnCount, bool WrapText = false)
        {
            var style = sheet.Workbook.GetHeaderLabelStyle();
            style.WrapText = WrapText;
            var row = sheet.GetRow(rowIndex);
            Range(startColIndex, columnCount).ForEach(i => row.SetStyle(i, style));
        }

        public static Option<ICell> Cell(this ISheet sheet, int columnIndex, int rowIndex)
        {
            return sheet.Row(rowIndex).Map(row => row.GetCell(columnIndex));
        }

        public static Option<IRow> Row(this ISheet sheet, int rowIndex)
        {
            return Optional(sheet.GetRow(rowIndex));
        }

        public static ICell GetCell(this ISheet sheet, int columnIndex, int rowIndex)
        {
            var row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            return row.GetCell(columnIndex) ?? row.CreateCell(columnIndex);
        }

        public static ICellStyle GetColorCellStyle(this ISheet sheet, XSSFWorkbook book, int columnIndex, int rowIndex, IndexedColors backGroundColor, IndexedColors? forGroundColor = null, HorizontalAlignment horizontalAlignment = HorizontalAlignment.General)
        {
            var cellStyle = book.CreateCellStyle();
            var cloumnstyle = sheet.GetCell(columnIndex, rowIndex).CellStyle;
            cellStyle.CloneStyleFrom(cloumnstyle);
            cellStyle.FillForegroundColor = backGroundColor.Index;
            cellStyle.FillPattern = FillPattern.SolidForeground;
            cellStyle.Alignment = horizontalAlignment;
            if (forGroundColor != null)
            {
                var font = book.CreateFont();
                var cellFont = cellStyle.GetFont(book);
                font.Color = forGroundColor.Index;
                //フォントサイズを設定（しないとExcelがつぶれる）
                font.FontHeightInPoints = cellFont.FontHeightInPoints;
                cellStyle.SetFont(font);
            }
            return cellStyle;
        }

        public static string GetValue(this IRow row, int columnIndex)
        {
            var cell = row.GetCell(columnIndex);
            if (cell == null)
            {
                return string.Empty;
            }
            return cell.GetValue();
        }

        public static int? GetInt(this IRow row, int columnIndex)
        {
            int? number = null;
            var intStr = row.GetValue(columnIndex);
            if (!string.IsNullOrWhiteSpace(intStr) && int.TryParse(intStr, out var intNumber))
            {
                number = intNumber;
            }
            return number;
        }

        public static decimal? GetDecimal(this IRow row, int columnIndex)
        {
            decimal? number = null;
            var decimalStr = row.GetValue(columnIndex);
            if (!string.IsNullOrWhiteSpace(decimalStr) && decimal.TryParse(decimalStr, out var decNumber))
            {
                number = decNumber;
            }
            return number;
        }
        
        public static DateTime? GetDateTime(this IRow row, int columnIndex)
        {
            DateTime? date = null;
            var dateStr = row.GetValue(columnIndex);
            if (!string.IsNullOrWhiteSpace(dateStr) && DateTime.TryParse(dateStr, out var datetime))
            {
                date = datetime;
            }
            return date;
        }

        public static TimeOnly? GetTimeOnly(this IRow row, int columnIndex)
        {
            TimeOnly? time = null;
            var timeStr = row.GetValue(columnIndex);
            if (!string.IsNullOrWhiteSpace(timeStr) && DateTime.TryParse(timeStr, out var datetime))
            {
                time = TimeOnly.FromDateTime(datetime);
            }
            return time;
        }

        public static double? GetDouble(this IRow row, int columnIndex)
        {
            double? number = null;
            var doubleStr = row.GetValue(columnIndex);
            if (!string.IsNullOrWhiteSpace(doubleStr) && double.TryParse(doubleStr, out var doublenumber))
            {
                number = doublenumber;
            }
            return number;
        }

        public static string GetCellValue(this ISheet sheet, int columnIndex, int rowIndex)
        {
            var cell = sheet.GetCell(columnIndex, rowIndex);
            return cell.GetValue();
        }

        private static string GetValue(this ICell cell)
        {
            switch (cell.CellType)
            {
                // 文字列型
                case CellType.String:
                    return cell.StringCellValue;
                // 数値型（日付の場合もここに入る）
                case CellType.Numeric:
                    // 日付型
                    // 本来はスタイルに合わせてフォーマットすべきだが、
                    // うまく表示できないケースが若干見られたので固定のフォーマットとして取得
                    return DateUtil.IsCellDateFormatted(cell) ? cell.DateCellValue?.ToString("yyyy/MM/dd HH:mm:ss")! : cell.NumericCellValue.ToString();
                // bool型(文字列でTrueとか入れておけばbool型として扱われた)
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                // 入力なし
                case CellType.Blank:
                    return cell.ToString()!;
                // 数式
                case CellType.Formula:
                    // 下記で数式の文字列が取得される
                    //cellStr = cell.CellFormula.ToString();
                    // 数式の元となったセルの型を取得して同様の処理を行う
                    // コメントは省略
                    switch (cell.CachedFormulaResultType)
                    {
                        case CellType.String:
                            return cell.StringCellValue;
                        case CellType.Numeric:
                            return DateUtil.IsCellDateFormatted(cell) ? cell.DateCellValue?.ToString("yyyy/MM/dd HH:mm:ss")! : cell.NumericCellValue.ToString();
                        case CellType.Boolean:
                            return cell.BooleanCellValue.ToString();
                        case CellType.Blank:
                            return string.Empty;
                        case CellType.Error:
                            return cell.ErrorCellValue.ToString();
                        case CellType.Unknown:
                            return string.Empty;
                        default:
                            return string.Empty;
                    }
                // エラー
                case CellType.Error:
                    return cell.ErrorCellValue.ToString();
                case CellType.Unknown:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        //セル設定(文字列用)
        public static void WriteCell(this ISheet sheet, int columnIndex, int rowIndex, string value)
        {
            var cell = sheet.GetCell(columnIndex, rowIndex);
            cell.SetCellValue(value);
        }

        //セル設定(数値用)
        public static void WriteCell(this ISheet sheet, int columnIndex, int rowIndex, double? value)
        {
            if (value == null)
            {
                return;
            }
            var cell = sheet.GetCell(columnIndex, rowIndex);
            cell.SetCellValue(value.Value);
        }

        //セル設定(数値用)
        public static void WriteCell(this ISheet sheet, int columnIndex, int rowIndex, decimal? value)
            => sheet.WriteCell(columnIndex, rowIndex, (double?)value);

        //セル設定(日付用)
        public static void WriteCell(this ISheet sheet, int columnIndex, int rowIndex, DateTime value)
        {
            var cell = sheet.GetCell(columnIndex, rowIndex);
            cell.SetCellValue(value);
        }

        //セル設定(日付用)
        public static void WriteCell(this ISheet sheet, int columnIndex, int rowIndex, DateTime? value)
        {
            var cell = sheet.GetCell(columnIndex, rowIndex);
            if (value.HasValue)
            {
                cell.SetCellValue(value.Value);
            }
        }

        //書式変更
        public static void WriteStyle(this ISheet sheet, int columnIndex, int rowIndex, ICellStyle style)
        {
            var cell = sheet.GetCell(columnIndex, rowIndex);
            cell.CellStyle = style;
        }

        //Excel関数用
        public static void WriteFormula(this ISheet sheet, int columnIndex, int rowIndex, string formula)
        {
            var cell = sheet.GetCell(columnIndex, rowIndex);
            cell.SetCellFormula(formula);
        }

        public static byte[] GetBytes(this IWorkbook book)
        {
            using var ms = new MemoryStream();

            book.Write(ms);
            var bytes = ms.ToArray();
            ms.Close();

            return bytes;
        }

        /// <summary>
        /// 行のコピー挿入
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="sourceRow">コピー先</param>
        /// <param name="targetRow">コピー元</param>
        public static void CopyAndInsertRow(this ISheet sheet, int sourceRow, int targetRow)
        {
            sheet.ShiftRows(sourceRow, sheet.LastRowNum, 1);
            sheet.CreateRow(sourceRow);
            sheet.CopyRow(targetRow, sourceRow);
            sheet.ShiftRows(sourceRow + 2, sheet.LastRowNum, -1);
        }

        /// <summary>
        /// 列のコピー挿入
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="sourceColumn">コピー先</param>
        /// <param name="targetColumn">コピー元</param>
        /// <param name="startRow">コピー範囲行_開始（指定なし：0行目）</param>
        /// <param name="endRow">コピー範囲行_終了（指定なし：最終行目）</param>
        public static void CopyAndInsertColumn(this ISheet sheet, int sourceColumn, int targetColumn, int? startRow = null, int? endRow = null)
        {
            if (!startRow.HasValue)
            {
                startRow = 0;
            }
            if (!endRow.HasValue)
            {
                endRow = sheet.LastRowNum;
            }
            // 全行数分回す
            Enumerable.Range(startRow.Value, endRow.Value - startRow.Value + 1).ForEach(rowIndex =>
            {
                // 開始列から後の列が対象
                //順にコピーしてズラすため後ろの列から行う
                for (var colNum = sheet.GetRow(rowIndex).LastCellNum; colNum >= targetColumn; colNum--)
                {
                    var celFrom = sheet.GetCell(colNum, rowIndex);
                    var celTo = sheet.GetCell(colNum + 1, rowIndex);

                    CopyCell(celFrom, celTo);
                }
                //最後に指定列のコピー
                CopyCell(sheet.GetCell(sourceColumn, rowIndex), sheet.GetCell(targetColumn, rowIndex));
            });
        }

        public static void CopyCell(ICell source, ICell destination)
        {
            if (destination != null && source != null)
            {
                //you can comment these out if you don't want to copy the style ...
                destination.CellComment = source.CellComment;
                destination.CellStyle = source.CellStyle;
                destination.Hyperlink = source.Hyperlink;

                switch (source.CellType)
                {
                    case CellType.Formula:
                        destination.CellFormula = source.CellFormula;
                        break;
                    case CellType.Numeric:
                        destination.SetCellValue(source.NumericCellValue);
                        break;
                    case CellType.String:
                        destination.SetCellValue(source.StringCellValue);
                        break;
                }
            }
        }

        /// <summary>
        /// 指定した箇所に罫線を描画
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param>
        /// <param name="borderType"></param>
        /// <param name="indexedColor"></param>
        /// <param name="borderStyle"></param>
        public static void WriteBorder(this ISheet sheet, int columnIndex, int rowIndex, BorderType borderType, IndexedColors? indexedColor = null, BorderStyle borderStyle = BorderStyle.Thin)
        {
            var colorIndex = indexedColor?.Index ?? IndexedColors.Black.Index;

            var cell = sheet.GetCell(columnIndex, rowIndex);
            var cellStyle = cell.CellStyle;

            var createCellStyle = sheet.Workbook.CreateCellStyle();
            createCellStyle.CloneStyleFrom(cellStyle);

            switch (borderType)
            {
                case BorderType.Top:
                    {
                        createCellStyle.BorderTop = borderStyle;
                        createCellStyle.TopBorderColor = colorIndex;
                        break;
                    }
                case BorderType.Bottom:
                    {
                        createCellStyle.BorderBottom= borderStyle;
                        createCellStyle.BottomBorderColor = colorIndex;
                        break;
                    }
                case BorderType.Left:
                    {
                        createCellStyle.BorderLeft = borderStyle;
                        createCellStyle.LeftBorderColor = colorIndex;
                        break;
                    }
                case BorderType.Right:
                    {
                        createCellStyle.BorderRight = borderStyle;
                        createCellStyle.RightBorderColor = colorIndex;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            cell.CellStyle = createCellStyle;
        }

        /// <summary>
        /// セルの削除
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="columnIndex"></param>
        /// <param name="rowIndex"></param>
        public static void RemoveCell(this ISheet sheet, int columnIndex, int rowIndex)
        {
            var row = sheet.GetRow(rowIndex);
            var cell = sheet.GetCell(columnIndex, rowIndex);
            row.RemoveCell(cell);
        }

        public static void Using(this IWorkbook book, Action<IWorkbook> action)
        {
            try
            {
                action(book);
            }
            finally
            {
                book.Close();
            }
        }

        public static A Using<A>(this IWorkbook book, Func<IWorkbook, A> func)
        {
            try
            {
                return func(book);
            }
            finally
            {
                book.Close();
            }
        }
    }

    public enum BorderType
    {
        Top,
        Bottom,
        Left,
        Right,
    }
}
