using Model.Model;
using static Zouryoku.Pages.KokyakuJohoHyoji.IndexModel;
using static Model.Enums.BusinessTripRole;
using static Model.Enums.EmployeeAuthority;

namespace ZouryokuTest.Pages.KokyakuJohoHyoji
{
    /// <summary>
    /// ビューモデルのテストクラス
    /// </summary>
    [TestClass]
    public class IndexModelKokyakuViewModelTests : IndexModelTestBase
    {
        // ======================================
        // 定数
        // ======================================

        private const long KokyakuId = 1;
        private const int KokyakuCode = 0;
        private const string KokyakuName = "A会社";
        private const string KokyakuNameKana = "エーカイシャ";
        private const string Ryakusyou = "A";
        private const string Shiten = "大阪";
        private const string YuubinBangou = "000-0000";
        private const string Jyuusyo1 = "大阪府大阪市";
        private const string Jyuusyo2 = "○○区";
        private const string Tel = "000-0000-0000";
        private const string Fax = "000-0000-0000";
        private const string Memo = "サンプル";
        private const string Url = "url";
        private const long GyousyuId = 1;
        private const string GyousyuName = "業種A";
        private const long EigyouBaseSyainId = 1;
        private const string EigyouSyainName = "営業A";

        // ======================================
        // 正常系
        // ======================================
        /// <summary>
        /// 正常系: 顧客会社IDを取得していること
        /// </summary>
        [TestMethod]
        public void KokyakuViewModel_初期化_顧客会社IDを取得()
        {
            // ================ Arrange ================ //
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            Assert.AreEqual(KokyakuId, viewModel.Id);
        }

        /// <summary>
        /// 正常系: 会社コードを取得していること
        /// </summary>
        [TestMethod]
        public void KokyakuViewModel_初期化_会社コードを取得()
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            Assert.AreEqual(KokyakuCode.ToString(), viewModel.Code);
        }

