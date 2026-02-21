using Model.Enums;
using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 日報実績Builder
    /// </summary>
    internal class NippouBuilder
    {
        private long? _id;
        private long? _syainId;
        private DateOnly? _nippouYmd;
        private short? _youbi;
        private TimeOnly? _syukkinHm1;
        private TimeOnly? _taisyutsuHm1;
        private TimeOnly? _syukkinHm2;
        private TimeOnly? _taisyutsuHm2;
        private TimeOnly? _syukkinHm3;
        private TimeOnly? _taisyutsuHm3;
        private decimal? _hJitsudou;
        private decimal? _hZangyo;
        private decimal? _hWarimashi;
        private decimal? _hShinyaZangyo;
        private decimal? _dJitsudou;
        private decimal? _dZangyo;
        private decimal? _dWarimashi;
        private decimal? _dShinyaZangyo;
        private decimal? _nJitsudou;
        private decimal? _nShinya;
        private decimal? _totalZangyo;
        private NippousCompanyCode? _kaisyaCode;
        private bool? _isRendouZumi;
        private DateOnly? _rendouYmd;
        private DailyReportStatusClassification? _tourokuKubun;
        private DateOnly? _kakuteiYmd;
        private long? _syukkinKubunId1;
        private long? _syukkinKubunId2;

        public NippouBuilder WithId(long id)
        {
            this._id = id;
            return this;
        }

        public NippouBuilder WithSyainId(long syainId)
        {
            this._syainId = syainId;
            return this;
        }

        public NippouBuilder WithNippouYmd(DateOnly nippouYmd)
        {
            this._nippouYmd = nippouYmd;
            return this;
        }

        public NippouBuilder WithYoubi(short youbi)
        {
            this._youbi = youbi;
            return this;
        }

        public NippouBuilder WithSyukkinHm1(TimeOnly? syukkinHm1)
        {
            this._syukkinHm1 = syukkinHm1;
            return this;
        }

        public NippouBuilder WithTaisyutsuHm1(TimeOnly? taisyutsuHm1)
        {
            this._taisyutsuHm1 = taisyutsuHm1;
            return this;
        }

        public NippouBuilder WithSyukkinHm2(TimeOnly? syukkinHm2)
        {
            this._syukkinHm2 = syukkinHm2;
            return this;
        }

        public NippouBuilder WithTaisyutsuHm2(TimeOnly? taisyutsuHm2)
        {
            this._taisyutsuHm2 = taisyutsuHm2;
            return this;
        }

        public NippouBuilder WithSyukkinHm3(TimeOnly? syukkinHm3)
        {
            this._syukkinHm3 = syukkinHm3;
            return this;
        }

        public NippouBuilder WithTaisyutsuHm3(TimeOnly? taisyutsuHm3)
        {
            this._taisyutsuHm3 = taisyutsuHm3;
            return this;
        }

        public NippouBuilder WithHJitsudou(decimal hJitsudou)
        {
            this._hJitsudou = hJitsudou;
            return this;
        }

        public NippouBuilder WithHZangyo(decimal hZangyo)
        {
            this._hZangyo = hZangyo;
            return this;
        }

        public NippouBuilder WithHWarimashi(decimal hWarimashi)
        {
            this._hWarimashi = hWarimashi;
            return this;
        }

        public NippouBuilder WithHShinyaZangyo(decimal hShinyaZangyo)
        {
            this._hShinyaZangyo = hShinyaZangyo;
            return this;
        }

        public NippouBuilder WithDJitsudou(decimal dJitsudou)
        {
            this._dJitsudou = dJitsudou;
            return this;
        }

        public NippouBuilder WithDZangyo(decimal dZangyo)
        {
            this._dZangyo = dZangyo;
            return this;
        }

        public NippouBuilder WithDWarimashi(decimal dWarimashi)
        {
            this._dWarimashi = dWarimashi;
            return this;
        }

        public NippouBuilder WithDShinyaZangyo(decimal dShinyaZangyo)
        {
            this._dShinyaZangyo = dShinyaZangyo;
            return this;
        }

        public NippouBuilder WithNJitsudou(decimal nJitsudou)
        {
            this._nJitsudou = nJitsudou;
            return this;
        }

        public NippouBuilder WithNShinya(decimal nShinya)
        {
            this._nShinya = nShinya;
            return this;
        }

        public NippouBuilder WithTotalZangyo(decimal totalZangyo)
        {
            this._totalZangyo = totalZangyo;
            return this;
        }

        public NippouBuilder WithKaisyaCode(NippousCompanyCode kaisyaCode)
        {
            this._kaisyaCode = kaisyaCode;
            return this;
        }

        public NippouBuilder WithIsRendouZumi(bool isRendouZumi)
        {
            this._isRendouZumi = isRendouZumi;
            return this;
        }

        public NippouBuilder WithRendouYmd(DateOnly rendouYmd)
        {
            this._rendouYmd = rendouYmd;
            return this;
        }

        public NippouBuilder WithTourokuKbn(DailyReportStatusClassification tourokuKbn)
        {
            this._tourokuKubun = tourokuKbn;
            return this;
        }

        public NippouBuilder WithKakuteiYmd(DateOnly kakuteiYmd)
        {
            this._kakuteiYmd = kakuteiYmd;
            return this;
        }

        public NippouBuilder WithSyukkinKubunId1(long syukkinKubunId1)
        {
            this._syukkinKubunId1 = syukkinKubunId1;
            return this;
        }

        public NippouBuilder WithSyukkinKubunId2(long syukkinKubunId2)
        {
            this._syukkinKubunId2 = syukkinKubunId2;
            return this;
        }



        public Nippou Build()
        {
            return new Nippou()
            {
                Id = _id ?? 1, 
                SyainId = _syainId ?? 1,
                NippouYmd = _nippouYmd ?? DateOnly.Parse("2026/01/01"),
                Youbi = _youbi ?? 0,
                SyukkinHm1 = _syukkinHm1,
                TaisyutsuHm1 = _taisyutsuHm1,
                SyukkinHm2 = _syukkinHm2,
                TaisyutsuHm2 = _taisyutsuHm2,
                SyukkinHm3 = _syukkinHm3,
                TaisyutsuHm3 = _taisyutsuHm3,
                HJitsudou = _hJitsudou,
                HZangyo = _hZangyo,
                HWarimashi = _hWarimashi,
                HShinyaZangyo = _hShinyaZangyo,
                DJitsudou = _dJitsudou,
                DZangyo = _dZangyo,
                DWarimashi = _dWarimashi,
                DShinyaZangyo = _dShinyaZangyo,
                NJitsudou = _nJitsudou,
                NShinya = _nShinya,
                TotalZangyo = _totalZangyo,
                KaisyaCode =_kaisyaCode ?? NippousCompanyCode.協和,
                IsRendouZumi = _isRendouZumi ?? false,
                RendouYmd = _rendouYmd,
                TourokuKubun = _tourokuKubun ?? DailyReportStatusClassification.一時保存,
                KakuteiYmd = _kakuteiYmd,
                SyukkinKubunId1 = _syukkinKubunId1 ?? 0,
                SyukkinKubunId2 = _syukkinKubunId2,
            };
        }
    }

}
