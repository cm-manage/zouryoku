using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using LanguageExt;
using static LanguageExt.Prelude;

namespace ZouryokuCommonLibrary.Utils
{
    public static class TimeCalculator
    {
        /// <summary>
        /// 出退勤時間を指示の有無により補正します。
        /// </summary>
        /// <remarks>
        /// 時間帯の分類：
        /// - 00:00 - 05:00 夜間
        /// - 05:00 - 08:30 早朝
        /// - 17:00 - 22:00 リフレッシュ
        /// - 22:00 - 24:00 深夜
        ///
        /// 補正ルール：
        /// - 早朝・リフレッシュの時間帯は指示がなければ補正で削除される。
        /// - 指示があれば打刻の時間に変更なし。
        /// - 夜間・深夜の時間帯は指示がなくても時間補正はしない。
        ///   画面に指示の入力を促すメッセージを表示する。
        /// </remarks>
        public static (string, string) Hosei(
            string syukkin, string taikin,
            bool yakan, bool shinya, bool soutyou, bool refresh,
            bool isRefreshDay)
            => Hosei(syukkin, taikin, yakan, shinya, soutyou, refresh, isRefreshDay, None);


        /// <summary>
        /// 出退勤時間を指示の有無により補正します。
        /// </summary>
        /// <remarks>
        /// 時間帯の分類：
        /// - 00:00 - 05:00 夜間
        /// - 05:00 - 08:30 早朝
        /// - 17:00 - 22:00 リフレッシュ
        /// - 22:00 - 24:00 深夜
        ///
        /// 補正ルール：
        /// - 早朝・リフレッシュの時間帯は指示がなければ補正で削除される。
        /// - 指示があれば打刻の時間に変更なし。
        /// - 夜間・深夜の時間帯は指示がなくても時間補正はしない。
        ///   画面に指示の入力を促すメッセージを表示する。
        /// </remarks>
        public static (string, string) Hosei(
            string syukkin, string taikin,
            bool yakan, bool shinya, bool soutyou, bool refresh,
            bool isRefreshDay, Option<string> shijiStartTime)
        {
            string Min(string x, string y) => string.IsNullOrEmpty(x) ? y : (string.Compare(x, y) <= 0 ? x : y);
            string Max(string x, string y) => string.IsNullOrEmpty(x) ? y : (string.Compare(x, y) <= 0 ? y : x);

            string Add5Minutes(string hhmm)
            {
                var time = TimeSpan.ParseExact(hhmm.Insert(2, ":"), @"hh\:mm", null);
                return time.Add(TimeSpan.FromMinutes(5)).ToString("hhmm");
            }

            string 五分遅れ補正(string dakoku, string kitei)
            {
                return string.Compare(dakoku, Add5Minutes(kitei)) <= 0 ? kitei : dakoku;
            }

            // 出勤時間補正
            string SyukkinHosei(string s, string t)
            {
                // 空は空や
                if (string.IsNullOrEmpty(s) && string.IsNullOrEmpty(t)) return "";
                // 前日からの夜間作業
                if (string.IsNullOrEmpty(s) && !string.IsNullOrEmpty(t) && yakan) return "0000";
                // 早朝勤務指示がなければ 8:30
                if (string.Compare(s, "0500") >= 0 && string.Compare(s, "0830") <= 0 && !soutyou) return Min(t, "0830");
                // 早朝勤務指示 がある場合は、指示の開始時間 7:10に打刻しても、指示が7:30~だった場合は7:30からになる
                if (string.Compare(s, "0500") >= 0 && string.Compare(s, "0830") <= 0 && soutyou)
                {
                    var kitei = shijiStartTime.IfNone("0830");
                    return 五分遅れ補正(s, kitei);
                }
                // 5分遅れまでは着替えとみなし補正
                if (string.Compare(s, "0831") >= 0 && string.Compare(s, "0835") <= 0) return "0830";
                return s;
            }

            //退勤時間補正
            string TaikinHosei(string s, string t)
            {
                // 空は空や
                if (string.IsNullOrEmpty(s) && string.IsNullOrEmpty(t)) return "";
                // 深夜作業
                if (!string.IsNullOrEmpty(s) && string.IsNullOrEmpty(t) && shinya) return "2400";
                // 早朝勤務指示がなければ 5:00 
                if (string.Compare(t, "0500") >= 0 && string.Compare(t, "0830") <= 0 && !soutyou) return Max(s, "0500");
                // リフレッシュデー指示が出ていなければ  17:30
                if (string.Compare(t, "1730") >= 0 && string.Compare(t, "2200") <= 0 && isRefreshDay && !refresh) return Max(s, "1730");
                return t;
            }

            var hoseiSyukkin = SyukkinHosei(syukkin, taikin);
            var hoseiTaikin = TaikinHosei(hoseiSyukkin, taikin);

            return (hoseiSyukkin, hoseiTaikin);
        }

