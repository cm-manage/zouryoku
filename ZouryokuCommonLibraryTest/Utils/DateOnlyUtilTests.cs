using Model.Model;
using static Model.Enums.RefreshDayFlag;
using static ZouryokuCommonLibrary.Utils.DateOnlyUtil;

namespace ZouryokuCommonLibraryTest.Utils
{
    [TestClass]
    public class DateOnlyUtilTests : BaseInMemoryDbContextTest
    {
        /// <summary>
        /// 水曜日。
        /// </summary>
        /// <value>2026/2/18</value>
        private readonly DateOnly _baseDateWednesday = new(2026, 2, 18);

        /// <summary>
        /// 金曜日。
        /// </summary>
        /// <returns>2026/2/20</returns>
        private readonly DateOnly _baseDateFriday = new(2026, 2, 20);

        /// <summary>
        /// 土曜日。
        /// </summary>
        /// <returns>2026/2/21</returns>
        private readonly DateOnly _baseDateSaturday = new(2026, 2, 21);

        [TestMethod]
        public async Task GetNextBusinessDay_翌日以降の通常営業日_取得する()
        {
            // Arrange --------------------------

            // 基準日付
            var baseDate = _baseDateWednesday;

            // Act ------------------------------

            // 翌営業日
            var nextBusinessDay = await GetNextBusinessDayAsync(db, baseDate);

            // Assert ---------------------------

            // 基準日の翌日
            var expectedDate = baseDate.AddDays(1);
            Assert.AreEqual(expectedDate, nextBusinessDay);
        }

        [TestMethod]
        public async Task GetNextBusinessDay_翌日以降のリフレッシュデー_取得する()
        {
            // Arrange --------------------------

            // 基準日付
            var baseDate = _baseDateWednesday;

            // 非稼働日
            // 基準日付の翌日に設定する
            var hikadoubiYmd = baseDate.AddDays(1);
            db.Add(new Hikadoubi()
            {
                Ymd = hikadoubiYmd,
                RefreshDay = リフレッシュデー,
            });
            db.SaveChanges();

            // Act ------------------------------

            // 翌営業日
            var nextBusinessDay = await GetNextBusinessDayAsync(db, baseDate);

            // Assert ---------------------------

            // 基準日の翌日
            var expectedDate = baseDate.AddDays(1);
            Assert.AreEqual(expectedDate, nextBusinessDay);
        }

        [TestMethod]
        [DataRow(1, DisplayName = "翌々日が翌営業日")]
        [DataRow(2, DisplayName = "翌々々日以降が翌営業日")]
        public async Task GetNextBusinessDay_営業日_翌営業日を取得する(int hikadoubiCount)
        {
            // Arrange --------------------------

            // 基準日付
            var baseDate = _baseDateWednesday;

            // DBに追加する非稼働日エンティティのリスト
            var hikadoubis = new List<Hikadoubi>();
            for (int i = 0; i < hikadoubiCount; i++)
            {
                hikadoubis.Add(new Hikadoubi()
                {
                    Ymd = baseDate.AddDays(i + 1),
                    RefreshDay = それ以外
                });
            }
            db.AddRange(hikadoubis);
            db.SaveChanges();

            // Act ------------------------------

            // 翌営業日
            var nextBusinessDay = await GetNextBusinessDayAsync(db, baseDate);

            // Assert ---------------------------

            // 基準日付の翌日
            // 引数に応じて変化する
            var expectedBusinessDay = hikadoubiCount switch
            {
                // 翌日が非稼働日で、翌々日が営業日の場合
                1 => baseDate.AddDays(2),
                // 翌日と翌々日が非稼働日で、さらに土日が続く場合
                2 => baseDate.AddDays(5),
                _ => throw new ArgumentException("想定していないDataRowが指定されています。")
            };
            Assert.AreEqual(expectedBusinessDay, nextBusinessDay);
        }

        [TestMethod]
        public async Task GetNextBusinessDay_翌日以降の非稼働日_取得しない()
        {
            // Arrange --------------------------

            // 基準日付
            var baseDate = _baseDateWednesday;

            // 非稼働日
            // 基準日付の翌日に設定する
            var hikadoubiYmd = baseDate.AddDays(1);
            db.Add(new Hikadoubi()
            {
                Ymd = hikadoubiYmd,
                RefreshDay = それ以外,
            });
            db.SaveChanges();

            // Act ------------------------------

            // 翌営業日
            var nextBusinessDay = await GetNextBusinessDayAsync(db, baseDate);

            // Assert ---------------------------

            Assert.AreNotEqual(hikadoubiYmd, nextBusinessDay);
        }

        [TestMethod]
        public async Task GetNextBusinessDay_翌日以降の土曜日_取得しない()
        {
            // Arrange --------------------------

            // 基準日付
            var baseDate = _baseDateFriday;

            // Act ------------------------------

            // 翌営業日
            // 金曜日基準で検索
            var nextBusinessDay = await GetNextBusinessDayAsync(db, baseDate);

            // Assert ---------------------------

            // 基準日の翌日（土曜日）
            var notExpectedDate = baseDate.AddDays(1);
            Assert.AreNotEqual(notExpectedDate, nextBusinessDay);
        }

        [TestMethod]
        public async Task GetNextBusinessDay_翌日以降の日曜日_取得しない()
        {
            // Arrange --------------------------

            // 基準日付
            var baseDate = _baseDateSaturday;

            // Act ------------------------------

            // 翌営業日
            // 土曜日基準で検索
            var nextBusinessDay = await GetNextBusinessDayAsync(db, baseDate);

            // Assert ---------------------------

            // 基準日の翌日（日曜日）
            var notExpectedDate = baseDate.AddDays(1);
            Assert.AreNotEqual(notExpectedDate, nextBusinessDay);
        }

    }
}
