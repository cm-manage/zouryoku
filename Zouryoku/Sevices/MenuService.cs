using Model.Model;
using Zouryoku.Enums;
using Zouryoku.Extensions;
using Zouryoku.Models.Menu;
using Zouryoku.Utils;

using static Zouryoku.Enums.MenuCode;

namespace Zouryoku.Sevices
{
    public class MenuService
    {
        // ---------------------------------------------
        // 定数
        // ---------------------------------------------

        // パラメータ名
        private const string ParamSyainId = "SyainId";
        private const string ParamCanAdd = "CanAdd";
        private const string ParamCanCardClick = "CanCardClick";

        // メニュータイトル
        private const string TopTitle = "トップページ";
        private const string KintaiKanriTitle = "勤怠管理";
        private const string SinseiKanriTitle = "申請管理";
        private const string DataSyuturyokuTitle = "データ出力";
        private const string KyukaKanriTitle = "休暇管理";
        private const string KensakuTitle = "検索";
        private const string SetteiTitle = "設定";
        private const string KanriTitle = "管理";
        private const string HelpTitle = "ヘルプ";
        private const string LogoutTitle = "ログアウト";

        // アイコン名
        private const string IconHouse = "fa-house";
        private const string IconClock = "fa-clock";
        private const string IconFileSignature = "fa-file-signature";
        private const string IconFileExport = "fa-file-export";
        private const string IconUmbrellaBeach = "fa-umbrella-beach";
        private const string IconMagnifyingGlass = "fa-magnifying-glass";
        private const string IconGear = "fa-gear";
        private const string IconUserShield = "fa-user-shield";
        private const string IconCircleQuestion = "fa-circle-question";
        private const string IconRightFromBracket = "fa-right-from-bracket";
        private const string IconFileAlt = "fa-file-alt";
        private const string IconListCheck = "fa-list-check";
        private const string IconFileLine = "fa-file-lines";
        private const string IconChartLine = "fa-chart-line";
        private const string IconClipboardList = "fa-clipboard-list";
        private const string IconExclamationCircle = "fa-exclamation-circle";
        private const string IconPencilAlt = "fa-pencil-alt";
        private const string IconUserClock = "fa-user-clock";
        private const string IconList = "fa-list";
        private const string IconCheckCircle = "fa-check-circle";
        private const string IconHistory = "fa-history";
        private const string IconDownload = "fa-download";
        private const string IconTasks = "fa-tasks";
        private const string IconCalendarCheck = "fa-calendar-check";
        private const string IconCalendarPlus = "fa-calendar-plus";
        private const string IconCalendarEdit = "fa-calendar-edit";
        private const string IconUserCheck = "fa-user-check";
        private const string IconUser = "fa-user";
        private const string IconBriefcase = "fa-briefcase";
        private const string IconImage = "fa-image";
        private const string IconUserCog = "fa-user-cog";
        private const string IconSlidersH = "fa-sliders-h";
        private const string IconUserLock = "fa-user-lock";
        private const string IconUnlockAlt = "fa-unlock-alt";
        private const string IconuserEdit = "fa-user-edit";
        private const string IconUsersCog = "fa-users-cog";
        private const string IconBuilding = "fa-building";
        private const string IconCogs = "fa-cogs";
        private const string IconCalendarAlt = "fa-calendar-alt";
        private const string IconSyncAlt = "fa-sync-alt";
        private const string IconCalendarDay = "fa-calendar-day";

        // ---------------------------------------------
        // パブリックメソッド
        // ---------------------------------------------

