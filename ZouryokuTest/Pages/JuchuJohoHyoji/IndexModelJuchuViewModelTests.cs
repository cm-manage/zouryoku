using Model.Enums;
using Model.Model;
using ZouryokuTest.Builder;
using ZouryokuTest.Pages.Builder;
using static Zouryoku.Pages.JuchuJohoHyoji.IndexModel;

namespace ZouryokuTest.Pages.JuchuJohoHyoji
{
    /// <summary>
    /// 受注ViewModelのテストクラス
    /// </summary>
    [TestClass]
    public class IndexModelJuchuViewModelTests : IndexModelTestBase
    {
        // ======================================
        // 定数
        // ======================================

        private const long JuchuId = 1;
        private const string ProjectNo = "00000";
        private const string JuchuNo = "00000";
        private const short JuchuGyoBangou = 0;
        private const string KeiyakuJotai = "自営";
        private const string SekoBumon = "部署A";
        private const string JuchuYmd = "2025/04/01";
        private const string KeiNm = "契約A";
        private const string JuchuNm = "受注A";
        private const string Bukken = "物件A";
        private const string ShohinName = "商品A";
        private const string HiyoShubetuName = "費用A";
        private const long JuchuKin = 1000000;
        private const string OkrTanName = "社員A";
        private const string TanName = "社員B";
        private const string UkTanName = "社員C";
        private const string ChaYmd = "2025/04/02";
        private const string NsyYmd = "2025/04/03";
        private const string KurYmd = "2025/04/04";
        private const string KnyYmd = "2025/04/05";
        private const bool GenkaToketu = false;
        private const string ToketuYmd = "2025/04/06";
        private const string Biko = "備考";
        private const ContractClassification KeiyakuJotaiKbn = ContractClassification.受注_自営;
        private const long BusyoId = 1;

        // ======================================
        // 補助メソッド
        // ======================================
        /// <summary>
        /// テスト用のエンティティを作成する
        /// </summary>
        /// <returns>リレーションを含むKings受注のエンティティ</returns>
        private KingsJuchu CreateEntityForViewModelTest()
        {
            // 受注
            var juchu = new KingsJuchuBuilder()
                .WithId(JuchuId)
                .WithJucYmd(DateOnly.Parse(JuchuYmd))
                .WithBukken(Bukken)
                .WithJucKin(JuchuKin)
                .WithChaYmd(DateOnly.Parse(ChaYmd))
                .WithProjectNo(ProjectNo)
                .WithHiyouShubetuCdName(HiyoShubetuName)
                .WithIsGenkaToketu(GenkaToketu)
                .WithBusyoId(BusyoId)
                .Build();

            // 部署
            juchu.Busyo = new BusyoBuilder()
                .WithId(BusyoId)
                .WithName(SekoBumon)
                .Build();

            return juchu;
        }

        // ======================================
        // 正常系
        // ======================================
        /// <summary>
        /// 正常系: 受注IDを取得していること
        /// </summary>
        [TestMethod]
        public void JuchuViewModel_初期化_受注IDを取得()
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            Assert.AreEqual(JuchuId, viewModel.Id);
        }

