using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 案件参照履歴のビルダー
    /// </summary>
    internal class AnkenSansyouRirekiBuilder
    {
        private long? _id;
        private long? _syainBaseId;
        private long? _ankenId;
        private DateTime? _sansyouTime;
        private uint? _version;

        public AnkenSansyouRirekiBuilder WithId(long id)
        {
            this._id = id;
            return this;
        }

        public AnkenSansyouRirekiBuilder WithSyainBaseId(long syainBaseId)
        {
            this._syainBaseId = syainBaseId;
            return this;
        }

        public AnkenSansyouRirekiBuilder WithAnkenId(long ankenId)
        {
            this._ankenId = ankenId;
            return this;
        }

        public AnkenSansyouRirekiBuilder WithSansyouTime(DateTime sansyouTime)
        {
            this._sansyouTime = sansyouTime;
            return this;
        }

        public AnkenSansyouRirekiBuilder WithVersion(uint version)
        {
            this._version = version;
            return this;
        }

        public AnkenSansyouRireki Build()
        {
            return new AnkenSansyouRireki()
            {
                Id = _id ?? 0,
                SyainBaseId = _syainBaseId ?? 1,
                AnkenId = _ankenId ?? 1,
                SansyouTime = _sansyouTime ?? DateTime.Now,
                Version = _version ?? 1
            };
        }

        public List<AnkenSansyouRireki> BuildMany(int startId, int count, Action<AnkenSansyouRireki> action)
        {
            List<AnkenSansyouRireki> list = new List<AnkenSansyouRireki>();
            for (int i = startId; i < startId + count; i++)
            {
                AnkenSansyouRireki ankenSansyouRireki = WithId(i).Build();
                action(ankenSansyouRireki);
                list.Add(ankenSansyouRireki);
            }

            return list;
        }
    }
}