        /// <summary>
        /// メニュー一覧を作成する
        /// 使用する権限は考慮しない
        /// 
        /// メンテナンス上の注意点：
        ///  URLパラメータを設定するために必要な引数は適宜追加してください
        /// </summary>
        public List<ParentMenu> CreateMenu(Syain loginUser)
        {
            // 各親メニューの設定
            ParentMenu topParentMenu = new(TopTitle, false, IconHouse, トップページ);
            ParentMenu kintaiKanriParentMenu = new(KintaiKanriTitle, true, IconClock);
            ParentMenu sinseiKanriParentMenu = new(SinseiKanriTitle, true, IconFileSignature);
            ParentMenu dataSyuturyokuParentMenu = new(DataSyuturyokuTitle, true, IconFileExport);
            ParentMenu kyukaKanriParentMenu = new(KyukaKanriTitle, true, IconUmbrellaBeach);
            ParentMenu kensakuParentMenu = new(KensakuTitle, true, IconMagnifyingGlass);
            ParentMenu setteiParentMenu = new(SetteiTitle, true, IconGear);
            ParentMenu kanriParentMenu = new(KanriTitle, true, IconUserShield);
            ParentMenu helpParentMenu = new(HelpTitle, true, IconCircleQuestion);
            ParentMenu logoutParentMenu = new(LogoutTitle, false, IconRightFromBracket, ログアウト);

            // 親メニューごとの子メニューの設定
            // 勤怠管理
            kintaiKanriParentMenu
                .AddChildMenu(勤務表, IconFileAlt, false, CreateKinmuhyouParams(loginUser))
                .AddChildMenu(出退勤一覧, IconListCheck, false)
                .AddChildMenu(勤務日報確認, IconFileLine, false)
                .AddChildMenu(勤務状況確認, IconChartLine, false)
                .AddChildMenu(労働状況報告, IconClipboardList, false)
                .AddChildMenu(勤務日報未確定チェック, IconExclamationCircle, false);

            // 申請管理
            sinseiKanriParentMenu
                .AddChildMenu(申請入力, IconPencilAlt, false)
                .AddChildMenu(代理申請入力, IconUserClock, false)
                .AddChildMenu(申請入力一覧, IconList, false)
                .AddChildMenu(申請確認, IconCheckCircle, false)
                .AddChildMenu(申請入力履歴, IconHistory, false);

            // データ出力
            dataSyuturyokuParentMenu
                .AddChildMenu(データ出力, IconDownload, false)
                .AddChildMenu(工数管理検索, IconTasks, false);

            // 休暇管理
            kyukaKanriParentMenu
                .AddChildMenu(有給_振替休暇確認, IconCalendarCheck, false)
                .AddChildMenu(振替休暇取得管理, IconCalendarPlus, false)
                .AddChildMenu(計画有給休暇登録, IconCalendarEdit, false)
                .AddChildMenu(計画有給休暇事業部承認, IconUserCheck, false)
                .AddChildMenu(計画有給休暇最終承認, IconUserShield, false);

            // 検索
            kensakuParentMenu
                .AddChildMenu(顧客, IconUser, true, CreateKokyakuMeiKensakuParams())
                .AddChildMenu(案件, IconBriefcase, true, CreateAnkenMeiKensakuParams())
                .AddChildMenu(顔写真一覧, IconImage, false);

            // 設定
            setteiParentMenu
                .AddChildMenu(個人設定, IconUserCog, false)
                .AddChildMenu(プロセス設定, IconSlidersH, false);

            // 管理
            kanriParentMenu
                .AddChildMenu(代理ログイン, IconUserLock, false)
                .AddChildMenu(日報確定解除, IconUnlockAlt, false)
                .AddChildMenu(代理入力設定, IconuserEdit, false)
                .AddChildMenu(社員マスタメンテナンス, IconUsersCog, false)
                .AddChildMenu(部署マスタメンテナンス, IconBuilding, false)
                .AddChildMenu(プロセス会社項目設定, IconCogs, false)
                .AddChildMenu(稼働日マスタメンテナンス, IconCalendarAlt, false)
                .AddChildMenu(KINGS連携プログラム稼働状況, IconSyncAlt, false)
                .AddChildMenu(年次有給休暇更新画面, IconCalendarCheck, false)
                .AddChildMenu(有給_振休管理, IconCalendarDay, false)
                .AddChildMenu(ロールメンテナンス, IconUserShield, false)
                .AddChildMenu(アプリケーション設定, IconSlidersH, false);

            // ヘルプ
            helpParentMenu
                .AddChildMenu(改訂履歴, IconHistory, false);

            return [
                topParentMenu,
                kintaiKanriParentMenu,
                sinseiKanriParentMenu,
                dataSyuturyokuParentMenu,
                kyukaKanriParentMenu,
                kensakuParentMenu,
                setteiParentMenu,
                kanriParentMenu,
                helpParentMenu,
                logoutParentMenu
                ];
        }