        /// <summary>
        /// 正常系: プロジェクト番号を取得していること
        /// </summary>
        [TestMethod]
        public void JuchuViewModel_初期化_プロジェクト番号を取得()
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            Assert.AreEqual(ProjectNo, viewModel.ProjectNo);
        }

        /// <summary>
        /// 正常系: 施工部門名を取得していること
        /// </summary>
        [TestMethod]
        public void JuchuViewModel_初期化_施工部門名を取得()
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            Assert.AreEqual(SekoBumon, viewModel.SekoBumon);
        }

        /// <summary>
        /// 正常系: 受注日を取得していること
        /// </summary>
        [TestMethod]
        public void JuchuViewModel_初期化_受注日を取得()
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            Assert.AreEqual(JuchuYmd, viewModel.JuchuYmd);
        }

        /// <summary>
        /// 正常系: 物件名を取得していること
        /// </summary>
        [TestMethod]
        public void JuchuViewModel_初期化_物件名を取得()
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            Assert.AreEqual(Bukken, viewModel.Bukken);
        }

        /// <summary>
        /// 正常系: 費用種別名を取得していること
        /// </summary>
        [TestMethod]
        public void JuchuViewModel_初期化_費用種別名を取得()
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            Assert.AreEqual(HiyoShubetuName, viewModel.HiyoShubetuName);
        }

        /// <summary>
        /// 正常系: 受注金額を取得していること
        /// </summary>
        [TestMethod]
        public void JuchuViewModel_初期化_受注金額を取得()
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            Assert.AreEqual(JuchuKin.ToString("N0"), viewModel.JuchuKin);
        }

        /// <summary>
        /// 正常系: 着工日を取得していること
        /// </summary>
        [TestMethod]
        public void JuchuViewModel_初期化_着工日を取得()
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            Assert.AreEqual(ChaYmd, viewModel.ChaYmd);
        }

        /// <summary>
        /// 正常系: 受注番号を取得していること
        /// </summary>
        [DataRow(JuchuNo, DisplayName = "受注番号がNULLでないとき")]
        [DataRow(null, DisplayName = "受注番号がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_受注番号を取得(string? juchuNo)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.JuchuuNo = juchuNo;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (juchuNo == null)
            {
                Assert.IsNull(viewModel.JuchuNo);
            }
            else
            {
                Assert.AreEqual(juchuNo, viewModel.JuchuNo);
            }
        }

        /// <summary>
        /// 正常系: 受注行番号を取得していること
        /// </summary>
        [DataRow(JuchuGyoBangou, DisplayName = "受注行番号がNULLでないとき")]
        [DataRow(null, DisplayName = "受注行番号がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_受注行番号を取得(short? juchuGyoBangou)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.JuchuuGyoNo = juchuGyoBangou;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (juchuGyoBangou == null)
            {
                Assert.IsNull(viewModel.JuchuGyoBangou);
            }
            else
            {
                Assert.AreEqual(juchuGyoBangou.ToString(), viewModel.JuchuGyoBangou);
            }
        }

        /// <summary>
        /// 正常系: 契約状態を取得していること
        /// </summary>
        [DataRow(KeiyakuJotai, DisplayName = "契約状態がNULLでないとき")]
        [DataRow(null, DisplayName = "契約状態がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_契約状態を取得(string? keiyakuJotai)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.KeiyakuJoutaiKbnName = keiyakuJotai;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (keiyakuJotai == null)
            {
                Assert.IsNull(viewModel.KeiyakuJotai);
            }
            else
            {
                Assert.AreEqual(keiyakuJotai, viewModel.KeiyakuJotai);
            }
        }

        /// <summary>
        /// 正常系: 契約先を取得していること
        /// </summary>
        [DataRow(KeiNm, DisplayName = "契約先がNULLでないとき")]
        [DataRow(null, DisplayName = "契約先がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_契約先を取得(string? keiNm)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.KeiNm = keiNm;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (keiNm == null)
            {
                Assert.IsNull(viewModel.KeiNm);
            }
            else
            {
                Assert.AreEqual(keiNm, viewModel.KeiNm);
            }
        }

        /// <summary>
        /// 正常系: 受注先を取得していること
        /// </summary>
        [DataRow(JuchuNm, DisplayName = "受注先がNULLでないとき")]
        [DataRow(null, DisplayName = "受注先がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_受注先を取得(string? juchuNm)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.JucNm = juchuNm;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (juchuNm == null)
            {
                Assert.IsNull(viewModel.JuchuNm);
            }
            else
            {
                Assert.AreEqual(juchuNm, viewModel.JuchuNm);
            }
        }

        /// <summary>
        /// 正常系: 商品名を取得していること
        /// </summary>
        [DataRow(ShohinName, DisplayName = "商品名がNULLでないとき")]
        [DataRow(null, DisplayName = "商品名がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_商品名を取得(string? shohinName)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.ShouhinName = shohinName;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (shohinName == null)
            {
                Assert.IsNull(viewModel.ShohinName);
            }
            else
            {
                Assert.AreEqual(shohinName, viewModel.ShohinName);
            }
        }

        /// <summary>
        /// 正常系: 送担当者を取得していること
        /// </summary>
        [DataRow(OkrTanName, DisplayName = "送担当者がNULLでないとき")]
        [DataRow(null, DisplayName = "送担当者がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_送担当者を取得(string? okrTanName)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.OkrTanNm1 = okrTanName;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (okrTanName == null)
            {
                Assert.IsNull(viewModel.OkrTanName);
            }
            else
            {
                Assert.AreEqual(okrTanName, viewModel.OkrTanName);
            }
        }

        /// <summary>
        /// 正常系: 担当者を取得していること
        /// </summary>
        [DataRow(TanName, DisplayName = "担当者がNULLでないとき")]
        [DataRow(null, DisplayName = "担当者がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_担当者を取得(string? tanName)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.OkrTanNm1 = tanName;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (tanName == null)
            {
                Assert.IsNull(viewModel.TanName);
            }
            else
            {
                Assert.AreEqual(tanName, viewModel.TanName);
            }
        }

        /// <summary>
        /// 正常系: 受担当者を取得していること
        /// </summary>
        [DataRow(UkTanName, DisplayName = "受担当者がNULLでないとき")]
        [DataRow(null, DisplayName = "受担当者がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_受担当者を取得(string? ukTanName)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.UkeTanNm1 = ukTanName;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (ukTanName == null)
            {
                Assert.IsNull(viewModel.UkTanName);
            }
            else
            {
                Assert.AreEqual(ukTanName, viewModel.UkTanName);
            }
        }

        /// <summary>
        /// 正常系: 納期竣工を取得していること
        /// </summary>
        [DataRow(NsyYmd, DisplayName = "納期竣工がNULLでないとき")]
        [DataRow(null, DisplayName = "納期竣工がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_納期竣工を取得(string? nsyYmd)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();

            if (nsyYmd != null)
                juchu.NsyYmd = DateOnly.Parse(nsyYmd);

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (nsyYmd == null)
            {
                Assert.IsNull(viewModel.NsyYmd);
            }
            else
            {
                Assert.AreEqual(nsyYmd, viewModel.NsyYmd!.ToString());
            }
        }

        /// <summary>
        /// 正常系: 売上計画日を取得していること
        /// </summary>
        [DataRow(KurYmd, DisplayName = "売上計画日がNULLでないとき")]
        [DataRow(null, DisplayName = "売上計画日がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_売上計画日を取得(string? kurYmd)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();

            if (kurYmd != null)
                juchu.KurYmd = DateOnly.Parse(kurYmd);

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (kurYmd == null)
            {
                Assert.IsNull(viewModel.KurYmd);
            }
            else
            {
                Assert.AreEqual(kurYmd, viewModel.KurYmd!.ToString());
            }
        }

        /// <summary>
        /// 正常系: 入金計画日を取得していること
        /// </summary>
        [DataRow(KnyYmd, DisplayName = "入金計画日がNULLでないとき")]
        [DataRow(null, DisplayName = "入金計画日がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_入金計画日を取得(string? knyYmd)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();

            if (knyYmd != null)
                juchu.KnyYmd = DateOnly.Parse(knyYmd);

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (knyYmd == null)
            {
                Assert.IsNull(viewModel.KnyYmd);
            }
            else
            {
                Assert.AreEqual(knyYmd, viewModel.KnyYmd!.ToString());
            }
        }

        /// <summary>
        /// 正常系: 原価凍結日を取得していること
        /// </summary>
        [DataRow(ToketuYmd, DisplayName = "原価凍結日がNULLでないとき")]
        [DataRow(null, DisplayName = "原価凍結日がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_原価凍結日を取得(string? toketuYmd)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();

            if (toketuYmd != null)
                juchu.ToketuYmd = DateOnly.Parse(toketuYmd);

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (toketuYmd  == null)
            {
                Assert.IsNull(viewModel.ToketuYmd);
            }
            else
            {
                Assert.AreEqual(toketuYmd, viewModel.ToketuYmd!.ToString());
            }
        }

        /// <summary>
        /// 正常系: 備考を取得していること
        /// </summary>
        [DataRow(Biko, DisplayName = "備考がNULLでないとき")]
        [DataRow(null, DisplayName = "備考がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_備考を取得(string? biko)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.Biko = biko;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            if (biko == null)
            {
                Assert.IsNull(viewModel.Biko);
            }
            else
            {
                Assert.AreEqual(biko, viewModel.Biko);
            }
        }

        /// <summary>
        /// 正常系: 契約状態区分を取得していること
        /// </summary>
        [DataRow(KeiyakuJotaiKbn, DisplayName = "契約状態区分がNULLでないとき")]
        [DataRow(null, DisplayName = "契約状態区分がNULLのとき")]
        [TestMethod]
        public void JuchuViewModel_初期化_契約状態区分を取得(ContractClassification? keiyakuJotaiKbn)
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.KeiyakuJoutaiKbn = keiyakuJotaiKbn;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            Assert.AreEqual(keiyakuJotaiKbn, viewModel.KeiyakuJoutaiKbn);
        }

        /// <summary>
        /// 正常系: 原価凍結がTrueの場合、凍結済みと表示される
        /// </summary>
        [TestMethod]
        public void JuchuViewModel_初期化時に原価凍結がTrue_凍結済みと表示()
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.IsGenkaToketu = true;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            Assert.AreEqual("凍結済み", viewModel.GenkaToketu);
        }

        /// <summary>
        /// 正常系: 原価凍結がFalseの場合、未と表示される
        /// </summary>
        [TestMethod]
        public void JuchuViewModel_初期化時に原価凍結がFalse_未と表示()
        {
            // ================ Arrange ================ //
            var juchu = CreateEntityForViewModelTest();
            juchu.IsGenkaToketu = false;

            // ================ Act ================ //
            var viewModel = new JuchuViewModel()
            {
                Juchu = juchu,
            };

            // ================ Assert ================ //
            Assert.AreEqual("未", viewModel.GenkaToketu);
        }
    }
}
