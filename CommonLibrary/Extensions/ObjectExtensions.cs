using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace CommonLibrary.Extensions
{
    /// <summary>
    /// object型の拡張メソッドを管理するクラス
    /// </summary>
    public static class ObjectExtensions
    {
        public static string ToJson(this object obj) => JsonSerializer.Serialize(obj);
    }
}
