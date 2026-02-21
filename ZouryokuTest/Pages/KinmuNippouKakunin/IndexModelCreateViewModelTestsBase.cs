using Model.Model;
using System.Collections;
using Zouryoku.Pages.KinmuNippouKakunin;
using static Model.Enums.HolidayFlag;

namespace ZouryokuTest.Pages.KinmuNippouKakunin
{
    /// <summary>
    /// 勤務日報確認画面 ViewModel 関係のユニットテスト用ヘルパーメソッド実装クラス
    /// </summary>
    public abstract partial class IndexModelCreateViewModelTestsBase : IndexModelTestsBase
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Arrange用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// 出勤区分情報をメモリ上で作成します。データベースへの登録は行われません。
        /// </summary>
        protected static SyukkinKubun CreateSyukkinKubun(string name = NotNullConstraintPlaceholder)
        {
            var filledName = GetOrNotNullConstraintPlaceholderString(name);

            return new SyukkinKubun
            {
                Id = default,
                CodeString = "00",
                Name = filledName,
                NameRyaku = NotNullConstraintPlaceholder,
                IsSyukkin = default,
                IsVacation = default,
                IsHoliday = default,
                IsNeedKubun1 = default,
                IsNeedKubun2 = default,
            };
        }

        /// <summary>
        /// 日報実績情報をメモリ上で作成します。データベースへの登録は行われません。
        /// </summary>
        /// <param name="syain">日報実績の社員</param>
        /// <param name="nippouYmd">日報実績の日付</param>
        /// <returns>作成された <see cref="Nippou"/> エンティティ。</returns>
        protected static Nippou CreateNippou(Syain syain, DateOnly nippouYmd, SyukkinKubun syukkinKubun1) => new()
        {
            Id = default,
            SyainId = default,
            NippouYmd = nippouYmd,
            Youbi = default,
            SyukkinHm1 = default,
            TaisyutsuHm1 = default,
            SyukkinHm2 = default,
            TaisyutsuHm2 = default,
            SyukkinHm3 = default,
            TaisyutsuHm3 = default,
            HJitsudou = default,
            HZangyo = default,
            HWarimashi = default,
            HShinyaZangyo = default,
            DJitsudou = default,
            DZangyo = default,
            DWarimashi = default,
            DShinyaZangyo = default,
            NJitsudou = default,
            NShinya = default,
            TotalZangyo = default,
            KaisyaCode = default,
            IsRendouZumi = default,
            RendouYmd = default,
            TourokuKubun = default,
            KakuteiYmd = default,
            SyukkinKubunId1 = default,
            SyukkinKubunId2 = default,
            Syain = syain,
            SyukkinKubunId1Navigation = syukkinKubun1,
        };

        /// <summary>
        /// 祝祭日の非稼働日情報をメモリ上で作成します。データベースへの登録は行われません。
        /// </summary>
        protected static Hikadoubi CreateHikadoubi(DateOnly ymd) => new()
        {
            Id = default,
            Ymd = ymd,
            SyukusaijitsuFlag = 祝祭日,
            RefreshDay = default
        };