        /// <summary>
        /// 正常系: 顧客名を取得していること
        /// </summary>
        [TestMethod]
        public void KokyakuViewModel_初期化_顧客名を取得()
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku,
            };

            // ================ Assert ================ //
            Assert.AreEqual(KokyakuName, viewModel.Name);
        }

        /// <summary>
        /// 正常系: 顧客名カナを取得していること
        /// </summary>
        [TestMethod]
        public void KokyakuViewModel_初期化_顧客名カナを取得()
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku,
            };

            // ================ Assert ================ //
            Assert.AreEqual(KokyakuNameKana, viewModel.NameKana);
        }

        /// <summary>
        /// 正常系: 略称を取得していること
        /// </summary>
        [TestMethod]
        public void KokyakuViewModel_初期化_略称を取得()
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            Assert.AreEqual(Ryakusyou, viewModel.Ryakusyou);
        }

        /// <summary>
        /// 正常系: 支店を取得していること
        /// </summary>
        [DataRow(Shiten, DisplayName = "支店がNullでないとき")]
        [DataRow(null, DisplayName = "支店がNullのとき")]
        [TestMethod]
        public void KokyakuViewModel_初期化_支店を取得(string? shiten)
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            kokyaku.Shiten = shiten;

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            if (shiten == null)
            {
                Assert.IsNull(viewModel.Shiten);
            }
            else
            {
                Assert.AreEqual(shiten, viewModel.Shiten);
            }
        }

        /// <summary>
        /// 正常系: 郵便番号を取得していること
        /// </summary>
        [DataRow(YuubinBangou, DisplayName = "郵便番号がNullでないとき")]
        [DataRow(null, DisplayName = "郵便番号がNullのとき")]
        [TestMethod]
        public void KokyakuViewModel_初期化_郵便番号を取得(string? yuubinBangou)
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            kokyaku.YuubinBangou = yuubinBangou;

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku,
            };

            // ================ Assert ================ //
            if (yuubinBangou == null)
            {
                Assert.IsNull(viewModel.YuubinnBangou);
            }
            else
            {
                Assert.AreEqual(yuubinBangou, viewModel.YuubinnBangou);
            }
        }

        /// <summary>
        /// 正常系: 住所1を取得していること
        /// </summary>
        [DataRow(Jyuusyo1, DisplayName = "住所1がNullでないとき")]
        [DataRow(null, DisplayName = "住所1がNullのとき")]
        [TestMethod]
        public void KokyakuViewModel_初期化_住所1を取得(string? jyuusyo1)
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            kokyaku.Jyuusyo1 = jyuusyo1;

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            if (jyuusyo1 == null)
            {
                Assert.IsNull(viewModel.Jyuusyo1);
            }
            else
            {
                Assert.AreEqual(jyuusyo1, viewModel.Jyuusyo1);
            }
        }

        /// <summary>
        /// 正常系: 住所2を取得していること
        /// </summary>
        [DataRow(Jyuusyo2, DisplayName = "住所2がNullでないとき")]
        [DataRow(null, DisplayName = "住所2がNullのとき")]
        [TestMethod]
        public void KokyakuViewModel_初期化_住所2を取得(string? jyuusyo2)
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            kokyaku.Jyuusyo2 = jyuusyo2;

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            if (jyuusyo2 == null)
            {
                Assert.IsNull(viewModel.Jyuusyo2);
            }
            else
            {
                Assert.AreEqual(jyuusyo2, viewModel.Jyuusyo2);
            }
        }

        /// <summary>
        /// 正常系: 電話番号を取得していること
        /// </summary>
        [DataRow(Tel, DisplayName = "電話番号がNullでないとき")]
        [DataRow(null, DisplayName = "電話番号がNullのとき")]
        [TestMethod]
        public void KokyakuViewModel_初期化_電話番号を取得(string? tel)
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            kokyaku.Tel = tel;

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            if (tel == null)
            {
                Assert.IsNull(viewModel.Tel);
            }
            else
            {
                Assert.AreEqual(tel, viewModel.Tel);
            }
        }

        /// <summary>
        /// 正常系: Faxを取得していること
        /// </summary>
        [DataRow(Fax, DisplayName = "FaxがNullでないとき")]
        [DataRow(null, DisplayName = "FaxがNullのとき")]
        [TestMethod]
        public void KokyakuViewModel_初期化_Faxを取得(string? fax)
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            kokyaku.Fax = fax;

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            if (fax == null)
            {
                Assert.IsNull(viewModel.Fax);
            }
            else
            {
                Assert.AreEqual(fax, viewModel.Fax);
            }
        }

        /// <summary>
        /// 正常系: メモを取得していること
        /// </summary>
        [DataRow(Memo, DisplayName = "メモがNullでないとき")]
        [DataRow(null, DisplayName = "メモがNullのとき")]
        [TestMethod]
        public void KokyakuViewModel_初期化_メモを取得(string? memo)
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            kokyaku.Memo = memo;

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            if (memo == null)
            {
                Assert.IsNull(viewModel.Memo);
            }
            else
            {
                Assert.AreEqual(memo, viewModel.Memo);
            }
        }

        /// <summary>
        /// 正常系: URLを取得していること
        /// </summary>
        [DataRow(Url, DisplayName = "URLがNullでないとき")]
        [DataRow(null, DisplayName = "URLがNullのとき")]
        [TestMethod]
        public void KokyakuViewModel_初期化_URLを取得(string? url)
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            kokyaku.Url = url;

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            if (url == null)
            {
                Assert.IsNull(viewModel.Url);
            }
            else
            {
                Assert.AreEqual(url, viewModel.Url);
            }
        }

        /// <summary>
        /// 正常系: 業種名を取得していること
        /// </summary>
        [TestMethod]
        public void KokyakuViewModel_初期化_業種名を取得()
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            Assert.AreEqual(GyousyuName, viewModel.GyousyuName);
        }

        /// <summary>
        /// 正常系: 営業社員名を取得していること
        /// </summary>
        [TestMethod]
        public void KokyakuViewModel_初期化_営業社員名を取得()
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            Assert.AreEqual(EigyouSyainName, viewModel.EigyouSyainName);
        }

        /// <summary>
        /// 正常系: 業種マスタに対応するデータが存在しない場合、業種名が空文字となる
        /// </summary>
        [TestMethod]
        public void KokyakuViewModel_初期化時に業種マスタのデータが存在しない_業種名が空文字()
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            kokyaku.Gyousyu = null;

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            Assert.IsNull(viewModel.GyousyuName);
        }

        /// <summary>
        /// 正常系: 社員マスタに対応するデータが存在しない場合、営業担当者名が空文字となる
        /// </summary>
        [TestMethod]
        public void KokyakuViewModel_初期化時に社員マスタのデータが存在しない_営業担当者名が空文字()
        {
            // ================ Arrange ================ //
            // 顧客会社
            var kokyaku = new KokyakuKaisha()
            {
                Id = KokyakuId,
                Code = KokyakuCode,
                Name = KokyakuName,
                NameKana = KokyakuNameKana,
                Ryakusyou = Ryakusyou,
                SearchName = "A会社",
                SearchNameKana = "エーカイシャ",
            };

            // 業種
            kokyaku.Gyousyu = new Gyousyu()
            {
                Id = GyousyuId,
                Name = GyousyuName,
                Code = "100",
            };

            // 社員BASE
            kokyaku.EigyoBaseSyain = new SyainBasis()
            {
                Id = EigyouBaseSyainId,
                Name = EigyouSyainName,
                Code = "100",
            };

            // 社員マスタ
            kokyaku.EigyoBaseSyain.Syains.Add(new Syain()
            {
                Code = "100",
                Name = EigyouSyainName,
                KanaName = "エーエイギョウ",
                Seibetsu = '1',
                BusyoCode = "100",
                SyokusyuCode = 100,
                SyokusyuBunruiCode = 100,
                NyuusyaYmd = new DateOnly(2020, 4, 1),
                StartYmd = new DateOnly(2020, 4, 1),
                EndYmd = new DateOnly(9999, 12, 31),
                Kyusyoku = 1,
                SyucyoSyokui = _2_6級,
                KingsSyozoku = "100",
                KaisyaCode = 1,
                IsGenkaRendou = false,
                Kengen = None,
                Jyunjyo = 1,
                Retired = false,
                SyainBaseId = EigyouBaseSyainId,
                BusyoId = 1,
                KintaiZokuseiId = 1,
                UserRoleId = 1,
            });

            kokyaku.EigyoBaseSyain!.Syains.Clear();

            // ================ Act ================ //
            var viewModel = new KokyakuViewModel()
            {
                Kokyaku = kokyaku
            };

            // ================ Assert ================ //
            Assert.IsNull(viewModel.EigyouSyainName);
        }
    }
}
