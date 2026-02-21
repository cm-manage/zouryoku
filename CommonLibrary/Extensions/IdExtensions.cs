namespace CommonLibrary.Extensions
{
    public static class IdExtensions
    {
        public static bool IsNew(this long id)
            => id == 0;

        public static bool IsNew(this int id)
            => id == 0;

        public static bool IsNotNew(this long id)
            => !id.IsNew();

        public static bool IsNotNew(this int id)
            => !id.IsNew();
    }
}
