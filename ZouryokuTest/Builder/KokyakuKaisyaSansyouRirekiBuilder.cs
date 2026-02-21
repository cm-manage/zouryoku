using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 顧客会社参照履歴のビルダー
    /// </summary>
    internal class KokyakuKaisyaSansyouRirekiBuilder()
    {
        private long? _id;
        private long? _syainBaseId;
        private long? _kokyakuKiasyaId;
        private DateTime? _sansyouTime;
        private uint? _version;

        public KokyakuKaisyaSansyouRirekiBuilder WithId(long? id)
        {
            _id = id;
            return this;
        }

        public KokyakuKaisyaSansyouRirekiBuilder WithKokyakuKaisyaId(long? kokyakuKaisyaId)
        {
            _kokyakuKiasyaId = kokyakuKaisyaId;
            return this;
        }

        public KokyakuKaisyaSansyouRirekiBuilder WithSyainBaseId(long? syainBaseId)
        {
            _syainBaseId = syainBaseId;
            return this;
        }

        public KokyakuKaisyaSansyouRirekiBuilder WithSansyouTime(DateTime? sanyouTime)
        {
            _sansyouTime = sanyouTime;
            return this;
        }

        public KokyakuKaisyaSansyouRirekiBuilder WithVersion(uint? version)
        {
            _version = version;
            return this;
        }

        public KokyakuKaisyaSansyouRireki Build()
        {
            var result = new KokyakuKaisyaSansyouRireki
            {
                KokyakuKaisyaId = _kokyakuKiasyaId ?? 1,
                SyainBaseId = _syainBaseId ?? 1,
                SansyouTime = _sansyouTime ?? DateTime.Now,
                Version = _version ?? 0,
            };

            if (_id.HasValue)
            {
                result.Id = _id.Value;
            }

            return result;
        }

        public List<KokyakuKaisyaSansyouRireki> BuildMany(int startId, int count, Action<KokyakuKaisyaSansyouRireki> action)
        {
            List<KokyakuKaisyaSansyouRireki> list = new List<KokyakuKaisyaSansyouRireki>();
            for (int i = startId; i < startId + count; i++)
            {
                KokyakuKaisyaSansyouRireki kokyakuKaisyaSansyouRireki = WithId(i).Build();
                action(kokyakuKaisyaSansyouRireki);
                list.Add(kokyakuKaisyaSansyouRireki);
            }

            return list;
        }
    }
}
