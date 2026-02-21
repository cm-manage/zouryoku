using Microsoft.CodeAnalysis;
using Model.Model;
using Zouryoku.Pages.AnkenMeiKensaku;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.AnkenMeiKensaku
{
    /// <summary>
    /// <see cref="IndexModel.OnPostDeleteHistoryAsync"/>のテストクラス
    /// </summary>
    [TestClass]
    public class OnPostDeleteHistoryAsyncTests : IndexModelTestBase
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

        [TestMethod]
        public async Task OnPostDeleteHistoryAsync_削除対象データに紐づく案件情報が存在しない_エラー()
        {
            // Act
            // 排他制御用のバージョンはダミー
            var response = await Model!.OnPostDeleteHistoryAsync(1, 1);

            // Assert
            AssertError(response, ErrorSelectedDataNotExists);
        }

        [TestMethod]
        public async Task OnPostDeleteHistoryAsync_削除対象のデータを削除()
        {
            // Arrange
            // 削除対象のデータが紐づく案件会社ID
            var ankenId = 1;
            db.Add(new AnkenSansyouRireki()
            {
                AnkenId = ankenId,
                SyainBaseId = LoginUserSyainBaseId,
                // 不要なNOT NULLカラムに適当に値を詰める
                SansyouTime = DateTime.MinValue,
            });
            db.Add(new Anken()
            {
                Id = ankenId,
                // 不要なNOT NULLカラムに適当に値を詰める
                KokyakuKaisyaId = 0,
                Name = string.Empty,
                SearchName = string.Empty,
            });
            db.SaveChanges();

            // 削除対象のデータのバージョンを取得
            var version = db.AnkenSansyouRirekis
                .Single(a => a.AnkenId == ankenId)
                .Version;

            // Act
            await Model!.OnPostDeleteHistoryAsync(ankenId, version);

            // Assert
            var isExist = db.AnkenSansyouRirekis
                .Any(x => x.AnkenId == ankenId);
            Assert.IsFalse(isExist);
        }

        [TestMethod]
        public async Task OnPostDeleteHistoryAsync_自分の参照履歴でないデータ_削除しない()
        {
            // Arrange
            // 削除対象のデータが紐づく案件会社ID
            var ankenId = 1;
            db.Add(new AnkenSansyouRireki()
            {
                AnkenId = ankenId,
                SyainBaseId = LoginUserSyainBaseId + 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                SansyouTime = DateTime.MinValue,
            });
            db.Add(new Anken()
            {
                Id = ankenId,
                // 不要なNOT NULLカラムに適当に値を詰める
                KokyakuKaisyaId = 0,
                Name = string.Empty,
                SearchName = string.Empty,
            });
            db.SaveChanges();

            // 削除対象のデータのバージョンを取得
            var version = db.AnkenSansyouRirekis
                .Single(a => a.AnkenId == ankenId)
                .Version;

            // Act
            await Model!.OnPostDeleteHistoryAsync(ankenId, version);

            // Assert
            var isExist = db.AnkenSansyouRirekis
                .Any(x => x.AnkenId == ankenId);
            Assert.IsTrue(isExist);
        }

        [TestMethod]
        public async Task OnPostDeleteHistoryAsync_案件IDが一致しないデータ_削除しない()
        {
            // Arrange
            // 削除対象のデータが紐づく案件会社ID
            var deletedAnkenId = 1;
            db.Add(new AnkenSansyouRireki()
            {
                AnkenId = deletedAnkenId,
                SyainBaseId = LoginUserSyainBaseId + 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                SansyouTime = DateTime.MinValue,
            });
            db.Add(new Anken()
            {
                Id = deletedAnkenId,
                // 不要なNOT NULLカラムに適当に値を詰める
                KokyakuKaisyaId = 0,
                Name = string.Empty,
                SearchName = string.Empty,
            });

            // 削除されないことを確認するためのデータ
            var ankenId = 2;
            db.Add(new AnkenSansyouRireki()
            {
                AnkenId = ankenId,
                SyainBaseId = LoginUserSyainBaseId + 1,
                // 不要なNOT NULLカラムに適当に値を詰める
                SansyouTime = DateTime.MinValue,
            });
            db.Add(new Anken()
            {
                Id = ankenId,
                // 不要なNOT NULLカラムに適当に値を詰める
                KokyakuKaisyaId = 0,
                Name = string.Empty,
                SearchName = string.Empty,
            });
            db.SaveChanges();

            // 削除対象のデータのバージョンを取得
            var version = db.AnkenSansyouRirekis
                .Single(a => a.AnkenId == deletedAnkenId)
                .Version;

            // Act
            await Model!.OnPostDeleteHistoryAsync(deletedAnkenId, version);

            // Assert
            var isExist = db.AnkenSansyouRirekis
                .Any(x => x.AnkenId == ankenId);
            Assert.IsTrue(isExist);
        }
    }
}