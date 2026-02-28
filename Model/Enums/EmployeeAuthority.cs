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

        [Display(Name = "労働状況報告（自部署）")]
        労働状況報告 = 1 << 0,                    //      1

        [Display(Name = "労働状況報告（全社）")]
        労働状況報告の部署選択 = 1 << 1,          //      2

        [Display(Name = "労働最終警告メール送信対象者")]
        労働最終警告メール送信対象者 = 1 << 2,    //      4

        [Display(Name = "勤務日報未確定チェックの部署選択・通知操作")]
        勤務日報未確定チェック = 1 << 3,          //      8

        [Display(Name = "出退勤一覧（全社）")]
        出退勤一覧画面の部署選択 = 1 << 4,        //     16

        [Display(Name = "出退勤一覧（修正・地図）")]
        出退勤一覧の打刻位置確認 = 1 << 5,        //     32

        [Display(Name = "部門プロセス設定")]
        部門プロセス設定 = 1 << 6,                //     64

        [Display(Name = "出退勤一覧（PCログ）")]
        PCログ出力 = 1 << 7,                      //    128

        [Display(Name = "勤怠データ出力")]
        勤怠データ出力 = 1 << 8,                  //    256

        [Display(Name = "年次有給休暇更新")]
        年次有給休暇更新 = 1 << 9,               //    512

        [Display(Name = "有給・振替管理")]
        有給振替管理 = 1 << 10,                   //   1024

        [Display(Name = "管理機能")]
        管理機能 = 1 << 11,                       //   2048

        [Display(Name = "申請承認者（申請代理入力）")]
        指示承認者 = 1 << 12,                     //   4096

        [Display(Name = "申請最終承認者")]
        指示最終承認者 = 1 << 13,                 //   8192

        [Display(Name = "勤怠代理入力（自部署）")]
        代理入力権限 = 1 << 14,                   //  16384

        [Display(Name = "計画休暇部門承認")]
        計画休暇承認 = 1 << 15,                  //   32768

        [Display(Name = "残業超過制限無効")]
        残業超過制限無効 = 1 << 16,               //  65536
    }
}
