using CommonLibrary.Extensions;
using Model.Enums;
using ZouryokuCommonLibrary.Utils;

namespace Zouryoku.Pages.JissekiNyuryoku
{
    // 時間コンテナ
    // 各時間の計算結果を表すクラス
    public class TimeContainer
    {
        // 平日用
        public int Jitsudou { get; }
        public int HZangyo { get; }
        public int HZangyoShinya { get; }
        public int HShinya { get; }

        // 土曜祝祭日用
        public int DJitsudou { get; }
        public int DZangyo { get; }
        public int DJitsudouShinya { get; }
        public int DZangyoShinya { get; }

        // 日曜用
        public int NJitsudou { get; }
        public int NShinya { get; }

        // トータル残業
        public int TotalZangyo { get; }

        // コンストラクタ
        private TimeContainer(
            int jitsudou = 0, int hZangyo = 0, int hZangyoShinya = 0, int hShinya = 0,
            int dJitsudou = 0, int dZangyo = 0, int dJitsudouShinya = 0, int dZangyoShinya = 0,
            int nJitsudou = 0, int nShinya = 0,
            int totalZangyo = 0)
        {
            Jitsudou = jitsudou;
            HZangyo = hZangyo;
            HZangyoShinya = hZangyoShinya;
            HShinya = hShinya;

            DJitsudou = dJitsudou;
            DZangyo = dZangyo;
            DJitsudouShinya = dJitsudouShinya;
            DZangyoShinya = dZangyoShinya;

            NJitsudou = nJitsudou;
            NShinya = nShinya;

            TotalZangyo = totalZangyo;
        }

        // 平日用Factory メソッド
        public static TimeContainer CreateHeijitsu(int jitsudou, int hZangyo, int hShinya, int hZangyoShinya, int totalZangyo)
            => new(
                jitsudou,
                hZangyo,
                hZangyoShinya,
                hShinya,
                totalZangyo: totalZangyo
                );

        // 土曜祝祭日用Factory メソッド
        public static TimeContainer CreateDosyuku(int dJitsudou, int dZangyo, int dJitsudouShinya, int dZangyoShinya, int totalZangyo)
            => new(
                dJitsudou: dJitsudou, 
                dZangyo: dZangyo,
                dJitsudouShinya: dJitsudouShinya,
                dZangyoShinya: dZangyoShinya,
                totalZangyo: totalZangyo
                );

        // 日曜用Factory メソッド
        public static TimeContainer CreateNichiyou(int nJitsudou, int nShinya, int totalZangyo)
            => new(
                nJitsudou: nJitsudou,
                nShinya: nShinya,
                totalZangyo: totalZangyo
                );
    }

    // 時間計算のInteface
    public interface ITimeCalculation
    {
        // 時間計算処理を行い、その結果をTimeContainerへ編集し返却する
        TimeContainer Calculate(NippouInputViewModel form);

        // 残業時間を計算し取得する
        public int GetZangyouTime(int jitsudou);
    }

    // 平日用時間計算
    public class WeekDayTimeCalculation : ITimeCalculation
    {
        // 時間計算処理
        public TimeContainer Calculate(NippouInputViewModel vm)
        {
            var jitsudouTime = vm.JitsudouTime;
            var jitsudou = JitsudouTimeIncludeYuukyuu(vm, jitsudouTime);
            var hZangyo = GetZangyouTime(jitsudou);

            var shinyaTime = vm.YakanShinyaTime;
            var hShinya = TimeCalculator.CalculationWarimashiTime(jitsudouTime, shinyaTime);
            var hZangyoShinya = TimeCalculator.CalculationShinyaCyokinTime(jitsudouTime, shinyaTime);

            return TimeContainer.CreateHeijitsu(
                jitsudou,
                hZangyo,
                hShinya,
                hZangyoShinya,
                hZangyo
            );
        }

        // 残業時間取得
        public int GetZangyouTime(int jitsudou)
        {
            return jitsudou - Time.kitei;
        }

