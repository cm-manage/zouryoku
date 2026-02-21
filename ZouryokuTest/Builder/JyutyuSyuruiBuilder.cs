using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 受注種類のビルダー
    /// </summary>
    internal class JyutyuSyuruiBuilder
    {
        private long? _id;
        private string? _code;
        private string? _name;

        public JyutyuSyuruiBuilder WithId(long id)
        {
            this._id = id;
            return this;
        }

        public JyutyuSyuruiBuilder WithCode(string code)
        {
            this._code = code;
            return this;
        }

        public JyutyuSyuruiBuilder WithName(string name)
        {
            this._name = name;
            return this;
        }

        public JyutyuSyurui Build()
        {
            return new JyutyuSyurui()
            {
                Id = _id ?? 0,
                Code = _code ?? "JS001",
                Name = _name ?? "受注種類A"
            };
        }
    }
}
