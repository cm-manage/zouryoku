using CommonLibrary.Extensions;
using LanguageExt;
using Microsoft.AspNetCore.Authentication;
using Model.Data;
using Newtonsoft.Json;
using System.Data;
using Zouryoku.Data;
using static CommonLibrary.Extensions.OptionExtensions;
using static LanguageExt.Prelude;

namespace Zouryoku.Extensions
{
    public static class SessionExtensions
    {
        /// <summary>
        /// JsonSerialize可能なオブジェクトを設定します。Serializeの仕様は以下を確認してください。
        /// https://docs.microsoft.com/ja-jp/dotnet/standard/serialization/system-text-json-how-to
        /// </summary>
        public static T Set<T>(this ISession session, T value, string? sessionName = null)
        {
            var name = OptionalT(sessionName).IfNone(() => typeof(T).FullName ?? string.Empty);
            session.SetString(name,
                JsonConvert.SerializeObject(
                    value,
                    Formatting.Indented,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
                )
            );
            return value;
        }

        public static Option<T> Get<T>(this ISession session)
            => Optional(session.GetString(typeof(T).FullName ?? string.Empty))
                .Map(x => Optional(JsonConvert.DeserializeObject<T>(x))).Flatten();

        public static Option<T> Get<T>(this ISession session, string? sessionName = null)
        {
            var name = OptionalT(sessionName).IfNone(() => typeof(T).FullName ?? string.Empty);

            if (typeof(T) == typeof(byte[]))
            {
                if (session.TryGetValue(name, out var value))
                {
                    return (T)(object)value;
                }
                else
                {
                    return None;
                }
            }
            else
            {
                return OptionalT(session.GetString(name))
                    .Map(x => Optional(JsonConvert.DeserializeObject<T>(x))).Flatten();
            }
        }

        public static void Clear<T>(this ISession session, string? sessionName = null)
        {
            var name = OptionalT(sessionName).IfNone(() => typeof(T).FullName ?? string.Empty);
            session.Remove(name);
        }

        public static void Clear(this ISession session, string sessionName)
        {
            session.Remove(sessionName);
        }

        public static Option<T> GetAndClear<T>(this ISession session, string? sessionName = null)
        {
            var result = session.Get<T>(sessionName);
            session.Clear<T>(sessionName);
            return result;
        }

        public static LoginInfo LoginInfo(this ISession session)
            => session.Get<LoginInfo>()
                .GetOrThrowException("LoginInfo method は FunctionAuthAttribute 処理後に呼び出してください");
    }
}
