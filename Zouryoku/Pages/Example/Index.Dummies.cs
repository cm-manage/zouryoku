using Model.Enums;
using Model.Model;

namespace Zouryoku.Pages.Example
{
    public partial class IndexModel
    {
        /// <summary>
        /// ダミーデータ作成
        /// </summary>
        private static Syain CreateDummySyain()
        {
            var dummyTsujoGyomu = new SyukkinKubun { Name = "通常業務" };
            var dummyKyujitsu = new SyukkinKubun { Name = "休日", IsHoliday = true };
            var dummySyain = new Syain
            {
                Name = "〇〇　一郎",
                WorkingHourSyains =
                [
                    new()
                    {
                        Hiduke = new DateOnly(2025, 5, 1),
                        SyukkinTime = new DateTime(2025, 5, 1, 8, 20, 0),
                        TaikinTime = new DateTime(2025, 5, 1, 18, 40, 0),
                        UkagaiHeader = new UkagaiHeader()
                    },
                    new()
                    {
                        Hiduke = new DateOnly(2025, 5, 2),
                        SyukkinTime = new DateTime(2025, 5, 2, 8, 12, 0),
                        TaikinTime = new DateTime(2025, 5, 2, 11, 50, 0)
                    },
                    new()
                    {
                        Hiduke = new DateOnly(2025, 5, 2),
                        SyukkinTime = new DateTime(2025, 5, 2, 13, 0, 0),
                        TaikinTime = new DateTime(2025, 5, 2, 21, 50, 0)
                    },
                    new()
                    {
                        Hiduke = new DateOnly(2025, 5, 7),
                        SyukkinTime = new DateTime(2025, 5, 7, 8, 12, 0),
                        TaikinTime = new DateTime(2025, 5, 7, 21, 50, 0)
                    },
                    new()
                    {
                        Hiduke = new DateOnly(2025, 5, 7),
                        SyukkinTime = new DateTime(2025, 5, 7, 8, 12, 0),
                        TaikinTime = new DateTime(2025, 5, 7, 18, 50, 0)
                    },
                ],
                Nippous =
                [
                    new()
                    {
                        NippouYmd = new DateOnly(2025, 5, 1),
                        SyukkinKubunId1Navigation = dummyTsujoGyomu,
                        SyukkinHm1 = new TimeOnly(8, 30),
                        TaisyutsuHm1 = new TimeOnly(18, 30)
                    },
                    new()
                    {
                        NippouYmd = new DateOnly(2025, 5, 2),
                        SyukkinKubunId1Navigation = dummyTsujoGyomu,
                        SyukkinHm1 = new TimeOnly(8, 30),
                        TaisyutsuHm1 = new TimeOnly(12, 0),
                        SyukkinHm2 = new TimeOnly(12, 45),
                        TaisyutsuHm2 = new TimeOnly(22, 50)
                    },
                    new()
                    {
                        NippouYmd = new DateOnly(2025, 5, 3),
                        SyukkinKubunId1Navigation = dummyKyujitsu
                    },
                    new()
                    {
                        NippouYmd = new DateOnly(2025, 5, 4),
                        SyukkinKubunId1Navigation = dummyKyujitsu
                    },
                    new()
                    {
                        NippouYmd = new DateOnly(2025, 5, 5),
                        SyukkinKubunId1Navigation = dummyKyujitsu
                    },
                    new()
                    {
                        NippouYmd = new DateOnly(2025, 5, 6),
                        SyukkinKubunId1Navigation = dummyKyujitsu
                    },
                    new()
                    {
                        NippouYmd = new DateOnly(2025, 5, 7),
                        SyukkinKubunId1Navigation = dummyTsujoGyomu,
                        SyukkinHm1 = new TimeOnly(8, 30),
                        TaisyutsuHm1 = new TimeOnly(21, 50)
                    },
                    new()
                    {
                        NippouYmd = new DateOnly(2025, 5, 8),
                        SyukkinKubunId1Navigation = dummyTsujoGyomu,
                        SyukkinHm1 = new TimeOnly(8, 30),
                        TaisyutsuHm1 = new TimeOnly(18, 50)
                    }
                ],
                PcLogs =
                [
                    new()
                    {
                        PcName = "PC-2962",
                        Datetime = new DateTime(2025, 5, 1, 8, 19, 0),
                        Operation = PcOperationType.ログオン
                    },
                    new()
                    {
                        PcName = "PC-2962",
                        Datetime = new DateTime(2025, 5, 1, 18, 41, 0),
                        Operation = PcOperationType.ログオフ
                    },
                    new()
                    {
                        PcName = "PC-2962",
                        Datetime = new DateTime(2025, 5, 2, 8, 12, 0),
                        Operation = PcOperationType.ログオン
                    },
                    new()
                    {
                        PcName = "PC-2962",
                        Datetime = new DateTime(2025, 5, 2, 11, 50, 0),
                        Operation = PcOperationType.ログオフ
                    },
                    new()
                    {
                        PcName = "PC-2962",
                        Datetime = new DateTime(2025, 5, 2, 13, 0, 0),
                        Operation = PcOperationType.ログオン
                    },
                    new()
                    {
                        PcName = "PC-2962",
                        Datetime = new DateTime(2025, 5, 2, 21, 50, 0),
                        Operation = PcOperationType.ログオフ
                    },
                    new()
                    {
                        PcName = "PC-2962",
                        Datetime = new DateTime(2025, 5, 7, 8, 11, 0),
                        Operation = PcOperationType.ログオン
                    },
                    new()
                    {
                        PcName = "PC-2962",
                        Datetime = new DateTime(2025, 5, 7, 21, 51, 0),
                        Operation = PcOperationType.ログオフ
                    },
                    new()
                    {
                        PcName = "PC-2962",
                        Datetime = new DateTime(2025, 5, 8, 8, 11, 0),
                        Operation = PcOperationType.ログオン
                    },
                    new()
                    {
                        PcName = "PC-2962",
                        Datetime = new DateTime(2025, 5, 8, 18, 51, 0),
                        Operation = PcOperationType.ログオフ
                    },
                ],
                UkagaiHeaders =
                [
                    new()
                    {
                        ShinseiYmd = new DateOnly(2025, 5, 7),
                        UkagaiShinseis =
                        [
                            new()
                            {
                                UkagaiSyubetsu = InquiryType.リフレッシュデー残業
                            }
                        ]
                    }
                ]
            };

            return dummySyain;
        }
    }
}
