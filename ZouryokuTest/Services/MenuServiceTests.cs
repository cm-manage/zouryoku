using Model.Enums;
using Model.Model;
using Zouryoku.Enums;
using Zouryoku.Models.Menu;
using Zouryoku.Services;

using static Model.Enums.EmployeeAuthority;
using static Zouryoku.Enums.MenuCode;

namespace ZouryokuTest.Services
{
    [TestClass]
    public class MenuServiceTests
    {
        /// <summary>
        /// サービスクラス
        /// </summary>
        private readonly MenuService _service = new();

        /// <summary>
        /// 親メニューの一覧
        /// </summary>
        private static readonly string[] second =
            [
                "トップページ",
                "勤怠管理",
                "申請管理",
                "データ出力",
                "休暇管理",
                "検索",
                "設定",
                "管理",
                "ヘルプ",
                "ログアウト"
            ];

        // ---------------------------------------------------------------------
        // CreateMenu Tests
        // ---------------------------------------------------------------------
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------
        [TestMethod]
        public void CreateMenu_親画面メニュー_正しく設定されているか()
        {
            // ---------- Arrange ----------
            // ログインユーザー
            var user = new Syain { Id = 10 };

            // ---------- Act ----------
            var result = _service.CreateMenu(user);

            // ---------- Assert ----------

            // 親メニューが正しく設定されているか
            Assert.IsTrue(result.Select(x => x.DisplayOwnName).SequenceEqual(second), "親メニューが正しくありません。");

        }

        [TestMethod]
        public void CreateMenu_子メニューを持たない親メニュー_正しく設定されていること()
        {
            // ---------- Arrange ----------
            // ログインユーザー
            var user = new Syain { Id = 10 };

            // ---------- Act ----------
            var result = _service.CreateMenu(user);

            // ---------- Assert ----------
            var top = result.Single(x => x.OwnMenuCode == トップページ);

            // トップメニューは子要素を設定しない
            Assert.IsFalse(top.HasChild, "子要素の設定フラグはFalseのはずです。");

            // 親メニューの設定
            Assert.AreEqual(トップページ, top.OwnMenuCode);
            // 子メニューが設定されていないこと
            Assert.IsEmpty(top.ChildMenus, "子メニューは設定されていないはずです。");
        }

        [TestMethod]
        public void CreateMenu_子メニューを持つ親メニュー_正しく設定されていること()
        {
            // ---------- Arrange ----------
            // ログインユーザー
            var user = new Syain { Id = 10 };

            // ---------- Act ----------
            var result = _service.CreateMenu(user);

            // ---------- Assert ----------
            // 勤怠管理の親メニューを取得
            var kintai = result.Single(x => x.DisplayOwnName == "勤怠管理");

            // 勤怠管理は子要素を設定する
            Assert.IsTrue(kintai.HasChild, "子要素の設定フラグはFalseのはずです。");
            Assert.HasCount(6, kintai.ChildMenus, "子要素は6件のはずです。");

            var kinmuhyou = kintai.ChildMenus.Single(x => x.MenuCode == 勤務表);

            // 子メニューに勤怠表を含んでいるか
            Assert.IsNotNull(kinmuhyou, "子メニューに勤怠表が含まれているはずです。");

            // 勤怠表の設定
            // IsModal
            Assert.IsFalse(kinmuhyou.IsOpenModal, "モーダルフラグはFalseのはずです。");

            // パラメータ
            Assert.IsNotNull(kinmuhyou.Param);
            var param = kinmuhyou.Param["SyainId"];
            Assert.AreEqual("10", kinmuhyou.Param!["SyainId"]);
        }

        // ---------------------------------------------------------------------
        // FilterMenus Tests
        // ---------------------------------------------------------------------
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------
        [TestMethod]
        public void FilterMenus_権限により子が表示できない_親も表示しない()
        {
            // ---------- Arrange ----------
            // ログインユーザー：権限無し
            var user = new Syain
            {
                Id = 10,
                Kengen = EmployeeAuthority.None,
            };

            // メニュー：子メニューあり
            var parentMenu = new ParentMenu("親", true, "");

            // 権限がないと表示されないメニューを追加
            parentMenu.AddChildMenu(申請確認, "", false);

            // ---------- Act ----------
            var result = _service.FilterMenus(
                [parentMenu],
                user,
                DeviceType.PC);

            // ---------- Assert ----------
            // 親メニューがすべて削除されていることを確認
            Assert.IsEmpty(result, "子メニューがすべて削除された場合、親メニューも削除されているはずです。");
        }

