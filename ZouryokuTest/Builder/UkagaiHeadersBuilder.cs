using Model.Enums;
using Model.Model;

namespace ZouryokuTest.Builder
{
    internal class UkagaiHeaderBuilder
    {
        private long? _id;
        private long? _syainId;
        private DateOnly? _shinseiYmd;
        private long? _shoninSyainId;
        private DateOnly? _shoninYmd;
        private long? _lastShoninSyainId;
        private ApprovalStatus? _status;
        private DateOnly? _lastShoninYmd;
        private DateOnly? _workYmd;
        private TimeOnly? _kaishiJikoku;
        private TimeOnly? _syuryoJikoku;
        private string? _biko;
        private bool? _invalid;

        public UkagaiHeaderBuilder WithId(long id)
        {
            this._id = id;
            return this;
        }

        public UkagaiHeaderBuilder WithSyainId(long syainId)
        {
            this._syainId = syainId;
            return this;
        }

        public UkagaiHeaderBuilder WithShinseiYmd(DateOnly shinseiYmd)
        {
            this._shinseiYmd = shinseiYmd;
            return this;
        }

        public UkagaiHeaderBuilder WithShoninSyainId(long shoninSyainId)
        {
            this._shoninSyainId = shoninSyainId;
            return this;
        }

        public UkagaiHeaderBuilder WithShoninYmd(DateOnly shoninYmd)
        {
            this._shoninYmd = shoninYmd;
            return this;
        }

        public UkagaiHeaderBuilder WithLastShoninSyainId(long lastShoninSyainId)
        {
            this._lastShoninSyainId = lastShoninSyainId;
            return this;
        }

        public UkagaiHeaderBuilder WithStatus(ApprovalStatus approvalStatus)
        {
            this._status = approvalStatus;
            return this;
        }

        public UkagaiHeaderBuilder WithLastShoninYmd(DateOnly lastShoninYmd)
        {
            this._lastShoninYmd = lastShoninYmd;
            return this;
        }

        public UkagaiHeaderBuilder WithWorkYmd(DateOnly workYmd)
        {
            this._workYmd = workYmd;
            return this;
        }

        public UkagaiHeaderBuilder WithKaishiJikoku(TimeOnly kaishiJikoku)
        {
            this._kaishiJikoku = kaishiJikoku;
            return this;
        }

        public UkagaiHeaderBuilder WithSyuryoJikoku(TimeOnly syuryoJikoku)
        {
            this._syuryoJikoku = syuryoJikoku;
            return this;
        }

        public UkagaiHeaderBuilder WithBiko(string biko)
        {
            this._biko = biko;
            return this;
        }

        public UkagaiHeaderBuilder WithInvalid(bool invalid)
        {
            this._invalid = invalid;
            return this;
        }

        public UkagaiHeader Build()
        {
            return new UkagaiHeader()
            {
                Id = _id ?? 1,
                SyainId = _syainId ?? 1,
                ShinseiYmd = _shinseiYmd ?? DateOnly.FromDateTime(new DateTime(2026, 1, 1)),
                ShoninSyainId = _shoninSyainId,
                ShoninYmd = _shoninYmd,
                LastShoninSyainId = _lastShoninSyainId,
                LastShoninYmd = _lastShoninYmd,
                Status = _status ?? ApprovalStatus.承認待,
                WorkYmd = _workYmd ?? DateOnly.FromDateTime(new DateTime(2026, 1, 1)),
                KaishiJikoku = _kaishiJikoku,
                SyuryoJikoku = _syuryoJikoku,
                Biko = _biko,
                Invalid = _invalid ?? false,
            };
        }
    }

}
