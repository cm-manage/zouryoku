using Microsoft.VisualStudio.TestTools.UnitTesting;
using LanguageExt;
using static LanguageExt.Prelude;
using static ZouryokuCommonLibrary.Utils.TimeCalculator;
using ZouryokuCommonLibrary.Utils;

namespace ZouryokuCommonLibraryTest.Utils
{
    [TestClass]
    public class TimeCalculatorTests
    {
        // 00:00 - 05:00 夜間
        // 05:00 - 08:30 早朝
        // 17:30 - 22:00 リフレッシュ
        // 22:00 - 24:00 深夜
        //
        // 早朝・リフレッシュの時間帯は指示がなければ補正で削除される
        // 指示があれば打刻の時間に変更なし
        // 夜間・深夜の時間帯は指示がなくても時間補正はしない、画面に指示の入力を促すメッセージを表示する
        // 
        // 補正メソッドの引数 前二つ時間、それ以降指示の有無、最後はリフレッシュデーかどうか
        // def hosei(syukkin, taikin, yakan, shinya, soutyou, refresh, isRefreshDay)

        [TestMethod]
        public void 空() =>
            Assert.AreEqual(("", ""), Hosei("", "", false, false, false, false, false));

        [TestMethod]
        public void 出勤のみ退勤なし() =>
            Assert.AreEqual(("0830", ""), Hosei("0827", "", false, false, false, false, false));

        [TestMethod]
        public void 退勤のみ出勤なし_非リフレッシュデー() =>
            Assert.AreEqual(("", "1756"), Hosei("", "1756", false, false, false, false, false));

        [TestMethod]
        public void 出勤_夜間作業_指示なし() =>
            Assert.AreEqual(("", "0420"), Hosei("", "0420", yakan: false, false, false, false, false));

        [TestMethod]
        public void 出勤_夜間作業_指示あり() =>
            Assert.AreEqual(("0000", "0420"), Hosei("", "0420", yakan: true, false, false, false, false));

        [TestMethod]
        public void 出勤_夜間作業_指示のみ() =>
            Assert.AreEqual(("", ""), Hosei("", "", yakan: true, false, false, false, false));

        [TestMethod]
        public void 出勤_早朝以前() =>
            Assert.AreEqual(("0200", "1200"), Hosei("0200", "1200", false, false, soutyou: false, false, false));

        [TestMethod]
        public void 出勤_早朝_指示なし()
        {
            Assert.AreEqual(("0830", "1200"), Hosei("0500", "1200", false, false, soutyou: false, false, false));
            Assert.AreEqual(("0830", "1200"), Hosei("0829", "1200", false, false, soutyou: false, false, false));
        }

        [TestMethod]
        public void 出勤_早朝_指示なし_早朝時間帯のみ() =>
            Assert.AreEqual(("0700", "0700"), Hosei("0530", "0700", false, false, soutyou: false, false, false));

        [TestMethod]
        public void 出勤_早朝_指示あり()
        {
            Assert.AreEqual(("0500", "1200"), Hosei("0500", "1200", false, false, soutyou: true, false, false, Some("0000")));
            Assert.AreEqual(("0631", "1200"), Hosei("0631", "1200", false, false, soutyou: true, false, false, Some("0000")));
        }

        [TestMethod]
        public void 出勤_早朝_指示あり_指示時刻前補正()
        {
            Assert.AreEqual(("0530", "1200"), Hosei("0500", "1200", false, false, soutyou: true, false, false, Some("0530")));
            Assert.AreEqual(("0700", "1200"), Hosei("0631", "1200", false, false, soutyou: true, false, false, Some("0700")));
        }

        [TestMethod]
        public void 出勤_早朝_指示あり_5分遅れ許容()
        {
            Assert.AreEqual(("0630", "1200"), Hosei("0635", "1200", false, false, soutyou: true, false, false, Some("0630")));
            Assert.AreEqual(("0636", "1200"), Hosei("0636", "1200", false, false, soutyou: true, false, false, Some("0630")));
        }

        [TestMethod]
        public void 出勤_早朝以降() =>
            Assert.AreEqual(("0901", "1200"), Hosei("0901", "1200", false, false, false, false, false));

        [TestMethod]
        public void 出勤_5分遅れOK() =>
            Assert.AreEqual(("0830", "1730"), Hosei("0835", "1730", false, false, false, false, false));

        [TestMethod]
        public void 退勤_深夜作業_指示なし() =>
            Assert.AreEqual(("1900", ""), Hosei("1900", "", false, false, false, false, false));

        [TestMethod]
        public void 退勤_深夜作業_指示あり() =>
            Assert.AreEqual(("1900", "2400"), Hosei("1900", "", false, shinya: true, false, false, false));

