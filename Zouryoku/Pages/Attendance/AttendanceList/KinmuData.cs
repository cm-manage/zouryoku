using CommonLibrary.Extensions;
using Model.Enums;
using Model.Model;
using static Model.Enums.AttendanceClassification;
using static Model.Enums.HolidayFlag;
using static Model.Enums.InquiryType;
using static Model.Enums.PcOperationType;
using static Zouryoku.Pages.Attendance.AttendanceList.StyleYoubiColorClasses;
using static Zouryoku.Utils.Const;

namespace Zouryoku.Pages.Attendance.AttendanceList
{
    public class KinmuData
    {
        public Syain SyainData { get; set; } = new();
        public DateOnly Hiduke { get; set; } = default;
        public Hikadoubi? HikadoubiData { get; set; } = default;
        public List<WorkingHour> WorkingHours { get; set; } = [];
        public Nippou? NippouData { get; set; } = default;
        public List<UkagaiShinsei> UkagaiShinseis { get; set; } = [];
        public List<PcLog> PcLogs { get; set; } = [];

        // 日付
        public string GetHiduke => $"{Hiduke:MM/dd}({Hiduke.DayOfWeek.ToJpShortString()})";

        /// <summary>
        /// 曜日表示用色クラス取得
        /// </summary>
        /// <returns>曜日表示用色クラス</returns>
        public string GetYoubisyoku()
        {
            if ((HikadoubiData?.SyukusaijitsuFlag ?? それ以外) == 祝祭日)
                return Holiday;

            return Hiduke.DayOfWeek switch
            {
                DayOfWeek.Sunday => Sunday,
                DayOfWeek.Saturday => Saturday,
                _ => Weekday,
            };
        }

        /// <summary>
        /// 打刻修正権限
        /// </summary>
        /// <param name="loginUser">ログインユーザー</param>
        /// <returns>打刻修正権限</returns>
        public bool IsDakoku(Syain loginUser, DateOnly today)
            => (Hiduke <= today &&
                NippouData == null &&
               (loginUser.IsCorrectingTimeStamps ||
                loginUser.SyainBaseId == SyainData.SyainBaseId));

        /// <summary>
        /// 表示用出退記録取得
        /// </summary>
        /// <param name="loginUser">ログインユーザー</param>
        /// <returns>表示用出退記録</returns>
        public SyuttaiKirokuData GetSyuttaiKirokuData(Syain loginUser)
        {
            var result = new SyuttaiKirokuData();

            // ガード節：出勤退勤データがなければ即返す
            if (WorkingHours.Count == 0)
                return result;

            // 伺い申請に夜間作業・深夜作業があるか
            bool isYakan = UkagaiShinseis.Any(x => x.UkagaiSyubetsu == 夜間作業);
            bool isShinya = UkagaiShinseis.Any(x => x.UkagaiSyubetsu == 深夜作業);

            // 出勤時間でソートし、上位3件を取得
            var list = WorkingHours
                .OrderBy(x => x.SyukkinTime)
                .Take(3)
                .ToList();

            // スロット定義
            // 各スロットに対応するアクションを設定
            // スロット数は出退勤1～3に対応
            var slots = new[]
            {
                new SyuttaiSlot {
                    SetSyukkin = v => result.SyukkinJikan1 = v,
                    SetTaikin  = v => result.TaikinJikan1 = v,
                    SetSyukkinPos = v => result.SyukkinPos1 = v,
                    SetTaikinPos  = v => result.TaikinPos1 = v,
                    SetHimaSyukkin = v => result.IsHimatagiSyukkin1 = v,
                    SetHimaTaikin  = v => result.IsHimatagiTaikin1 = v
                },
                new SyuttaiSlot {
                    SetSyukkin = v => result.SyukkinJikan2 = v,
                    SetTaikin  = v => result.TaikinJikan2 = v,
                    SetSyukkinPos = v => result.SyukkinPos2 = v,
                    SetTaikinPos  = v => result.TaikinPos2 = v,
                    SetHimaSyukkin = _ => {},
                    SetHimaTaikin  = v => result.IsHimatagiTaikin2 = v
                },
                new SyuttaiSlot {
                    SetSyukkin = v => result.SyukkinJikan3 = v,
                    SetTaikin  = v => result.TaikinJikan3 = v,
                    SetSyukkinPos = v => result.SyukkinPos3 = v,
                    SetTaikinPos  = v => result.TaikinPos3 = v,
                    SetHimaSyukkin = _ => {},
                    SetHimaTaikin  = v => result.IsHimatagiTaikin3 = v
                },
            };

            // 各スロットに値を設定
            // 取得した出退勤データをスロットにマッピング
            // 出勤時間、退勤時間、位置情報、日跨ぎフラグを設定
            for (int i = 0; i < list.Count; i++)
            {
                var w = list[i];
                var slot = slots[i];

                // 出勤時間・退勤時間設定
                slot.SetSyukkin(w.SyukkinTime?.ToHHmm() ?? "");
                slot.SetTaikin(w.TaikinTime?.ToHHmm() ?? "");

                // 位置情報設定（権限がある場合のみ）
                if (loginUser.IsCheckStampPosition)
                {
                    slot.SetSyukkinPos(w.GetSyukkinPosition);
                    slot.SetTaikinPos(w.GetTaikinPosition);
                }

                // 日跨ぎフラグ設定
                slot.SetHimaSyukkin(isYakan && w.SyukkinTime == null);
                slot.SetHimaTaikin(isShinya && w.SyukkinTime != null && w.TaikinTime == null);
            }

            return result;
        }

