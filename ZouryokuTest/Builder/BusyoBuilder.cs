using Model.Model;

namespace ZouryokuTest.Pages.Builder
{
    /// <summary>
    /// 部署マスタ用のBuilder
    /// </summary>
    public class BusyoBuilder
    {
        private long? _id;
        private string? _code;
        private string? _name;
        private string? _kanaName;
        private string? _oyaCode;
        private DateOnly? _startYmd;
        private DateOnly? _endYmd;
        private short? _jyunjyo;
        private string? _kasyoCode;
        private string? _kaikeiCode;
        private string? _keiriCode;
        private bool? _isActive;
        private string? _ryakusyou;
        private long? _busyoBaseId;
        private long? _oyaId;
        private long? _shoninBusyoId;

        public BusyoBuilder WithId(long id)
        {
            _id = id;
            return this;
        }

        public BusyoBuilder WithCode(string code)
        {
            _code = code;
            return this;
        }

        public BusyoBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public BusyoBuilder WithKanaName(string kanaName)
        {
            _kanaName = kanaName;
            return this;
        }

        public BusyoBuilder WithOyaCode(string oyaCode)
        {
            _oyaCode = oyaCode;
            return this;
        }

        public BusyoBuilder WithStartYmd(DateOnly? startYmd)
        {
            _startYmd = startYmd;
            return this;
        }

        public BusyoBuilder WithEndYmd(DateOnly? endYmd)
        {
            _endYmd = endYmd;
            return this;
        }

        public BusyoBuilder WithJyunjyo(short jyunjyo)
        {
            _jyunjyo = jyunjyo;
            return this;
        }

        public BusyoBuilder WithKasyoCode(string kasyoCode)
        {
            _kasyoCode = kasyoCode;
            return this;
        }

        public BusyoBuilder WithKaikeiCode(string kaikeiCode)
        {
            _kaikeiCode = kaikeiCode;
            return this;
        }

        public BusyoBuilder WithKeiriCode(string? keiriCode)
        {
            _keiriCode = keiriCode;
            return this;
        }

        public BusyoBuilder WithIsActive(Boolean isActive)
        {
            _isActive = isActive;
            return this;
        }

        public BusyoBuilder WithRyakusyou(string? ryakusyou)
        {
            _ryakusyou = ryakusyou;
            return this;
        }

        public BusyoBuilder WithBusyoBaseId(long busyoBaseId)
        {
            _busyoBaseId = busyoBaseId;
            return this;
        }

        public BusyoBuilder WithOyaId(long? oyaId)
        {
            _oyaId = oyaId;
            return this;
        }

        public BusyoBuilder WithShoninBusyoId(long? shoninBusyoId)
        {
            _shoninBusyoId = shoninBusyoId;
            return this;
        }

        public Busyo Build()
        {
            var result = new Busyo()
            {
                Code = _code ?? "100",
                Name = _name ?? "部署A",
                KanaName = _kanaName ?? "ブショエー",
                OyaCode = _oyaCode ?? string.Empty,
                StartYmd = _startYmd ?? DateOnly.Parse("2025/04/01"),
                EndYmd = _endYmd ?? DateOnly.Parse("9999/12/31"),
                Jyunjyo = _jyunjyo ?? 1,
                KasyoCode = _kasyoCode ?? "1",
                KaikeiCode = _kaikeiCode ?? "1",
                KeiriCode = _keiriCode,
                IsActive = _isActive ?? true,
                Ryakusyou = _ryakusyou,
                BusyoBaseId = _busyoBaseId ?? 1,
                OyaId = _oyaId,
                ShoninBusyoId = _shoninBusyoId
            };

            if (_id.HasValue)
            {
                result.Id = _id.Value;
            }

            return result;
        }
    }
}
