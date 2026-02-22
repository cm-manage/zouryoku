using Model.Enums;
using Model.Model;

namespace ZouryokuTest.Builder
{
    internal class UserRoleBuilder
    {
        private long? _id;
        private short? _code;
        private string? _name;
        private short? _jyunjo;
        private EmployeeAuthority _kengen;

        public UserRoleBuilder WithId(long? id)
        {
            this._id = id;
            return this;
        }

        public UserRoleBuilder WithCode(short? code)
        {
            this._code = code;
            return this;
        }

        public UserRoleBuilder WithName(string? name)
        {
            this._name = name;
            return this;
        }

        public UserRoleBuilder WithJunjo(short? junjo)
        {
            this._jyunjo = junjo;
            return this;
        }

        public UserRoleBuilder WithKengen(EmployeeAuthority kengen)
        {
            this._kengen = kengen;
            return this;
        }

        /// <summary>
        /// 値が設定されていないNOT NULLのカラムにデフォルト値を設定してビルドする
        /// </summary>
        /// <returns></returns>
        public UserRole Build()
        {
            var result = new UserRole()
            {
                Code = _code ?? 0,
                Name = _name ?? "株式会社サンプル",
                Jyunjo = _jyunjo ?? 0,
                Kengen = _kengen,
            };

            if (_id.HasValue)
            {
                result.Id = _id.Value;
            }

            return result;
        }
    }
}
