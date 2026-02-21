using static LanguageExt.Prelude;

namespace CommonLibrary.Extensions
{
    public static class DecimalExtensions
    {
        public static int? ToInt(this decimal? value)
            => Optional(value).Map(x => (int?)x.ToInt()).IfNoneUnsafe(() => null);

        public static int ToInt(this decimal value)
            => decimal.ToInt32(value);
    }
}
