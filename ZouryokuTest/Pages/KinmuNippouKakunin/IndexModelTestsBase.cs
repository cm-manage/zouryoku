using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Time.Testing;
using Model.Model;
using Moq;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.KinmuNippouKakunin;
using Zouryoku.Pages.Shared;
using static Model.Enums.ResponseStatus;

namespace ZouryokuTest.Pages.KinmuNippouKakunin
{
    public abstract partial class IndexModelTestsBase : BaseInMemoryDbContextTest
    {
        /// <summary>
        /// テストデータのNOT NULL制約を満たす以外に意味を持たない文字列です。
        /// </summary>
        /// <remarks>
        /// この定数は、事業部や社員エンティティのテストデータ作成時に、
        /// テストの本質的な検証に関係しない必須項目を埋める目的でのみ使用します。
        /// テストがこの値の内容に依存しないよう、業務的な意味を持つ値に変更してはなりません。
        /// また、関連するエンティティのカラムに最大長さ制約が設定されていることを想定し、
        /// その制約に抵触しないよう「N/A」という短めの文字列を設定しています。
        /// </remarks>
        protected const string NotNullConstraintPlaceholder = "N/A";

        /// <summary>
        /// <see cref="Seed4SyainsForOrderAsync"/> の①「部署コード：1、並び順序：1、社員番号：3」の部署コード
        /// </summary>
        private const string BusyoCode1 = "1";

        /// <summary>
        /// <see cref="Seed4SyainsForOrderAsync"/> の④「部署コード：2」の部署コード
        /// </summary>
        private const string BusyoCode2 = "2";

        /// <summary>
        /// <see cref="Seed4SyainsForOrderAsync"/> の①「部署コード：1、並び順序：1、社員番号：3」の並び順序
        /// </summary>
        private const short SyainJyunjyo1 = 1;

        /// <summary>
        /// <see cref="Seed4SyainsForOrderAsync"/> の②「部署コード：1、並び順序：2、社員番号：2」の並び順序
        /// </summary>
        private const short SyainJyunjyo2 = 2;

        /// <summary>
        /// <see cref="Seed4SyainsForOrderAsync"/> の③「部署コード：1、並び順序：2、社員番号：1」の並び順序
        /// </summary>
        private const short SyainJyunjyo3 = 2;

        /// <summary>
        /// <see cref="Seed4SyainsForOrderAsync"/> の①「部署コード：1、並び順序：1、社員番号：3」の社員番号
        /// </summary>
        private const string SyainCode1 = "3";

        /// <summary>
        /// <see cref="Seed4SyainsForOrderAsync"/> の②「部署コード：1、並び順序：2、社員番号：2」の社員番号
        /// </summary>
        private const string SyainCode2 = "2";

        /// <summary>
        /// <see cref="Seed4SyainsForOrderAsync"/> の③「部署コード：1、並び順序：2、社員番号：1」の社員番号
        /// </summary>
        private const string SyainCode3 = "1";

        protected static DateTime Today => new DateTime(2025, 5, 2);

        protected static DateOnly FirstDayOfMonth => new DateOnly(2025, 5, 1);

        /// <summary>
        /// 勤務日報確認画面用の <see cref="IndexModel"/> を生成し、テスト実行に必要なコンテキスト情報を設定します。
        /// </summary>
        /// <param name="loginUser">セッションに設定するログインユーザー（社員）情報。</param>
        /// <param name="onRender"><see cref="IView.RenderAsync"/> のコールバック。</param>
        /// <returns>ページコンテキスト、TempData、およびログイン情報が設定された <see cref="IndexModel"/> インスタンス。</returns>
        protected IndexModel CreateModel(Syain loginUser, Action<IndexModel.DaysViewModel?>? onRender = null)
        {
            // ViewModel の内容を検証するため、
            // ICompositeViewEngine と IView をモック化して RenderAsync のコールバックでモデルをキャプチャできるようにする
            var viewEngineMock = new Mock<ICompositeViewEngine>();
            var viewMock = new Mock<IView>();

            // viewEngine.FindViewがviewResultを返すようにセットアップ
            viewEngineMock
                .Setup(engine => engine.FindView(It.IsAny<ActionContext>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(ViewEngineResult.Found("TestView", viewMock.Object));

            // RenderAsync が呼ばれたときに ViewData.Model（IndexModel.DaysViewModel）をキャプチャし、
            // 実際のビュー描画処理に依存せずにテスト側で ViewModel の内容を検証できるようにする
            viewMock.Setup(x => x.RenderAsync(It.IsAny<ViewContext>()))
                .Callback<ViewContext>(vc => onRender?.Invoke((IndexModel.DaysViewModel?)vc.ViewData.Model))
                .Returns(Task.CompletedTask);

            // システム日時を固定するための FakeTimeProvider
            fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));
            fakeTimeProvider.SetLocalNow(Today);

            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngineMock.Object, fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };
            model.HttpContext.Session.Set(new LoginInfo { User = loginUser });
            return model;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // データシード用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 必須文字列項目を埋める（null/空白は NotNullConstraintPlaceholder に変換）
        /// </summary>
        /// <param name="value">処理対象の文字列</param>
        /// <returns>処理後の文字列</returns>
        protected static string GetOrNotNullConstraintPlaceholderString(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? NotNullConstraintPlaceholder : value;
        }