        [TestMethod]
        public void 退勤_深夜作業_指示のみ() =>
            Assert.AreEqual(("", ""), Hosei("", "", false, shinya: true, false, false, false));

        [TestMethod]
        public void 退勤_早朝以前() =>
            Assert.AreEqual(("0000", "0459"), Hosei("0000", "0459", false, false, soutyou: false, false, false));

        [TestMethod]
        public void 退勤_早朝_指示なし()
        {
            Assert.AreEqual(("0000", "0500"), Hosei("0000", "0500", false, false, soutyou: false, false, false));
            Assert.AreEqual(("0000", "0500"), Hosei("0000", "0830", false, false, soutyou: false, false, false));
        }

        [TestMethod]
        public void 退勤_早朝_指示あり()
        {
            Assert.AreEqual(("0000", "0501"), Hosei("0000", "0501", false, false, soutyou: true, false, false));
            Assert.AreEqual(("0000", "0830"), Hosei("0000", "0830", false, false, soutyou: true, false, false));
        }

        [TestMethod]
        public void 退勤_早朝以降() =>
            Assert.AreEqual(("0000", "1200"), Hosei("0000", "1200", false, false, soutyou: false, false, false));

        [TestMethod]
        public void 退勤_リフレッシュ以前() =>
            Assert.AreEqual(("0830", "1729"), Hosei("0830", "1729", false, false, false, refresh: false, isRefreshDay: true));

        [TestMethod]
        public void 退勤_リフレッシュ_非リフレッシュデー()
        {
            Assert.AreEqual(("0830", "1915"), Hosei("0830", "1915", false, false, false, refresh: false, isRefreshDay: false));
            Assert.AreEqual(("0830", "2200"), Hosei("0830", "2200", false, false, false, refresh: false, isRefreshDay: false));
        }

        [TestMethod]
        public void 退勤_リフレッシュ_指示なし()
        {
            Assert.AreEqual(("0830", "1730"), Hosei("0830", "1915", false, false, false, refresh: false, isRefreshDay: true));
            Assert.AreEqual(("0830", "1730"), Hosei("0830", "2200", false, false, false, refresh: false, isRefreshDay: true));
        }

        [TestMethod]
        public void 退勤_リフレッシュ_指示なし_時間帯のみ()
        {
            Assert.AreEqual(("1730", "1730"), Hosei("1730", "1915", false, false, false, refresh: false, isRefreshDay: true));
            Assert.AreEqual(("2100", "2100"), Hosei("2100", "2200", false, false, false, refresh: false, isRefreshDay: true));
        }

        [TestMethod]
        public void 退勤_リフレッシュ_指示あり()
        {
            Assert.AreEqual(("0830", "1915"), Hosei("0830", "1915", false, false, false, refresh: true, isRefreshDay: true));
            Assert.AreEqual(("0830", "2200"), Hosei("0830", "2200", false, false, false, refresh: true, isRefreshDay: true));
        }

        [TestMethod]
        public void 退勤_リフレッシュ以降() =>
            Assert.AreEqual(("0830", "2245"), Hosei("0830", "2245", false, false, false, refresh: false, isRefreshDay: true));

        [TestMethod]
        public void calculationJitsudouTime_空() =>
            Assert.AreEqual(0, CalculationJitsudouTime("", "", "", "", "", ""));
        [TestMethod]
        public void calculationJitsudouTime_同一時刻() =>
            Assert.AreEqual(0, CalculationJitsudouTime("0100", "0100", "", "", "", ""));
        [TestMethod]
        public void calculationJitsudouTime_一日と混同しそうな同一時刻() =>
            Assert.AreEqual(0, CalculationJitsudouTime("0000", "0000", "", "", "", ""));
        [TestMethod]
        public void calculationJitsudouTime_一日() => // 1日の休憩合計2時間30分
            Assert.AreEqual(21 * 60 + 30, CalculationJitsudouTime("0000", "2400", "", "", "", ""));
        [TestMethod]
        public void calculationJitsudouTime_時間の逆転はエラー() =>
            Assert.ThrowsExactly<System.ArgumentException>(() => CalculationJitsudouTime("0001", "0000", "", "", "", ""));

        [TestMethod]
        public void calculationJitsudouTime_3か所どこを使用しても一緒()
        {
            Assert.AreEqual(Time.kitei, CalculationJitsudouTime("0830", "1730", "", "", "", ""));
            Assert.AreEqual(Time.kitei, CalculationJitsudouTime("", "", "0830", "1730", "", ""));
            Assert.AreEqual(Time.kitei, CalculationJitsudouTime("", "", "", "", "0830", "1730"));
        }


