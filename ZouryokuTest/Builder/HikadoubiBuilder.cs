using Model.Enums;
using Model.Model;

namespace ZouryokuTest.Pages.Builder
{
    /// <summary>
    /// 非稼働日マスタ用のBuilder
    /// </summary>
    public class HikadoubiBuilder
    {
        private long? _id;
        private DateOnly _ymd;
        private HolidayFlag _syukusaijitsuFlag;
        private RefreshDayFlag _refreshDay;

        public HikadoubiBuilder WithId(long id)
        {
            _id = id;
            return this;
        }

        public HikadoubiBuilder WithYmd(DateOnly ymd)
        {
            _ymd = ymd;
            return this;
        }

        public HikadoubiBuilder WithSyukusaijitsuFlag(HolidayFlag syukusaijitsuFlag)
        {
            _syukusaijitsuFlag = syukusaijitsuFlag;
            return this;
        }

        public HikadoubiBuilder WithRefreshDay(RefreshDayFlag refreshDay)
        {
            _refreshDay = refreshDay;
            return this;
        }

        public Hikadoubi Build()
        {
            return new Hikadoubi
            {
                Id = _id ?? 0,
                Ymd = _ymd,
                SyukusaijitsuFlag = _syukusaijitsuFlag,
                RefreshDay = _refreshDay,
            };
        }
    }
}