        /// <summary>
        /// 部署マスタ情報をメモリ上で作成します。データベースへの登録は行われません。
        /// </summary>
        protected static Busyo CreateBusyo(string code = NotNullConstraintPlaceholder)
        {
            var filledCode = GetOrNotNullConstraintPlaceholderString(code);

            return new Busyo
            {
                Id = default,
                Code = filledCode,
                Name = NotNullConstraintPlaceholder,
                KanaName = NotNullConstraintPlaceholder,
                OyaCode = NotNullConstraintPlaceholder,
                StartYmd = default,
                EndYmd = default,
                Jyunjyo = default,
                KasyoCode = NotNullConstraintPlaceholder,
                KaikeiCode = NotNullConstraintPlaceholder,
                KeiriCode = default,
                IsActive = default,
                Ryakusyou = default,
                BusyoBaseId = default,
                OyaId = default,
                ShoninBusyoId = default,
                BusyoBase = new BusyoBasis
                {
                    Id = default,
                    Name = NotNullConstraintPlaceholder,
                    BumoncyoId = default
                }
            };
        }

        /// <summary>
        /// 新しい社員情報と所属部署をメモリ上で作成します。データベースへの登録は行われません。
        /// </summary>
        /// <param name="busyo">
        /// 生成する <see cref="Syain"/> の所属部署。
        /// 省略時 (<c>null</c> の場合) は新規の <see cref="Busyo"/> を作成して設定します。
        /// </param>
        /// <remarks>
        /// 返却される <see cref="Syain"/> は、<see cref="Syain.Busyo"/> プロパティに、
        /// 引数または新規作成した <see cref="Busyo"/> が設定されていますが、
        /// <see cref="Busyo.Id"/> は未設定（デフォルト値）のままです。
        /// </remarks>
        /// <returns>作成された <see cref="Syain"/> エンティティ。</returns>
        protected static Syain CreateSyainWithBusyo(
            Busyo? busyo = null,
            short jyunjyo = default,
            string code = NotNullConstraintPlaceholder)
        {
            busyo ??= CreateBusyo();
            var filledCode = GetOrNotNullConstraintPlaceholderString(code);

            return new Syain
            {
                Id = default,
                Code = filledCode,
                Name = NotNullConstraintPlaceholder,
                KanaName = NotNullConstraintPlaceholder,
                Seibetsu = default,
                BusyoCode = busyo.Code,
                SyokusyuCode = default,
                SyokusyuBunruiCode = default,
                NyuusyaYmd = default,
                StartYmd = default,
                EndYmd = default,
                Kyusyoku = default,
                SyucyoSyokui = default,
                KingsSyozoku = NotNullConstraintPlaceholder,
                KaisyaCode = default,
                IsGenkaRendou = default,
                EMail = default,
                KeitaiMail = default,
                Kengen = default,
                Jyunjyo = jyunjyo,
                Retired = default,
                GyoumuTypeId = default,
                PhoneNumber = default,
                SyainBaseId = default,
                BusyoId = default,
                KintaiZokuseiId = default,
                UserRoleId = default,
                Busyo = busyo,
                SyainBase = new SyainBasis
                {
                    Id = default,
                    Name = NotNullConstraintPlaceholder,
                    Code = filledCode
                }
            };
        }

        /// <summary>
        /// 部署ごとの並び順（並び順序の降順、社員番号の昇順）を検証するための社員マスタデータを登録します。
        /// ①「部署コード：1、並び順序：1、社員番号：3」
        /// ②「部署コード：1、並び順序：2、社員番号：2」
        /// ③「部署コード：1、並び順序：2、社員番号：1」
        /// ④「部署コード：2」
        /// </summary>
        /// <returns>登録したデータのうち、①の社員情報。</returns>
        protected async Task<Syain> Seed4SyainsForOrderAsync()
        {
            var busyo1 = CreateBusyo(BusyoCode1);
            var busyo2 = CreateBusyo(BusyoCode2);
            var syain1 = CreateSyainWithBusyo(busyo: busyo1, jyunjyo: SyainJyunjyo1, code: SyainCode1);
            var syain2 = CreateSyainWithBusyo(busyo: busyo1, jyunjyo: SyainJyunjyo2, code: SyainCode2);
            var syain3 = CreateSyainWithBusyo(busyo: busyo1, jyunjyo: SyainJyunjyo3, code: SyainCode3);
            var syain4 = CreateSyainWithBusyo(busyo: busyo2, jyunjyo: default, code: NotNullConstraintPlaceholder);
            db.AddRange(busyo1, busyo2, syain1, syain2, syain3, syain4);
            await db.SaveChangesAsync();
            return syain1;
        }