        [TestMethod]
        public void FilterMenus_表示端末により子が表示できない_親も表示しない()
        {
            // ---------- Arrange ----------
            // ログインユーザー：権限なし
            var user = new Syain
            {
                Id = 10,
                Kengen = EmployeeAuthority.None,
            };

            // メニュー：子メニューあり
            var parentMenu = new ParentMenu("親", true, "");

            // 権限がなくても表示されるがモバイルで表示できないメニューを追加
            parentMenu.AddChildMenu(申請入力履歴, "", false);

            // ---------- Act ----------
            var result = _service.FilterMenus(
                [parentMenu],
                user,
                DeviceType.MOBILE);

            // ---------- Assert ----------
            // 親メニューがすべて削除されていることを確認
            Assert.IsEmpty(result, "子メニューがすべて削除された場合、親メニューも削除されているはずです。");
        }

        [TestMethod]
        public void FilterMenus_子が表示できる_親も表示()
        {
            // ---------- Arrange ----------
            // ログインユーザー：権限あり
            var user = new Syain
            {
                Id = 10,
                Kengen = 指示承認者,
            };

            // メニュー：子メニューあり
            var parentMenu = new ParentMenu("親", true, "");

            // 権限がないと表示されないがモバイルで表示できるメニューを追加
            parentMenu.AddChildMenu(申請確認, "", false);

            // 権限がなくても表示されるがモバイルで表示できないメニューを追加
            parentMenu.AddChildMenu(申請入力履歴, "", false);

            // ---------- Act ----------
            var result = _service.FilterMenus(
                [parentMenu],
                user,
                DeviceType.MOBILE);

            // ---------- Assert ----------
            // 親メニューが残っていることを確認
            Assert.HasCount(1, result, "子メニューが表示されている場合、親メニューも表示されているはずです。");

            // 残っている子メニューが申請確認であることを確認
            var childMenu = result.Single().ChildMenus.Single();
            Assert.AreEqual(申請確認, childMenu.MenuCode, "表示されている子メニューは申請確認のはずです。");
        }

        [TestMethod]
        public void FilterMenus_権限がないログインユーザー_子メニューを持たない親を表示しない()
        {
            // ---------- Arrange ----------
            // ログインユーザー：権限なし
            var user = new Syain
            {
                Id = 10,
                Kengen = EmployeeAuthority.None,
            };

            // メニュー：子メニューなし
            // 権限がないと表示されないメニュー
            var parentMenu = new ParentMenu("親", false, "", 申請確認);

            // ---------- Act ----------
            var result = _service.FilterMenus(
                [parentMenu],
                user,
                DeviceType.PC);

            // ---------- Assert ----------
            // 親メニューがすべて削除されていることを確認
            Assert.IsEmpty(result, "親メニューは削除されているはずです。");
        }

        [TestMethod]
        public void FilterMenus_表示できない端末_子メニューを持たない親を表示しない()
        {
            // ---------- Arrange ----------
            // ログインユーザー：権限なし
            var user = new Syain
            {
                Id = 10,
                Kengen = EmployeeAuthority.None,
            };

            // メニュー：子メニューなし
            // 権限がないと表示されないメニュー
            var parentMenu = new ParentMenu("親", false, "", 申請入力履歴);

            // ---------- Act ----------
            var result = _service.FilterMenus(
                [parentMenu],
                user,
                DeviceType.MOBILE);

            // ---------- Assert ----------
            // 親メニューがすべて削除されていることを確認
            Assert.IsEmpty(result, "親メニューは削除されているはずです。");
        }

        [TestMethod]
        public void FilterMenus_表示できる端末かつ権限があるユーザー_子メニューを持たない親を表示しない()
        {
            // ---------- Arrange ----------
            // ログインユーザー：権限なし
            var user = new Syain
            {
                Id = 10,
                Kengen = 指示承認者,
            };

            // メニュー：子メニューなし
            // 権限がないと表示されないメニュー
            var parentMenu = new ParentMenu("親", false, "", 申請確認);

            // ---------- Act ----------
            var result = _service.FilterMenus(
                [parentMenu],
                user,
                DeviceType.MOBILE);

            // ---------- Assert ----------
            // 親メニューが残っていることを確認
            Assert.HasCount(1, result, "親メニューが表示されているはずです。");
        }
    }
}
