using Microsoft.EntityFrameworkCore;
using Model.Enums;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.YukyuKeikakuToroku;
using ZouryokuTest.Builder;
using static Model.Enums.LeavePlanStatus;

namespace ZouryokuTest.Pages.YukyuKeikakuToroku
{
    public abstract class IndexModelTestsBase : BaseInMemoryDbContextTest
    {
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
        private SyainBasis AddNewSyainBasis() => db.Add(new SyainBasisBuilder().Build()).Entity;

        /// <summary>
        /// IDを自動採番した新しい社員情報をDBに追加して返します。
        /// </summary>
        protected Syain AddNewSyain() => db.Add(new SyainBuilder()
            .WithSyainBaseId(AddNewSyainBasis().Id)
            .Build()).Entity;

        /// <summary>
        /// 今年の有給年度情報をDBに追加して返します。
        /// </summary>
        protected YukyuNendo AddYukyuNendoOfThisYear() => db.Add(new YukyuNendoBuilder()
            .WithStartDate(new DateOnly(2024, 1, 1))
            .WithEndDate(new DateOnly(2024, 12, 31))
            .WithIsThisYear(true)
            .Build()).Entity;

        /// <summary>
        /// 今年ではない有給年度情報をDBに追加して返します。
        /// </summary>
        protected YukyuNendo AddYukyuNendoOfNotThisYear() => db.Add(new YukyuNendoBuilder()
            .WithStartDate(new DateOnly(2023, 1, 1))
            .WithEndDate(new DateOnly(2023, 12, 31))
            .WithIsThisYear(false)
            .Build()).Entity;

        /// <summary>
        /// シャッフルされた <see cref="YukyuKeikakuMeisai.Ymd"/> を持つ計画有給休暇明細と計画有給休暇情報をDBに追加して返します。
        /// </summary>
        protected YukyuKeikaku AddYukyuKeikakuAndMeisaiWithShuffledYmds(LeavePlanStatus status, Syain syain, YukyuNendo yukyuNendo) =>
            db.Add(new YukyuKeikakuBuilder()
                .WithStatus(status)
                .WithSyainBaseId(syain.SyainBase.Id)
                .WithYukyuNendoId(yukyuNendo.Id)
                .WithYukyuKeikakuMeisais(
                    // 検索結果の順序が正しいことをテストするため、適当な日付順で登録
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 7), IsTokukyu = true },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 6), IsTokukyu = true },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 2), IsTokukyu = false },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 4), IsTokukyu = false },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 3), IsTokukyu = false },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 5), IsTokukyu = false },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(2024, 11, 1), IsTokukyu = false })
                .Build()).Entity;

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

        protected static IndexModel.YukyuKeikakuViewModel CreateRequest(uint version, params IReadOnlyList<IndexModel.Meisai> meisais) => new()
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
    }
}