        /// <summary>
        /// 人送りの循環を検証するための社員マスタデータを登録します。
        /// ①「部署コード：1」
        /// ②「部署コード：2」
        /// </summary>
        /// <returns>登録したデータのうち、①の社員情報。</returns>
        protected async Task<Syain> Seed2SyainsForCirculateAsync()
        {
            var busyo1 = CreateBusyo(BusyoCode1);
            var busyo2 = CreateBusyo(BusyoCode2);
            var syain1 = CreateSyainWithBusyo(busyo: busyo1);
            var syain2 = CreateSyainWithBusyo(busyo: busyo2);
            db.AddRange(busyo1, busyo2, syain1, syain2);
            await db.SaveChangesAsync();
            return syain1;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Arrange用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected IndexModel.DaysQuery CreateDaysQuery(long targetSyainId, DateOnly? targetYm = null) => new()
        {
            TargetYm = targetYm ?? FirstDayOfMonth,
            TargetSyainId = targetSyainId
        };

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Assert用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected static void AssertViewModelEqual(IndexModel.DaysViewModel expected, IndexModel.DaysViewModel? actual)
        {
            Assert.IsNotNull(actual, "ViewModel が設定されていません。");
            AssertTargetSyainEqual(expected, actual);
            AssertHierarchicalBusyoNamesEqual(expected.TargetSyainHierarchicalBusyoNames, actual.TargetSyainHierarchicalBusyoNames);
            AssertDaysEqual(expected.Days, actual.Days);
        }

        /// <summary>
        /// 対象社員情報のアサートを行う。
        /// </summary>
        /// <param name="expected">期待されるビュー モデル。</param>
        /// <param name="actual">実際のビュー モデル。</param>
        private static void AssertTargetSyainEqual(IndexModel.DaysViewModel expected, IndexModel.DaysViewModel actual)
        {
            Assert.AreEqual(expected.TargetSyainName, actual.TargetSyainName, "TargetSyainName が一致しません。");
        }

        /// <summary>
        /// 階層的部所名一覧のアサートを行う。
        /// </summary>
        /// <param name="expected">期待される部所名一覧。</param>
        /// <param name="actual">実際の部所名一覧。</param>
        private static void AssertHierarchicalBusyoNamesEqual(IReadOnlyList<string> expected, IReadOnlyList<string> actual)
        {
            Assert.HasCount(expected.Count, actual, "TargetSyainHierarchicalBusyoNames の件数が一致しません。");
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i], actual[i], $"TargetSyainHierarchicalBusyoNames[{i}] が一致しません。");
            }
        }

        private static void AssertDaysEqual(IReadOnlyList<IndexModel.Day> expected, IReadOnlyList<IndexModel.Day> actual)
        {
            Assert.HasCount(expected.Count, actual, "Days 一覧の件数が一致しません。");
            for (int i = 0; i < expected.Count; i++)
            {
                var expectedDay = expected[i];
                var actualDay = actual[i];
                Assert.AreEqual(expectedDay.Date, actualDay.Date, $"{i + 1}件目の Date が一致しません。");
                Assert.AreEqual(expectedDay.IsHikadoubi, actualDay.IsHikadoubi, $"{i + 1}件目の IsHikadoubi が一致しません。");
                Assert.AreEqual(expectedDay.SyukkinKubun1, actualDay.SyukkinKubun1, $"{i + 1}件目の SyukkinKubun1 が一致しません。");
                Assert.AreEqual(expectedDay.SyukkinKubun2, actualDay.SyukkinKubun2, $"{i + 1}件目の SyukkinKubun2 が一致しません。");
                AssertSyukkinHmsEqual(expectedDay.SyukkinHms, actualDay.SyukkinHms, i, nameof(IndexModel.Day.SyukkinHms));
                AssertSyukkinHmsEqual(expectedDay.TaisyutsuHms, actualDay.TaisyutsuHms, i, nameof(IndexModel.Day.TaisyutsuHms));
                Assert.AreEqual(expectedDay.HZangyo, actualDay.HZangyo, $"{i + 1}件目の HZangyo が一致しません。");
                Assert.AreEqual(expectedDay.HWarimashi, actualDay.HWarimashi, $"{i + 1}件目の HWarimashi が一致しません。");
                Assert.AreEqual(expectedDay.HShinyaZangyo, actualDay.HShinyaZangyo, $"{i + 1}件目の HShinyaZangyo が一致しません。");
                Assert.AreEqual(expectedDay.DZangyo, actualDay.DZangyo, $"{i + 1}件目の DZangyo が一致しません。");
                Assert.AreEqual(expectedDay.DWarimashi, actualDay.DWarimashi, $"{i + 1}件目の DWarimashi が一致しません。");
                Assert.AreEqual(expectedDay.DShinyaZangyo, actualDay.DShinyaZangyo, $"{i + 1}件目の DShinyaZangyo が一致しません。");
                Assert.AreEqual(expectedDay.NJitsudou, actualDay.NJitsudou, $"{i + 1}件目の NJitsudou が一致しません。");
                Assert.AreEqual(expectedDay.NShinya, actualDay.NShinya, $"{i + 1}件目の NShinya が一致しません。");
                AssertAnkensEqual(expectedDay.Ankens, actualDay.Ankens, i);
            }
        }

        private static void AssertSyukkinHmsEqual(
            IReadOnlyList<TimeOnly> expected, IReadOnlyList<TimeOnly> actual, int index, string propertyName)
        {
            Assert.HasCount(expected.Count, actual, $"{index + 1}件目の {propertyName} 一覧の件数が一致しません。");
            for (var j = 0; j < expected.Count; j++)
            {
                Assert.AreEqual(expected[j], actual[j], $"{index + 1}件目の {propertyName}[{j}] が一致しません。");
            }
        }

        private static void AssertAnkensEqual(
            IReadOnlyList<IndexModel.DayAnken> expected, IReadOnlyList<IndexModel.DayAnken> actual, int index)
        {
            Assert.HasCount(expected.Count, actual, $"{index + 1}件目の Ankens 件数が一致しません。");
            for (int j = 0; j < expected.Count; j++)
            {
                Assert.AreEqual(expected[j].AnkenName, actual[j].AnkenName, $"{index + 1}-{j + 1}件目の AnkenName が一致しません。");
                Assert.AreEqual(expected[j].ProjectNo, actual[j].ProjectNo, $"{index + 1}-{j + 1}件目の ProjectNo が一致しません。");
                Assert.AreEqual(expected[j].JuchuuNo, actual[j].JuchuuNo, $"{index + 1}-{j + 1}件目の JuchuuNo が一致しません。");
                Assert.AreEqual(expected[j].JuchuuGyoNo, actual[j].JuchuuGyoNo, $"{index + 1}-{j + 1}件目の JuchuuGyoNo が一致しません。");
                Assert.AreEqual(expected[j].ChaYmd, actual[j].ChaYmd, $"{index + 1}-{j + 1}件目の ChaYmd が一致しません。");
            }
        }

        /// <summary>
        /// <paramref name="actualResult"/> が成功レスポンス (<see cref="正常"/>) かつ期待するデータであることを検証します。
        /// </summary>
        /// <param name="expectedSyainId">期待する社員ID</param>
        /// <param name="actualResult">検証する <see cref="IActionResult"/></param>
        protected static void AssertSuccessJson(long expectedSyainId, IActionResult actualResult)
        {
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(actualResult, "JsonResult が返るべきです。");
            var responseJson = Assert.IsInstanceOfType<ResponseJson>(jsonResult.Value, "ResponseJson が返るべきです。");
            Assert.AreEqual(正常, responseJson.Status, "ステータスが一致しません。");
            Assert.AreEqual(expectedSyainId, responseJson.Data, "データが一致しません。");
        }

        /// <summary>
        /// <paramref name="modelState"/> がエラー (<see cref="ModelStateDictionary.IsValid"/>) == false)
        /// かつ期待するデータであることを検証します。
        /// </summary>
        /// <param name="expectedSyainId">期待するエラーメッセージ</param>
        /// <param name="actualResult">検証する <see cref="ModelStateDictionary"/></param>
        protected static void AssertModelStateErrors(string expectedErrorMessage, ModelStateDictionary modelState)
        {
            var expectedErrors = new Dictionary<string, string[]>
            {
                [""] = [expectedErrorMessage]
            };
            var actualErrors = modelState.Errors();
            Assert.HasCount(expectedErrors.Count, actualErrors, "エラー件数が一致しません。");
            foreach (var expectedError in expectedErrors)
            {
                if (!actualErrors.TryGetValue(expectedError.Key, out var actualErrorMessages))
                {
                    Assert.Fail($"エラーキー '{expectedError.Key}' が見つかりません。");
                }

                CollectionAssert.AreEquivalent(
                    expectedError.Value, actualErrorMessages, $"エラーキー '{expectedError.Key}' のエラーメッセージが一致しません。");
            }
        }
    }
}
