using Model.Enums;
using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// 勤怠属性用のビルダー
    /// </summary>
    internal class KintaiZokuseiBuilder
    {
        private long? _id;
        private string? _name;
        private decimal? _seigenTime;
        private bool? _isMinashi;
        private decimal? _maxLimitTime;
        private bool? _isOvertimeLimit3m;
        private EmployeeWorkType? _code;

        public KintaiZokuseiBuilder WithId(long? id)
        {
            _id = id;
            return this;
        }

        public KintaiZokuseiBuilder WithName(string? name)
        {
            _name = name;
            return this;
        }

        public KintaiZokuseiBuilder WithSeigenTime(decimal? seigenTime)
        {
            _seigenTime = seigenTime;
            return this;
        }

        public KintaiZokuseiBuilder WithIsMinashi(bool? isMinashi)
        {
            _isMinashi = isMinashi;
            return this;
        }

        public KintaiZokuseiBuilder WithMaxLimitTime(decimal? maxLimitTime)
        {
            _maxLimitTime = maxLimitTime;
            return this;
        }

        public KintaiZokuseiBuilder WithIsOvertimeLimit3m(bool? isOvertimeLimit3m)
        {
            _isOvertimeLimit3m = isOvertimeLimit3m;
            return this;
        }

        public KintaiZokuseiBuilder WithCode(EmployeeWorkType? code)
        {
            _code = code;
            return this;
        }

        public KintaiZokusei Build()
        {
            var result = new KintaiZokusei
            {
                Name = _name ?? "標準",
                SeigenTime = _seigenTime ?? 45.00m,
                IsMinashi = _isMinashi ?? false,
                MaxLimitTime = _maxLimitTime ?? 0m,
                IsOvertimeLimit3m = _isOvertimeLimit3m ?? false,
                Code = _code ?? EmployeeWorkType.月45時間
            };

            if (_id.HasValue)
            {
                result.Id = _id.Value;
            }

            return result;
        }
    }
}
