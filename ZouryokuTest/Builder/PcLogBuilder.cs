using Model.Enums;
using Model.Model;

namespace ZouryokuTest.Builder
{
    /// <summary>
    /// Pcログ用のBuilder
    /// </summary>
    internal class PcLogBuilder
    {
        private long _id;
        private DateTime _datetime;
        private string _pcName = string.Empty;
        private PcOperationType _operation;
        private string? _userName;
        private long? _syainId;

        public PcLogBuilder WithId(long id)
        {
            _id = id;
            return this;
        }

        public PcLogBuilder WithDatetime(DateTime datetime)
        {
            _datetime = datetime;
            return this;
        }

        public PcLogBuilder WithPcName(string pcName)
        {
            _pcName = pcName;
            return this;
        }

        public PcLogBuilder WithOperation(PcOperationType operation)
        {
            _operation = operation;
            return this;
        }
        public PcLogBuilder WithUserName(string userName)
        {
            _userName = userName;
            return this;
        }

        public PcLogBuilder WithSyainId(long? syainId)
        {
            _syainId = syainId;
            return this;
        }

        public PcLog Build()
        {
            return new PcLog
            {
                Id = _id,
                Datetime =_datetime,
                PcName = _pcName,
                Operation = _operation,
                UserName = _userName,
                SyainId = _syainId,
            };
        }
    }
}
