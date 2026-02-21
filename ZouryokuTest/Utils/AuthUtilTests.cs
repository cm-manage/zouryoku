using Model.Enums;
using Model.Model;
using Zouryoku.Enums;
using Zouryoku.Utils;

using static Model.Enums.EmployeeAuthority;
using static Zouryoku.Enums.MenuCode;

namespace ZouryokuTest.Utils
{
    [TestClass]
    public class AuthUtilTests
    {
        // ---------------------------------------------------------------------
        // IsAuth Tests
        // ---------------------------------------------------------------------
        // -----------------------------------------------------
        // 正常系テストケース
        // -----------------------------------------------------
        [TestMethod]
        public void IsAuth_権限無しメニュー_trueを返却()
        {
            // ---------- Arrange ----------
            // 社員：権限無し
            var user = new Syain
            {
                Kengen = EmployeeAuthority.None
            };

            // ---------- Act ----------
            // 権限が不要なトップページへ遷移
            var result = AuthUtil.IsAuth(トップページ, user);

            // ---------- Assert ----------
            // Trueを返却
            Assert.IsTrue(result, "Trueになるはずです。");
        }

        [TestMethod]
        public void IsAuth_メニューの必要権限を持つ_trueを返却()
        {
            // ---------- Arrange ----------
            // 社員：労働状況報告の権限あり
            var user = new Syain
            {
                Kengen = EmployeeAuthority.労働状況報告
            };

            // ---------- Act ----------
            // 指示承認者・指示最終承認者の権限がある社員が遷移できる申請確認へ遷移
            var result = AuthUtil.IsAuth(MenuCode.労働状況報告, user);

            // ---------- Assert ----------
            // Trueを返却
            Assert.IsTrue(result, "Trueになるはずです。");
        }

        [TestMethod]
        public void IsAuth_メニューの必要権限を持たない_falseを返却()
        {
            // ---------- Arrange ----------
            // 社員：その他の権限
            var user = new Syain
            {
                Kengen = PCログ出力
            };

            // ---------- Act ----------
            // 指示承認者・指示最終承認者の権限がある社員が遷移できる申請確認へ遷移
            var result = AuthUtil.IsAuth(MenuCode.労働状況報告, user);

            // ---------- Assert ----------
            // Falseを返却
            Assert.IsFalse(result, "Falseになるはずです。");
        }

        [TestMethod]
        public void IsAuth_複数フラグの一部を持つ_trueを返却()
        {
            // ---------- Arrange ----------
            // 社員：指示承認者＋その他の権限
            var user = new Syain
            {
                Kengen = 指示承認者 | PCログ出力　
            };

            // ---------- Act ----------
            // 指示承認者・指示最終承認者の権限がある社員が遷移できる申請確認へ遷移
            var result = AuthUtil.IsAuth(申請確認, user);

            // ---------- Assert ----------
            // Trueを返却
            Assert.IsTrue(result, "Trueになるはずです。");
        }
    }
}
