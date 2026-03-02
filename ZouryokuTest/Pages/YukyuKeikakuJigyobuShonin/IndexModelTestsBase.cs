using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Pages.YukyuKeikakuJigyobuShonin;
using static Model.Enums.EmployeeAuthority;
using static Model.Enums.ResponseStatus;

namespace ZouryokuTest.Pages.YukyuKeikakuJigyobuShonin
{
    public abstract class IndexModelTestsBase : BaseInMemoryDbContextTest
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

        protected const int ThisYear = 2024;

        /// <summary>
        /// 計画有給休暇事業部承認画面用の <see cref="IndexModel"/> を生成し、テスト実行に必要なコンテキスト情報を設定します。
        /// </summary>
        /// <param name="loginUserSyain">セッションに設定するログインユーザー（社員）情報。</param>
        /// <returns>ページコンテキスト、TempData、およびログイン情報が設定された <see cref="IndexModel"/> インスタンス。</returns>
        protected IndexModel CreateModel(Syain loginUserSyain)
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };
            model.HttpContext.Session.Set(new LoginInfo { User = loginUserSyain });
            return model;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // データシード用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 部署マスタ情報をメモリ上で作成します。データベースへの登録は行われません。
        /// </summary>
        protected static Busyo CreateBusyo() => new Busyo
        {
            Id = default,
            Code = NotNullConstraintPlaceholder,
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

        /// <summary>
        /// 新しい社員情報と所属部署をメモリ上で作成します。データベースへの登録は行われません。
        /// </summary>
        /// <param name="busyo">
        /// 生成する <see cref="Syain"/> の所属部署。
        /// 省略時 (<c>null</c> の場合) は新規の <see cref="Busyo"/> を作成して設定します。
        /// </param>
        /// <param name="bumoncyo">
        /// true のとき部門長として作成します。
        /// 省略時は false。
        /// </param>
        /// <remarks>
        /// 返却される <see cref="Syain"/> は、<see cref="Syain.Busyo"/> プロパティに、
        /// 引数または新規作成した <see cref="Busyo"/> が設定されていますが、
        /// <see cref="Busyo.Id"/> は未設定（デフォルト値）のままです。
        /// テストコードなどでデータベースに登録する際は、社員エンティティだけでなく、関連する部署エンティティも含めて
        /// <c>AddRange</c> する（例: <c>db.AddRange(syain.Busyo, syain);</c>）必要がある点に注意してください。
        /// </remarks>
        /// <returns>作成された <see cref="Syain"/> エンティティ。</returns>
        protected static Syain CreateSyainWithBusyo(Busyo? busyo = null, bool bumoncyo = false)
        {
            var syain = new Syain
            {
                Id = default,
                Code = NotNullConstraintPlaceholder,
                Name = NotNullConstraintPlaceholder,
                KanaName = NotNullConstraintPlaceholder,
                Seibetsu = default,
                BusyoCode = NotNullConstraintPlaceholder,
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
                Jyunjyo = default,
                Retired = default,
                GyoumuTypeId = default,
                PhoneNumber = default,
                SyainBaseId = default,
                BusyoId = default,
                KintaiZokuseiId = default,
                UserRoleId = default,
                Busyo = busyo ?? CreateBusyo(),
                SyainBase = new SyainBasis
                {
                    Id = default,
                    Name = default,
                    Code = NotNullConstraintPlaceholder
                }
            };
            if (bumoncyo)
            {
                syain.Busyo.BusyoBase.Bumoncyo = syain.SyainBase;
            }
            return syain;
        }

        /// <summary>
        /// 有給年度情報をメモリ上で作成します。データベースへの登録は行われません。
        /// </summary>
        /// <param name="isThisYear">対象年度が当年度の場合は true、非当年度（昨年度として生成）の場合は false を指定します。</param>
        /// <returns>作成された <see cref="YukyuNendo"/> エンティティ。</returns>
        protected static YukyuNendo CreateYukyuNendo(bool isThisYear)
        {
            var year = isThisYear ? ThisYear : ThisYear - 1;
            return new YukyuNendo
            {
                Id = default,
                Nendo = default,
                StartDate = new DateOnly(year, 4, 1),
                EndDate = new DateOnly(year + 1, 3, 31),
                IsThisYear = isThisYear,
                Updated = default
            };
        }

        /// <summary>
        /// 指定社員の計画有給休暇をメモリ上で作成します。データベースへの登録は行われません。
        /// </summary>
        /// <param name="syain">計画有給休暇を作成する対象社員。</param>
        /// <param name="yukyuNendo">計画有給休暇を紐付ける有給年度。</param>
        /// <param name="status">計画有給休暇のステータス。</param>
        /// <returns>作成された <see cref="YukyuKeikaku"/> エンティティ。</returns>
        protected static YukyuKeikaku CreateYukyuKeikaku(Syain syain, YukyuNendo yukyuNendo, LeavePlanStatus status)
        {
            if (syain.SyainBase is null)
                throw new ArgumentNullException(
                    nameof(syain),
                    $"{nameof(syain.SyainBase)} が null です。" +
                    $"CreateSyainWithBusyo メソッドまたは手動で {nameof(syain.SyainBase)} を設定してください。");

            return new YukyuKeikaku
            {
                Id = default,
                YukyuNendoId = default,
                SyainBaseId = default,
                SyainBase = syain.SyainBase,
                YukyuNendo = yukyuNendo,
                Status = status
            };
        }

        /// <summary>
        /// ログインユーザー社員マスタ情報を作成・登録します。
        /// </summary>
        protected async Task<Syain> SeedLoginUserSyain(bool bumoncyo, bool jinzai)
        {
            Syain loginUserSyain;
            if (bumoncyo)
            {
                loginUserSyain = CreateSyainWithBusyo(bumoncyo: true);
                db.Add(loginUserSyain);
            }
            else
            {
                var bumoncyoSyain = CreateSyainWithBusyo(bumoncyo: true);
                loginUserSyain = CreateSyainWithBusyo(bumoncyoSyain.Busyo);
                db.AddRange(bumoncyoSyain, loginUserSyain);
            }
            if (bumoncyo)
            {
                loginUserSyain.Kengen |= 計画休暇承認;
            }
            if (jinzai)
            {
                loginUserSyain.Kengen |= 指示最終承認者;
            }
            await db.SaveChangesAsync();
            return loginUserSyain;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Arrange用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 更新系リクエストの作成
        /// </summary>
        protected static IndexModel.JigyoubuShoninViewModel CreateViewModelForRequest(
            bool jinzai,
            params IEnumerable<(YukyuKeikaku yukyuKeikaku, bool isChecked)> keikakus) => new IndexModel.JigyoubuShoninViewModel(
                jinzai ? IndexModel.Authority.Jinzai : IndexModel.Authority.Bumoncyo,
                [.. keikakus.Select(k => new IndexModel.Keikaku
                {
                    Id = k.yukyuKeikaku.Id,
                    IsChecked = k.isChecked,
                    // 以下のプロパティはテストに影響しない
                    YukyuKeikakuStatus = default,
                    SyainName = default!,
                    BusyoName = default!,
                    Meisais = default!
                })]);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Assert用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected static void AssertKeikakusAreEqual(
            IReadOnlyList<IndexModel.Keikaku> expected, IReadOnlyList<IndexModel.Keikaku> actual)
        {
            Assert.IsNotNull(actual, "Keikakus プロパティが設定されていません。");
            Assert.HasCount(expected.Count, actual, "Keikakus 一覧の件数が一致しません。");
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual(expected[i].Id, actual[i].Id, $"{i}件目の Id が一致しません。");
                Assert.AreEqual(expected[i].YukyuKeikakuStatus, actual[i].YukyuKeikakuStatus, $"{i}件目の Status が一致しません。");
                Assert.AreEqual(expected[i].SyainName, actual[i].SyainName, $"{i}件目の SyainName が一致しません。");
                Assert.AreEqual(expected[i].BusyoName, actual[i].BusyoName, $"{i}件目の BusyoName が一致しません。");

                Assert.IsNotNull(actual[i].Meisais, $"{i}件目の Meisais プロパティが設定されていません。");
                Assert.HasCount(IndexModel.MeisaiPerYukyuKeikaku, actual[i].Meisais, $"{i}件目の Meisais 一覧の件数が一致しません。");
                for (int j = 0; j < expected[i].Meisais.Count; j++)
                {
                    Assert.AreEqual(
                        expected[i].Meisais[j].Ymd, actual[i].Meisais[j].Ymd, $"{i},{j}件目の Ymd が一致しません。");
                    Assert.AreEqual(
                        expected[i].Meisais[j].IsTokukyu, actual[i].Meisais[j].IsTokukyu, $"{i},{j}件目の IsTokukyu が一致しません。");
                }
            }
        }

        /// <summary>
        /// <paramref name="actualResult"/> がエラーかつ期待するデータであることを検証します。
        /// </summary>
        /// <param name="expectedErrorMessage">期待するエラーメッセージ</param>
        /// <param name="actualResult">検証する <see cref="IActionResult"/></param>
        protected static void AssertRedirectError(string expectedErrorMessage, IActionResult actualResult)
        {
            var redirect = Assert.IsInstanceOfType<RedirectToPageResult>(actualResult);
            Assert.AreEqual("/ErrorMessage", redirect.PageName);
            Assert.AreEqual(expectedErrorMessage, redirect.RouteValues?["errorMessage"]);
        }

        /// <summary>
        /// <paramref name="result"/> がエラーレスポンス (<see cref="エラー"/>) かつ
        /// 期待するエラーメッセージであることを検証します。
        /// </summary>
        /// <param name="result">検証する <see cref="IActionResult"/></param>
        /// <param name="expectedMessage">期待するエラーメッセージ</param>
        protected static void AssertErrorJson(IActionResult result, string expectedMessage)
        {
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result, "JsonResult が返るべきです。");
            var responseJson = Assert.IsInstanceOfType<ResponseJson>(jsonResult.Value, "ResponseJson が返るべきです。");
            Assert.AreEqual(エラー, responseJson.Status, "ステータスが一致しません。");
            Assert.AreEqual(expectedMessage, responseJson.Message, "エラーメッセージが一致しません。");
        }

        /// <summary>
        /// <paramref name="result"/> が期待するエラーメッセージであることを検証します。
        /// </summary>
        /// <param name="result">検証する <see cref="IActionResult"/></param>
        /// <param name="expectedErrors">期待するエラーメッセージ配列</param>
        protected void AssertErrors(IActionResult result, params string[] expectedErrors)
        {
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result, "JsonResult が返るべきです。");
            var errors = GetErrors(jsonResult, string.Empty);
            Assert.IsNotNull(errors, "エラーメッセージが存在しません。");
            Assert.HasCount(1, errors, "エラーメッセージの件数が一致しません。");
            CollectionAssert.AreEqual(expectedErrors, errors, "エラーメッセージが一致しません。");
        }
    }
}
