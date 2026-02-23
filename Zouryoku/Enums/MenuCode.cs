using Model.Enums;
using Zouryoku.Attributes;

namespace Zouryoku.Enums
{
    /// <summary>
    /// メニューコード
    /// </summary>
    public enum MenuCode
    {
        [MenuInfo]
        None,

        [MenuInfo(
            title: "トップページ",
            url: "/Index")]
        トップページ,

        [MenuInfo(
            title: "勤務表",
            url: "/Kinmuhyo/Index")]
        勤務表,

        [MenuInfo(
            title: "出退勤一覧",
            url: "/Attendance/AttendanceList/Index")]
        出退勤一覧,

        [MenuInfo(
            title: "勤務日報確認",
            url: "/KinmuNippouKakunin/Index",
            canDisplayMobile: false)]
        勤務日報確認,

        [MenuInfo(
            title: "勤務状況確認",
            url: "/KinmuJokyoKakunin/Index",
            canDisplayMobile: false)]
        勤務状況確認,

        [MenuInfo(
            title: "労働状況報告",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.労働状況報告)]
        労働状況報告,

        [MenuInfo(
            title: "勤務日報未確定チェック",
            url: "/KinmuNippouMiKakuteiCheck/Index",
            canDisplayMobile: false)]
        勤務日報未確定チェック,

        [MenuInfo(
            title: "申請入力")]
        申請入力,

        [MenuInfo(
            title: "代理申請入力",
            kengen:
                EmployeeAuthority.指示承認者 |
                EmployeeAuthority.指示最終承認者)]
        代理申請入力,

        [MenuInfo(
            title: "申請入力一覧")]
        申請入力一覧,

        [MenuInfo(
            title: "申請確認",
            kengen:
                EmployeeAuthority.指示承認者 |
                EmployeeAuthority.指示最終承認者)]
        申請確認,

        [MenuInfo(
            title: "申請入力履歴",
            canDisplayMobile: false)]
        申請入力履歴,

        [MenuInfo(
            title: "データ出力",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.勤怠データ出力)]
        データ出力,

        [MenuInfo(
            title: "工数管理検索",
            canDisplayMobile: false)]
        工数管理検索,

        [MenuInfo(
            title: "有給・振替休暇確認",
            canDisplayMobile: false)]
        有給_振替休暇確認,

        [MenuInfo(
            title: "振替休暇取得管理",
            canDisplayMobile: false)]
        振替休暇取得管理,

        [MenuInfo(
            title: "計画有給休暇登録",
            url: "/YukyuKeikakuToroku/Index",
            canDisplayMobile: false)]
        計画有給休暇登録,

        [MenuInfo(
            title: "計画有給休暇事業部承認",
            url: "/YukyuKeikakuJigyobuShonin/Index",
            canDisplayMobile: false)]
        計画有給休暇事業部承認,

        [MenuInfo(
            title: "計画有給休暇最終承認",
            url: "/YukyuKeikakuJigyobuShonin/Index",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.指示最終承認者)]
        計画有給休暇最終承認,

        [MenuInfo(
            title: "顧客",
            url: "/KokyakuMeiKensaku/Index")]
        顧客,

        [MenuInfo(
            title: "案件",
            url: "/AnkenMeiKensaku/Index")]
        案件,

        [MenuInfo(
            title: "顔写真一覧",
            url: "/Maintenance/PhotoList/Index",
            canDisplayMobile: false)]
        顔写真一覧,

        [MenuInfo(
            title: "個人設定",
            url: "/Maintenance/Syains/Index",
            canDisplayMobile: false)]
        個人設定,

        [MenuInfo(
            title: "部門プロセス設定",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.部門プロセス設定)]
        プロセス設定,

        [MenuInfo(
            title: "代理ログイン",
            kengen: EmployeeAuthority.管理機能利用_その他)]
        代理ログイン,

        [MenuInfo(
            title: "日報確定解除",
            kengen: EmployeeAuthority.管理機能利用_その他)]
        日報確定解除,

        [MenuInfo(
            title: "社員マスタメンテナンス",
            url: "/SyainMasterMaintenanceKensaku/Index",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.管理機能利用_その他)]
        社員マスタメンテナンス,

        [MenuInfo(
            title: "部署マスタメンテナンス",
            url: "/BusyoMasterMaintenanceKensaku/Index",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.管理機能利用_その他)]
        部署マスタメンテナンス,

        [MenuInfo(
            title: "プロセス会社項目設定",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.管理機能利用_その他)]
        プロセス会社項目設定,

        [MenuInfo(
            title: "稼働日マスタメンテナンス",
            url: "/Maintenance/Operationday/Index",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.管理機能利用_その他)]
        稼働日マスタメンテナンス,

        [MenuInfo(
            title: "KINGS連携プログラム稼働状況",
            url: "Maintenance/ServiceHistory/Index",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.管理機能利用_その他)]
        KINGS連携プログラム稼働状況,

        [MenuInfo(
            title: "年次有給休暇更新画面",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.管理機能利用_人財向け)]
        年次有給休暇更新画面,

        [MenuInfo(
            title: "有給・振休管理",
            url: "Maintenance/Kintais/Index",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.有給_振替管理)]
        有給_振休管理,

        [MenuInfo(
            title: "ロールメンテナンス",
            url: "/RoleDefaultKengen/Index",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.管理機能利用_その他)]
        ロールメンテナンス,

        [MenuInfo(
            title: "アプリケーション設定",
            url: "/Maintenance/AppSettings/Index",
            canDisplayMobile: false,
            kengen: EmployeeAuthority.管理機能利用_その他)]
        アプリケーション設定,

        [MenuInfo(
            title: "改訂履歴",
            url: "RevisionHistory/Index")]
        改訂履歴,

        [MenuInfo(
            title: "ログアウト",
            url: "/SignOut")]
        ログアウト,
    }
}
