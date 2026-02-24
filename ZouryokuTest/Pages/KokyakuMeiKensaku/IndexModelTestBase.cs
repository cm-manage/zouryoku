using CommonLibrary.Extensions;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.KokyakuMeiKensaku;
using ZouryokuTest.Builder;

namespace ZouryokuTest.Pages.KokyakuMeiKensaku
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
        protected const long LoginUserSyainBaseId = 999;

        /// <summary>
        /// ログインユーザーの社員コード
        /// </summary>
        protected const string LoginUserCode = "99999";

        /// <summary>
        /// ログインユーザーの氏名
        /// </summary>
        protected const string LoginUserName = "ログインユーザー";

        // テストデータ用
        // --------------------------------------

        /// <summary>
        /// 社員マスタの検索用テストデータの特殊ケースのID
        /// </summary>
        protected enum SyainIdForSearch
        {
            有効開始日_正常_境界値 = 1,
            有効期限_正常_代表値 = 2,
            有効終了日_正常_境界値 = 3,
            有効開始日_不正_代表値 = 4,
            有効開始日_不正_境界値 = 5,
            有効終了日_不正_境界値 = 6,
            有効終了日_不正_代表値 = 7,
        }

        /// <summary>
        /// 顧客会社の検索用テストデータの特殊ケースのID
        /// </summary>
        protected enum KokyakuKaishaIdForSearch
        {
            社員BaseID_NULL = 8,
            社員BaseID_社員BASEに存在しない = 9,
            社員BaseID_社員マスタに存在しない = 10,

        }

        /// <summary>
        /// 参照履歴の検索用テストデータの特殊ケースのID
        /// </summary>
        protected enum SansyouRirekiIdForSearch
        {
            顧客ID_顧客会社に存在しない = 11,
            社員BaseID_ログインユーザーと異なる = 12,
        }

        /// <summary>
        /// 参照履歴の削除用テストデータの特殊ケースのID
        /// </summary>
        protected enum SansyouRirekiIdForDelete
        {
            社員BaseID_ログインユーザーと異なる = 3,
        }

        // ======================================
        // 補助メソッド
        // ======================================

        /// <summary>
        /// ログインユーザー情報を含んだ<see cref="IndexModel"/>インスタンスの作成
        /// </summary>
        /// <returns><see cref="IndexModel"/>のインスタンス</returns>
        protected IndexModel CreateModel()
        {
            // IndexModelのインスタンスを作成
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine, fakeTimeProvider)
            {
                PageContext = GetPageContext(),
                TempData = GetTempData()
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
            // 社員BASEマスタへデータを追加
            var empBase = new SyainBasisBuilder()
                .WithId(LoginUserSyainBaseId)
                .WithCode(LoginUserCode)
                .WithName(LoginUserName)
                .Build();
            db.SyainBases.Add(empBase);

            // 社員マスタへデータを追加
            var emp = new SyainBuilder()
                .WithId(LoginUserSyainBaseId)
                .WithCode(LoginUserCode)
                .WithSyainBaseId(LoginUserSyainBaseId)
                .WithName(LoginUserName)
                .WithKanaName(LoginUserName)
                .Build();
            db.Syains.Add(emp);
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

        /// <summary>
        /// テスト仕様書の検索条件用テストデータを作成する
        /// </summary>
        protected void CreateDataForSearch()
        {
            // 社員BASEマスタ
            var syainBasis = CreateSyainBasisList(8);

            // 社員マスタ
            var syains = CreateSyainList(7);
            // テストケースに併せてデータを変形する
            // NOTE: データのIDとリストのインデックスは1だけずれている
            syains[(int)SyainIdForSearch.有効開始日_正常_境界値 - 1].StartYmd = fakeTimeProvider.Now().AddDays(-1).ToDateOnly();
            syains[(int)SyainIdForSearch.有効終了日_正常_境界値 - 1].EndYmd = fakeTimeProvider.Now().AddDays(1).ToDateOnly();
            syains[(int)SyainIdForSearch.有効開始日_不正_代表値 - 1].StartYmd = fakeTimeProvider.Now().AddDays(15).ToDateOnly();
            syains[(int)SyainIdForSearch.有効開始日_不正_境界値 - 1].StartYmd = fakeTimeProvider.Now().AddDays(1).ToDateOnly();
            syains[(int)SyainIdForSearch.有効終了日_不正_境界値 - 1].EndYmd = fakeTimeProvider.Now().AddDays(-1).ToDateOnly();
            syains[(int)SyainIdForSearch.有効終了日_不正_代表値 - 1].EndYmd = fakeTimeProvider.Now().AddDays(-15).ToDateOnly();

            // 顧客会社マスタ
            var kokyakus = CreateKokyakuKaishaList(10, true);
            // テストケースに併せてデータを変形する
            // NOTE: データのIDとリストのインデックスは1だけずれている
            kokyakus[(int)KokyakuKaishaIdForSearch.社員BaseID_NULL - 1].EigyoBaseSyainId = null;
            kokyakus[(int)KokyakuKaishaIdForSearch.社員BaseID_社員BASEに存在しない - 1].EigyoBaseSyainId = 100;
            kokyakus[(int)KokyakuKaishaIdForSearch.社員BaseID_社員マスタに存在しない - 1].EigyoBaseSyainId = 8;

            // 顧客会社参照履歴
            var rirekis = CreateKokyakuKaisyaSansyouRirekiList(12);
            // テストケースに併せてデータを変形する
            // NOTE: データのIDとリストのインデックスは1だけずれている
            rirekis[(int)SansyouRirekiIdForSearch.顧客ID_顧客会社に存在しない - 1].KokyakuKaisyaId = 100;
            rirekis[(int)SansyouRirekiIdForSearch.社員BaseID_ログインユーザーと異なる - 1].SyainBaseId = 1;
            rirekis[(int)SansyouRirekiIdForSearch.社員BaseID_ログインユーザーと異なる - 1].KokyakuKaisyaId = 1;

            // インメモリDBに反映
            db.AddRange(syainBasis);
            db.AddRange(syains);
            db.AddRange(kokyakus);
            db.AddRange(rirekis);
        }

        /// <summary>
        /// 取得データ確認用サンプルデータを生成する<br/>
        /// 顧客会社テーブルにN件、顧客会社参照履歴テーブルにN件追加する
        /// </summary>
        /// <param name="N">テスト仕様書内のN</param>
        protected void CreateDataForAcquire(int N)
        {
            // 顧客会社
            var kokyakus = CreateKokyakuKaishaList(N, true);

            // 顧客会社参照履歴
            var rirekis = CreateKokyakuKaisyaSansyouRirekiList(N);
            // 参照時間の降順がIDの昇順と一致するように設定
            for (int i = 1; i < rirekis.Count; i++)
                rirekis[i].SansyouTime = rirekis[i].SansyouTime.AddDays(-i);

            // インメモリDBに反映
            db.AddRange(kokyakus);
            db.AddRange(rirekis);
        }

        /// <summary>
        /// ビューモデルに格納されているべき顧客名を取得する
        /// </summary>
        /// <param name="customerId">顧客会社ID</param>
        /// <returns>顧客会社マスタ.顧客名</returns>
        protected string GetExpectedCustomerName(long customerId) => "株式会社サンプル" + customerId.ToString("D2");

        /// <summary>
        /// ビューモデルに格納されているべき住所を取得する
        /// </summary>
        /// <param name="customerId">顧客会社ID</param>
        /// <returns>顧客会社マスタ.住所1+顧客会社マスタ.住所2</returns>
        protected string GetExpectedAddress(long customerId) => "住所1" + customerId.ToString("D2") + "住所2" + customerId.ToString("D2");

        /// <summary>
        /// ビューモデルに格納されているべき電話番号を取得する
        /// </summary>
        /// <param name="customerId">顧客会社ID</param>
        /// <returns>顧客会社マスタ.TEL番号</returns>
        protected string GetExpectedTel(long customerId) => "TEL" + customerId.ToString("D2");

        /// <summary>
        /// ビューモデルに格納されているべき営業担当者名を取得する
        /// </summary>
        /// <param name="customerId">顧客会社ID</param>
        /// <returns>社員マスタ.社員名</returns>
        protected string GetExpectedSalesEmpName(long customerId) => "サンプル太郎" + customerId;

        /// <summary>
        /// 顧客情報のビューモデルに対するAssertのアセット
        /// </summary>
        /// <param name="actualModel">実際のインスタンス</param>
        /// <param name="expectedCustomerId">想定される顧客会社ID</param>
        /// <param name="isExpectedSalesEmpNameNull">営業社員=NULLを要求するかどうか</param>
        protected void AssertCustomerVM(CustomerViewModel actualModel, long expectedCustomerId, bool isExpectedSalesEmpNameNull = false)
        {
            Assert.AreEqual(expectedCustomerId, actualModel.KokyakuKaishaId);
            Assert.AreEqual(GetExpectedCustomerName(expectedCustomerId), actualModel.Name);
            Assert.AreEqual(GetExpectedAddress(expectedCustomerId), actualModel.Address);
            Assert.AreEqual(GetExpectedTel(expectedCustomerId), actualModel.Tel);

            // 営業社員がNULLであることが期待される場合
            if (isExpectedSalesEmpNameNull)
            {
                Assert.IsNull(actualModel.SalesPersonName);
            }
            // そうでない場合
            else
            {
                Assert.AreEqual(GetExpectedSalesEmpName(expectedCustomerId), actualModel.SalesPersonName);
            }
        }

        /// <summary>
        /// 削除用データを作成する
        /// </summary>
        protected void CreateDataForManage()
        {
            // 参照履歴
            var rirekis = CreateKokyakuKaisyaSansyouRirekiList(3);
            // テストケースに併せてデータを変形する
            // NOTE: データのIDとリストのインデックスは1だけずれている
            rirekis[(int)SansyouRirekiIdForDelete.社員BaseID_ログインユーザーと異なる - 1].SyainBaseId = 1;

            // 顧客会社
            var kokyakus = CreateKokyakuKaishaList(4);

            // インメモリDBへ反映
            db.AddRange(rirekis);
            db.AddRange(kokyakus);
        }

        /// <summary>
        /// 並び順チェック用のテストデータを作成する
        /// </summary>
        protected void CreateDataForSort()
        {
            // 顧客会社マスタ
            var kokyakus = CreateKokyakuKaishaList(3);
            // 顧客名カナの昇順のとき、IDが3,1,2の順となるよう調整
            for (int i = 0; i < kokyakus.Count; i++)
                kokyakus[i].NameKana = ((i + 1) % 3).ToString();

            // 顧客会社参照履歴
            var rirekis = CreateKokyakuKaisyaSansyouRirekiList(3);
            // 参照履歴の降順のとき、IDが3,2,1の順となるように調整
            for (int i = 0; i < rirekis.Count; i++)
                rirekis[i].SansyouTime = rirekis[i].SansyouTime.AddDays(i);

            // DBに格納
            db.AddRange(kokyakus);
            db.AddRange(rirekis);
        }

        // ======================================
        // privateメソッド
        // ======================================

        /// <summary>
        /// 社員BASEエンティティのリストを作成する
        /// </summary>
        /// <param name="count">データの個数</param>
        /// <returns><see cref="SyainBasis"/>のインスタンスのリスト</returns>
        private List<SyainBasis> CreateSyainBasisList(int count)
            => [.. Enumerable.Range(1, count).Select(i => new SyainBasisBuilder()
                .WithId(i)
                .Build())];

        /// <summary>
        /// 社員エンティティのリストを作成する
        /// </summary>
        /// <param name="count">データの個数</param>
        /// <returns><see cref="Syain"/>のリスト</returns>
        private List<Syain> CreateSyainList(int count)
            => [.. Enumerable.Range(1, count).Select(i => new SyainBuilder()
                .WithId(i)
                .WithName(GetExpectedSalesEmpName(i))
                .WithSyainBaseId(i)
                .Build())];

        /// <summary>
        /// 顧客会社エンティティのリストを作成する
        /// </summary>
        /// <param name="count">データの個数</param>
        /// <returns><see cref="KokyakuKaisha"/>のリスト</returns>
        private List<KokyakuKaisha> CreateKokyakuKaishaList(int count, bool withNumber = false)
        {
            return [.. Enumerable.Range(1, count).Select(i => {
                // 各カラムに付与するサフィックス
                var suffix = withNumber ? i.ToString("D2") : string.Empty;

                return new KokyakuKaishaBuilder()
                    .WithId(i)
                    .WithName("株式会社サンプル" + suffix)
                    .WithNameKana("カブシキガイシャサンプル" + suffix)
                    .WithJyuusyo1("住所1" + suffix)
                    .WithJyuusyo2("住所2" + suffix)
                    .WithTel("TEL" + suffix)
                    .WithEigyoBaseSyainId(i)
                    .Build();
             })];
        }

        /// <summary>
        /// 顧客会社参照履歴エンティティのリストを作成する
        /// </summary>
        /// <param name="count">データの個数</param>
        /// <returns><see cref="KokyakuKaisyaSansyouRireki"/>のリスト</returns>
        private List<KokyakuKaisyaSansyouRireki> CreateKokyakuKaisyaSansyouRirekiList(int count)
        {
            // 参照時間をfakeTimeProviderの1日前で固定する（更新テストで差分を検出するため）
            var pastTime = fakeTimeProvider.Now().AddDays(-1);

            return [..Enumerable.Range(1, count).Select(i => new KokyakuKaisyaSansyouRirekiBuilder()
                .WithId(i)
                .WithSyainBaseId(LoginUserSyainBaseId)
                .WithKokyakuKaisyaId(i)
                .WithSansyouTime(pastTime)
                .Build())];
        }
    }
}