        /// <summary>
        /// 複数の部署マスタデータをメモリ上で作成します。データベースへの登録は行われません。
        /// 「ID：1、親ID：空、部署Baseの部門長ID：空」
        /// ├「ID：2、親ID：1、部署Baseの部門長ID：空」
        /// └「ID：3、親ID：1、部署Baseの部門長ID：3」
        /// 　├「ID：4、親ID：3、部署Baseの部門長ID：4」
        /// 　└「ID：5、親ID：3、部署Baseの部門長ID：空」
        /// </summary>
        /// <returns>追加した部署マスタデータ</returns>
        protected static BusyoHierarchy CreateBusyoHierarchy()
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
            return new BusyoHierarchy(busyo1, busyo2, busyo3, busyo4, busyo5);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Assert用ヘルパーメソッド
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 有効系アサーション
        /// </summary>
        protected static void AssertPopulatesList(
            Syain expectedTargetSyain, BusyoHierarchy expectedBusyos, DateOnly expectedYm,
            IndexModel.DaysViewModel? actualViewModel)
        {
            Assert.IsNotNull(actualViewModel, "ViewModel が null であってはなりません。");
            Assert.HasCount(2, actualViewModel.TargetSyainHierarchicalBusyoNames, "TargetSyainHierarchicalBusyoNames が一致しません。");
            Assert.AreEqual(
                expectedBusyos.Busyo1.Name,
                actualViewModel.TargetSyainHierarchicalBusyoNames[0],
                "TargetSyainHierarchicalBusyoNames の1件目が一致しません。");
            Assert.AreEqual(
                expectedBusyos.Busyo3.Name,
                actualViewModel.TargetSyainHierarchicalBusyoNames[1],
                "TargetSyainHierarchicalBusyoNames の2件目が一致しません。");
            Assert.AreEqual(expectedTargetSyain.Name, actualViewModel.TargetSyainName, "TargetSyainName が一致しません。");

            var daysInMonth = DateTime.DaysInMonth(expectedYm.Year, expectedYm.Month);
            var expectedViewModel = new IndexModel.DaysViewModel
            {
                TargetSyainName = expectedTargetSyain.Name,
                TargetSyainHierarchicalBusyoNames = [expectedBusyos.Busyo1.Name, expectedBusyos.Busyo3.Name],
                Days =
                [
                    new IndexModel.Day(
                        new DateOnly(expectedYm.Year, expectedYm.Month, 1),
                        NippouSet.Hikadoubi1,
                        new Nippou
                        {
                            SyukkinKubunId1Navigation = new SyukkinKubun
                            {
                                Name = NippouSet.SyukkinKubunName1
                            },
                            SyukkinKubunId2Navigation = new SyukkinKubun
                            {
                                Name = NippouSet.SyukkinKubunName2
                            },
                            SyukkinHm1 = NippouSet.SyukkinHm1,
                            TaisyutsuHm1 = NippouSet.TaisyutsuHm1,
                            SyukkinHm2 = NippouSet.SyukkinHm2,
                            TaisyutsuHm2 = NippouSet.TaisyutsuHm2,
                            SyukkinHm3 = NippouSet.SyukkinHm3,
                            TaisyutsuHm3 = NippouSet.TaisyutsuHm3,
                            HZangyo = NippouSet.HZangyo,
                            HWarimashi = NippouSet.HWarimashi,
                            HShinyaZangyo = NippouSet.HShinyaZangyo,
                            DZangyo = NippouSet.DZangyo,
                            DWarimashi = NippouSet.DWarimashi,
                            DShinyaZangyo = NippouSet.DShinyaZangyo,
                            NJitsudou = NippouSet.NJitsudou,
                            NShinya = NippouSet.NShinya,
                            NippouAnkens =
                            [
                                new NippouAnken
                                {
                                    Ankens = new Anken
                                    {
                                        Name = NippouSet.AnkenName1,
                                        KingsJuchu = new KingsJuchu
                                        {
                                            ChaYmd = NippouSet.ChaYmd1,
                                            ProjectNo = NippouSet.ProjectNo1,
                                            JuchuuNo = NippouSet.JuchuuNo1,
                                            JuchuuGyoNo = NippouSet.JuchuuGyoNo1
                                        }
                                    }
                                },
                                new NippouAnken
                                {
                                    Ankens = new Anken
                                    {
                                        Name = NippouSet.AnkenName2,
                                        KingsJuchu = new KingsJuchu
                                        {
                                            ChaYmd = NippouSet.ChaYmd2,
                                            ProjectNo = NippouSet.ProjectNo2,
                                            JuchuuNo = NippouSet.JuchuuNo2,
                                            JuchuuGyoNo = NippouSet.JuchuuGyoNo2
                                        }
                                    }
                                },
                                new NippouAnken
                                {
                                    Ankens = new Anken
                                    {
                                        Name = NippouSet.AnkenName3,
                                        KingsJuchu = new KingsJuchu
                                        {
                                            ChaYmd = NippouSet.ChaYmd3,
                                            ProjectNo = NippouSet.ProjectNo3,
                                            JuchuuNo = NippouSet.JuchuuNo3,
                                            JuchuuGyoNo = NippouSet.JuchuuGyoNo3
                                        }
                                    }
                                },
                            ]
                        }),

                    // 1日と月末日はNippouが存在するケースを想定しているため、2日から月末の前日までを生成
                    .. Enumerable.Range(2, daysInMonth - 2)
                        .Select(day => new IndexModel.Day(new DateOnly(expectedYm.Year, expectedYm.Month, day), false)),

                    new IndexModel.Day(
                        new DateOnly(expectedYm.Year, expectedYm.Month, daysInMonth),
                        NippouSet.Hikadoubi2,
                        new Nippou
                        {
                            SyukkinKubunId1Navigation = new SyukkinKubun
                            {
                                Name = NippouSet.SyukkinKubunName3
                            },
                            SyukkinKubunId2Navigation = null,
                            SyukkinHm1 = null,
                            TaisyutsuHm1 = null,
                            SyukkinHm2 = null,
                            TaisyutsuHm2 = null,
                            SyukkinHm3 = null,
                            TaisyutsuHm3 = null,
                            HZangyo = null,
                            HWarimashi = null,
                            HShinyaZangyo = null,
                            DZangyo = null,
                            DWarimashi = null,
                            DShinyaZangyo = null,
                            NJitsudou = null,
                            NShinya = null,
                            NippouAnkens = []
                        })
                ]
            };
            AssertViewModelEqual(expectedViewModel, actualViewModel);
        }

