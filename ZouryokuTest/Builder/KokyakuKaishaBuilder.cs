using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 顧客会社のビルダー
    /// </summary>
    internal class KokyakuKaishaBuilder
    {
        private long? _id;
        private int? _code;
        private string? _name;
        private string? _nameKana;
        private string? _ryakusyou;
        private string? _yuubinBangou;
        private string? _jyuusyo1;
        private string? _jyuusyo2;
        private string? _tel;
        private string? _fax;
        private string? _gyousyuCode;
        private string? _memo;
        private string? _url;
        private int? _syoukaisyaCode;
        private string? _syoukaiNaiyou;
        private string? _shiten;
        private long? _gyousyuId;
        private long? _eigyoBaseSyainId;
        private long? _syokaiBaseSyainId;
        private string? _searchName;
        private string? _searchNameKana;

        public KokyakuKaishaBuilder WithId(long? id)
        {
            this._id = id;
            return this;
        }

        public KokyakuKaishaBuilder WithCode(int? code)
        {
            this._code = code;
            return this;
        }

        public KokyakuKaishaBuilder WithName(string? name)
        {
            this._name = name;
            return this;
        }

        public KokyakuKaishaBuilder WithNameKana(string? nameKana)
        {
            this._nameKana = nameKana;
            return this;
        }

        public KokyakuKaishaBuilder WithRyakusyou(string? ryakusyou)
        {
            this._ryakusyou = ryakusyou;
            return this;
        }

        public KokyakuKaishaBuilder WithYuubinBangou(string? yuubinBangou)
        {
            this._yuubinBangou = yuubinBangou;
            return this;
        }

        public KokyakuKaishaBuilder WithJyuusyo1(string? jyuusyo1)
        {
            this._jyuusyo1 = jyuusyo1;
            return this;
        }

        public KokyakuKaishaBuilder WithJyuusyo2(string? jyuusyo2)
        {
            this._jyuusyo2 = jyuusyo2;
            return this;
        }

        public KokyakuKaishaBuilder WithTel(string? tel)
        {
            this._tel = tel;
            return this;
        }

        public KokyakuKaishaBuilder WithFax(string? fax)
        {
            this._fax = fax;
            return this;
        }

        public KokyakuKaishaBuilder WithGyousyuCode(string? gyousyuCode)
        {
            this._gyousyuCode = gyousyuCode;
            return this;
        }

        public KokyakuKaishaBuilder WithMemo(string? memo)
        {
            this._memo = memo;
            return this;
        }

        public KokyakuKaishaBuilder WithUrl(string? url)
        {
            this._url = url;
            return this;
        }

        public KokyakuKaishaBuilder WithSyoukaisyaCode(int? syoukaisyaCode)
        {
            this._syoukaisyaCode = syoukaisyaCode;
            return this;
        }

        public KokyakuKaishaBuilder WithSyoukaiNaiyou(string? syoukaiNaiyou)
        {
            this._syoukaiNaiyou = syoukaiNaiyou;
            return this;
        }

        public KokyakuKaishaBuilder WithShiten(string? shiten)
        {
            this._shiten = shiten;
            return this;
        }

        public KokyakuKaishaBuilder WithGyousyuId(long? gyousyuId)
        {
            this._gyousyuId = gyousyuId;
            return this;
        }

        public KokyakuKaishaBuilder WithEigyoBaseSyainId(long? eigyoBaseSyainId)
        {
            this._eigyoBaseSyainId = eigyoBaseSyainId;
            return this;
        }

        public KokyakuKaishaBuilder WithSyokaiBaseSyainId(long? syokaiBaseSyainId)
        {
            this._syokaiBaseSyainId = syokaiBaseSyainId;
            return this;
        }

        public KokyakuKaishaBuilder WithSearchName(string? searchName)
        {
            this._searchName = searchName;
            return this;
        }

        public KokyakuKaishaBuilder WithSearchNameKana(string? searchNameKana)
        {
            this._searchNameKana = searchNameKana;
            return this;
        }

        /// <summary>
        /// 値が設定されていないNOT NULLのカラムにデフォルト値を設定してビルドする
        /// </summary>
        /// <returns></returns>
        public KokyakuKaisha Build()
        {
            var result = new KokyakuKaisha()
            {
                Code = _code ?? 0,
                Name = _name ?? "株式会社サンプル",
                NameKana = _nameKana ?? "カブシキガイシャサンプル",
                Ryakusyou = _ryakusyou ?? "サンプル",
                YuubinBangou = _yuubinBangou,
                Jyuusyo1 = _jyuusyo1,
                Jyuusyo2 = _jyuusyo2,
                Tel = _tel,
                Fax = _fax,
                GyousyuCode = _gyousyuCode,
                Memo = _memo,
                Url = _url,
                SyoukaisyaCode = _syoukaisyaCode,
                SyoukaiNaiyou = _syoukaiNaiyou,
                Shiten = _shiten,
                GyousyuId = _gyousyuId,
                EigyoBaseSyainId = _eigyoBaseSyainId,
                SyokaiBaseSyainId = _syokaiBaseSyainId,
                SearchName = _searchName ?? "株式会社サンプル",
                SearchNameKana = _searchNameKana ?? "カブシキガイシャサンプル"
            };

            if (_id.HasValue)
            {
                result.Id = _id.Value;
            }

            return result;
        }
    }
}
