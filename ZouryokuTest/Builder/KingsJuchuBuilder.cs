using Model.Enums;
using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// KINGS受注登録のビルダー
    /// </summary>
    internal class KingsJuchuBuilder
    {
        private long? _id;
        private DateOnly? _jucYmd;
        private DateOnly? _entYmd;
        private string? _keiNm;
        private string? _keiKn;
        private string? _jucNm;
        private string? _jucKn;
        private string? _bukken;
        private string? _iriBusCd;
        private long? _jucKin;
        private string? _okrTanCd1;
        private string? _okrTanNm1;
        private string? _ukeTanCd1;
        private string? _ukeTanNm1;
        private DateOnly? _chaYmd;
        private DateOnly? _nsyYmd;
        private DateOnly? _kurYmd;
        private DateOnly? _knyYmd;
        private string? _biko;
        private string? _jsriCd;
        private string? _jucCd;
        private string? _projectNo;
        private string? _juchuuNo;
        private short? _juchuuGyoNo;
        private ContractClassification? _keiyakuJoutaiKbn;
        private string? _keiyakuJoutaiKbnName;
        private string? _sekouBumonCd;
        private short? _hiyouShubetuCd;
        private string? _hiyouShubetuCdName;
        private bool? _isGenkaToketu;
        private DateOnly? _toketuYmd;
        private short? _nendo;
        private string? _shouhinName;
        private long? _busyoId;
        private string? _searchKeiNm;
        private string? _searchKeiKn;
        private string? _searchJucNm;
        private string? _searchJucKin;
        private string? _searchBukken;

        public KingsJuchuBuilder WithId(long id)
        {
            this._id = id;
            return this;
        }

        public KingsJuchuBuilder WithJucYmd(DateOnly jucYmd)
        {
            this._jucYmd = jucYmd;
            return this;
        }

        public KingsJuchuBuilder WithEntYmd(DateOnly entYmd)
        {
            this._entYmd = entYmd;
            return this;
        }

        public KingsJuchuBuilder WithKeiNm(string keiNm)
        {
            this._keiNm = keiNm;
            return this;
        }

        public KingsJuchuBuilder WithKeiKn(string keiKn)
        {
            this._keiKn = keiKn;
            return this;
        }

        public KingsJuchuBuilder WithJucNm(string jucNm)
        {
            this._jucNm = jucNm;
            return this;
        }

        public KingsJuchuBuilder WithJucKn(string jucKn)
        {
            this._jucKn = jucKn;
            return this;
        }

        public KingsJuchuBuilder WithBukken(string bukken)
        {
            this._bukken = bukken;
            return this;
        }

        public KingsJuchuBuilder WithIriBusCd(string iriBusCd)
        {
            this._iriBusCd = iriBusCd;
            return this;
        }

        public KingsJuchuBuilder WithJucKin(long jucKin)
        {
            this._jucKin = jucKin;
            return this;
        }

        public KingsJuchuBuilder WithOkrTanCd1(string okrTanCd1)
        {
            this._okrTanCd1 = okrTanCd1;
            return this;
        }

        public KingsJuchuBuilder WithOkrTanNm1(string okrTanNm1)
        {
            this._okrTanNm1 = okrTanNm1;
            return this;
        }

        public KingsJuchuBuilder WithUkeTanCd1(string ukeTanCd1)
        {
            this._ukeTanCd1 = ukeTanCd1;
            return this;
        }

        public KingsJuchuBuilder WithUkeTanNm1(string ukeTanNm1)
        {
            this._ukeTanNm1 = ukeTanNm1;
            return this;
        }

        public KingsJuchuBuilder WithChaYmd(DateOnly chaYmd)
        {
            this._chaYmd = chaYmd;
            return this;
        }

        public KingsJuchuBuilder WithNsyYmd(DateOnly nsyYmd)
        {
            this._nsyYmd = nsyYmd;
            return this;
        }

        public KingsJuchuBuilder WithKurYmd(DateOnly kurYmd)
        {
            this._kurYmd = kurYmd;
            return this;
        }

        public KingsJuchuBuilder WithKnyYmd(DateOnly knyYmd)
        {
            this._knyYmd = knyYmd;
            return this;
        }

        public KingsJuchuBuilder WithBiko(string biko)
        {
            this._biko = biko;
            return this;
        }

        public KingsJuchuBuilder WithJsriCd(string jsriCd)
        {
            this._jsriCd = jsriCd;
            return this;
        }

        public KingsJuchuBuilder WithJucCd(string jucCd)
        {
            this._jucCd = jucCd;
            return this;
        }

        public KingsJuchuBuilder WithProjectNo(string projectNo)
        {
            this._projectNo = projectNo;
            return this;
        }

        public KingsJuchuBuilder WithJuchuuNo(string juchuuNo)
        {
            this._juchuuNo = juchuuNo;
            return this;
        }

        public KingsJuchuBuilder WithJuchuuGyoNo(short juchuuGyoNo)
        {
            this._juchuuGyoNo = juchuuGyoNo;
            return this;
        }

        public KingsJuchuBuilder WithKeiyakuJoutaiKbn(ContractClassification keiyakuJoutaiKbn)
        {
            this._keiyakuJoutaiKbn = keiyakuJoutaiKbn;
            return this;
        }

        public KingsJuchuBuilder WithKeiyakuJoutaiKbnName(string keiyakuJoutaiKbnName)
        {
            this._keiyakuJoutaiKbnName = keiyakuJoutaiKbnName;
            return this;
        }

        public KingsJuchuBuilder WithSekouBumonCd(string sekouBumonCd)
        {
            this._sekouBumonCd = sekouBumonCd;
            return this;
        }

        public KingsJuchuBuilder WithHiyouShubetuCd(short hiyouShubetuCd)
        {
            this._hiyouShubetuCd = hiyouShubetuCd;
            return this;
        }

        public KingsJuchuBuilder WithHiyouShubetuCdName(string hiyouShubetuCdName)
        {
            this._hiyouShubetuCdName = hiyouShubetuCdName;
            return this;
        }

        public KingsJuchuBuilder WithIsGenkaToketu(bool isGenkaToketu)
        {
            this._isGenkaToketu = isGenkaToketu;
            return this;
        }

        public KingsJuchuBuilder WithToketuYmd(DateOnly toketuYmd)
        {
            this._toketuYmd = toketuYmd;
            return this;
        }

        public KingsJuchuBuilder WithNendo(short nendo)
        {
            this._nendo = nendo;
            return this;
        }

        public KingsJuchuBuilder WithShouhinName(string shouhinName)
        {
            this._shouhinName = shouhinName;
            return this;
        }

        public KingsJuchuBuilder WithBusyoId(long busyoId)
        {
            this._busyoId = busyoId;
            return this;
        }

        public KingsJuchuBuilder WithSearchKeiNm(string searchKeiNm)
        {
            this._searchKeiNm = searchKeiNm;
            return this;
        }

        public KingsJuchuBuilder WithSearchKeiKn(string searchKeiKn)
        {
            this._searchKeiKn = searchKeiKn;
            return this;
        }

        public KingsJuchuBuilder WithSearchJucNm(string searchJucNm)
        {
            this._searchJucNm = searchJucNm;
            return this;
        }

        public KingsJuchuBuilder WithSearchJucKn(string searchJucKn)
        {
            this._searchJucKin = searchJucKn;
            return this;
        }

        public KingsJuchuBuilder WithSearchBukken(string searchBukken)
        {
            this._searchBukken = searchBukken;
            return this;
        }

        public KingsJuchu Build()
        {
            return new KingsJuchu()
            {
                Id = _id ?? 0,
                JucYmd = _jucYmd ?? new DateOnly(2025, 6, 1),
                EntYmd = _entYmd ?? new DateOnly(2025, 6, 2),
                KeiNm = _keiNm,
                KeiKn = _keiKn,
                JucNm = _jucNm,
                JucKn = _jucKn,
                Bukken = _bukken ?? "受注A",
                IriBusCd = _iriBusCd,
                JucKin = _jucKin ?? 1000000,
                OkrTanCd1 = _okrTanCd1,
                OkrTanNm1 = _okrTanNm1,
                UkeTanCd1 = _ukeTanCd1,
                UkeTanNm1 = _ukeTanNm1,
                ChaYmd = _chaYmd ?? new DateOnly(2025, 6, 3),
                NsyYmd = _nsyYmd,
                KurYmd = _kurYmd,
                KnyYmd = _knyYmd,
                Biko = _biko,
                JsriCd = _jsriCd,
                JucCd = _jucCd,
                ProjectNo = _projectNo ?? "P1001",
                JuchuuNo = _juchuuNo,
                JuchuuGyoNo = _juchuuGyoNo,
                KeiyakuJoutaiKbn = _keiyakuJoutaiKbn,
                KeiyakuJoutaiKbnName = _keiyakuJoutaiKbnName,
                SekouBumonCd = _sekouBumonCd ?? "SB001",
                HiyouShubetuCd = _hiyouShubetuCd ?? 1,
                HiyouShubetuCdName = _hiyouShubetuCdName ?? "費用種別A",
                IsGenkaToketu = _isGenkaToketu ?? false,
                ToketuYmd = _toketuYmd,
                Nendo = _nendo ?? 2025,
                ShouhinName = _shouhinName,
                BusyoId = _busyoId ?? 1,
                SearchKeiNm = _searchKeiNm,
                SearchKeiKn = _searchKeiKn,
                SearchJucNm = _searchJucNm,
                SearchJucKn = _searchJucKin,
                SearchBukken = _bukken ?? "受注A"
            };
        }
    }
}
