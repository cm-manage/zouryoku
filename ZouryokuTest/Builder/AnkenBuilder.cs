using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 案件のビルダー
    /// </summary>
    internal class AnkenBuilder
    {
        private long? _id;
        private string? _name;
        private string? _naiyou;
        private long? _kokyakuKaisyaId;
        private long? _kingsJuchuId;
        private long? _jyutyuSyuruiId;
        private long? _syainBaseId;
        private string? _searchName;
        private uint? _version;

        public AnkenBuilder WithId(long id)
        {
            this._id = id;
            return this;
        }


        public AnkenBuilder WithName(string name)
        {
            this._name = name;
            return this;
        }


        public AnkenBuilder WithNaiyou(string naiyou)
        {
            this._naiyou = naiyou;
            return this;
        }


        public AnkenBuilder WithKokyakuKaisyaId(long kokyakuKaisyaId)
        {
            this._kokyakuKaisyaId = kokyakuKaisyaId;
            return this;
        }


        public AnkenBuilder WithKingsJuchuId(long kingsJuchuId)
        {
            this._kingsJuchuId = kingsJuchuId;
            return this;
        }


        public AnkenBuilder WithJyutyuSyuruiId(long jyutyuSyuruiId)
        {
            this._jyutyuSyuruiId = jyutyuSyuruiId;
            return this;
        }


        public AnkenBuilder WithSyainBaseId(long syainBaseId)
        {
            this._syainBaseId = syainBaseId;
            return this;
        }


        public AnkenBuilder WithSearchName(string searchName)
        {
            this._searchName = searchName;
            return this;
        }

        public AnkenBuilder WithVersion(uint version)
        {
            this._version = version;
            return this;
        }

        public Anken Build()
        {
            return new Anken()
            {

                Id = _id ?? 0,
                Name = _name ?? "案件ﾒｲｼｮｳA",
                Naiyou = _naiyou,
                KokyakuKaisyaId = _kokyakuKaisyaId,
                KingsJuchuId = _kingsJuchuId,
                JyutyuSyuruiId = _jyutyuSyuruiId,
                SyainBaseId = _syainBaseId,
                SearchName = _searchName ?? "案件メイショウA",
                Version = _version ?? 0u
            };
        }
    }
}