        // 表示用出退記録取得用クラス
        // スロットごとにアクションをまとめる
        private class SyuttaiSlot
        {
            // 出勤アクション
            public required Action<string> SetSyukkin { get; init; }
            // 退勤アクション
            public required Action<string> SetTaikin { get; init; }
            // 出勤位置アクション
            public required Action<string> SetSyukkinPos { get; init; }
            // 退勤位置アクション
            public required Action<string> SetTaikinPos { get; init; }
            // 日跨ぎ出勤アクション
            public required Action<bool> SetHimaSyukkin { get; init; }
            // 日跨ぎ退勤アクション
            public required Action<bool> SetHimaTaikin { get; init; }
        }
        /// <summary>
        /// 表示用日報取得
        /// </summary>
        /// <param name="loginUser">ログインユーザー</param>
        /// <returns>表示用日報</returns>
        public NippouData GetNippouData(Syain loginUser)
        {
            var result = new NippouData();

            // 日報がなければ抜ける
            if (NippouData == null)
                return result;

            // 出勤区分
            if (NippouData.SyukkinKubunId1Navigation != null)
            {
                result.SyukkinKubunList
                           .Add(GetSyukkinKubunName(loginUser.SyainBaseId,
                                                       SyainData.SyainBaseId,
                                                       NippouData.SyukkinKubunId1Navigation));
            }
            if (NippouData.SyukkinKubunId2Navigation != null)
            {
                result.SyukkinKubunList
                           .Add(GetSyukkinKubunName(loginUser.SyainBaseId,
                                                       SyainData.SyainBaseId,
                                                       NippouData.SyukkinKubunId2Navigation));
            }

            // 出勤時間１
            result.Syukkin1 = NippouData.SyukkinHm1.ToStrByHHmmOrEmpty();

            // 出勤時間２
            result.Syukkin2 = NippouData.SyukkinHm2.ToStrByHHmmOrEmpty();

            // 出勤時間３
            result.Syukkin3 = NippouData.SyukkinHm3.ToStrByHHmmOrEmpty();

            // 退出時間１
            result.Taisyutsu1 = NippouData.TaisyutsuHm1.ToStrByHHmmOrEmpty();

            // 退出時間２
            result.Taisyutsu2 = NippouData.TaisyutsuHm2.ToStrByHHmmOrEmpty();

            // 退出時間３
            result.Taisyutsu3 = NippouData.TaisyutsuHm3.ToStrByHHmmOrEmpty();
            
            return result;
        }

        /// <summary>
        /// 出勤区分名取得
        /// </summary>
        /// <param name="loginSyainBaseId">ログインユーザー社員BASE ID</param>
        /// <param name="syainBaseId">社員BASE ID</param>
        /// <param name="syukkinKubun">出勤区分</param>
        /// <returns></returns>
        private static string GetSyukkinKubunName(long loginSyainBaseId, long syainBaseId, SyukkinKubun syukkinKubun)
        {
            if (loginSyainBaseId != syainBaseId && syukkinKubun.Code == 生理休暇)
                return "その他特別休暇";

            return syukkinKubun.Name ?? "";
        }

