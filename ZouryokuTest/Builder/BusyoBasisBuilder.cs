using Model.Model;

namespace ZouryokuTest.Pages.Builder
{
    /// <summary>
    /// 部署Baseマスタ用のBuilder
    /// </summary>
    public class BusyoBasisBuilder
    {
        private long? _id;
        private string? _name;
        private long? _bumoncyoId;

        public BusyoBasisBuilder WithId(long id)
        {
            _id = id;
            return this;
        }

        public BusyoBasisBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public BusyoBasisBuilder WithBumoncyoId(long? bumoncyoId)
        {
            _bumoncyoId = bumoncyoId;
            return this;
        }

        public BusyoBasis Build()
        {
            return new BusyoBasis
            {
                Id = _id ?? 0,
                Name = _name ?? "サンプル",
                BumoncyoId = _bumoncyoId
            };
        }
    }
}
