using Model.Model;
using Zouryoku.Pages.KokyakuMeiKensaku;
using ZouryokuTest.Builder;

namespace ZouryokuTest.Pages.KokyakuMeiKensaku
{
    /// <summary>
    /// ビューモデルのテストクラス
    /// </summary>
    [TestClass]
    public class IndexModelCustomerViewModelTests : IndexModelTestBase
    {
        // ======================================
        // 定数
        // ======================================

        private const long CustomerId = 1;
        // ビルダーの検索用カラムと異なる文字列を設定
        private const string CustomerName = "顧客名称";
        private const string CustomerNameKana = "顧客名カナ";
        private const string Address1 = "住所1";
        private const string Address2 = "住所2";
        private const string Tel = "TEL番号";
        private const string SalesEmpName = "営業担当者";
        private const uint Version = 1u;

        // ======================================
        // 補助メソッド
        // ======================================

        /// <summary>
        /// テスト用のエンティティを作成する
        /// </summary>
        /// <returns>リレーションを含む顧客会社参照履歴のエンティティ</returns>
        private KokyakuKaisyaSansyouRireki CreateEntityForViewModelTest()
        {
            // 顧客会社参照履歴
            var rireki = new KokyakuKaisyaSansyouRirekiBuilder()
                .WithVersion(Version)
                .Build();

            // 顧客会社
            rireki.KokyakuKaisya = new KokyakuKaishaBuilder()
                .WithId(CustomerId)
                .WithName(CustomerName)
                .WithNameKana(CustomerNameKana)
                .Build();

            // 社員BASE
            rireki.KokyakuKaisya.EigyoBaseSyain = new SyainBasisBuilder().Build();

            // 社員マスタ
            rireki.KokyakuKaisya.EigyoBaseSyain.Syains.Add(new SyainBuilder()
                .WithName(SalesEmpName)
                .Build());

            return rireki;
        }

        /// <summary>
        /// 正常系：顧客会社IDを取得していること
        /// </summary>
        [TestMethod]
        public void FromEntity_顧客会社IDを取得()
        {
            // Arrange
            var kokyaku = CreateEntityForViewModelTest().KokyakuKaisya;

            // Act
            var viewModel = new CustomerViewModel(kokyaku);

            // Assert
            Assert.AreEqual(CustomerId, viewModel.KokyakuKaishaId);
        }

        /// <summary>
        /// 正常系：顧客名を取得していること
        /// </summary>
        [TestMethod]
        public void FromEntity_顧客名を取得()
        {
            // Arrange
            var kokyaku = CreateEntityForViewModelTest().KokyakuKaisya;

            // Act
            var viewModel = new CustomerViewModel(kokyaku);

            // Assert
            Assert.AreEqual(CustomerName, viewModel.Name);
        }

        /// <summary>
        /// 正常系：住所を取得していること
        /// </summary>
        [TestMethod]
        [DataRow(Address1, Address2, DisplayName = "住所1と住所2がともにNULLでないとき")]
        [DataRow(Address1, null, DisplayName = "住所2がNULLのとき")]
        [DataRow(null, Address2, DisplayName = "住所1がNULLのとき")]
        [DataRow(null, null, DisplayName = "住所1と住所2がともにNULLのとき")]
        public void FromEntity_住所を取得(string? address1, string? address2)
        {
            // Arrange
            var kokyaku = CreateEntityForViewModelTest().KokyakuKaisya;
            kokyaku.Jyuusyo1 = address1;
            kokyaku.Jyuusyo2 = address2;

            // Act
            var viewModel = new CustomerViewModel(kokyaku);

            // Assert
            var expectedAddress = $"{address1}{address2}";
            Assert.AreEqual(expectedAddress, viewModel.Address);
        }

        /// <summary>
        /// 正常系：TEL番号を取得していること
        /// </summary>
        [TestMethod]
        [DataRow(Tel, DisplayName = "TEL番号がNULLでないとき")]
        [DataRow(null, DisplayName = "TEL番号がNULLのとき")]
        public void FromEntity_TEL番号を取得(string? tel)
        {
            // Arrange
            var kokyaku = CreateEntityForViewModelTest().KokyakuKaisya;
            kokyaku.Tel = tel;

            // Act
            var viewModel = new CustomerViewModel(kokyaku);

            // Assert
            Assert.AreEqual(tel, viewModel.Tel);
        }

        /// <summary>
        /// 正常系：営業社員名を取得する
        /// </summary>
        [TestMethod]
        public void FromEntity_営業担当者を取得()
        {
            // Arrange
            var kokyaku = CreateEntityForViewModelTest().KokyakuKaisya;

            // Act
            var viewModel = new CustomerViewModel(kokyaku);

            // Assert
            Assert.AreEqual(SalesEmpName, viewModel.SalesPersonName);
        }

        /// <summary>
        /// 正常系：社員マスタに対応するデータが存在しないとき営業社員名がNULLとなる
        /// </summary>
        [TestMethod]
        public void FromEntity_社員マスタにデータが存在しない_営業担当者がNULL()
        {
            // Arrange
            var kokyaku = CreateEntityForViewModelTest().KokyakuKaisya;
            // 社員マスタ部分を破棄
            kokyaku.EigyoBaseSyain!.Syains.Clear();

            // Act
            var viewModel = new CustomerViewModel(kokyaku);

            // Assert
            Assert.IsNull(viewModel.SalesPersonName);
        }
        /// <summary>
        /// 正常系：社員BASEマスタに対応するデータが存在しないとき営業社員名がNULLとなる
        /// </summary>
        [TestMethod]
        public void FromEntity_社員BASEマスタにデータが存在しない_営業担当者がNULL()
        {
            // Arrange
            var kokyaku = CreateEntityForViewModelTest().KokyakuKaisya;
            // 社員BASEマスタ部分を破棄
            kokyaku.EigyoBaseSyain = null;

            // Act
            var viewModel = new CustomerViewModel(kokyaku);

            // Assert
            Assert.IsNull(viewModel.SalesPersonName);
        }

