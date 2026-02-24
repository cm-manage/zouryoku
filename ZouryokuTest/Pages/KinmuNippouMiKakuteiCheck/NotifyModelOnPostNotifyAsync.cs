using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model.Model;
using static Model.Enums.FunctionalClassification;
using static Zouryoku.Pages.KinmuNippouMiKakuteiCheck.NotifyModel;

namespace ZouryokuTest.Pages.KinmuNippouMiKakuteiCheck
{
    [TestClass]
    public class NotifyModelOnPostNotifyAsync : TestBase
    {
        // アクセス認可
        // --------------------------------------

        [TestMethod]
        [DataRow(10, DisplayName = "代表値: 実績締め日前日以前")]
        [DataRow(15, DisplayName = "境界値: 実績締め日")]
        [DataRow(19, DisplayName = "境界値: 確定期限の翌営業日の翌日")]
        [DataRow(24, DisplayName = "代表値: 確定期限の翌営業日の翌々日以降")]
        public async Task OnGetAsync_通知可能期間でない_403ページへのリダイレクトを返却する(int nowDay)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 2, nowDay));
            var model = CreateNotifyModel();

            db.AddRange([
                new JissekiKakuteiSimebi()
                {
                    KakuteiKigenYmd = new(2026, 2, 3),
                },
                new JissekiKakuteiSimebi()
                {
                    KakuteiKigenYmd = new(2026, 2, 17),
                },
                new JissekiKakuteiSimebi(){
                    KakuteiKigenYmd = new(2026, 3, 3),
                }
            ]);
            db.SaveChanges();

            // Act
            // ----------------------------------

            var result = await model.OnPostNotifyAsync();

            // Assert
            // ----------------------------------

            Assert.IsInstanceOfType<RedirectResult>(result);
            var redirectResult = (RedirectResult)result;
            Assert.AreEqual(Url403, redirectResult.Url);
        }

        // データの登録
        // --------------------------------------

        [TestMethod]
        public async Task OnGetAsync_送信内容テーブル_データを登録する()
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 2, 16));
            var model = CreateNotifyModel();

            // 認可を通るために確定期限を登録しておく
            db.AddRange([
                new JissekiKakuteiSimebi()
                {
                    KakuteiKigenYmd = new(2026, 2, 3),
                },
                new JissekiKakuteiSimebi()
                {
                    KakuteiKigenYmd = new(2026, 2, 17),
                },
                new JissekiKakuteiSimebi(){
                    KakuteiKigenYmd = new(2026, 3, 3),
                }
            ]);
            db.SaveChanges();

            // 登録前の件数
            var beforeCount = db.MessageContents.Count();

            // 通知送信対象の社員BaseIDの配列を設定
            model.ReceiveSyainBaseIds = [1];

            // Act
            // ----------------------------------

            await model.OnPostNotifyAsync();

            // Assert
            // ----------------------------------

            // 1件増えていることを確認
            var afterCount = db.MessageContents.Count();
            Assert.AreEqual(beforeCount + 1, afterCount);

            // 登録データの正当性を確認
            var addedMessageContent = db.MessageContents.Single();
            Assert.AreEqual(LoginUser.Id, addedMessageContent.SyainId);
            Assert.AreEqual(model.SendMessage, addedMessageContent.Content);
            Assert.AreEqual(未確定通知, addedMessageContent.FunctionType);
        }

        [TestMethod]
        public async Task OnGetAsync_未確定通知履歴テーブル_データを登録する()
        {
            // Arrange
            // ----------------------------------

            var now = new DateTime(2026, 2, 16, 9, 0, 0);
            fakeTimeProvider.SetLocalNow(now);
            var model = CreateNotifyModel();
            var expectedJissekiKakuteiSimebiId = 999;

            // 認可を通るために確定期限を登録しておく
            // 通知対象の期間のIDも検証するため、IDを明示する
            db.AddRange([
                new JissekiKakuteiSimebi()
                {
                    Id = expectedJissekiKakuteiSimebiId - 1,
                    KakuteiKigenYmd = new(2026, 2, 3),
                },
                new JissekiKakuteiSimebi()
                {
                    Id = expectedJissekiKakuteiSimebiId,
                    KakuteiKigenYmd = new(2026, 2, 17),
                },
                new JissekiKakuteiSimebi(){
                    Id = expectedJissekiKakuteiSimebiId + 1,
                    KakuteiKigenYmd = new(2026, 3, 3),
                }
            ]);
            db.SaveChanges();

            // 登録前の件数
            var beforeCount = db.MikakuteiTsuchiRirekis.Count();

            // 通知送信対象の社員BaseIDの配列を設定
            model.ReceiveSyainBaseIds = [1];

            // Act
            // ----------------------------------

            await model.OnPostNotifyAsync();

            // Assert
            // ----------------------------------

            // 1件増えていることを確認
            var afterCount = db.MikakuteiTsuchiRirekis.Count();
            Assert.AreEqual(beforeCount + 1, afterCount);

            // 登録データの正当性を確認
            var addedRireki = db.MikakuteiTsuchiRirekis.Single();
            Assert.AreEqual(LoginUser.SyainBaseId, addedRireki.SendSyainBaseId);
            Assert.AreEqual(expectedJissekiKakuteiSimebiId, addedRireki.JissekiKakuteiSimebiId);
            Assert.AreEqual(now, addedRireki.TuutiSousinNitizi);
            Assert.AreEqual(model.SendMessage, addedRireki.SendMessage);
        }

        [TestMethod]
        public async Task OnGetAsync_社員_未確定通知履歴中間テーブル_データを登録する()
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 2, 16));
            var model = CreateNotifyModel();

            // 認可を通るために確定期限を登録しておく
            db.AddRange([
                new JissekiKakuteiSimebi()
                {
                    KakuteiKigenYmd = new(2026, 2, 3),
                },
                new JissekiKakuteiSimebi()
                {
                    KakuteiKigenYmd = new(2026, 2, 17),
                },
                new JissekiKakuteiSimebi(){
                    KakuteiKigenYmd = new(2026, 3, 3),
                }
            ]);
            db.SaveChanges();

            // 登録前の件数
            var beforeCount = db.SyainTsuchiRirekiRels.Count();

            // 通知送信対象の社員BaseIDの配列を設定
            var expectedCount = 3;
            model.ReceiveSyainBaseIds = [.. Enumerable
                .Range(1, expectedCount)
                .Select(i => (long)i)];

            // Act
            // ----------------------------------

            await model.OnPostNotifyAsync();

            // Assert
            // ----------------------------------

            // 3件増えていることを確認
            var afterCount = db.SyainTsuchiRirekiRels.Count();
            Assert.AreEqual(beforeCount + expectedCount, afterCount);

            // 未確定通知履歴テーブルに追加された1件のIDを取得
            var rirekiId = db.MikakuteiTsuchiRirekis.Single().Id;

            // 登録データの正当性を確認
            var addedRels = db.SyainTsuchiRirekiRels.ToList();
            SyainTsuchiRirekiRel? addedRel;
            foreach (var syainBaseId in model.ReceiveSyainBaseIds)
            {
                addedRel = db.SyainTsuchiRirekiRels
                    .SingleOrDefault(st => st.TsuchiSyainBaseId == syainBaseId);
                Assert.IsNotNull(addedRel, $"社員BaseIDが{syainBaseId}のデータが存在しません。");
                Assert.AreEqual(rirekiId, addedRel.MikakuteiTsuchiRirekiId,
                    $"社員BaseIDが{syainBaseId}のデータの未確定通知履歴IDが追加されたデータのものではありません。");
            }
        }

        // 古いデータの削除
        // --------------------------------------

        [TestMethod]
        [DataRow(0, DisplayName = "限界値: 登録後の件数が1件")]
        [DataRow(3, DisplayName = "代表値: 登録後の件数")]
        [DataRow(MaxMikakuteiTsuchiRirekiCount - 1, DisplayName = "境界値: 登録後の件数が上限")]
        public async Task OnGetAsync_登録後の通知履歴データが上限以下_データを削除しない(int beforeCount)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 2, 16));
            var model = CreateNotifyModel();

            // 認可を通るために確定期限を登録しておく
            db.AddRange([
                new JissekiKakuteiSimebi()
                {
                    KakuteiKigenYmd = new(2026, 2, 3),
                },
                new JissekiKakuteiSimebi()
                {
                    KakuteiKigenYmd = new(2026, 2, 17),
                },
                new JissekiKakuteiSimebi(){
                    KakuteiKigenYmd = new(2026, 3, 3),
                }
            ]);
            // 未確定通知履歴テーブルと中間テーブルにデータを作成しておく
            db.AddRange(Enumerable
                .Range(0, beforeCount)
                .Select(i => new MikakuteiTsuchiRireki()
                {
                    Id = i + 1,
                    // ログインユーザの社員BaseIDに関係ないことを確かめるために設定
                    SendSyainBaseId = LoginUser.SyainBaseId + i,
                    JissekiKakuteiSimebiId = i + 1,
                    TuutiSousinNitizi = DateTime.MinValue.AddDays(i),
                    SendMessage = string.Empty,
                    // 各データに対して中間テーブルに i + 1 件登録しておく
                    // ID = k のデータには k 件の中間テーブルデータが紐づく
                    SyainTsuchiRirekiRels = [.. Enumerable
                        .Range(1, i + 1)
                        .Select(j => new SyainTsuchiRirekiRel() { TsuchiSyainBaseId = i })],
                })
                .ToList());
            db.SaveChanges();

            // Act
            // ----------------------------------

            await model.OnPostNotifyAsync();

            // Assert
            // ----------------------------------

            // 未確定通知履歴テーブルの件数が増えていることを確認
            var afterCount = db.MikakuteiTsuchiRirekis.Count();
            Assert.AreEqual(beforeCount + 1, afterCount);

            // どのデータも削除されていないことを確認
            MikakuteiTsuchiRireki? rireki;
            List<SyainTsuchiRirekiRel> rels;
            for (int i = 1; i <= beforeCount; i++)
            {
                rireki = db.MikakuteiTsuchiRirekis
                    .SingleOrDefault(r => r.Id == i);
                Assert.IsNotNull(rireki, $"{i}番目のデータが削除されています。");
                rels = [.. db.SyainTsuchiRirekiRels.Where(st => st.MikakuteiTsuchiRirekiId == rireki.Id)];
                Assert.HasCount(i, rels, $"{i}番目のデータの中間テーブルデータの個数が変化しています。");
            }
        }

        [TestMethod]
        [DataRow(MaxMikakuteiTsuchiRirekiCount, DisplayName = "境界値: 登録後の件数が上限 + 1件")]
        [DataRow(MaxMikakuteiTsuchiRirekiCount + 5, DisplayName = "代表値")]
        public async Task OnGetAsync_登録後の通知履歴データが上限超過_古いデータを削除する(int beforeCount)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, 2, 16));
            var model = CreateNotifyModel();

            // 認可を通るために確定期限を登録しておく
            db.AddRange([
                new JissekiKakuteiSimebi()
                {
                    KakuteiKigenYmd = new(2026, 2, 3),
                },
                new JissekiKakuteiSimebi()
                {
                    KakuteiKigenYmd = new(2026, 2, 17),
                },
                new JissekiKakuteiSimebi(){
                    KakuteiKigenYmd = new(2026, 3, 3),
                }
            ]);
            // 未確定通知履歴テーブルと中間テーブルにデータを作成しておく
            db.AddRange(Enumerable
                .Range(0, beforeCount)
                .Select(i => new MikakuteiTsuchiRireki()
                {
                    Id = i + 1,
                    // ログインユーザの社員BaseIDに関係ないことを確かめるために設定
                    SendSyainBaseId = LoginUser.SyainBaseId + i,
                    JissekiKakuteiSimebiId = i + 1,
                    // IDが大きいほど古いデータになるように設定
                    // IDがMaxMikakuteiTsuchiRirekiCount以上のデータが削除される
                    TuutiSousinNitizi = DateTime.MaxValue.AddDays(-i),
                    SendMessage = string.Empty,
                    // 各データに対して中間テーブルに i + 1 件登録しておく
                    // ID = k のデータには k 件の中間テーブルデータが紐づく
                    SyainTsuchiRirekiRels = [.. Enumerable
                        .Range(1, i + 1)
                        .Select(j => new SyainTsuchiRirekiRel() { TsuchiSyainBaseId = i })],
                })
                .ToList());
            db.SaveChanges();

            // Act
            // ----------------------------------

            await model.OnPostNotifyAsync();

            // Assert
            // ----------------------------------

            // 未確定通知履歴テーブルの件数が上限値であることを確認
            var afterCount = db.MikakuteiTsuchiRirekis.Count();
            Assert.AreEqual(MaxMikakuteiTsuchiRirekiCount, afterCount);

            // 削除されたデータが古いものであることを確認
            MikakuteiTsuchiRireki? rireki;
            List<SyainTsuchiRirekiRel> rels;
            for (int i = MaxMikakuteiTsuchiRirekiCount; i < beforeCount + 1; i++)
            {
                rireki = db.MikakuteiTsuchiRirekis
                    .FirstOrDefault(r => r.Id == i);
                Assert.IsNull(rireki, $"{i}番目のデータが削除されていません。");
                rels = [.. db.SyainTsuchiRirekiRels.Where(st => st.MikakuteiTsuchiRirekiId == i)];
                Assert.IsEmpty(rels, $"{i}番目のデータの中間テーブルのデータが残存しています。");
            }
        }
    }
}
