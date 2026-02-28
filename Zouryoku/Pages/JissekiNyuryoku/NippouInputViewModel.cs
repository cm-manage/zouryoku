using CommonLibrary.Extensions;
using LanguageExt.Pipes;
using Microsoft.AspNetCore.Mvc.Rendering;
using Model.Enums;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using ZouryokuCommonLibrary.Utils;
using static Zouryoku.Utils.Const;

namespace Zouryoku.Pages.JissekiNyuryoku
{
    /// <summary>
    /// 実績入力欄View Model
    /// </summary>
    public class JissekiInputViewModel
    {
        public int Index { get; set; }

        // 日報案件のId
        public long? Id { get; set; }

        // 案件ID
        public long? AnkensId { get; set; }

        [Display(Name = "顧客名")]
        [Required(ErrorMessage = ErrorSelectRequired)]
        public string? KokyakuName { get; set; }

        [Display(Name = "件名")]
        public string? AnkenName { get; set; }

        [Display(Name = "実績時間")]
        public short? JissekiJikan { get; set; }

        // 顧客会社ID
        public long? KokyakuKaisyaId { get; set; }

        [Display(Name = "プロセス")]
        public long? BumonProcessId { get; set; }

        // 原価連携フラグ
        public bool IsLinked { get; set; } = false;

        [Display(Name = "受注番号")]
        [Required(ErrorMessage = ErrorSelectRequired)]
        public string? KingsJuchuNo { get; set; }

        [Display(Name = "着工日")]
        public DateOnly? ChaYmd { get; set; }

        // KINGS受注の原価凍結フラグ
        public bool? IsGenkaToketu { get; set; }

        // Version
        public uint? Version { get; set; }

        // 削除対象
        public bool IsDelete { get; set; } = false;

        // 部門プロセスリスト
        public List<SelectListItem> BumonProcessList { get; set; } = [];

        public static JissekiInputViewModel FromEntity(NippouAnken entity, int index, List<SelectListItem> bumonProcessList)
            => new()
            {
                Index = index,
                Id = entity.Id,
                AnkensId = entity.AnkensId,
                KokyakuName = entity.KokyakuName,
                AnkenName = entity.AnkenName,
                JissekiJikan = entity.JissekiJikan,
                KokyakuKaisyaId = entity.KokyakuKaisyaId,
                BumonProcessId = entity.BumonProcessId,
                IsLinked = entity.IsLinked,
                KingsJuchuNo = entity.Ankens.KingsJuchu?.KingsJuchuNo,
                ChaYmd = entity.Ankens.KingsJuchu?.ChaYmd,
                IsGenkaToketu = entity.Ankens.KingsJuchu?.IsGenkaToketu,
                Version = entity.Version,
                BumonProcessList = bumonProcessList,
            };
    }

    // 日報実績　View Model
    public class NippouInputViewModel
    {
        // 実績年月日
        public DateOnly JissekiDate { get; set; }

        /// <summary>
        /// 出退勤時間１（HH:mm～HH:mm）
        /// </summary>
        public string? Syuttaikin1 { get; set; }

        /// <summary>
        /// 出退勤時間２（HH:mm～HH:mm）
        /// </summary>
        public string? Syuttaikin2 { get; set; }

        /// <summary>
        /// 出退勤時間３（HH:mm～HH:mm）
        /// </summary>
        public string? Syuttaikin3 { get; set; }

        /// <summary>
        /// 実働時間（単位：分）
        /// </summary>
        [Display(Name = "勤務時間")]
        public int JitsudouTime { get; set; }

        public string JitsudouTimeText => Time.ToHHmmWithColon(JitsudouTime);

        /// <summary>
        /// 実績時間（時間:分）
        /// </summary>
        [Display(Name = "実績時間")]
        public int TotalJissekiJikan { get; set; }

        public string TotalJissekiJikanText => Time.ToHHmmWithColon(TotalJissekiJikan);

        /// <summary>
        /// 振替休日取得予定日
        /// </summary>
        [Display(Name = "振替休暇取得予定日")]
        public DateOnly? FurikyuYoteiDate { get; set; }

        /// <summary>
        /// 出勤区分１のオプションリスト
        /// </summary>
        public List<SelectListItem> SyukkinKubun1List { get; set; } = [];

        /// <summary>
        /// 出勤区分２のオプションリスト
        /// </summary>
        public List<SelectListItem> SyukkinKubun2List { get; set; } = [];

        /// <summary>
        /// 出勤区分１
        /// </summary>
        public AttendanceClassification SyukkinKubun1 { get; set; }

        /// <summary>
        /// 出勤区分２
        /// </summary>
        public AttendanceClassification SyukkinKubun2 { get; set; }

        /// <summary>
        /// 申請情報欄のリスト
        /// </summary>
        public List<ShinseiInfoViewModel> ShinseiInfos { get; set; } = [];

        /// <summary>
        /// 実績入力欄のリスト
        /// </summary>
        public List<JissekiInputViewModel> JissekiInputs { get; set; } = [];


