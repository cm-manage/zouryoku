using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 日報実績⇔案件のビルダー
    /// </summary>
    internal class NippouAnkenBuilder
    {
        private long? _id;
        private long? _nippouId;
        private long? _ankensId;
        private string? _kokyakuName;
        private string? _ankenName;
        private short? _jissekiJikan;
        private long? _kokyakuKaisyaId;
        private long? _bumonProcessId;
        private bool? _isLinked;

        public NippouAnkenBuilder WithId(long id)
        {
            this._id = id;
            return this;
        }

        public NippouAnkenBuilder WithNippouId(long nippouId)
        {
            this._nippouId = nippouId;
            return this;
        }

        public NippouAnkenBuilder WithAnkensId(long ankensId)
        {
            this._ankensId = ankensId;
            return this;
        }

        public NippouAnkenBuilder WithKokyakuName(string kokyakuName)
        {
            this._kokyakuName = kokyakuName;
            return this;
        }

        public NippouAnkenBuilder WithAnkenName(string ankenName)
        {
            this._ankenName = ankenName;
            return this;
        }

        public NippouAnkenBuilder WithJissekiJikan(short jissekiJikan)
        {
            this._jissekiJikan = jissekiJikan;
            return this;
        }

        public NippouAnkenBuilder WithKokyakuKaisyaId(long kokyakuKaisyaId)
        {
            this._kokyakuKaisyaId = kokyakuKaisyaId;
            return this;
        }

        public NippouAnkenBuilder WithBumonProcessId(long bumonProcessId)
        {
            this._bumonProcessId = bumonProcessId;
            return this;
        }

        public NippouAnkenBuilder WithIsLinked(bool isLinked)
        {
            this._isLinked = isLinked;
            return this;
        }

        public NippouAnken Build()
        {
            return new NippouAnken()
            {
                Id = _id ?? 0,
                NippouId = _nippouId ?? 1,
                AnkensId = _ankensId ?? 1,
                KokyakuName = _kokyakuName ?? "顧客A",
                AnkenName = _ankenName ?? "案件A",
                JissekiJikan = _jissekiJikan ?? 1200,
                KokyakuKaisyaId = _kokyakuKaisyaId ?? 1,
                BumonProcessId = _bumonProcessId,
                IsLinked = _isLinked ?? false
            };
        }
    }
}
