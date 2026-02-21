using CommonLibrary.Extensions;
using Model.Enums;
using Model.Model;
using static Model.Enums.BusinessTripRole;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 社員マスタ用のビルダー
    /// </summary>
    internal class SyainBuilder
    {
        private long? _id;
        private string? _code;
        private string? _name;
        private string? _kanaName;
        private char? _seibetsu;
        private string? _busyoCode;
        private int? _syokusyuCode;
        private int? _syokusyuBunruiCode;
        private DateOnly? _nyuusyaYmd;
        private DateOnly? _startYmd;
        private DateOnly? _endYmd;
        private short? _kyusyoku;
        private BusinessTripRole? _syucyoSyokui;
        private string? _kingsSyozoku;
        private short? _kaisyaCode;
        private bool? _isGenkaRendou;
        private string? _eMail;
        private string? _keitaiMail;
        private EmployeeAuthority? _kengen;
        private short? _jyunjyo;
        private bool? _retired;
        private long? _gyoumuTypeId;
        private string? _phoneNumber;
        private long? _syainBaseId;
        private long? _busyoId;
        private long? _kintaiZokuseiId;
        private long? _userRoleId;


        public SyainBuilder WithId(long? id)
        {
            _id = id;
            return this;
        }

        public SyainBuilder WithCode(string? code)
        {
            _code = code;
            return this;
        }

        public SyainBuilder WithName(string? name)
        {
            _name = name;
            return this;
        }

        public SyainBuilder WithKanaName(string? kanaName)
        {
            _kanaName = kanaName;
            return this;
        }

        public SyainBuilder WithSeibetsu(char? seibetsu)
        {
            _seibetsu = seibetsu;
            return this;
        }

        public SyainBuilder WithBusyoCode(string? busyoCode)
        {
            _busyoCode = busyoCode;
            return this;
        }

        public SyainBuilder WithSyokusyuCode(int? syokusyuCode)
        {
            _syokusyuCode = syokusyuCode;
            return this;
        }

        public SyainBuilder WithSyokusyuBunruiCode(int? syokusyuBunruiCode)
        {
            _syokusyuBunruiCode = syokusyuBunruiCode;
            return this;
        }

        public SyainBuilder WithNyuusyaYmd(DateOnly? nyuusyaYmd)
        {
            _nyuusyaYmd = nyuusyaYmd;
            return this;
        }

        public SyainBuilder WithStartYmd(DateOnly? startYmd)
        {
            _startYmd = startYmd;
            return this;
        }

        public SyainBuilder WithEndYmd(DateOnly? endYmd)
        {
            _endYmd = endYmd;
            return this;
        }

        public SyainBuilder WithKyusyoku(short? kyusyoku)
        {
            _kyusyoku = kyusyoku;
            return this;
        }

        public SyainBuilder WithSyucyoSyokui(BusinessTripRole? syucyoSyokui)
        {
            _syucyoSyokui = syucyoSyokui;
            return this;
        }

        public SyainBuilder WithKingsSyozoku(string? kingsSyozoku)
        {
            _kingsSyozoku = kingsSyozoku;
            return this;
        }

        public SyainBuilder WithKaisyaCode(short? kaisyaCode)
        {
            _kaisyaCode = kaisyaCode;
            return this;
        }

        public SyainBuilder WithIsGenkaRendou(bool? isGenkaRendou)
        {
            _isGenkaRendou = isGenkaRendou;
            return this;
        }

        public SyainBuilder WithEMail(string? eMail)
        {
            _eMail = eMail;
            return this;
        }

        public SyainBuilder WithKeitaiMail(string? keitaiMail)
        {
            _keitaiMail = keitaiMail;
            return this;
        }

        public SyainBuilder WithKengen(EmployeeAuthority kengen)
        {
            _kengen = kengen;
            return this;
        }

        public SyainBuilder WithJyunjyo(short? jyunjyo)
        {
            _jyunjyo = jyunjyo;
            return this;
        }

        public SyainBuilder WithRetired(bool? retired)
        {
            _retired = retired;
            return this;
        }

        public SyainBuilder WithGyoumuTypeId(long? gyoumuTypeId)
        {
            _gyoumuTypeId = gyoumuTypeId;
            return this;
        }

        public SyainBuilder WithPhoneNumber(string? phoneNumber)
        {
            _phoneNumber = phoneNumber;
            return this;
        }

        public SyainBuilder WithSyainBaseId(long? syainBaseId)
        {
            _syainBaseId = syainBaseId;
            return this;
        }

        public SyainBuilder WithBusyoId(long? busyoId)
        {
            _busyoId = busyoId;
            return this;
        }

        public SyainBuilder WithKintaiZokuseiId(long? kintaiZokuseiId)
        {
            _kintaiZokuseiId = kintaiZokuseiId;
            return this;
        }

        public SyainBuilder WithUserRoleId(long? userRoleId)
        {
            _userRoleId = userRoleId;
            return this;
        }

        public Syain Build()
        {
            var result = new Syain
            {
                Code = _code ?? "00000",
                Name = _name ?? "サンプル太郎",
                KanaName = _kanaName ?? "サンプルタロウ",
                Seibetsu = _seibetsu ?? '1',
                BusyoCode = _busyoCode ?? "000",
                SyokusyuCode = _syokusyuCode ?? 0,
                SyokusyuBunruiCode = _syokusyuBunruiCode ?? 0,
                NyuusyaYmd = _nyuusyaYmd ?? DateTime.Now.AddYears(-1).ToDateOnly(),
                StartYmd = _startYmd ?? DateTime.Now.AddMonths(-1).ToDateOnly(),
                EndYmd = _endYmd ?? DateTime.Now.AddMonths(1).ToDateOnly(),
                Kyusyoku = _kyusyoku ?? 0,
                SyucyoSyokui = _syucyoSyokui ?? _2_6級,
                KingsSyozoku = _kingsSyozoku ?? "00000",
                KaisyaCode = _kaisyaCode ?? 0,
                IsGenkaRendou = _isGenkaRendou ?? false,
                EMail = _eMail,
                KeitaiMail = _keitaiMail,
                Kengen = _kengen ?? 0,
                Jyunjyo = _jyunjyo ?? 0,
                Retired = _retired ?? false,
                GyoumuTypeId = _gyoumuTypeId,
                PhoneNumber = _phoneNumber,
                SyainBaseId = _syainBaseId ?? 1,
                BusyoId = _busyoId ?? 1,
                KintaiZokuseiId = _kintaiZokuseiId ?? 1,
                UserRoleId = _userRoleId ?? 1
            };

            if (_id.HasValue)
            {
                result.Id = _id.Value;
            }

            return result;
        }
    }
}
