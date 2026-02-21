using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 有給年度のビルダー
    /// </summary>
    internal class YukyuNendoBuilder
    {
        private long? _id;
        private short? _nendo;
        private DateOnly? _startDate;
        private DateOnly? _endDate;
        private bool? _isThisYear;
        private bool? _updated;

        public YukyuNendoBuilder WithId(long id)
        {
            this._id = id;
            return this;
        }

        public YukyuNendoBuilder WithNendo(short nendo)
        {
            this._nendo = nendo;
            return this;
        }

        public YukyuNendoBuilder WithStartDate(DateOnly startDate)
        {
            this._startDate = startDate;
            return this;
        }

        public YukyuNendoBuilder WithEndDate(DateOnly endDate)
        {
            this._endDate = endDate;
            return this;
        }

        public YukyuNendoBuilder WithIsThisYear(bool isThisYear)
        {
            this._isThisYear = isThisYear;
            return this;
        }

        public YukyuNendoBuilder WithUpdated(bool updated)
        {
            this._updated = updated;
            return this;
        }

        public YukyuNendo Build()
        {
            return new YukyuNendo()
            {
                Id = _id ?? 0,
                Nendo = _nendo ?? 0,
                StartDate = _startDate ?? new DateOnly(2024, 1, 1),
                EndDate = _endDate ?? new DateOnly(2024, 12, 31),
                IsThisYear = _isThisYear ?? false,
                Updated = _updated ?? false
            };
        }
    }
}