        /// <summary>
        /// 表示用PCログ取得（片方欠けも出力）
        /// </summary>
        /// <param name="loginUser">ログインユーザー</param>
        /// <returns>表示用PCログ</returns>
        public List<PcLogData> GetPcLogDataList(Syain loginUser)
        {
            // ガード節：出力不可・null・空なら即返す
            if (!loginUser.IsPcLogOutput || PcLogs == null || PcLogs.Count == 0)
                return new();

            return PcLogs
                // まず PC名→日時でソートしてからグループ化（比較回数削減）
                .OrderBy(x => x.PcName)
                .ThenBy(x => x.Datetime)
                .GroupBy(x => x.PcName)
                .SelectMany(g =>
                {
                    // state: (current: 作成中の1件, done: 完成済みのリスト)
                    var state = g.Aggregate(
                        seed: (current: (PcLogData?)null, done: new List<PcLogData>()),
                        func: (state, log) =>
                        {
                            var (current, done) = state;

                            if (log.Operation == ログオン)
                            {
                                // 連続ログオンなら前の current を片方欠けとして確定
                                if (current is { LogonTime: not null, LogoffTime: null })
                                {
                                    done.Add(current);
                                    current = null;
                                }

                                current ??= new PcLogData { PcName = g.Key };
                                current.LogonTime = log.Datetime;
                            }
                            else // ログオフ
                            {
                                if (current is { LogonTime: not null })
                                {
                                    // 正常にペア完成
                                    current.LogoffTime = log.Datetime;
                                    done.Add(current);
                                    current = null; // 次に備える
                                }
                                else
                                {
                                    // ログオン無しのログオフ → 終了のみの片方欠けを作成
                                    done.Add(new PcLogData
                                    {
                                        PcName = g.Key,
                                        LogoffTime = log.Datetime
                                    });
                                }
                            }

                            return (current, done);
                        });

                    // グループ末尾に current が残っていれば片方欠けのまま追加
                    return state.current == null
                        ? state.done
                        : state.done.Append(state.current);
                })
                // 完全空行（両方 null）は除外。片方欠けは残す
                .Where(p => p.LogonTime != null || p.LogoffTime != null)
                // 時間順にソートしなおす
                .OrderBy(x => x.StartTime)
                .ThenBy(x => x.EndTime)
                .ToList();
        }

        /// <summary>
        /// 表示用伺い申請取得
        /// </summary>
        /// <returns>表示用伺い申請</returns>
        public List<string> GetUkagaiShinseiList()
        {
            var result = new List<string>();

            // 出力対象外種別（HashSetでContains高速化）
            var rejectSet = new HashSet<InquiryType>
            {
                時間外労働時間制限拡張,
                打刻時間修正,
            };

            // null・空以外なら伺い申請を検索、それ以外は代理入力履歴へ
            if (UkagaiShinseis != null && UkagaiShinseis.Count != 0)
            {
                // まず除外し、その後に表示名を生成
                result = UkagaiShinseis
                        .Where(x => !rejectSet.Contains(x.UkagaiSyubetsu))
                        .Select(u =>
                        {
                            // 表示基底名
                            var ukagaiSyubetuName = u.UkagaiSyubetsu.ToString();

                            string suffix = string.Empty;

                            // ヌル安全
                            var header = u.UkagaiHeader;
                            if (header != null && u.UkagaiSyubetsu == 休暇申請)
                            {
                                // 排他的に付与（終日／午前／午後）
                                if (header.KaishiJikoku == BusinessHoursAmStart &&
                                    header.SyuryoJikoku == BusinessHoursPmEnd)
                                {
                                    suffix = "（終日）";
                                }
                                else if (header.KaishiJikoku == BusinessHoursAmStart &&
                                         header.SyuryoJikoku == BusinessHoursAmEnd)
                                {
                                    suffix = "（午前）";
                                }
                                else if (header.KaishiJikoku == BusinessHoursPmStart &&
                                         header.SyuryoJikoku == BusinessHoursPmEnd)
                                {
                                    suffix = "（午後）";
                                }
                            }

                            return $"{ukagaiSyubetuName}{suffix}";
                        })
                        .ToList();

            }

            // 代理入力履歴確認
            if (NippouData != null &&
                NippouData.DairiNyuryokuRirekis != null &&
                0 < NippouData.DairiNyuryokuRirekis.Count)
            {
                result.Add("代理入力");
            }

            return result;
        }
    }
}