        // 半日有給の場合、実働に4時間を足す(パート勤務含む)
        // 平日固有の計算処理
        public int JitsudouTimeIncludeYuukyuu(NippouInputViewModel vm, int jitsudouTime)
        {
            bool isHalfOrPart =
                vm.SyukkinKubun1 is AttendanceClassification.半日勤務 or AttendanceClassification.パート勤務;

            bool isYuukyuuApplicable =
                !vm.IsMukyuuHaldDay && vm.SyukkinKubun2 != AttendanceClassification.None;

            if (!(isHalfOrPart && isYuukyuuApplicable))
                return jitsudouTime;

            if (vm.SyukkinHm1 == null || vm.SyukkinHm2 == null)
                return jitsudouTime;

            // 出退勤時刻を分に換算
            int syukkin = Time.ConvertJikan(vm.SyukkinHm1?.ToStrByHHmmNoColon() ?? string.Empty);
            int taisyutsu = Time.ConvertJikan(vm.TaisyutsuHm1?.ToStrByHHmmNoColon() ?? string.Empty);

            // 午後が半日有給
            if (taisyutsu <= Time.lunchEndTime)
                return jitsudouTime + Time.kyuukaPM;

            // 午前が半日有給休
            if (Time.lunchStartTime <= syukkin)
                return jitsudouTime + Time.kyuukaAM;

            // 午前・午後、どちらの実働が長いか
            int pmLength = taisyutsu - Time.lunchEndTime;
            int amLength = Time.lunchStartTime - syukkin;

            // 午前の実働が長い
            if (pmLength < amLength)
                return jitsudouTime + Time.kyuukaPM;

            return jitsudouTime + Time.kyuukaAM;
        }
    }

    // 土曜祝日用時間計算
    public class SaturdayHolidayTimeCalculation : ITimeCalculation
    {
        // 時間計算処理
        public TimeContainer Calculate(NippouInputViewModel form)
        {
            var dJitsudou = form.JitsudouTime;
            var dZangyo = GetZangyouTime(dJitsudou);

            var shinyaTime = form.YakanShinyaTime;
            var dJitsudouShinya = TimeCalculator.CalculationWarimashiTime(dJitsudou, shinyaTime);
            var dZangyoShinya = TimeCalculator.CalculationShinyaCyokinTime(dJitsudou, shinyaTime);

            return TimeContainer.CreateDosyuku(
                dJitsudou,
                dZangyo,
                dJitsudouShinya,
                dZangyoShinya,
                totalZangyo: dZangyo
            );
        }

        // 残業時間取得
        public int GetZangyouTime(int jitsudou)
        {
            var diff = jitsudou - Time.kitei;
            return Math.Max(diff, Time.zero);
        }
    }

    // 日曜用時間計算
    public class SundayTimeCalculation : ITimeCalculation
    {
        // 時間計算処理
        public TimeContainer Calculate(NippouInputViewModel form)
        {
            var nJitsudou = form.JitsudouTime;
            var nShinya = form.YakanShinyaTime;
            var totalZangyo = GetZangyouTime(nJitsudou);

            return TimeContainer.CreateNichiyou(
                nJitsudou,
                nShinya,
                totalZangyo: totalZangyo
            );
        }
        // 残業時間取得
        public int GetZangyouTime(int jitsudou)
        {
            return jitsudou;
        }

    }

    // 時間計算クラスのFactory
    public static class TimeCalculationFactory
    {
        public static ITimeCalculation Create(DayType dayType)
            => dayType switch
            {
                DayType.平日 => new WeekDayTimeCalculation(),
                DayType.土曜祝祭日 => new SaturdayHolidayTimeCalculation(),
                DayType.日曜 => new SundayTimeCalculation(),
                _ => throw new ArgumentOutOfRangeException(nameof(dayType), "パラメータの値が不正です。")
            };
    }
}