        // 出勤時間１
        public TimeOnly? SyukkinHm1 { get; set; }

        // 退出時間１
        public TimeOnly? TaisyutsuHm1 { get; set; }

        // 出勤時間２
        public TimeOnly? SyukkinHm2 { get; set; }

        // 退出時間２
        public TimeOnly? TaisyutsuHm2 { get; set; }

        // 出勤時間３
        public TimeOnly? SyukkinHm3 { get; set; }

        // 退出時間３
        public TimeOnly? TaisyutsuHm3 { get; set; }

        // 日報のID
        public long? Id { get; set; }

        // 代理入力
        public bool IsDairiInput { get; set; }

        // 社員BaseId
        public long SyainBaseId { get; set; }

        /// <summary>
        /// 日報のVersion
        /// </summary>
        public uint? Version { get; set; }

        public string? MessageString { get; set; }

        /// <summary>
        /// 確定ボタンの表示制御
        /// </summary>
        /// <returns>表示：true、非表示：false</returns>
        public bool IsKakuteiButtonVisible { get; set; }

        /// <summary>
        /// 確定解除ボタンの表示制御
        /// </summary>
        /// <returns>表示：true、非表示：false</returns>
        public bool IsKakuteiKaijoButtonVisible { get; set; }

        /// <summary>
        /// 一時保存ボタンの表示制御
        /// </summary>
        /// <returns>表示：true、非表示：false</returns>
        public bool IsIchijiHozonButtonVisible { get; set; }

        /// <summary>
        /// 土曜日の実働時間
        /// </summary>
        public decimal? DJitsudou { get; set; } = 0m;

        /// <summary>
        /// 日曜日の実働時間
        /// </summary>
        public decimal? NJitsudou { get; set; } = 0m;

        /// <summary>
        /// 残業合計時間
        /// </summary>
        public decimal TotalZangyo { get; set; } = 0m;

        /// <summary>
        /// 3ヶ月合計残業時間
        /// </summary>
        public decimal Total3MonthZangyoTotal { get; set; } = 0m;

        /// <summary>
        /// 累積残業時間
        /// </summary>
        public decimal RuisekiJikangai { get; set; } = 0m;

        public int YakanTime { get; set; }

        public int ShinyaTime { get; set; }

        public int YakanShinyaTime { get; set; }

        /// <summary>
        /// 勤務時間１～３全てがブランクかどうか
        /// </summary>
        public bool IsSyuttaikinAllBlank =>
            SyukkinHm1 == null && TaisyutsuHm1 == null &&
            SyukkinHm2 == null && TaisyutsuHm2 == null &&
            SyukkinHm3 == null && TaisyutsuHm3 == null;

        /// <summary>
        /// 出勤区分1、２はそれぞれ異なる区分が設定されている前提で引数で指定された区分でない方を返します。
        /// </summary>
        /// <param name="syukkinKubun">対象の出勤区分</param>
        /// <returns>対象の出勤区分でないほうの出勤区分</returns>
        public AttendanceClassification OtherSyukkinKubun(AttendanceClassification syukkinKubun) => (SyukkinKubun1 == syukkinKubun) ? SyukkinKubun2 : SyukkinKubun1;

        /// <summary>
        /// 出勤区分１が半日休暇の区分か否か
        /// </summary>
        public bool IsHannitiKyuuka => SyukkinKubun1 == AttendanceClassification.半日振休 || SyukkinKubun1 == AttendanceClassification.半日有給;

        /// <summary>
        /// 出勤区分２が半日休暇の区分か否か
        /// </summary>
        public bool IsHannitiKyuuka2 => SyukkinKubun2 == AttendanceClassification.半日振休 || SyukkinKubun2 == AttendanceClassification.半日有給;

        /// <summary>
        /// 出勤区分２が半日無給になるか否か
        /// </summary>
        public bool IsMukyuuHaldDay => new[]{
                                            AttendanceClassification.非常勤休暇,
                                            AttendanceClassification.出産休業,
                                            AttendanceClassification.業務上傷病休業,
                                            AttendanceClassification.介護休業,
                                            AttendanceClassification.欠勤
                                            }
                                            .Contains(SyukkinKubun2);
    }

    /// <summary>
    /// 出退勤時間のセットを保持するクラス
    /// </summary>
    public class Syuttaikin
    {
        // 0:00のTimeOnly定数
        private static readonly TimeOnly zeroHour = new(0, 0, 0);

        // 24:00の文字列定数
        private static readonly string twentyFourHour = "24:00";

        // 出勤時刻
        private TimeOnly? _syukkin;

        // 退出時刻
        private TimeOnly? _taisyutsu;

        // 出勤時刻（HHmm形式)
        private string _syukkinStr;

        // 退出時刻（HHmm形式）
        private string _taisyutsuStr;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="syukkin">出勤時刻</param>
        /// <param name="taisyutsu">退出時刻</param>
        public Syuttaikin(TimeOnly? syukkin, TimeOnly? taisyutsu)
        {
            Syukkin = syukkin;
            Taisyutsu = taisyutsu;

            _syukkinStr = syukkin?.ToStrByHHmmNoColon() ?? string.Empty;
            _taisyutsuStr = Replace0To24NoColonStr(taisyutsu);
        }

