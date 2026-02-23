using CommonLibrary.Extensions;
using NPOI.Util;
using Zouryoku.Enums;
using Zouryoku.Extensions;
using Zouryoku.Models.Menu;

namespace ZouryokuTest.Models
{
    [TestClass]
    public class ParentMenuTests
    {
        // ---------------------------------------------------------------------
        // DisplayOwnName Tests
        // ---------------------------------------------------------------------
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------
        [TestMethod]
        public void DisplayOwnName_メニューコードの設定なし_設定した表示名を返却()
        {
            // ---------- Arrange ----------
            // メニューコードを設定しない
            var parentMenu = new ParentMenu("テスト", false, "");

            // ---------- Act ----------
            // ---------- Assert ----------
            Assert.AreEqual("テスト", parentMenu.DisplayOwnName, "表示名が一致しません");

        }

        [TestMethod]
        public void DisplayOwnName_メニューコードの設定あり_設定したメニューの表示名を返却()
        {
            // ---------- Arrange ----------
            // メニューコードを設定する
            var parentMenu = new ParentMenu("テスト", false, "", MenuCode.トップページ);

            // ---------- Act ----------
            // ---------- Assert ----------
            Assert.AreEqual(MenuCode.トップページ.GetDisplayName(), parentMenu.DisplayOwnName, "表示名が一致しません");

        }

        // ---------------------------------------------------------------------
        // OwnUrl Tests
        // ---------------------------------------------------------------------
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------

        [TestMethod]
        public void OwnUrl_メニューコードの設定なし_デフォルトのURLを返却()
        {
            // ---------- Arrange ----------
            // メニューコードを設定しない
            var parentMenu = new ParentMenu("テスト", false, "");

            // ---------- Act ----------
            // ---------- Assert ----------
            Assert.AreEqual("/", parentMenu.OwnUrl, "URLが一致しません");
        }

        [TestMethod]
        public void OwnUrl_メニューコードの設定あり_設定したメニューのURLを返却()
        {
            // ---------- Arrange ----------
            // メニューコードを設定する
            var parentMenu = new ParentMenu("テスト", false, "", MenuCode.案件);

            // ---------- Act ----------
            // ---------- Assert ----------
            Assert.AreEqual(MenuCode.案件.GetUrl(), parentMenu.OwnUrl, "URLが一致しません");

        }

        // ---------------------------------------------------------------------
        // AddChildMenu Tests
        // ---------------------------------------------------------------------
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------
        [TestMethod]
        public void AddChildMenu_子メニューを追加_子メニューが追加された自分自身を返却()
        {
            // ---------- Arrange ----------
            var parent = new ParentMenu("親", true, "icon");
            var parentCopy = parent.Copy();

            // ---------- Act ----------
            var result = parentCopy.AddChildMenu(MenuCode.勤務表, "child-icon", false);

            // ---------- Assert ----------
            // ChildMenus以外は同じであること
            Assert.AreEqual(parent.DisplayOwnName, result.DisplayOwnName);
            Assert.AreEqual(parent.HasChild, result.HasChild);
            Assert.AreEqual(parent.OwnIconClass, result.OwnIconClass);
            Assert.AreEqual(parent.OwnMenuCode, result.OwnMenuCode);
            Assert.AreEqual(parent.OwnUrl, result.OwnUrl);

            // ChildMenusは追加された子メニューが含まれていること
            Assert.HasCount(1, result.ChildMenus);
            Assert.AreEqual(MenuCode.勤務表, result.ChildMenus[0].MenuCode);
            Assert.AreEqual("child-icon", result.ChildMenus[0].IconClass);
        }

        // ---------------------------------------------------------------------
        // ChangeChildMenus Tests
        // ---------------------------------------------------------------------
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------
        [TestMethod]
        public void ChangeChildMenus_子メニューを設定_子メニューが変更された自分自身を返却()
        {
            // ---------- Arrange ----------
            var parent = new ParentMenu("親", true, "icon");

            // あらかじめ親メニューに勤務表を設定
            parent.AddChildMenu(MenuCode.勤務表, "child-icon", false);
            var parentCopy = parent.Copy();

            // 変更後の子メニュー
            var changeChildMenu = new ChildMenu(MenuCode.申請確認, "new-icon", true);

            // ---------- Act ----------
            var result = parentCopy.ChangeChildMenus([changeChildMenu]);

            // ---------- Assert ----------
            // ChildMenus以外は同じであること
            Assert.AreEqual(parent.DisplayOwnName, result.DisplayOwnName);
            Assert.AreEqual(parent.HasChild, result.HasChild);
            Assert.AreEqual(parent.OwnIconClass, result.OwnIconClass);
            Assert.AreEqual(parent.OwnMenuCode, result.OwnMenuCode);
            Assert.AreEqual(parent.OwnUrl, result.OwnUrl);

            // ChildMenusは変更された子メニューが含まれていること
            Assert.HasCount(1, result.ChildMenus);
            Assert.AreEqual(MenuCode.申請確認, result.ChildMenus[0].MenuCode);
            Assert.AreEqual("new-icon", result.ChildMenus[0].IconClass);
        }
    }
}