        /// <summary>
        /// 正常系：顧客会社IDを取得していること
        /// </summary>
        [TestMethod]
        public void FromHistoryEntity_顧客会社IDを取得()
        {
            // Arrange
            var history = CreateEntityForViewModelTest();

            // Act
            var viewModel = new CustomerViewModel(history);

            // Assert
            Assert.AreEqual(CustomerId, viewModel.KokyakuKaishaId);
        }

        /// <summary>
        /// 正常系：顧客名を取得していること
        /// </summary>
        [TestMethod]
        public void FromHistoryEntity_顧客名を取得()
        {
            // Arrange
            var history = CreateEntityForViewModelTest();

            // Act
            var viewModel = new CustomerViewModel(history);

            // Assert
            Assert.AreEqual(CustomerName, viewModel.Name);
        }

        /// <summary>
        /// 正常系：住所を取得していること
        /// </summary>
        [TestMethod]
        [DataRow(Address1, Address2, DisplayName = "住所1と住所2がともにNULLでないとき")]
        [DataRow(Address1, null, DisplayName = "住所2がNULLのとき")]
        [DataRow(null, Address2, DisplayName = "住所1がNULLのとき")]
        [DataRow(null, null, DisplayName = "住所1と住所2がともにNULLのとき")]
        public void FromHistoryEntity_住所を取得(string? address1, string? address2)
        {
            // Arrange
            var history = CreateEntityForViewModelTest();
            history.KokyakuKaisya.Jyuusyo1 = address1;
            history.KokyakuKaisya.Jyuusyo2 = address2;

            // Act
            var viewModel = new CustomerViewModel(history);

            // Assert
            var expectedAddress = $"{address1}{address2}";
            Assert.AreEqual(expectedAddress, viewModel.Address);
        }

        /// <summary>
        /// 正常系：TEL番号を取得していること
        /// </summary>
        [TestMethod]
        [DataRow(Tel, DisplayName = "TEL番号がNULLでないとき")]
        [DataRow(null, DisplayName = "TEL番号がNULLのとき")]
        public void FromHistoryEntity_TEL番号を取得(string? tel)
        {
            // Arrange
            var history = CreateEntityForViewModelTest();
            history.KokyakuKaisya.Tel = tel;

            // Act
            var viewModel = new CustomerViewModel(history);

            // Assert
            Assert.AreEqual(tel, viewModel.Tel);
        }

        /// <summary>
        /// 正常系：営業社員名を取得する
        /// </summary>
        [TestMethod]
        public void FromHistoryEntity_営業担当者を取得()
        {
            // Arrange
            var history = CreateEntityForViewModelTest();

            // Act
            var viewModel = new CustomerViewModel(history);

            // Assert
            Assert.AreEqual(SalesEmpName, viewModel.SalesPersonName);
        }

        /// <summary>
        /// 正常系：社員マスタに対応するデータが存在しないとき営業社員名がNULLとなる
        /// </summary>
        [TestMethod]
        public void FromHistoryEntity_社員マスタにデータが存在しない_営業担当者がNULL()
        {
            // Arrange
            var history = CreateEntityForViewModelTest();
            // 社員マスタ部分を破棄
            history.KokyakuKaisya.EigyoBaseSyain!.Syains.Clear();

            // Act
            var viewModel = new CustomerViewModel(history);

            // Assert
            Assert.IsNull(viewModel.SalesPersonName);
        }
        /// <summary>
        /// 正常系：社員BASEマスタに対応するデータが存在しないとき営業社員名がNULLとなる
        /// </summary>
        [TestMethod]
        public void FromHistoryEntity_社員BASEマスタにデータが存在しない_営業担当者がNULL()
        {
            // Arrange
            var history = CreateEntityForViewModelTest();
            // 社員BASEマスタ部分を破棄
            history.KokyakuKaisya.EigyoBaseSyain = null;

            // Act
            var viewModel = new CustomerViewModel(history);

            // Assert
            Assert.IsNull(viewModel.SalesPersonName);
        }

        /// <summary>
        /// 正常系：バージョンを取得すること
        /// </summary>
        [TestMethod]
        public void FromHistoryEntity_バージョンを取得する()
        {
            // Arrange
            var history = CreateEntityForViewModelTest();

            // Act
            var viewModel = new CustomerViewModel(history);

            // Assert
            Assert.AreEqual(Version, viewModel.Version);
        }

        [TestMethod]
        public void Source_エンティティが設定されていない_InvalidOperationExceptionを発生させる()
        {
            // Arrange
            // 顧客会社の情報を持たない参照履歴のエンティティを引数に渡す
            var history = new KokyakuKaisyaSansyouRirekiBuilder().Build();
            var viewModel = new CustomerViewModel(history);

            // Act / Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                // エンティティを設定せずにプロパティを呼び出す
                _ = viewModel.Name;
            });
            Assert.AreEqual("顧客会社情報と顧客会社参照履歴の情報の両方が設定されていません。", exception.Message);
        }
    }
}
