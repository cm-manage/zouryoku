using System;
using System.ComponentModel.DataAnnotations;

namespace Model.Enums
{
    /// <summary>
    /// 社員権限
    /// </summary>
    [Flags]
    public enum EmployeeAuthority : int
    {
        [Display(Name = "None")]
        None = 0,

        [Display(Name = "労働状況報告")]
        労働状況報告 = 1 << 0,                    //      1

        [Display(Name = "労働最終警告メール送信対象者")]
        労働最終警告メール送信対象者 = 1 << 1,    //      2

        [Display(Name = "勤務日報未確定チェック")]
        勤務日報未確定チェック = 1 << 2,          //      4

        [Display(Name = "勤務日報未確定者への通知")]
        勤務日報未確定者への通知 = 1 << 3,        //      8

        [Display(Name = "出退勤一覧画面の部署選択")]
        出退勤一覧画面の部署選択 = 1 << 4,        //     16

        [Display(Name = "出退勤一覧の打刻時間修正")]
        出退勤一覧の打刻時間修正 = 1 << 5,        //     32

        [Display(Name = "出退勤一覧の打刻位置確認")]
        出退勤一覧の打刻位置確認 = 1 << 6,        //     64

        [Display(Name = "部門プロセス設定")]
        部門プロセス設定 = 1 << 7,                //    128

        [Display(Name = "PCログ出力")]
        PCログ出力 = 1 << 8,                      //    256

        [Display(Name = "勤怠データ出力")]
        勤怠データ出力 = 1 << 9,                  //    512

        [Display(Name = "管理機能利用_人財向け")]
        管理機能利用_人財向け = 1 << 10,          //   1024

        [Display(Name = "管理機能利用_その他")]
        管理機能利用_その他 = 1 << 11,            //   2048

        [Display(Name = "指示承認者")]
        指示承認者 = 1 << 12,                     //   4096

        [Display(Name = "指示最終承認者")]
        指示最終承認者 = 1 << 13,                 //   8192

        [Display(Name = "計画休暇承認")]
        計画休暇承認 = 1 << 14,                   //  16384

        [Display(Name = "労働状況報告の部署選択")]
        労働状況報告の部署選択 = 1 << 15,         //  32768

        [Display(Name = "有給・振替管理")]
        有給_振替管理 = 1 << 16,                  //  65536

        [Display(Name = "残業超過制限無効")]
        残業超過制限無効 = 1 << 17,               // 131072
    }
}