        /// <summary>
        /// メニューをフィルタリングする
        /// ログインユーザーの権限およびデバイスタイプに基づいて表示可能なメニューのみを返す
        /// 
        /// メンテンナンス上の注意点：
        ///  メニューコード毎の権限設定や表示可否設定の変更はEnums/MenuCode.csで行ってください
        /// </summary>
        /// <param name="parentMenus">フィルター対象の親メニュー一覧</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <param name="deviceType">デバイスタイプ</param>
        /// <returns>フィルタリング後の親メニュー一覧</returns>
        public List<ParentMenu> FilterMenus(List<ParentMenu> parentMenus, Syain loginUser, DeviceType deviceType)
            => [.. parentMenus
                    .Select(parent => parent.ChangeChildMenus(FilterChildMenus(parent, loginUser, deviceType)))
                    .Where(parent => IsVisibleParentMenu(parent, loginUser, deviceType))];

        // ---------------------------------------------
        // プライベートメソッド
        // ---------------------------------------------
        /// <summary>
        /// 勤務表のパラメータ作成処理
        /// </summary>
        /// <param name="loginUser">ログインユーザー</param>
        /// <returns>勤務表の呼び出しパラメータ</returns>
        private static Dictionary<string, string> CreateKinmuhyouParams(Syain loginUser)
            => new()
            {
                { ParamSyainId, loginUser.Id.ToString() }
            };

        /// <summary>
        /// 顧客名検索のパラメータ作成処理
        /// </summary>
        /// <returns>顧客名検索の呼び出しパラメータ</returns>
        private static Dictionary<string, string> CreateKokyakuMeiKensakuParams()
            => new()
            {
                { ParamCanCardClick, false.ToString() }
            };

        /// <summary>
        /// 案件名検索のパラメータ作成処理
        /// </summary>
        /// <returns>案件名検索の呼び出しパラメータ</returns>
        private static Dictionary<string, string> CreateAnkenMeiKensakuParams()
            => new()
            {
                { ParamCanAdd, true.ToString() },
                { ParamCanCardClick, false.ToString() }
            };

        /// <summary>
        /// 親メニューの子メニューをフィルタリングする
        /// </summary>
        /// <param name="parentMenu">フィルタリングを行う親メニュー</param>
        /// <param name="loginUser">ログインユーザー</param>
        /// <param name="deviceType">表示するデバイスタイプ</param>
        /// <returns>
        /// 親メニューが子メニューを持つ場合、ログインユーザーの権限およびデバイスタイプに基づいて表示可能な子メニューのみを返す
        /// 親メニューが子メニューを持たない場合、空のリストを返す
        /// </returns>
        private List<ChildMenu> FilterChildMenus(ParentMenu parentMenu, Syain loginUser, DeviceType deviceType)
            => parentMenu.HasChild ? [.. parentMenu.ChildMenus.Where(childMenu => IsVisibleChildMenu(childMenu, loginUser, deviceType))] : [];

        /// <summary>
        /// メニューの親要素が表示可能かどうかを判定する
        /// 
        /// 親メニューが子メニューを持つ場合、少なくとも1つの子メニューが表示可能であれば親メニューも表示可能とする
        /// 親メニューが子メニューを持たない場合、自身のメニューコードに基づいて表示可能かどうかを判定する
        /// </summary>
        /// <param name="parentMenu">判定対象の親メニュー</param>
        /// <param name="loginUser">ログインユーザー</param>
        /// <param name="deviceType">表示するデバイスタイプ</param>
        /// <returns>true:表示可能 false:表示不可</returns>
        private bool IsVisibleParentMenu(ParentMenu parentMenu, Syain loginUser, DeviceType deviceType)
            => parentMenu.HasChild ?
                parentMenu.ChildMenus.Any(child => IsVisibleChildMenu(child, loginUser, deviceType)) :
                parentMenu.OwnMenuCode.HasValue
                    && AuthUtil.IsAuth(parentMenu.OwnMenuCode.Value, loginUser)
                    && parentMenu.OwnMenuCode.Value.GetCanDisplay(deviceType);

        /// <summary>
        /// メニューの子要素が表示可能かどうかを判定する
        /// </summary>
        /// <param name="childMenu">判定対象の子メニュー</param>
        /// <param name="loginUser">ログインユーザー</param>
        /// <param name="deviceType">表示するデバイスタイプ</param>
        /// <returns>true:表示可能 false:表示不可</returns>
        private bool IsVisibleChildMenu(ChildMenu childMenu, Syain loginUser, DeviceType deviceType)
            => AuthUtil.IsAuth(childMenu.MenuCode, loginUser)
                && childMenu.MenuCode.GetCanDisplay(deviceType);
    }
}
