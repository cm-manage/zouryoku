using CommonLibrary.Extensions;
using Model.Enums;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.JuchuJohoKensaku;

namespace ZouryokuTest.Pages.JuchuJohoKensaku
{
    /// <summary>
    /// <see cref="IndexModel"/>のテストに使用する補助クラス
    /// </summary>
    public class IndexModelTestBase : BaseInMemoryDbContextTest
    {
        // =====================================
        // 定数
        // =====================================

        // ログインユーザー情報
        // -------------------------------------

        /// <summary>
        /// ログインユーザーの社員BASE ID
        /// </summary>
        protected const long LoginUserSyainBaseId = 111;

        /// <summary>
        /// ログインユーザーの部署コード
        /// </summary>
        protected const string LoginUserBusyoCode = "131";

        // ======================================
        // プロパティ
        // ======================================

        /// <summary>
        /// テスト対象のモデル。
        /// TestInitializeでここにモデルを詰める。
        /// </summary>
        protected IndexModel? Model { get; set; }

        // ======================================
        // 補助メソッド
        // ======================================

        /// <summary>
        /// ログインユーザー情報を含んだ<see cref="IndexModel"/>インスタンスの作成
        /// </summary>
        /// <returns><see cref="Model"/>のインスタンス</returns>
        protected IndexModel CreateModel()
        {
            // IndexModelのインスタンスを作成
            var now = new DateTime(2026, 2, 26, 16, 0, 0);
            fakeTimeProvider.SetLocalNow(now);
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine, fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData(),
                SearchConditions = new() { }
            };

            // ログインユーザー用の社員BASEデータと社員データをインメモリDBに作成
            CreateSyainForLoginUser();
            db.SaveChanges();

            // モデルのセッションにLoginInfoを作成
            var loginUser = db.Syains
                .Single(emp => emp.SyainBaseId == LoginUserSyainBaseId);
            SetLoginUser(model, loginUser);

            return model;
        }

        /// <summary>
        /// ログインユーザー用の社員データを作成する
        /// </summary>
        protected void CreateSyainForLoginUser()
        {
            var syainBase = new SyainBasis
            {
                Id = LoginUserSyainBaseId,
                // 必要ないNOT NULLカラムには適当に値を詰める
                Code = "9999",
                Name = "ログインユーザー",
            };
            var syain = new Syain
            {
                SyainBase = syainBase,
                BusyoCode = LoginUserBusyoCode,
                // 必要ないNOT NULLカラムには適当に値を詰める
                Id = 1,
                Code = string.Empty,
                Name = string.Empty,
                KanaName = string.Empty,
                Seibetsu = '0',
                SyokusyuCode = 999,
                NyuusyaYmd = DateOnly.MinValue,
                StartYmd = DateOnly.MinValue,
                EndYmd = DateOnly.MaxValue,
                Kyusyoku = 999,
                SyucyoSyokui = BusinessTripRole._7_8級,
                KingsSyozoku = string.Empty,
                KaisyaCode = 999,
                IsGenkaRendou = false,
                Kengen = EmployeeAuthority.None,
                Jyunjyo = 999,
                Retired = false,
                BusyoId = 999,
                KintaiZokuseiId = 999,
                UserRoleId = 999
            };
            db.Syains.Add(syain);
        }