        // Time.scala で定義された休憩時間
        //  val 休憩0130_0145 = ( 1 * 60 + 30,  1 * 60 + 45)
        //  val 休憩0330_0345 = ( 3 * 60 + 30,  3 * 60 + 45)
        //  val 休憩0530_0545 = ( 5 * 60 + 30,  5 * 60 + 45)
        //  val 休憩1200_1300 = (12 * 60,      13 * 60     )
        //  val 休憩1930_1945 = (19 * 60 + 30, 19 * 60 + 45)
        //  val 休憩2130_2145 = (21 * 60 + 30, 21 * 60 + 45)
        //  val 休憩2330_2345 = (23 * 60 + 30, 23 * 60 + 45)
        [TestMethod]
        public void calculationJitsudouTime_休憩0130_0145()
        {
            Assert.AreEqual(90, CalculationJitsudouTime("0000", "0130", "", "", "", ""));
            Assert.AreEqual(90, CalculationJitsudouTime("0000", "0137", "", "", "", ""));
            Assert.AreEqual(90, CalculationJitsudouTime("0000", "0145", "", "", "", ""));
            Assert.AreEqual(91, CalculationJitsudouTime("0000", "0146", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("0130", "0140", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("0130", "0145", "", "", "", ""));
            Assert.AreEqual(5, CalculationJitsudouTime("0135", "0150", "", "", "", ""));
        }

        [TestMethod]
        public void 休憩0330_0345()
        {
            Assert.AreEqual(90, CalculationJitsudouTime("0200", "0330", "", "", "", ""));
            Assert.AreEqual(90, CalculationJitsudouTime("0200", "0337", "", "", "", ""));
            Assert.AreEqual(90, CalculationJitsudouTime("0200", "0345", "", "", "", ""));
            Assert.AreEqual(91, CalculationJitsudouTime("0200", "0346", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("0330", "0340", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("0330", "0345", "", "", "", ""));
            Assert.AreEqual(5, CalculationJitsudouTime("0335", "0350", "", "", "", ""));
        }

        [TestMethod]
        public void 休憩0530_0545()
        {
            Assert.AreEqual(90, CalculationJitsudouTime("0400", "0530", "", "", "", ""));
            Assert.AreEqual(90, CalculationJitsudouTime("0400", "0537", "", "", "", ""));
            Assert.AreEqual(90, CalculationJitsudouTime("0400", "0545", "", "", "", ""));
            Assert.AreEqual(91, CalculationJitsudouTime("0400", "0546", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("0530", "0540", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("0530", "0545", "", "", "", ""));
            Assert.AreEqual(5, CalculationJitsudouTime("0535", "0550", "", "", "", ""));
        }

        [TestMethod]
        public void 休憩1200_1300()
        {
            Assert.AreEqual(60, CalculationJitsudouTime("1100", "1200", "", "", "", ""));
            Assert.AreEqual(60, CalculationJitsudouTime("1100", "1230", "", "", "", ""));
            Assert.AreEqual(60, CalculationJitsudouTime("1100", "1300", "", "", "", ""));
            Assert.AreEqual(61, CalculationJitsudouTime("1100", "1301", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("1200", "1230", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("1200", "1300", "", "", "", ""));
            Assert.AreEqual(5, CalculationJitsudouTime("1230", "1305", "", "", "", ""));
        }

        [TestMethod]
        public void 休憩1930_1945()
        {
            Assert.AreEqual(90, CalculationJitsudouTime("1800", "1930", "", "", "", ""));
            Assert.AreEqual(90, CalculationJitsudouTime("1800", "1937", "", "", "", ""));
            Assert.AreEqual(90, CalculationJitsudouTime("1800", "1945", "", "", "", ""));
            Assert.AreEqual(91, CalculationJitsudouTime("1800", "1946", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("1930", "1940", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("1930", "1945", "", "", "", ""));
            Assert.AreEqual(5, CalculationJitsudouTime("1935", "1950", "", "", "", ""));
        }

        [TestMethod]
        public void 休憩2130_2145()
        {
            Assert.AreEqual(90, CalculationJitsudouTime("2000", "2130", "", "", "", ""));
            Assert.AreEqual(90, CalculationJitsudouTime("2000", "2137", "", "", "", ""));
            Assert.AreEqual(90, CalculationJitsudouTime("2000", "2145", "", "", "", ""));
            Assert.AreEqual(91, CalculationJitsudouTime("2000", "2146", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("2130", "2140", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("2130", "2145", "", "", "", ""));
            Assert.AreEqual(5, CalculationJitsudouTime("2135", "2150", "", "", "", ""));
        }

        [TestMethod]
        public void 休憩2330_2345()
        {
            Assert.AreEqual(90, CalculationJitsudouTime("2200", "2330", "", "", "", ""));
            Assert.AreEqual(90, CalculationJitsudouTime("2200", "2337", "", "", "", ""));
            Assert.AreEqual(90, CalculationJitsudouTime("2200", "2345", "", "", "", ""));
            Assert.AreEqual(91, CalculationJitsudouTime("2200", "2346", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("2330", "2340", "", "", "", ""));
            Assert.AreEqual(0, CalculationJitsudouTime("2330", "2345", "", "", "", ""));
            Assert.AreEqual(5, CalculationJitsudouTime("2335", "2350", "", "", "", ""));
        }

        [TestMethod]
        public void getIncludeTimeWithout休憩_休憩時間指定で0() =>
            Assert.AreEqual(0, GetIncludeTimeWithout休憩("1130", "1430", Time.休憩1200_1300));
        [TestMethod]
        public void getIncludeTimeWithout休憩_休憩後1分ずつ() =>
            Assert.AreEqual(2, GetIncludeTimeWithout休憩("1130", "1430", (11 * 60 + 59, 13 * 60 + 1)));
        [TestMethod]
        public void getIncludeTimeWithout休憩_深夜作業控除15分() =>
            Assert.AreEqual(105, GetIncludeTimeWithout休憩("0000", "2400", Time.深夜作業));
        [TestMethod]
        public void getIncludeTimeWithout休憩_範囲外前方() =>
            Assert.AreEqual(0, GetIncludeTimeWithout休憩("0000", "0830", Time.リフレッシュ));
        [TestMethod]
        public void getIncludeTimeWithout休憩_範囲外後方() =>
            Assert.AreEqual(0, GetIncludeTimeWithout休憩("2215", "2400", Time.リフレッシュ));
        [TestMethod]
        public void getIncludeTimeWithout休憩_部分一致前方() =>
            Assert.AreEqual(225, GetIncludeTimeWithout休憩("1700", "2135", Time.リフレッシュ));
        [TestMethod]
        public void getIncludeTimeWithout休憩_部分一致後方() =>
            Assert.AreEqual(120, GetIncludeTimeWithout休憩("1938", "2400", Time.リフレッシュ));
        [TestMethod]
        public void getIncludeTimeWithout休憩_部分一致内包() =>
            Assert.AreEqual(105, GetIncludeTimeWithout休憩("1938", "2138", Time.リフレッシュ));
        [TestMethod]
        public void getIncludeTimeWithout休憩_早朝() =>
            Assert.AreEqual(195, GetIncludeTimeWithout休憩("0000", "0900", Time.早朝作業));

        [TestMethod]
        public void getIncludeTime_全包含() =>
            Assert.AreEqual(60, GetIncludeTime("1130", "1430", Time.lunchStartTime, Time.lunchEndTime));

        int shinyaMaeStartTime = 21 * 60 + 30; //21:30
        int shinyaMaeEndTime = 22 * 60; // 22:00

        [TestMethod]
        public void getIncludeTime_前方含む() =>
            Assert.AreEqual(15, GetIncludeTime("2100", "2145", shinyaMaeStartTime, shinyaMaeEndTime));
        [TestMethod]
        public void getIncludeTime_後方含む()
        {
            Assert.AreEqual(13, GetIncludeTime("2147", "2200", shinyaMaeStartTime, shinyaMaeEndTime));
            Assert.AreEqual(13, GetIncludeTime("2147", "2400", shinyaMaeStartTime, shinyaMaeEndTime));
        }
        [TestMethod]
        public void getIncludeTime_含まない()
        {
            Assert.AreEqual(0, GetIncludeTime("0830", "1200", Time.lunchStartTime, Time.lunchEndTime));
            Assert.AreEqual(0, GetIncludeTime("2400", "2400", Time.lunchStartTime, Time.lunchEndTime));
        }

        [TestMethod]
        public void calculationWarimashiTime_8時間以内の勤務では_深夜勤務時間が全額割り増しとなる() =>
            Assert.AreEqual(45, CalculationWarimashiTime(8 * 60, 45));
        [TestMethod]
        public void calculationWarimashiTime_8時間超の勤務では_残業代にならなかった時間分が割り増しとなる() =>
            Assert.AreEqual(25, CalculationWarimashiTime(8 * 60 + 20, 45));

        [TestMethod]
        public void calculationShinyaCyokinTime_8時間以内の勤務では_残業代はでません()
        {
            Assert.AreEqual(0, CalculationShinyaCyokinTime(8 * 60, 45));
        }

        [TestMethod]
        public void calculationShinyaCyokinTime_8時間超の勤務では_残業代にならなかった時間分が割り増しとなる()
        {
            Assert.AreEqual(20, CalculationShinyaCyokinTime(8 * 60 + 20, 45));
        }
    }
}

