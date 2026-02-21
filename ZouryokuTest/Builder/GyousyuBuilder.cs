using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 業種マスタ用のBuilder
    /// </summary>
    public class GyousyuBuilder
    {
        private long? _id;
        private string? _code;
        private string? _name;

        public GyousyuBuilder WithId(long id)
        {
            _id = id; 
            return this; 
        }

        public GyousyuBuilder WithCode(string code)
        {
            _code = code;
            return this; 
        }

        public GyousyuBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public Gyousyu Build()
        {
            var result = new Gyousyu
            {
                Code = _code ?? "000",
                Name = _name ?? "サンプル"
            };

            if (_id.HasValue)
            {
                result.Id = _id.Value;
            }

            return result;
        }
    }
}
