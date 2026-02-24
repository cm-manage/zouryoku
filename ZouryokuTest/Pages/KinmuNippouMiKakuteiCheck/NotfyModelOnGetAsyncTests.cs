using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model.Model;
using static Model.Enums.BusinessTripRole;
using static Model.Enums.EmployeeAuthority;
using static Zouryoku.Pages.KinmuNippouMiKakuteiCheck.NotifyModel;
using static Zouryoku.Utils.Const;
using static Zouryoku.Utils.JissekiKakuteiSimeUtil;

namespace ZouryokuTest.Pages.KinmuNippouMiKakuteiCheck
{
    [TestClass]
    public class NotfyModelOnGetAsyncTests : TestBase
    {
        // ======================================
        // ヘルパーメソッド
        // ======================================

        private void AssertViewModel(MikakuteiTsuchiRirekiViewModel viewModel, DateTime expectedSendDateTime,
            string expectedSyainName, int expectedSendCount, string? message = null)
        {
            Assert.AreEqual(expectedSendDateTime, viewModel.SendDateTime, message);
            Assert.AreEqual(expectedSyainName, viewModel.SyainName, message);
            Assert.AreEqual(expectedSendCount, viewModel.SendCount, message);
        }

        // ======================================
        // テストメソッド
        // ======================================

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

            var result = await model.OnGetAsync([]);

            // Assert
            // ----------------------------------

            Assert.IsInstanceOfType<RedirectResult>(result);
            var redirectResult = (RedirectResult)result;
            Assert.AreEqual(Url403, redirectResult.Url);
        }

        // 未確定通知履歴の取得
        // --------------------------------------

        [TestMethod]
        public async Task OnGetAsync_未確定通知履歴_全件取得している()
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

            var expectedTotalCount = 10;
            var rirekis = Enumerable
                .Range(1, expectedTotalCount)
                .Select(i => new MikakuteiTsuchiRireki()
                {
                    TuutiSousinNitizi = new(2026, 1, 31 - i, 9, 0, 0),
                    SendSyainBase = new()
                    {
                        Syains = [new()
                        {
                            Name = i.ToString(),
                            // 不要なNOT NULLカラムに値を詰める
                            Code = string.Empty,
                            KanaName = string.Empty,
                            Seibetsu = '0',
                            BusyoCode = string.Empty,
                            SyokusyuCode = 0,
                            NyuusyaYmd = DateOnly.MinValue,
                            StartYmd = DateOnly.MinValue,
                            EndYmd = DateOnly.MaxValue,
                            Kyusyoku = 0,
                            SyucyoSyokui = _7_8級,
                            KingsSyozoku = string.Empty,
                            KaisyaCode = 0,
                            IsGenkaRendou = false,
                            Kengen = None,
                            Jyunjyo = 0,
                            Retired = false,
                            KintaiZokuseiId = 0,
                            BusyoId = 0,
                            UserRoleId = 0,
                        }],
                        // 不要なNOT NULLカラムに値を詰める
                        Code = string.Empty,
                    },
                    SendMessage = i.ToString(),
                    SyainTsuchiRirekiRels = [.. Enumerable
                        .Range(0, i)
                        .Select(static i => new SyainTsuchiRirekiRel())],
                })
                .ToList();
            db.AddRange(rirekis);
            db.SaveChanges();

            // Act
            // ----------------------------------

            await model.OnGetAsync([]);

            // Assert
            // ----------------------------------

            var actualViewModels = model.MikakuteiTsuchiRirekis;
            Assert.HasCount(expectedTotalCount, actualViewModels);