        /// <summary>
        /// 無効系アサーション
        /// </summary>
        protected static void AssertSetsEmptyList(
            Syain expectedTargetSyain, BusyoHierarchy expectedBusyos, DateOnly expectedYm,
            IndexModel.DaysViewModel? actualViewModel)
        {
            Assert.IsNotNull(actualViewModel, "ViewModel が null であってはなりません。");
            Assert.HasCount(2, actualViewModel.TargetSyainHierarchicalBusyoNames, "TargetSyainHierarchicalBusyoNames が一致しません。");
            Assert.AreEqual(
                expectedBusyos.Busyo1.Name,
                actualViewModel.TargetSyainHierarchicalBusyoNames[0],
                "TargetSyainHierarchicalBusyoNames の1件目が一致しません。");
            Assert.AreEqual(
                expectedBusyos.Busyo3.Name,
                actualViewModel.TargetSyainHierarchicalBusyoNames[1],
                "TargetSyainHierarchicalBusyoNames の2件目が一致しません。");
            Assert.AreEqual(expectedTargetSyain.Name, actualViewModel.TargetSyainName, "TargetSyainName が一致しません。");

            var daysInMonth = DateTime.DaysInMonth(expectedYm.Year, expectedYm.Month);
            var expectedViewModel = new IndexModel.DaysViewModel
            {
                TargetSyainName = expectedTargetSyain.Name,
                TargetSyainHierarchicalBusyoNames = [expectedBusyos.Busyo1.Name, expectedBusyos.Busyo3.Name],
                Days = [.. Enumerable.Range(1, daysInMonth)
                     .Select(day => new IndexModel.Day(new DateOnly(expectedYm.Year, expectedYm.Month, day), false))]
            };
            AssertViewModelEqual(expectedViewModel, actualViewModel);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // 内部レコード・内部クラス
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// <see cref="CreateBusyoHierarchy()"/> の戻り値
        /// </summary>
        protected record BusyoHierarchy(Busyo Busyo1, Busyo Busyo2, Busyo Busyo3, Busyo Busyo4, Busyo Busyo5) : IEnumerable<Busyo>
        {
            /// <inheritdoc />
            public IEnumerator<Busyo> GetEnumerator()
            {
                yield return Busyo1;
                yield return Busyo2;
                yield return Busyo3;
                yield return Busyo4;
                yield return Busyo5;
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
