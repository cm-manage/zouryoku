using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 代理入力履歴用のBuilder
    /// </summary>
    internal class DairiNyuryokuRirekiBuilder
    {
        private long _id;
        private long _dairiNyuryokuSyainId;
        private DateTime _dairiNyuryokuTime;
        private long _nippouId;
        private bool _invalid;

        public DairiNyuryokuRirekiBuilder WithId(long id)
        {
            _id = id;
            return this;
        }

        public DairiNyuryokuRirekiBuilder WithDairiNyuryokuTime(DateTime dairiNyuryokuTime)
        {
            _dairiNyuryokuTime = dairiNyuryokuTime;
            return this;
        }

        public DairiNyuryokuRirekiBuilder WithDairiNyuryokuSyainId(long dairiNyuryokuSyainId)
        {
            _dairiNyuryokuSyainId = dairiNyuryokuSyainId;
            return this;
        }

        public DairiNyuryokuRirekiBuilder WithNippouId(long nippouId)
        {
            _nippouId = nippouId;
            return this;
        }
        public DairiNyuryokuRirekiBuilder WithInvalid(bool invalid)
        {
            _invalid = invalid;
            return this;
        }

        public DairiNyuryokuRireki Build()
        {
            return new DairiNyuryokuRireki
            {
                Id = _id,
                DairiNyuryokuSyainId = _dairiNyuryokuSyainId,
                DairiNyuryokuTime = _dairiNyuryokuTime,
                NippouId = _nippouId,
                Invalid = _invalid,
            };
        }
    }
}
