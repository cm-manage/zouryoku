using Model.Model;
using System.Collections;

namespace ZouryokuTest.Pages.KinmuNippouKakunin
{
    public partial class IndexModelCreateViewModelTestsBase
    {
        protected record NippouSet(
            Nippou Nippou1, Nippou Nippou2, Nippou Nippou3, Nippou Nippou4, Nippou Nippou5, Nippou Nippou6, Nippou Nippou7, Nippou Nippou8)
            : IEnumerable<Nippou>
        {
            public static TimeOnly SyukkinHm1 => new(8, 30);
            public static TimeOnly TaisyutsuHm1 => new(9, 30);
            public static TimeOnly SyukkinHm2 => new(10, 30);
            public static TimeOnly TaisyutsuHm2 => new(11, 30);
            public static TimeOnly SyukkinHm3 => new(12, 30);
            public static TimeOnly TaisyutsuHm3 => new(13, 30);
            public static decimal HZangyo => 0.1m;
            public static decimal HWarimashi => 0.2m;
            public static decimal HShinyaZangyo => 0.3m;
            public static decimal DZangyo => 0.4m;
            public static decimal DWarimashi => 0.5m;
            public static decimal DShinyaZangyo => 0.6m;
            public static decimal NJitsudou => 0.7m;
            public static decimal NShinya => 0.8m;
            public static string AnkenName1 => "案件1";
            public static DateOnly ChaYmd1 => new(2025, 1, 1);
            public static string ProjectNo1 => "11111-111111";
            public static string JuchuuNo1 => "A11111111111";
            public static short JuchuuGyoNo1 => 1;
            public static string AnkenName2 => "案件2";
            public static DateOnly ChaYmd2 => new(2025, 2, 1);
            public static string ProjectNo2 => "22222-222222";
            public static string JuchuuNo2 => "B22222222222";
            public static short JuchuuGyoNo2 => 2;
            public static string AnkenName3 => "案件3";
            public static DateOnly ChaYmd3 => new(2025, 3, 1);
            public static string ProjectNo3 => "33333-333333";
            public static string JuchuuNo3 => "C33333333333";
            public static short JuchuuGyoNo3 => 3;
            public static string SyukkinKubunName1 => "出勤区分1";
            public static string SyukkinKubunName2 => "出勤区分2";
            public static string SyukkinKubunName3 => "出勤区分3";
            public static bool Hikadoubi1 => true;
            public static bool Hikadoubi2 => false;

            /// <inheritdoc />
            public IEnumerator<Nippou> GetEnumerator()
            {
                yield return Nippou1;
                yield return Nippou2;
                yield return Nippou3;
                yield return Nippou4;
                yield return Nippou5;
                yield return Nippou6;
                yield return Nippou7;
                yield return Nippou8;
            }

            /// <inheritdoc />
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            /// 複数の日報実績データをメモリ上で作成します。データベースへの登録は行われません。
            /// ①「社員ID：1、実績年月日：前月の最終日」
            /// ②「社員ID：1、実績年月日：月の1日」
            /// ③「社員ID：1、実績年月日：月の最終日」
            /// ④「社員ID：1、実績年月日：次月の1日」
            /// ⑤「社員ID：2、実績年月日：前月の最終日」
            /// ⑥「社員ID：2、実績年月日：月の1日」
            /// ⑦「社員ID：2、実績年月日：月の最終日」
            /// ⑧「社員ID：2、実績年月日：次月の1日」
            /// </summary>
            /// <returns>追加した日報実績データ</returns>
            public static NippouSet Create(Syain targetSyain, Syain otherSyain, DateOnly targetYm)
            {
                var firstDayOfMonth = new DateOnly(targetYm.Year, targetYm.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                var syukkinKubun3 = CreateSyukkinKubun(SyukkinKubunName3);
                var nippou1 = CreateEmptyItem(targetSyain, firstDayOfMonth.AddDays(-1), syukkinKubun3);
                var nippou2 = CreateItem(targetSyain, firstDayOfMonth); // 取得対象
                var nippou3 = CreateEmptyItem(targetSyain, lastDayOfMonth, syukkinKubun3); // 取得対象
                var nippou4 = CreateEmptyItem(targetSyain, lastDayOfMonth.AddDays(1), syukkinKubun3);
                var nippou5 = CreateEmptyItem(otherSyain, firstDayOfMonth.AddDays(-1), syukkinKubun3);
                var nippou6 = CreateEmptyItem(otherSyain, firstDayOfMonth, syukkinKubun3);
                var nippou7 = CreateEmptyItem(otherSyain, lastDayOfMonth, syukkinKubun3);
                var nippou8 = CreateEmptyItem(otherSyain, lastDayOfMonth.AddDays(1), syukkinKubun3);
                return new NippouSet(nippou1, nippou2, nippou3, nippou4, nippou5, nippou6, nippou7, nippou8);
            }

            /// <summary>
            /// 勤務日報確認画面で使用するすべての項目を設定した日報実績情報をメモリ上で作成します。データベースへの登録は行われません。
            /// </summary>
            private static Nippou CreateItem(Syain syain, DateOnly nippouYmd) => new()
            {
                Id = default,
                SyainId = default,
                NippouYmd = nippouYmd,
                Youbi = default,
                SyukkinHm1 = SyukkinHm1,
                TaisyutsuHm1 = TaisyutsuHm1,
                SyukkinHm2 = SyukkinHm2,
                TaisyutsuHm2 = TaisyutsuHm2,
                SyukkinHm3 = SyukkinHm3,
                TaisyutsuHm3 = TaisyutsuHm3,
                HJitsudou = default,
                HZangyo = HZangyo,
                HWarimashi = HWarimashi,
                HShinyaZangyo = HShinyaZangyo,
                DJitsudou = default,
                DZangyo = DZangyo,
                DWarimashi = DWarimashi,
                DShinyaZangyo = DShinyaZangyo,
                NJitsudou = NJitsudou,
                NShinya = NShinya,
                TotalZangyo = default,
                KaisyaCode = default,
                IsRendouZumi = default,
                RendouYmd = default,
                TourokuKubun = default,
                KakuteiYmd = default,
                SyukkinKubunId1 = default,
                SyukkinKubunId2 = default,
                NippouAnkens =
                [
                    new NippouAnken
                    {
                        Id = default,
                        NippouId = default,
                        AnkensId = default,
                        KokyakuName = NotNullConstraintPlaceholder,
                        AnkenName = NotNullConstraintPlaceholder,
                        JissekiJikan = default,
                        KokyakuKaisyaId = default,
                        BumonProcessId = default,
                        IsLinked = default,
                        Ankens = new Anken
                        {
                            Id = 3, // ID並び順を検証するため、IDは小さいほど後に設定する
                            Name = AnkenName3,
                            Naiyou = default,
                            KokyakuKaisyaId = default,
                            KingsJuchuId = default,
                            JyutyuSyuruiId = default,
                            SyainBaseId = default,
                            SearchName = NotNullConstraintPlaceholder,
                            KingsJuchu = new KingsJuchu
                            {
                                Id = default,
                                JucYmd = default,
                                EntYmd = default,
                                KeiNm = default,
                                KeiKn = default,
                                JucNm = default,
                                JucKn = default,
                                Bukken = NotNullConstraintPlaceholder,
                                IriBusCd = default,
                                JucKin = default,
                                OkrTanCd1 = default,
                                OkrTanNm1 = default,
                                UkeTanCd1 = default,
                                UkeTanNm1 = default,
                                ChaYmd = ChaYmd3,
                                NsyYmd = default,
                                KurYmd = default,
                                KnyYmd = default,
                                Biko = default,
                                JsriCd = default,
                                JucCd = default,
                                ProjectNo = ProjectNo3,
                                JuchuuNo = JuchuuNo3,
                                JuchuuGyoNo = JuchuuGyoNo3,
                                KeiyakuJoutaiKbnName = default,
                                SekouBumonCd = NotNullConstraintPlaceholder,
                                HiyouShubetuCd = default,
                                HiyouShubetuCdName = NotNullConstraintPlaceholder,
                                IsGenkaToketu = default,
                                ToketuYmd = default,
                                Nendo = default,
                                ShouhinName = default,
                                BusyoId = default,
                                SearchKeiNm = default,
                                SearchKeiKn = default,
                                SearchJucNm = default,
                                SearchJucKn = default,
                                SearchBukken = NotNullConstraintPlaceholder,
                            }
                        }
                    },
                    new NippouAnken
                    {
                        Id = default,
                        NippouId = default,
                        AnkensId = default,
                        KokyakuName = NotNullConstraintPlaceholder,
                        AnkenName = NotNullConstraintPlaceholder,
                        JissekiJikan = default,
                        KokyakuKaisyaId = default,
                        BumonProcessId = default,
                        IsLinked = default,
                        Ankens = new Anken
                        {
                            Id = 2, // ID並び順を検証するため、IDは小さいほど後に設定する
                            Name = AnkenName2,
                            Naiyou = default,
                            KokyakuKaisyaId = default,
                            KingsJuchuId = default,
                            JyutyuSyuruiId = default,
                            SyainBaseId = default,
                            SearchName = NotNullConstraintPlaceholder,
                            KingsJuchu = new KingsJuchu
                            {
                                Id = default,
                                JucYmd = default,
                                EntYmd = default,
                                KeiNm = default,
                                KeiKn = default,
                                JucNm = default,
                                JucKn = default,
                                Bukken = NotNullConstraintPlaceholder,
                                IriBusCd = default,
                                JucKin = default,
                                OkrTanCd1 = default,
                                OkrTanNm1 = default,
                                UkeTanCd1 = default,
                                UkeTanNm1 = default,
                                ChaYmd = ChaYmd2,
                                NsyYmd = default,
                                KurYmd = default,
                                KnyYmd = default,
                                Biko = default,
                                JsriCd = default,
                                JucCd = default,
                                ProjectNo = ProjectNo2,
                                JuchuuNo = JuchuuNo2,
                                JuchuuGyoNo = JuchuuGyoNo2,
                                KeiyakuJoutaiKbnName = default,
                                SekouBumonCd = NotNullConstraintPlaceholder,
                                HiyouShubetuCd = default,
                                HiyouShubetuCdName = NotNullConstraintPlaceholder,
                                IsGenkaToketu = default,
                                ToketuYmd = default,
                                Nendo = default,
                                ShouhinName = default,
                                BusyoId = default,
                                SearchKeiNm = default,
                                SearchKeiKn = default,
                                SearchJucNm = default,
                                SearchJucKn = default,
                                SearchBukken = NotNullConstraintPlaceholder,
                            }
                        }
                    },
                    new NippouAnken
                    {
                        Id = default,
                        NippouId = default,
                        AnkensId = default,
                        KokyakuName = NotNullConstraintPlaceholder,
                        AnkenName = NotNullConstraintPlaceholder,
                        JissekiJikan = default,
                        KokyakuKaisyaId = default,
                        BumonProcessId = default,
                        IsLinked = default,
                        Ankens = new Anken
                        {
                            Id = 1, // ID並び順を検証するため、IDは小さいほど後に設定する
                            Name = AnkenName1,
                            Naiyou = default,
                            KokyakuKaisyaId = default,
                            KingsJuchuId = default,
                            JyutyuSyuruiId = default,
                            SyainBaseId = default,
                            SearchName = NotNullConstraintPlaceholder,
                            KingsJuchu = new KingsJuchu
                            {
                                Id = default,
                                JucYmd = default,
                                EntYmd = default,
                                KeiNm = default,
                                KeiKn = default,
                                JucNm = default,
                                JucKn = default,
                                Bukken = NotNullConstraintPlaceholder,
                                IriBusCd = default,
                                JucKin = default,
                                OkrTanCd1 = default,
                                OkrTanNm1 = default,
                                UkeTanCd1 = default,
                                UkeTanNm1 = default,
                                ChaYmd = ChaYmd1,
                                NsyYmd = default,
                                KurYmd = default,
                                KnyYmd = default,
                                Biko = default,
                                JsriCd = default,
                                JucCd = default,
                                ProjectNo = ProjectNo1,
                                JuchuuNo = JuchuuNo1,
                                JuchuuGyoNo = JuchuuGyoNo1,
                                KeiyakuJoutaiKbnName = default,
                                SekouBumonCd = NotNullConstraintPlaceholder,
                                HiyouShubetuCd = default,
                                HiyouShubetuCdName = NotNullConstraintPlaceholder,
                                IsGenkaToketu = default,
                                ToketuYmd = default,
                                Nendo = default,
                                ShouhinName = default,
                                BusyoId = default,
                                SearchKeiNm = default,
                                SearchKeiKn = default,
                                SearchJucNm = default,
                                SearchJucKn = default,
                                SearchBukken = NotNullConstraintPlaceholder,
                            }
                        }
                    }
                ],
                Syain = syain,
                SyukkinKubunId1Navigation = CreateSyukkinKubun(SyukkinKubunName1),
                SyukkinKubunId2Navigation = CreateSyukkinKubun(SyukkinKubunName2),
            };

            /// <summary>
            /// 必須項目のみを設定した日報実績情報をメモリ上で作成します。データベースへの登録は行われません。
            /// </summary>
            private static Nippou CreateEmptyItem(Syain syain, DateOnly nippouYmd, SyukkinKubun syukkinKubun1)
            {
                return new Nippou
                {
                    Id = default,
                    SyainId = default,
                    NippouYmd = nippouYmd,
                    Youbi = default,
                    SyukkinHm1 = null,
                    TaisyutsuHm1 = null,
                    SyukkinHm2 = null,
                    TaisyutsuHm2 = null,
                    SyukkinHm3 = null,
                    TaisyutsuHm3 = null,
                    HJitsudou = default,
                    HZangyo = null,
                    HWarimashi = null,
                    HShinyaZangyo = null,
                    DJitsudou = default,
                    DZangyo = null,
                    DWarimashi = null,
                    DShinyaZangyo = null,
                    NJitsudou = null,
                    NShinya = null,
                    TotalZangyo = default,
                    KaisyaCode = default,
                    IsRendouZumi = default,
                    RendouYmd = default,
                    TourokuKubun = default,
                    KakuteiYmd = default,
                    SyukkinKubunId1 = default,
                    SyukkinKubunId2 = default,
                    NippouAnkens = [],
                    Syain = syain,
                    SyukkinKubunId1Navigation = syukkinKubun1,
                    SyukkinKubunId2Navigation = null,
                };
            }
        }
    }
}
