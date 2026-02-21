using System.Collections.Immutable;

namespace ZouryokuCommonLibrary.ModelsPartial
{
    public class Operation : CodeName<bool>
    {
        private Operation(bool code, string name)
        {
            Code = code;
            Name = name;
        }

        public const bool 稼働中_bool = true;
        public const bool 稼働停止_bool = false;

        public static readonly Operation 稼働中 = new Operation(稼働中_bool, "稼働中");
        public static readonly Operation 稼働停止 = new Operation(稼働停止_bool, "稼働停止");

        public static readonly ImmutableList<Operation> OperationList = ImmutableList.Create(
            稼働中,
            稼働停止
            );


    }
}