        /// <summary>
        /// 出勤時間から指定時間間隔を引いた結果を算出します。
        /// 各休憩時間が含まれる場合、それらを差し引く処理も行います。
        /// </summary>
        public static int CalcJitsudouTimes(string sHHMM, string tHHMM)
        {
            if (string.IsNullOrEmpty(sHHMM) || string.IsNullOrEmpty(tHHMM)) return 0;
            if (sHHMM == tHHMM) return 0;

            int ConvertJikan(string hhmm) => int.Parse(hhmm.Substring(0, 2)) * 60 + int.Parse(hhmm.Substring(2));

            int startTime = ConvertJikan(sHHMM);
            int endTime = ConvertJikan(tHHMM);

            if (endTime < startTime)
                throw new ArgumentException($"出勤時刻 <= 退勤時刻の必要があります. 出勤：{sHHMM} - 退勤：{tHHMM}");

            int termTime = endTime - startTime;

            int GetIncludeTime(string s, string t, int start, int end)
            {
                int sTime = ConvertJikan(s);
                int tTime = ConvertJikan(t);
                return Math.Max(0, Math.Min(tTime, end) - Math.Max(sTime, start));
            }

            var breaks = Time.休憩時間List;
            int totalBreak = breaks.Sum(b => GetIncludeTime(sHHMM, tHHMM, b.Item1, b.Item2));

            return termTime - totalBreak;
        }

        /// <summary>
        /// 割増時間計算 (8時間以内深夜は残業代は支給されないが、深夜割り増しが支給される)
        /// </summary>
        public static int CalculationWarimashiTime(int jitsudou, int shinya)
        {
            int times = jitsudou - Time.kitei;
            if (times <= Time.zero) return shinya;
            if (Time.zero < (shinya - times)) return shinya - times;
            return Time.zero;
        }

        /// <summary>
        /// 深夜残業時間(8時間以上超過分は、深夜割り増し + 残業割り増し)を計算
        /// </summary>
        public static int CalculationShinyaCyokinTime(int jitsudou, int shinya)
        {
            int times = jitsudou - Time.kitei;
            if (Time.zero < times)
            {
                return times <= shinya ? times : shinya;
            }
            return Time.zero;
        }

        /// <summary>
        /// 実働時間を計算
        /// </summary>
        public static int CalculationJitsudouTime(string syukkin1, string taikin1, string syukkin2, string taikin2, string syukkin3, string taikin3)
        {
            return CalcJitsudouTimes(syukkin1, taikin1) +
                   CalcJitsudouTimes(syukkin2, taikin2) +
                   CalcJitsudouTimes(syukkin3, taikin3);
        }

        /// <summary>
        /// 指定時間にtermの範囲が何分あるか計算します。休憩時間は除外されます
        /// </summary>
        public static int GetIncludeTimeWithout休憩(string sHHMM, string tHHMM, (int, int) term)
        {
            string sterm = Time.ToHHmm(term.Item1);
            string tterm = Time.ToHHmm(term.Item2);
            string s = string.Compare(sHHMM, sterm) < 0 ? sterm : sHHMM;
            string e = string.Compare(tHHMM, tterm) < 0 ? tHHMM : tterm;
            if (string.Compare(s, e) >= 0) e = s;

            int r = CalcJitsudouTimes(s, e);
            return r;
        }

