using Model.Enums;
using Model.Model;
using System.Collections;
using Zouryoku.Pages.YukyuKeikakuJigyobuShonin;
using static Model.Enums.LeavePlanStatus;

namespace ZouryokuTest.Pages.YukyuKeikakuJigyobuShonin
{
    /// <summary>
    /// 計画有給休暇事業部承認画面のユニットテストのヘルパーメソッド実装クラス
    /// </summary>
    public abstract partial class IndexModelOnGetTestsBase : IndexModelTestsBase
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Arrange用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 複数の部署マスタデータをメモリ上で作成します。データベースへの登録は行われません。
        /// SyainSet の作成で使用することを想定しているため、アクセス修飾子は private とします。
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// 　　├「ID：6、親ID：5、部署Baseの部門長ID：6」
        /// 　　└「ID：7、親ID：5、部署Baseの部門長ID：空」
        /// </summary>
        /// <returns>追加した部署マスタデータ</returns>
        private static BusyoHierarchy CreateBusyoHierarchy()
        {
            var busyo1 = new Busyo
            {
                Id = default,
                Code = NotNullConstraintPlaceholder,
                Name = "部署1",
                KanaName = NotNullConstraintPlaceholder,
                OyaCode = NotNullConstraintPlaceholder,
                StartYmd = default,
                EndYmd = default,
                Jyunjyo = 7,
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
                    Name = default,
                    BumoncyoId = default
                }
            };
            var busyo2 = new Busyo
            {
                Id = default,
                Code = NotNullConstraintPlaceholder,
                Name = "部署2",
                KanaName = NotNullConstraintPlaceholder,
                OyaCode = NotNullConstraintPlaceholder,
                StartYmd = default,
                EndYmd = default,
                Jyunjyo = 6,
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
                    Name = default,
                    BumoncyoId = default
                },
                Oya = busyo1
            };
            var busyo3 = new Busyo
            {
                Id = default,
                Code = NotNullConstraintPlaceholder,
                Name = "部署3",
                KanaName = NotNullConstraintPlaceholder,
                OyaCode = NotNullConstraintPlaceholder,
                StartYmd = default,
                EndYmd = default,
                Jyunjyo = 5,
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
                    Name = default,
                    BumoncyoId = default
                },
                Oya = busyo1
            };
            var busyo4 = new Busyo
            {
                Id = default,
                Code = NotNullConstraintPlaceholder,
                Name = "部署4",
                KanaName = NotNullConstraintPlaceholder,
                OyaCode = NotNullConstraintPlaceholder,
                StartYmd = default,
                EndYmd = default,
                Jyunjyo = 4,
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
                    Name = default,
                    BumoncyoId = default
                },
                Oya = busyo3
            };
            var busyo5 = new Busyo
            {
                Id = default,
                Code = NotNullConstraintPlaceholder,
                Name = "部署5",
                KanaName = NotNullConstraintPlaceholder,
                OyaCode = NotNullConstraintPlaceholder,
                StartYmd = default,
                EndYmd = default,
                Jyunjyo = 3,
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
                    Name = default,
                    BumoncyoId = default
                },
                Oya = busyo3
            };
            var busyo6 = new Busyo
            {
                Id = default,
                Code = NotNullConstraintPlaceholder,
                Name = "部署6",
                KanaName = NotNullConstraintPlaceholder,
                OyaCode = NotNullConstraintPlaceholder,
                StartYmd = default,
                EndYmd = default,
                Jyunjyo = 2,
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
                    Name = default,
                    BumoncyoId = default
                },
                Oya = busyo5
            };
            var busyo7 = new Busyo
            {
                Id = default,
                Code = NotNullConstraintPlaceholder,
                Name = "部署7",
                KanaName = NotNullConstraintPlaceholder,
                OyaCode = NotNullConstraintPlaceholder,
                StartYmd = default,
                EndYmd = default,
                Jyunjyo = 1,
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
                    Name = default,
                    BumoncyoId = default
                },
                Oya = busyo5
            };
            return new BusyoHierarchy(busyo1, busyo2, busyo3, busyo4, busyo5, busyo6, busyo7);
        }

        /// <summary>
        /// 指定の社員マスタデータに、計画有給休暇データとその明細7件を設定します。
        /// 対象年度（<paramref name="yukyuNendo"/>）のうち、指定した年（ThisYear）と月（<paramref name="month"/>）に、
        /// 日付（1 ～ 7日）で明細の日付を設定します。
        /// </summary>
        /// <param name="syain">計画有給休暇データを紐付ける社員マスタデータ。</param>
        /// <param name="yukyuNendo">対象となる有給年度マスタデータ。</param>
        /// <param name="status">登録する計画有給休暇のステータス。</param>
        /// <param name="month">明細の日付を設定する月（1～12）。</param>
        /// <returns>初期化された計画有給休暇データ</returns>
        /// <remarks>
        /// このメソッドは <paramref name="syain"/> およびその関連エンティティをメモリ上で作成・変更するのみです。
        /// データベースへの保存は行いません。必要に応じて呼び出し側で SaveChangesAsync() を実行してください。
        /// </remarks>
        protected static YukyuKeikaku InitializeSyainYukyuKeikakus(Syain syain, YukyuNendo yukyuNendo, LeavePlanStatus status, int month)
        {
            // month パラメータが 1～12 の範囲外の場合は例外を投げることで、テストデータ作成時の実装ミスを検出する
            if (month < 1 || 12 < month)
                throw new ArgumentOutOfRangeException(
                    nameof(month),
                    month,
                    $"月は1～12の範囲で指定してください。（指定値: {month}）");

            if (syain.SyainBase is null)
                throw new ArgumentNullException(
                    nameof(syain),
                    $"{nameof(syain.SyainBase)} が null です。" +
                    $"CreateSyainWithBusyo メソッドまたは手動で {nameof(syain.SyainBase)} を設定してください。");

            // 引数の値で社員データの計画有給休暇を初期化する
            var yukyuKeikaku = new YukyuKeikaku
            {
                SyainBase = syain.SyainBase,
                YukyuNendo = yukyuNendo,
                Status = status,

                // 明細の追加順はあえて日付順にせず（追加順：1, 7, 3, 4, 5, 2, 6）とする。
                // これにより、画面側やドメインロジックでの「日付によるソート処理」が、
                // 登録順に依存せず日付の昇順に並び替えられること（ソート後の期待結果：1, 2, 3, 4, 5, 6, 7）
                // を検証することを目的としている。
                // また、IsTokukyu（特休フラグ）が true / false で混在するデータを用意することで、
                // 日付ソートと併せて、特休フラグを考慮した表示・集計ロジックの挙動も合わせて確認する。
                YukyuKeikakuMeisais =
                [
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(ThisYear, month, 1), IsTokukyu = false },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(ThisYear, month, 7), IsTokukyu = false },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(ThisYear, month, 3), IsTokukyu = true },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(ThisYear, month, 4), IsTokukyu = false },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(ThisYear, month, 5), IsTokukyu = true },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(ThisYear, month, 2), IsTokukyu = false },
                    new YukyuKeikakuMeisai { Ymd = new DateOnly(ThisYear, month, 6), IsTokukyu = false }
                ]
            };
            syain.SyainBase.YukyuKeikakus = [yukyuKeikaku];
            return yukyuKeikaku;
        }

        /// <summary>
        /// 指定の社員マスタデータに、計画有給休暇データとその明細7件を設定します。
        /// 対象年度（<paramref name="yukyuNendo"/>）のうち、指定した年（ThisYear）と月（<paramref name="month"/>）に、
        /// 日付（1 ～ 7日）で明細の日付を設定します。
        /// </summary>
        /// <param name="syain">計画有給休暇データを紐付ける社員マスタデータ。</param>
        /// <param name="yukyuNendo">対象となる有給年度マスタデータ。</param>
        /// <param name="status">登録する計画有給休暇のステータス。</param>
        /// <param name="month">明細の日付を設定する月（1～12）。</param>
        /// <returns>初期化された計画有給休暇データのリスト</returns>
        /// <remarks>
        /// このメソッドは <paramref name="syain"/> およびその関連エンティティをメモリ上で作成・変更するのみです。
        /// データベースへの保存は行いません。必要に応じて呼び出し側で SaveChangesAsync() を実行してください。
        /// </remarks>
        protected static List<YukyuKeikaku> InitializeSyainYukyuKeikakus(
            params IEnumerable<(Syain syain, YukyuNendo yukyuNendo, LeavePlanStatus status, int month)> values)
        {
            var yukyuKeikakus = new List<YukyuKeikaku>();
            foreach (var (syain, yukyuNendo, status, month) in values)
            {
                var yukyuKeikaku = InitializeSyainYukyuKeikakus(syain, yukyuNendo, status, month);
                yukyuKeikakus.Add(yukyuKeikaku);
            }
            return yukyuKeikakus;
        }

        /// <summary>
        /// 社員マスタデータをメモリ上で作成します。データベースへの登録は行われません。
        /// SyainSet の作成で使用することを想定しているため、アクセス修飾子は private とします。
        /// </summary>
        /// <param name="busyo">追加する社員の所属部署</param>
        /// <param name="jyunjyo">追加する社員の順序</param>
        /// <returns>追加した社員マスタデータ</returns>
        private static Syain CreateSyain(Busyo busyo, short jyunjyo) => new Syain
        {
            Id = default,
            Code = NotNullConstraintPlaceholder,
            Name = $"{busyo.Name}社員{jyunjyo}",
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
                Name = default,
                Code = NotNullConstraintPlaceholder
            }
        };

        /// <summary>
        /// 複数の社員マスタデータをメモリ上で作成します。データベースへの登録は行われません。
        /// </summary>
        /// <returns>追加した社員マスタデータ</returns>
        protected static SyainSet CreateSyainSet()
        {
            var (busyo1, busyo2, busyo3, busyo4, busyo5, busyo6, busyo7) = CreateBusyoHierarchy();
            var syains = new SyainSet(
                CreateSyain(busyo1, 0),
                CreateSyain(busyo2, 0),
                CreateSyain(busyo3, 0),
                CreateSyain(busyo4, 0),
                CreateSyain(busyo5, 0),
                CreateSyain(busyo6, 0),
                CreateSyain(busyo7, SyainSetConst.Syain7AJyunjyo),
                CreateSyain(busyo7, SyainSetConst.Syain7BJyunjyo));
            busyo3.BusyoBase.Bumoncyo = syains.Syain3.SyainBase;
            busyo4.BusyoBase.Bumoncyo = syains.Syain4.SyainBase;
            busyo6.BusyoBase.Bumoncyo = syains.Syain6.SyainBase;
            return syains;
        }

        /// <summary>
        /// 指定の社員マスタデータに、計画有給休暇データとその明細7件を設定します。
        /// </summary>
        /// <param name="syains">計画有給休暇データを紐付ける社員マスタデータセット</param>
        /// <param name="yukyuNendo">計画有給休暇データの年度</param>
        /// <returns>追加した計画有給休暇データ</returns>
        protected static YukyuKeikakuSet InitializeSyainYukyuKeikakus(SyainSet syains, YukyuNendo yukyuNendo) => new(
            InitializeSyainYukyuKeikakus(syains.Syain1, yukyuNendo, SyainSetConst.Syain1Status, SyainSetConst.Syain1Month),
            InitializeSyainYukyuKeikakus(syains.Syain2, yukyuNendo, SyainSetConst.Syain2Status, SyainSetConst.Syain2Month),
            InitializeSyainYukyuKeikakus(syains.Syain3, yukyuNendo, SyainSetConst.Syain3Status, SyainSetConst.Syain3Month),
            InitializeSyainYukyuKeikakus(syains.Syain4, yukyuNendo, SyainSetConst.Syain4Status, SyainSetConst.Syain4Month),
            InitializeSyainYukyuKeikakus(syains.Syain5, yukyuNendo, SyainSetConst.Syain5Status, SyainSetConst.Syain5Month),
            InitializeSyainYukyuKeikakus(syains.Syain6, yukyuNendo, SyainSetConst.Syain6Status, SyainSetConst.Syain6Month),
            InitializeSyainYukyuKeikakus(syains.Syain7A, yukyuNendo, SyainSetConst.Syain7AStatus, SyainSetConst.Syain7AMonth),
            InitializeSyainYukyuKeikakus(syains.Syain7B, yukyuNendo, SyainSetConst.Syain7BStatus, SyainSetConst.Syain7BMonth));

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Assert用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 有効系アサーション（一般ユーザ、部門長）
        /// </summary>
        protected static void AssertPopulatesList(
            long expectedYukyuKeikaku3Id, long expectedYukyuKeikaku5Id, long expectedYukyuKeikaku7AId, long expectedYukyuKeikaku7BId,
            bool bumoncyo, IndexModel.JigyoubuShoninViewModel? actualViewModel)
        {
            Assert.IsNotNull(actualViewModel);

            // 特定の権限が取得されること
            var expectedAuthority = bumoncyo ? IndexModel.Authority.Bumoncyo : IndexModel.Authority.None;
            Assert.AreEqual(expectedAuthority, actualViewModel.Authority, "Authority が一致しません。");

            if (bumoncyo)
            {
                // ボタンが表示扱いとなること
                Assert.IsTrue(actualViewModel.ButtonsAreVisible, "ボタンが表示されていません。");
            }
            else
            {
                // ボタンが非表示扱いとなること
                Assert.IsFalse(actualViewModel.ButtonsAreVisible, "ボタンが表示されています。");
            }

            // 部署列が非表示扱いとなること
            Assert.IsFalse(actualViewModel.BusyoColumnIsVisible, "部署列が表示されています。");

            if (bumoncyo)
            {
                // 計画有給休暇「③、⑦」のチェックボックスが非表示扱いとなること
                // 計画有給休暇「⑤」のチェックボックスが表示扱いとなること
                Assert.IsFalse(actualViewModel.GetCheckboxIsVisible(0), "1件目にチェックボックスが表示されています。");
                Assert.IsFalse(actualViewModel.GetCheckboxIsVisible(1), "2件目にチェックボックスが表示されています。");
                Assert.IsTrue(actualViewModel.GetCheckboxIsVisible(2), "3件目にチェックボックスが表示されていません。");
                Assert.IsFalse(actualViewModel.GetCheckboxIsVisible(3), "4件目にチェックボックスが表示されています。");
            }
            else
            {
                // チェックボックスが非表示扱いとなること
                Assert.IsFalse(actualViewModel.GetCheckboxIsVisible(0), "1件目にチェックボックスが表示されています。");
                Assert.IsFalse(actualViewModel.GetCheckboxIsVisible(1), "2件目にチェックボックスが表示されています。");
                Assert.IsFalse(actualViewModel.GetCheckboxIsVisible(2), "3件目にチェックボックスが表示されています。");
                Assert.IsFalse(actualViewModel.GetCheckboxIsVisible(3), "4件目にチェックボックスが表示されています。");
            }

            // 部署マスタの順序の昇順、社員マスタの順序の降順で並ぶこと
            var expected = new[]
            {
                new IndexModel.Keikaku
                {
                     // 計画有給休暇のID・ステータスが取得されること
                    Id = expectedYukyuKeikaku7BId,
                    YukyuKeikakuStatus = SyainSetConst.Syain7BStatus,

                    // 社員マスタ「社員BaseID：計画有給休暇の社員BaseID」の社員氏名が取得されること
                    SyainName = "部署7社員2",

                    // 部署マスタ「ID：社員マスタの部署ID」の部署名称が取得されること
                    BusyoName = "部署7",

                    // 計画有給休暇明細「計画有給ID：計画有給休暇のID」の計画有給年月日・特休フラグの7件が取得されること
                    // 計画有給休暇明細の計画有給年月日の昇順で並ぶこと
                    Meisais =
                    [
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 1), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 2), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 3), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 4), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 5), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 6), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 7), IsTokukyu = false }
                    ]
                },
                new IndexModel.Keikaku
                {
                    Id = expectedYukyuKeikaku7AId,
                    YukyuKeikakuStatus = SyainSetConst.Syain7AStatus,
                    SyainName = "部署7社員1",
                    BusyoName = "部署7",
                    Meisais =
                    [
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 1), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 2), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 3), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 4), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 5), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 6), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 7), IsTokukyu = false }
                    ]
                },
                new IndexModel.Keikaku
                {
                    Id = expectedYukyuKeikaku5Id,
                    YukyuKeikakuStatus = SyainSetConst.Syain5Status,
                    SyainName = "部署5社員0",
                    BusyoName = "部署5",
                    Meisais =
                    [
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain5Month, 1), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain5Month, 2), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain5Month, 3), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain5Month, 4), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain5Month, 5), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain5Month, 6), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain5Month, 7), IsTokukyu = false }
                    ]
                },
                new IndexModel.Keikaku
                {
                    Id = expectedYukyuKeikaku3Id,
                    YukyuKeikakuStatus = SyainSetConst.Syain3Status,
                    SyainName = "部署3社員0",
                    BusyoName = "部署3",
                    Meisais =
                    [
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain3Month, 1), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain3Month, 2), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain3Month, 3), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain3Month, 4), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain3Month, 5), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain3Month, 6), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain3Month, 7), IsTokukyu = false }
                    ]
                }
            };
            AssertKeikakusAreEqual(expected, actualViewModel.Keikakus);
        }

        /// <summary>
        /// 有効系アサーション（人財）
        /// </summary>
        protected static void AssertPopulatesListWhenJinzai(
            long expectedYukyuKeikaku6Id, long expectedYukyuKeikaku7AId, long expectedYukyuKeikaku7BId,
            IndexModel.JigyoubuShoninViewModel? actualViewModel)
        {
            Assert.IsNotNull(actualViewModel);

            // 「人財権限」が取得されること
            Assert.AreEqual(IndexModel.Authority.Jinzai, actualViewModel.Authority, "Authority が一致しません。");

            // ボタンが表示扱いとなること
            Assert.IsTrue(actualViewModel.ButtonsAreVisible, "ボタンが表示されていません。");

            // 部署列が表示扱いとなること
            Assert.IsTrue(actualViewModel.BusyoColumnIsVisible, "部署列が表示されていません。");

            // チェックボックスが非表示扱いとなること
            Assert.IsTrue(actualViewModel.GetCheckboxIsVisible(0), "1件目にチェックボックスが表示されていません。");
            Assert.IsTrue(actualViewModel.GetCheckboxIsVisible(1), "2件目にチェックボックスが表示されていません。");
            Assert.IsTrue(actualViewModel.GetCheckboxIsVisible(2), "3件目にチェックボックスが表示されていません。");

            var expected = new[]
            {
                new IndexModel.Keikaku
                {
                    // 計画有給休暇のID・ステータスが取得されること
                    Id = expectedYukyuKeikaku7BId,
                    YukyuKeikakuStatus = 人財承認待ち,

                    // 社員マスタ「社員BaseID：計画有給休暇の社員BaseID」の社員氏名が取得されること
                    SyainName = "部署7社員2",

                    // 部署マスタ「ID：社員マスタの部署ID」の部署名称が取得されること
                    BusyoName = "部署7",

                    // 計画有給休暇明細「計画有給ID：計画有給休暇のID」の計画有給年月日・特休フラグの7件が取得されること
                    // 計画有給休暇明細の計画有給年月日の昇順で並ぶこと
                    Meisais =
                    [
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 1), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 2), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 3), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 4), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 5), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 6), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7BMonth, 7), IsTokukyu = false }
                    ]
                },
                new IndexModel.Keikaku
                {
                    Id = expectedYukyuKeikaku7AId,
                    YukyuKeikakuStatus = 人財承認待ち,
                    SyainName = "部署7社員1",
                    BusyoName = "部署7",
                    Meisais =
                    [
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 1), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 2), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 3), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 4), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 5), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 6), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain7AMonth, 7), IsTokukyu = false }
                    ]
                },
                new IndexModel.Keikaku
                {
                    // 計画有給休暇のID・ステータスが取得されること
                    Id = expectedYukyuKeikaku6Id,
                    YukyuKeikakuStatus = 人財承認待ち,

                    // 社員マスタ「社員BaseID：計画有給休暇の社員BaseID」の社員氏名が取得されること
                    SyainName = "部署6社員0",

                    // 部署マスタ「ID：社員マスタの部署ID」の部署名称が取得されること
                    BusyoName = "部署6",

                    // 計画有給休暇明細「計画有給ID：計画有給休暇のID」の計画有給年月日・特休フラグの7件が取得されること
                    // 計画有給休暇明細の計画有給年月日の昇順で並ぶこと
                    Meisais =
                    [
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain6Month, 1), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain6Month, 2), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain6Month, 3), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain6Month, 4), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain6Month, 5), IsTokukyu = true },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain6Month, 6), IsTokukyu = false },
                        new IndexModel.Meisai { Ymd = new DateOnly(ThisYear, SyainSetConst.Syain6Month, 7), IsTokukyu = false }
                    ]
                }
            };
            AssertKeikakusAreEqual(expected, actualViewModel.Keikakus);
        }

        /// <summary>
        /// 無効系アサーション
        /// </summary>
        protected static void AssertSetsEmptyList(bool bumoncyo, IndexModel.JigyoubuShoninViewModel? actualViewModel)
        {
            Assert.IsNotNull(actualViewModel);

            // 特定の権限が取得されること
            var expectedAuthority = bumoncyo ? IndexModel.Authority.Bumoncyo : IndexModel.Authority.None;
            Assert.AreEqual(expectedAuthority, actualViewModel.Authority, "Authority が一致しません。");

            if (bumoncyo)
            {
                // ボタンが表示扱いとなること
                Assert.IsTrue(actualViewModel.ButtonsAreVisible, "ボタンが表示されていません。");
            }
            else
            {
                // ボタンが非表示扱いとなること
                Assert.IsFalse(actualViewModel.ButtonsAreVisible, "ボタンが表示されています。");
            }

            // 部署列が非表示扱いとなること
            Assert.IsFalse(actualViewModel.BusyoColumnIsVisible, "部署列が表示されています。");

            // チェックボックスが非表示扱いとなること
            Assert.IsFalse(actualViewModel.GetCheckboxIsVisible(0), "1件目にチェックボックスが表示されています。");
            Assert.IsFalse(actualViewModel.GetCheckboxIsVisible(1), "2件目にチェックボックスが表示されています。");
            Assert.IsFalse(actualViewModel.GetCheckboxIsVisible(2), "3件目にチェックボックスが表示されています。");
            Assert.IsFalse(actualViewModel.GetCheckboxIsVisible(3), "4件目にチェックボックスが表示されています。");

            // 部署マスタの順序の昇順、社員マスタの順序の降順で並ぶこと
            var expected = new[]
            {
                new IndexModel.Keikaku
                {
                     // 計画有給休暇のID・ステータスが空で取得されること
                    Id = null,
                    YukyuKeikakuStatus = null,

                    // 社員マスタ「社員BaseID：計画有給休暇の社員BaseID」の社員氏名が取得されること
                    SyainName = "部署7社員2",

                    // 部署マスタ「ID：社員マスタの部署ID」の部署名称が取得されること
                    BusyoName = "部署7",

                    // 計画有給年月日・特休フラグが空の7件が取得されること
                    Meisais =
                    [
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null }
                    ]
                },
                new IndexModel.Keikaku
                {
                    Id = null,
                    YukyuKeikakuStatus = null,
                    SyainName = "部署7社員1",
                    BusyoName = "部署7",
                    Meisais =
                    [
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null }
                    ]
                },
                new IndexModel.Keikaku
                {
                    Id = null,
                    YukyuKeikakuStatus = null,
                    SyainName = "部署5社員0",
                    BusyoName = "部署5",
                    Meisais =
                    [
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null }
                    ]
                },
                new IndexModel.Keikaku
                {
                    Id = null,
                    YukyuKeikakuStatus = null,
                    SyainName = "部署3社員0",
                    BusyoName = "部署3",
                    Meisais =
                    [
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null },
                        new IndexModel.Meisai { Ymd = null, IsTokukyu = null }
                    ]
                }
            };
            AssertKeikakusAreEqual(expected, actualViewModel.Keikakus);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 内部レコード・内部クラス
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <see cref="CreateBusyoHierarchy()"/> の戻り値
        /// 内部実装の詳細であるため、アクセス修飾子は private とします。
        /// </summary>
        private record BusyoHierarchy(
            Busyo Busyo1, Busyo Busyo2, Busyo Busyo3, Busyo Busyo4, Busyo Busyo5, Busyo Busyo6, Busyo Busyo7);

        /// <summary>
        /// <see cref="CreateSyainSet"/> の戻り値
        /// 複数のテスト用社員エンティティを 1 セットとして扱うためのレコードです。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 本レコードは、保持している全ての <see cref="Syain"/> を簡潔に列挙できるようにするため、
        /// <see cref="IEnumerable{T}"/> を実装しています。これによりテストコード側では、
        /// 各プロパティ（Syain1 ～ Syain7B）に個別にアクセスするだけでなく、
        /// <c>foreach</c> や LINQ を用いて一括で検証（例：全社員の部署・ステータスの一括確認）を行うことを想定しています。
        /// </para>
        /// <para>
        /// 列挙順序は、プロパティ定義順（Syain1, Syain2, Syain3, Syain4, Syain5, Syain6, Syain7A, Syain7B）となるように実装しており、
        /// シーケンス比較など順序を前提としたユニットテストでも利用できるようにしています。
        /// </para>
        /// </remarks>
        protected record SyainSet(
            Syain Syain1, Syain Syain2, Syain Syain3, Syain Syain4, Syain Syain5, Syain Syain6, Syain Syain7A, Syain Syain7B)
            : IEnumerable<Syain>
        {
            /// <inheritdoc />
            public IEnumerator<Syain> GetEnumerator()
            {
                yield return Syain1;
                yield return Syain2;
                yield return Syain3;
                yield return Syain4;
                yield return Syain5;
                yield return Syain6;
                yield return Syain7A;
                yield return Syain7B;
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// <see cref="CreateSyainSet"/> の戻り値
        /// 複数のテスト用計画有給休暇エンティティを 1 セットとして扱うためのレコードです。
        /// </summary>
        protected record YukyuKeikakuSet(
            YukyuKeikaku YukyuKeikaku1, YukyuKeikaku YukyuKeikaku2, YukyuKeikaku YukyuKeikaku3, YukyuKeikaku YukyuKeikaku4,
            YukyuKeikaku YukyuKeikaku5, YukyuKeikaku YukyuKeikaku6, YukyuKeikaku YukyuKeikaku7A, YukyuKeikaku YukyuKeikaku7B);
    }
}