        /// <summary>
        /// <paramref name="model"/>のセッションにログイン情報を作成する
        /// </summary>
        /// <param name="model">ログイン情報を作成するモデル</param>
        /// <param name="loginUser">ログインユーザーの<see cref="Syain"/>インスタンス</param>
        protected void SetLoginUser(IndexModel model, Syain loginUser)
        {
            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = loginUser };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);
        }

        // ======================================
        // データ追加共通処理
        // ======================================

        /// <summary>
        /// 基本データ（KINGS受注登録・KINGS受注参照履歴）を1件登録する。
        /// 指定ありの項目は、引数で渡された値を設定する。
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="jucYmd">受注日</param>
        /// <param name="keiKn">契約先カナ</param>
        /// <param name="jucKn">受注先カナ</param>
        /// <param name="bukken">件名</param>
        /// <param name="iriBusCd">送り元部署コード</param>
        /// <param name="iriBusCdIsSpecified">送り元部署コードにnullを明示的に指定するフラグ</param>
        /// <param name="okrTanCd1">送り担当者コード</param>
        /// <param name="okrTanNm1">送り担当者氏名</param>
        /// <param name="chaYmd">着工日</param>
        /// <param name="isGenkaToketu">原価凍結フラグ</param>
        /// <param name="keiyakuJoutaiKbn">契約状態区分名</param>
        /// <param name="nendo">デフォルトはシステム日付の年度、指定した分年数を足す</param>
        /// <param name="syainBaseId">社員BaseID</param>
        /// <param name="sansyouTime">参照時間</param>
        /// <param name="rirekiInsert">KINGS受注参照履歴へのINSERTフラグ デフォルトはtrue</param>
        public void AddKingsJuchu(
            int? id = null,
            DateOnly? jucYmd = null,
            string? keiKn = null,
            string? jucKn = null,
            string? bukken = null,
            string? iriBusCd = null,
            bool iriBusCdIsSpecified = false,
            string? okrTanCd1 = null,
            string? okrTanNm1 = null,
            DateOnly? chaYmd = null,
            bool? isGenkaToketu = null,
            ContractClassification? keiyakuJoutaiKbn = null,
            int? nendo = 0,
            long? syainBaseId = null,
            DateTime? sansyouTime = null,
            bool rirekiInsert = true)
        {
            nendo ??= 0;
            var today = new DateOnly(2026, 2, 26);

            var kings = new KingsJuchu
            {
                Id = id ?? 1,
                JucYmd = jucYmd ?? new DateOnly(2026, 1, 1),
                EntYmd = new DateOnly(2026, 1, 1),
                KeiNm = "契約先サンプル",
                KeiKn = keiKn ?? "ｹｲﾔｸｻｷｻﾝﾌﾟﾙ",
                JucNm = "受注先サンプル",
                JucKn = jucKn ?? "ｼﾞｭﾁｭｳｻｷｻﾝﾌﾟﾙ",
                Bukken = bukken ?? "件名サンプル",
                IriBusCd = iriBusCdIsSpecified || iriBusCd != null
                            ? iriBusCd      // nullもそのままセットされる
                            : "100",        // 未指定ならデフォルト
                JucKin = 1000000,
                OkrTanCd1 = okrTanCd1 ?? "25000",
                OkrTanNm1 = okrTanNm1 ?? "送り担当者サンプル",
                ChaYmd = chaYmd ?? new DateOnly(2025, 1, 1),
                IsGenkaToketu = isGenkaToketu ?? false,
                ProjectNo = "13125-500701",
                JuchuuNo = "J13025000111",
                JuchuuGyoNo = 11,
                KeiyakuJoutaiKbn = keiyakuJoutaiKbn ?? ContractClassification.経費,
                SekouBumonCd = "131",
                HiyouShubetuCd = 0,
                HiyouShubetuCdName = "費用種別名サンプル",
                Nendo = (short)today.AddYears(nendo.Value).GetFiscalYear(),
                ShouhinName = "商品名サンプル",
                BusyoId = 0,
                SearchKeiNm = "1Aア",
                SearchKeiKn = "2Bイ",
                SearchJucNm = "3Cウ",
                SearchJucKn = "4Dエ",
                SearchBukken = "5Eオ",
            };
            db.Add(kings);

            if (!rirekiInsert) return;

            var rireki = new KingsJuchuSansyouRireki
            {
                Id = id ?? 1,
                SyainBaseId = syainBaseId ?? 111,
                SansyouTime = sansyouTime ?? new DateTime(2026, 1, 1, 0, 0, 01),
                KingsJuchu = kings
            };
            db.Add(rireki);
        }

        /// <summary>
        /// 複数データ（KINGS受注登録・KINGS受注参照履歴）を登録する。
        /// </summary>
        /// <param name="n">件数</param>
        public void AddKingsJuchus(int n)
        {
            for (int i = 0; i < n; i++)
            {
                AddKingsJuchu(i + 1);
            }
        }
    }
}