        /// <summary>
        /// 出勤時刻
        /// </summary>
        public TimeOnly? Syukkin
        {
            get => _syukkin;
            set
            {
                _syukkin = value;
                _syukkinStr = value?.ToStrByHHmmNoColon() ?? string.Empty;
            }
        }

        /// <summary>
        /// 退出時刻
        /// </summary>
        public TimeOnly? Taisyutsu
        {
            get => _taisyutsu;
            set
            {
                _taisyutsu = value;
                _taisyutsuStr = Replace0To24NoColonStr(value);
            }
        }

        /// <summary>
        /// 出勤時刻（HHmm形式）
        /// </summary>
        public string SyukkinStr => _syukkinStr;

        /// <summary>
        /// 退出時刻（HHmm形式）
        /// </summary>
        public string TaisyutsuStr => _taisyutsuStr;

        /// <summary>
        /// 夜間時間帯の作業時間
        /// </summary>
        public int YakanTime => TimeCalculator.GetIncludeTimeWithout休憩(_syukkinStr, _taisyutsuStr, Time.夜間作業);

        ///// <summary>
        ///// 早朝時間帯の作業時間
        ///// </summary>
        //public int SouchouTime => TimeCalculator.GetIncludeTimeWithout休憩(_syukkinStr, _taisyutsuStr, Time.早朝作業);

        /// <summary>
        /// 深夜時間帯の作業時間
        /// </summary>
        public int ShinyaTime => TimeCalculator.GetIncludeTimeWithout休憩(_syukkinStr, _taisyutsuStr, Time.深夜作業);

        /// <summary>
        /// 夜間作業時間 + 深夜作業時間
        /// </summary>
        public int YakanShinyaTime => YakanTime + ShinyaTime;

        /// <summary>
        /// TimeOnlyの24:00を文字列の0000(HHmm形式)に変換する
        /// </summary>
        /// <param name="time">変換対象時刻</param>
        /// <returns>変換後時刻（HHmm形式の文字列）</returns>
        public static string Replace0To24NoColonStr(TimeOnly? time)
        {
            return ReplaceTo24WithColonStr(time).Replace(":", string.Empty);
        }

        /// <summary>
        /// TimeOnlyの24:00を文字列の00:00(HH:mm形式)に変換する
        /// </summary>
        /// <param name="time">変換対象時刻</param>
        /// <returns>変換後時刻（HH:mm形式の文字列）</returns>
        public static string ReplaceTo24WithColonStr(TimeOnly? time)
        {
            if (time == null)
                return string.Empty;

            if (time == zeroHour)
                return twentyFourHour;

            return time.ToStrByHHmmOrEmpty();
        }

        /// <summary>
        /// 文字列（HHmm形式）の時刻をTimeOnlyへ変換する
        /// </summary>
        /// <param name="hhmm">対象の文字列時刻（HHmm形式）</param>
        /// <returns>変換後のTimeOnly型の時刻</returns>
        public static TimeOnly? ToTimeOnlyFromHHmm(string? hhmm)
        {
            if (string.IsNullOrWhiteSpace(hhmm))
                return null;

            return (TimeOnly.TryParseExact(hhmm, "HHmm", out var result))
                ? result
                : null;
        }
    }

    /// <summary>
    /// 申請情報欄のViewModel
    /// </summary>
    public class ShinseiInfoViewModel
    {
        // 伺い入力ヘッダー（伺い申請情報含む）
        private readonly UkagaiHeader _ukagaiHeader;

        // 伺い申請情報の伺い種別のリスト
        private readonly List<InquiryType> _ukagaiSyubetsu = [];

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ukagaiHeader">伺い入力ヘッダー（伺い申請情報含む）</param>
        public ShinseiInfoViewModel(UkagaiHeader ukagaiHeader)
        {
            _ukagaiHeader = ukagaiHeader;
            _ukagaiSyubetsu = _ukagaiHeader.UkagaiShinseis.Select(row => row.UkagaiSyubetsu).ToList();
        }

        /// <summary>
        /// 申請年月日
        /// </summary>
        public string ShinseiYmd => _ukagaiHeader.ShinseiYmd.YMDSlash();

        /// <summary>
        /// 承認年月日
        /// </summary>
        public string ShoninYmd => _ukagaiHeader.ShoninYmd == null ? string.Empty : ((DateOnly)_ukagaiHeader.ShoninYmd).YMDSlash();

        /// <summary>
        /// ステータス
        /// </summary>
        public string Status => _ukagaiHeader.Status.GetDisplayName() ?? string.Empty;

        /// <summary>
        /// 伺い種別コードの名称
        /// </summary>
        public string UkagaiSyubetsName => String.Join("・", _ukagaiSyubetsu.Select(x => x.GetDisplayName()));
    }
}