        /// <summary>
        /// 指定時間間隔に含まれる時間数を計算
        /// ※指定時間間隔は startTerm < endTerm とする
        /// ※退出時間 - 出勤時間 <= 24時間とする
        /// </summary>
        public static int GetIncludeTime(string sHHMM, string tHHMM, int sTerm, int eTerm)
        {
            if (sHHMM == tHHMM) return 0;

            int syukkinTime = Time.ConvertJikan(sHHMM);
            int taisyutsuTime = Time.ConvertJikan(tHHMM);

            // 日をまたぐ場合
            if (taisyutsuTime <= syukkinTime)
            {
                if (sTerm <= taisyutsuTime)
                {
                    // sTerm <= 退出時間 | 出勤時間 < eTerm
                    if (syukkinTime <= eTerm)
                        return (eTerm - syukkinTime) + (taisyutsuTime - sTerm);
                    // sTerm < 退出時間 | eTerm <= 出勤時間
                    else if (eTerm <= syukkinTime && syukkinTime < taisyutsuTime && taisyutsuTime < eTerm)
                        return taisyutsuTime - sTerm;
                    else if (eTerm <= syukkinTime && eTerm <= taisyutsuTime)
                        return eTerm - sTerm;
                }

                taisyutsuTime += Time.oneday;
            }
            // 日を跨ぐ場合でも syukkinTime >= eTerm は24時間足されているから有効
            if (eTerm <= syukkinTime || taisyutsuTime <= sTerm)
                return Time.zero;
            
            // 日を跨がない場合
            if (syukkinTime <= sTerm)
            {
                // 出勤時間 < sTerm --- eTerm < 退出時間
                if (eTerm <= taisyutsuTime)
                    return eTerm - sTerm;
                // 出勤時間 < sTerm < 退出時間 < eTerm
                else if (taisyutsuTime <= eTerm)
                    return taisyutsuTime - sTerm;
            }
            else if (syukkinTime >= sTerm)
            {
                // sTerm < 出勤時間 < eTerm < 退出時間
                if (eTerm <= taisyutsuTime)
                    return eTerm - syukkinTime;
            }

            // sTerm < 出勤時間 < 退出時間 < eTerm
            return taisyutsuTime - syukkinTime;
        }
    }

    public static class Time
    {
        public const int oneday = 24 * 60;
        public const int zero = 0;
        public const int kitei = 8 * 60;
        public const int kiteiMinus = -kitei;

        public const int shinyaSeparate = 5 * 60;

        public const int soutyouStartTime = 5 * 60;
        public const int soutyouEndTime = 8 * 60 + 30;

        public const int lunchStartTime = 12 * 60;
        public const int lunchEndTime = 13 * 60;

        public static readonly (int, int) 夜間作業 = (0, 5 * 60);
        public static readonly (int, int) 早朝作業 = (5 * 60, 8 * 60 + 30);
        public static readonly (int, int) リフレッシュ = (17 * 60 + 30, 22 * 60);
        public static readonly (int, int) 深夜作業 = (22 * 60, 24 * 60);

        public static readonly (int, int) 休憩0130_0145 = (1 * 60 + 30, 1 * 60 + 45);
        public static readonly (int, int) 休憩0330_0345 = (3 * 60 + 30, 3 * 60 + 45);
        public static readonly (int, int) 休憩0530_0545 = (5 * 60 + 30, 5 * 60 + 45);
        public static readonly (int, int) 休憩1200_1300 = (12 * 60, 13 * 60);
        public static readonly (int, int) 休憩1930_1945 = (19 * 60 + 30, 19 * 60 + 45);
        public static readonly (int, int) 休憩2130_2145 = (21 * 60 + 30, 21 * 60 + 45);
        public static readonly (int, int) 休憩2330_2345 = (23 * 60 + 30, 23 * 60 + 45);

        public static readonly List<(int, int)> 休憩時間List = new List<(int, int)>
        {
            休憩0130_0145,
            休憩0330_0345,
            休憩0530_0545,
            休憩1200_1300,
            休憩1930_1945,
            休憩2130_2145,
            休憩2330_2345
        };

        public const int kyuukaAM = 3 * 60 + 30; //3.5 * 60 minuts
        public const int kyuukaPM = 4 * 60 + 30; //4.5 * 60 minuts
        public const int flex = 4 * 60;
        public const int flexMinus = -flex;
        public const int kyuujistuHalfBorder = 4 * 60;
        public const double HHMM1145 = 11.75 * 60;

        public static readonly decimal ichinichi = 1.0m;
        public static readonly decimal hanniti = 0.5m;
        public static readonly decimal syukkinNashi = 0m;

        public static readonly decimal yuukyuuSyoukaDay = 1.0m;
        public static readonly decimal yuukyuuSyoukaHalfDay = 0.5m;

        /// <summary>
        /// HHMM形式の文字列（例: "0830"）を分単位の整数に変換
        /// </summary>
        public static int ConvertJikan(string hhmm)
        {
            return int.Parse(hhmm.Substring(0, 2)) * 60 + int.Parse(hhmm.Substring(2));
        }

        /// <summary>
        /// 分単位の整数をHHMM形式の文字列に変換（例: 510 → "0830"）
        /// </summary>
        public static string ToHHmm(int minutes)
        {
            int hh = minutes / 60;
            int mm = minutes % 60;
            return $"{hh:D2}{mm:D2}";
        }
    }
}