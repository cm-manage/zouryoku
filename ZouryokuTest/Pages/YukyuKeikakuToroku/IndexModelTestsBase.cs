using Microsoft.AspNetCore.Mvc;
using Model.Enums;
using Model.Extensions;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;
using Zouryoku.Pages.YukyuKeikakuToroku;
using static Model.Enums.LeavePlanStatus;
using static Model.Enums.ResponseStatus;

namespace ZouryokuTest.Pages.YukyuKeikakuToroku
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
        private const string NotNullConstraintPlaceholder = "N/A";

        /// <summary>
        /// 計画有給休暇登録画面用の <see cref="IndexModel"/> を生成し、テスト実行に必要なコンテキスト情報を設定します。
        /// </summary>
        /// <param name="loginUser">セッションに設定するログインユーザー（社員）情報。</param>
        /// <returns>ページコンテキスト、TempData、およびログイン情報が設定された <see cref="IndexModel"/> インスタンス。</returns>
        protected IndexModel CreateModel(Syain loginUser)
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options)
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
        /// IDを自動採番した新しい社員BASE情報をDBに追加して返します。
        /// このメソッドは、社員BASEと社員情報の整合性を保つために <see cref="AddNewSyain"/> から内部的に呼び出されます。
        /// </summary>
        /// <remarks>
        /// テストコードから直接呼び出すことは想定していません。社員データの追加が必要な場合は、整合性を保つために
        /// <see cref="AddNewSyain"/> を使用してください。
        /// </remarks>
        private SyainBasis AddNewSyainBasis() => db.SyainBases.AddReturn(new SyainBasis
        {
            Id = default,
            Name = default,
            Code = NotNullConstraintPlaceholder
        });

        /// <summary>
        /// IDを自動採番した新しい社員情報をDBに追加して返します。
        /// </summary>
        protected Syain AddNewSyain() => db.Syains.AddReturn(new Syain
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
            SyainBase = AddNewSyainBasis()
        });

        /// <summary>
        /// 今年の有給年度情報をDBに追加して返します。
        /// </summary>
        protected YukyuNendo AddYukyuNendoOfThisYear() => db.YukyuNendos.AddReturn(new YukyuNendo
        {
            Id = default,
            Nendo = default,
            StartDate = new DateOnly(2024, 4, 1),
            EndDate = new DateOnly(2025, 3, 31),
            IsThisYear = true,
            Updated = default
        });

        /// <summary>
        /// 今年ではない有給年度情報をDBに追加して返します。
        /// </summary>
        protected YukyuNendo AddYukyuNendoOfNotThisYear() => db.YukyuNendos.AddReturn(new YukyuNendo
        {
            Id = default,
            Nendo = default,
            StartDate = new DateOnly(2023, 4, 1),
            EndDate = new DateOnly(2024, 3, 31),
            IsThisYear = false,
            Updated = default
        });

        /// <summary>
        /// シャッフルされた <see cref="YukyuKeikakuMeisai.Ymd"/> を持つ計画有給休暇明細と計画有給休暇情報をDBに追加して返します。
        /// </summary>
        protected YukyuKeikaku AddYukyuKeikakuAndMeisaiWithShuffledYmds(LeavePlanStatus status, Syain syain, YukyuNendo yukyuNendo) =>
            db.YukyuKeikakus.AddReturn(new YukyuKeikaku
            {
                Id = default,
                YukyuNendoId = default,
                SyainBaseId = default,
                SyainBase = syain.SyainBase,
                YukyuNendo = yukyuNendo,
                Status = status,
                YukyuKeikakuMeisais =
                [
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 7), IsTokukyu = true },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 6), IsTokukyu = true },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 2), IsTokukyu = false },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 4), IsTokukyu = false },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 3), IsTokukyu = false },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 5), IsTokukyu = false },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 1), IsTokukyu = false }
                ]
            });

        /// <summary>
        /// シャッフルされた <see cref="YukyuKeikakuMeisai.Ymd"/> を持つ計画有給休暇明細と計画有給休暇情報をDBに追加して返します。
        /// </summary>
        protected YukyuKeikaku AddYukyuKeikakuAndMeisaiWithShuffledYmds(Syain syain, YukyuNendo yukyuNendo)
            => AddYukyuKeikakuAndMeisaiWithShuffledYmds(
                人財承認待ち, // テストに関与しない項目を適当な値で埋める
                syain, yukyuNendo);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Arrange用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected static IndexModel.YukyuKeikakuViewModel CreateRequest(
            uint version, params IReadOnlyList<IndexModel.Meisai> meisais) => new()
        {
            YukyuKeikakuStatus = default,
            YukyuNendoStartDate = default,
            YukyuNendoEndDate = default,
            Version = version,
            Meisais = meisais
        };

        protected static IndexModel.YukyuKeikakuViewModel CreateRequest(params IReadOnlyList<IndexModel.Meisai> meisais)
            => CreateRequest(default, meisais);

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Assert用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected static void AssertAreEqual(
            IReadOnlyList<IndexModel.Meisai> expectedMeisais, IReadOnlyList<IndexModel.Meisai> actualMeisais)
        {
            Assert.IsNotNull(actualMeisais, "Meisais プロパティが設定されていません。");
            Assert.HasCount(expectedMeisais.Count, actualMeisais, "Meisais 一覧の件数が一致しません。");
            for (int i = 0; i < expectedMeisais.Count; i++)
            {
                Assert.AreEqual(expectedMeisais[i].Id, actualMeisais[i].Id, $"{i}件目の Id が一致しません。");
                Assert.AreEqual(expectedMeisais[i].Ymd, actualMeisais[i].Ymd, $"{i}件目の Ymd が一致しません。");
                Assert.AreEqual(expectedMeisais[i].IsTokukyu, actualMeisais[i].IsTokukyu, $"{i}件目の IsTokukyu が一致しません。");
            }
        }

        protected static void AssertAreEqual(
            IReadOnlyList<YukyuKeikakuMeisai> expectedYukyuKeikakuMeisais,
            IReadOnlyList<YukyuKeikakuMeisai> actualYukyuKeikakuMeisais)
        {
            Assert.HasCount(expectedYukyuKeikakuMeisais.Count, actualYukyuKeikakuMeisais, "YukyuKeikakuMeisai 一覧の件数が一致しません。");
            for (int i = 0; i < expectedYukyuKeikakuMeisais.Count; i++)
            {
                Assert.AreEqual(
                    expectedYukyuKeikakuMeisais[i].Id, actualYukyuKeikakuMeisais[i].Id,
                    $"{i}件目の Id が一致しません。");
                Assert.AreEqual(
                    expectedYukyuKeikakuMeisais[i].Ymd, actualYukyuKeikakuMeisais[i].Ymd,
                    $"{i}件目の Ymd が一致しません。");
                Assert.AreEqual(
                    expectedYukyuKeikakuMeisais[i].IsTokukyu, actualYukyuKeikakuMeisais[i].IsTokukyu,
                    $"{i}件目の IsTokukyu が一致しません。");
            }
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
