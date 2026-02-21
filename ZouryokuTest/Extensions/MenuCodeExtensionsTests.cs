using Zouryoku.Extensions;
using static Zouryoku.Enums.DeviceType;
using static Zouryoku.Enums.MenuCode;

namespace ZouryokuTest.Extensions
{
    [TestClass]
    public class MenuCodeExtensionsTests
    {
        // ---------------------------------------------------------------------
        // GetCanDisplay Tests
        // ---------------------------------------------------------------------
        [TestMethod]
        public void GetCanDisplay_PC端末の場合_PCの表示区分を取得()
        {
            // ---------- Arrange ----------
            var menuCode = 勤務日報確認;
            var device = PC;

            // 属性から期待値を取得
            var attr = menuCode.GetMenuInfoAttribute();

            // ---------- Act ----------
            var result = 勤務日報確認.GetCanDisplay(device);

            // ---------- Assert ----------
            Assert.AreEqual(attr.CanDisplayPc, result, "PC表示可否が一致しません");
        }

        [TestMethod]
        public void GetCanDisplay_MOBILE端末の場合_MOBILEの表示区分を取得()
        {
            // ---------- Arrange ----------
            var menuCode = 勤務日報確認;
            var device = MOBILE;

            // 属性から期待値を取得
            var attr = menuCode.GetMenuInfoAttribute();

            // ---------- Act ----------
            var result = 勤務日報確認.GetCanDisplay(device);

            // ---------- Assert ----------
            Assert.AreEqual(attr.CanDisplayMobile, result, "MOBILE表示可否が一致しません");
        }
    }
}
