using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 社員BASEマスタのビルダー
    /// </summary>
    internal class SyainBasisBuilder
    {
        private long? _id;
        private string? _name;
        private string? _code;

        public SyainBasisBuilder WithId(long? id)
        {
            _id = id;
            return this;
        }

        public SyainBasisBuilder WithName(string? name)
        {
            _name = name;
            return this;
        }

        public SyainBasisBuilder WithCode(string? code)
        {
            _code = code;
            return this;
        }

        public SyainBasis Build()
        {
            var result = new SyainBasis
            {
                Name = _name ?? "サンプル太郎",
                Code = _code ?? "00000"
            };

            if (_id.HasValue)
            {
                result.Id = _id.Value;
            }

            return result;
        }
    }
}
