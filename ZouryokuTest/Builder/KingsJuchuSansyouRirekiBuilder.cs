using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// KINGS受注参照履歴のビルダー
    /// </summary>
    internal class KingsJuchuSansyouRirekiBuilder
    {
        private long? _id;
        private long? _syainBaseId;
        private DateTime? _sansyouTime;
        private long? _kingsJuchuId;

        public KingsJuchuSansyouRirekiBuilder WithId(long id)
        {
            this._id = id;
            return this;
        }

        public KingsJuchuSansyouRirekiBuilder WithSyainBaseId(long syainBaseId)
        {
            this._syainBaseId = syainBaseId;
            return this;
        }

        public KingsJuchuSansyouRirekiBuilder WithSansyouTime(DateTime sansyouTime)
        {
            this._sansyouTime = sansyouTime;
            return this;
        }

        public KingsJuchuSansyouRirekiBuilder WithKingsJuchuId(long kingsJuchuId)
        {
            this._kingsJuchuId = kingsJuchuId;
            return this;
        }

        public KingsJuchuSansyouRireki Build()
        {
            var result = new KingsJuchuSansyouRireki
            {
                SyainBaseId = _syainBaseId ?? 1,
                SansyouTime = _sansyouTime ?? new DateTime(2025, 6, 1),
                KingsJuchuId = _kingsJuchuId ?? 1,
            };

            if (_id.HasValue)
            {
                result.Id = _id.Value;
            }

            return result;
        }

        public List<KingsJuchuSansyouRireki> BuildMany(int startId, int count, Action<KingsJuchuSansyouRireki> action)
        {
            List<KingsJuchuSansyouRireki> list = new List<KingsJuchuSansyouRireki>();
            for (int i = startId; i < startId + count; i++)
            {
                KingsJuchuSansyouRireki kingsJuchuSansyouRireki = WithId(i).Build();
                action(kingsJuchuSansyouRireki);
                list.Add(kingsJuchuSansyouRireki);
            }

            return list;
        }
    }
}
