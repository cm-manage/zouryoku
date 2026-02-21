using System.Collections.Immutable;

namespace ZouryokuCommonLibrary.ModelsPartial
{
    public class MailSent : CodeName<bool>
    {
        private MailSent(bool code, string name)
        {
            Code = code;
            Name = name;
        }

        public const bool 送信済_bool = true;
        public const bool 未送信_bool = false;

        public static readonly MailSent 送信済 = new MailSent(送信済_bool, "送信済");
        public static readonly MailSent 未送信 = new MailSent(未送信_bool, "未送信");

        public static readonly ImmutableList<MailSent> MailSentList = ImmutableList.Create(
            送信済,
            未送信
            );


    }
}