            DateTime expectedSendDateTime;
            string expectedSyainName;
            int expectedCount;
            for (int i = 0; i < expectedTotalCount; i++)
            {
                expectedSendDateTime = rirekis[i].TuutiSousinNitizi;
                expectedSyainName = rirekis[i].SendSyainBase.Syains.First().Name;
                expectedCount = rirekis[i].SyainTsuchiRirekiRels.Count;
                AssertViewModel(actualViewModels[i], expectedSendDateTime, expectedSyainName, expectedCount,
                    $"{i}番目のデータが一致していません。");
            }
        }

        [TestMethod]
        public async Task OnGetAsync_未確定通知履歴_送信日時の降順である()
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

            var rirekis = Enumerable
                .Range(1, 3)
                .Select(i =>
                {
                    // 順番をシャッフルするための変数
                    // これを格納したカラムは2, 3, 1の順に格納される
                    var shuffleNumber = (short)(i % 3 + 1);
                    var shuffleStr = shuffleNumber.ToString();

                    return new MikakuteiTsuchiRireki()
                    {
                        // 1日前, 2日前, 3日前の順番に格納される
                        // 参照時間の降順であれば、他カラムは2, 3, 1と並ぶ
                        TuutiSousinNitizi = new(2026, 1, 31 - i),
                        // 他カラムでソートされていないことを確認するためにすべてシャッフルする
                        Id = shuffleNumber,
                        JissekiKakuteiSimebiId = shuffleNumber,
                        SendSyainBase = new()
                        {
                            Id = shuffleNumber,
                            Code = shuffleStr,
                            Syains = [new()
                            {
                                Id = shuffleNumber,
                                Code = shuffleStr,
                                Name = shuffleStr,
                                StartYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                                EndYmd = DateOnly.MaxValue.AddDays(-shuffleNumber),
                                Retired = false,
                                KanaName = shuffleStr,
                                Seibetsu = shuffleStr.ToCharArray()[0],
                                BusyoCode = shuffleStr,
                                SyokusyuCode = shuffleNumber,
                                NyuusyaYmd = DateOnly.MinValue.AddDays(shuffleNumber),
                                Kyusyoku = shuffleNumber,
                                SyucyoSyokui = _7_8級,
                                KingsSyozoku = shuffleStr,
                                KaisyaCode = shuffleNumber,
                                IsGenkaRendou = false,
                                Kengen = None,
                                Jyunjyo = shuffleNumber,
                                UserRoleId = shuffleNumber,
                                EMail = shuffleStr,
                                KeitaiMail = shuffleStr,
                                GyoumuTypeId = shuffleNumber,
                                PhoneNumber = shuffleStr,
                                BusyoId = shuffleNumber,
                                KintaiZokuseiId = shuffleNumber,
                            }],
                        },
                        SendMessage = shuffleStr,
                        SyainTsuchiRirekiRels = [
                            new()
                            {
                                Id = shuffleNumber,
                                TsuchiSyainBaseId = shuffleNumber,
                            }
                        ],
                    };
                })
                .ToList();
            db.AddRange(rirekis);
            db.SaveChanges();

            // Act
            // ----------------------------------

            await model.OnGetAsync([]);

            // Assert
            // ----------------------------------

            var expectedSyainNames = new List<string>() { "2", "3", "1" };
            var actualSyainNames = model.MikakuteiTsuchiRirekis.Select(vm => vm.SyainName).ToList();
            CollectionAssert.AreEqual(expectedSyainNames, actualSyainNames);
        }

        // 送信予定メッセージの生成
        // --------------------------------------

        [TestMethod]
        [DataRow(2, 1, DisplayName = "一か月締めの場合")]
        [DataRow(2, 16, DisplayName = "中締めの場合")]
        [DataRow(3, 1, DisplayName = "月末締めの場合")]
        public async Task OnGetAsync_送信予定メッセージを生成する(int nowMonth, int nowDay)
        {
            // Arrange
            // ----------------------------------

            fakeTimeProvider.SetLocalNow(new(2026, nowMonth, nowDay));
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

            await model.OnGetAsync([]);

            // Assert
            // ----------------------------------

            // 通知対象を適切に取得しているかを検証するために、通知対象の期間を手動で設定する

            int expectedMonth = -1;
            int expectedStartDay = -1;
            int expectedEndDay = -1;
            string expectedSpanStr = "-";
            switch ((nowMonth, nowDay))
            {
                case (2, 1):
                    expectedMonth = 1;
                    expectedStartDay = 1;
                    expectedEndDay = 31;
                    expectedSpanStr = "";
                    break;
                case (2, 16):
                    expectedMonth = 2;
                    expectedStartDay = 1;
                    expectedEndDay = NakajimeDay;
                    expectedSpanStr = "前半";
                    break;
                case (3, 1):
                    expectedMonth = 2;
                    expectedStartDay = NakajimeDay + 1;
                    expectedEndDay = 28;
                    expectedSpanStr = "後半";
                    break;
            }

            var expectedSendMessage = NippouMikakuteiTsuchiMessage.Format(
                expectedMonth, expectedSpanStr, expectedStartDay, expectedEndDay);
            Assert.AreEqual(expectedSendMessage, model.SendMessage);
        }

        [TestMethod]
        public async Task OnGetAsync_GETパラメータ_取得している()
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

            // Act
            // ----------------------------------

            var expectedSyainBaseIds = new long[] { 1, 2, 3 };
            await model.OnGetAsync(expectedSyainBaseIds);

            // Assert
            // ----------------------------------

            CollectionAssert.AreEquivalent(expectedSyainBaseIds, model.ReceiveSyainBaseIds);
        }
    }
}
