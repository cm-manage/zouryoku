using Microsoft.AspNetCore.Mvc;
using Model.Model;
using Zouryoku.Pages.AnkenMeiKensaku;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.AnkenMeiKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnPostSelectAsync"/>のテストクラス
    /// </summary>
    [TestClass]
    public class OnPostSelectAsyncTest : IndexModelTestBase
    {
        // ======================================
        // テストの初期化処理
        // ======================================

        /// <summary>
        /// IndexModelを作成する。
        /// </summary>
        [TestInitialize]
        public void TestInit()
        {
            Model = CreateModel();
        }

        // ======================================
        // テストメソッド
        // ======================================

        // 存在性チェック
        // --------------------------------------

        [TestMethod]
        public async Task OnPostSelectAsync_選択した案件が存在しない_エラー()
        {
            // Arrange
            var ankenId = 1;
            db.Add(new Anken()
            {
                Id = ankenId,
                // 不要なNOT NULLカラムに適当に値を詰める
                KokyakuKaisyaId = 0,
                Name = string.Empty,
                SearchName = string.Empty,
            });
            db.SaveChanges();

            // Act
            // 存在しない案件IDを指定
            var response = await Model!.OnPostSelectAsync(ankenId + 1);

            // Assert
            Assert.IsInstanceOfType<JsonResult>(response);
            var jsonResult = (JsonResult)response;
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors);
            Assert.HasCount(1, errors);
            Assert.AreEqual(ErrorSelectedDataNotExists, errors[0]);
        }

        // 共通処理呼び出しの確認
        // --------------------------------------

        [TestMethod]
        public async Task OnPostSelectAsync_案件参照履歴更新の共通処理が呼ばれていること()
        {
            // Arrange
            var rirekis = new List<AnkenSansyouRireki>();
            var ankens = new List<Anken>();
            for (int i = 1; i <= 50; i++)
            {
                ankens.Add(new()
                {
                    Id = i,
                    // 不要なNOT NULLカラムに適当に値を詰める
                    KokyakuKaisyaId = 0,
                    Name = string.Empty,
                    SearchName = string.Empty,
                });
                rirekis.Add(new()
                {
                    Id = i,
                    AnkenId = i,
                    SyainBaseId = LoginUserSyainBaseId,
                    // ID == 1のデータが一番古い
                    SansyouTime = DateTime.MinValue.AddDays(i),
                });
            }
            db.AddRange(rirekis);
            db.AddRange(ankens);

            // 参照履歴に追加される案件情報
            var ankenId = 100;
            db.Add(new Anken()
            {
                Id = ankenId,
                // 不要なNOT NULLカラムに適当に値を詰める
                KokyakuKaisyaId = 0,
                Name = string.Empty,
                SearchName = string.Empty,
            });

            db.SaveChanges();

            // Act
            await Model!.OnPostSelectAsync(ankenId);

            // Assert
            // 参照履歴が追加されていること
            var isExist = db.AnkenSansyouRirekis
                .Any(x => x.AnkenId == ankenId);
            Assert.IsTrue(isExist);
            // 参照履歴テーブルの件数が50件になっていること
            var count = db.AnkenSansyouRirekis.ToList().Count;
            Assert.AreEqual(50, count);
        }
    }
}