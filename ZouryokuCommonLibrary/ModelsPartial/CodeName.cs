using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Immutable;
using System.Linq;

namespace ZouryokuCommonLibrary.ModelsPartial
{
    public class CodeName<T>
    {
        public T? Code { get; protected set; }
        public string? Name { get; protected set; }

        public override string ToString()
            => Code + ":" + Name;

        /// <summary>
        /// セレクトボックス用
        /// </summary>
        public static ImmutableList<SelectListItem> SelectItems<A, B>(ImmutableList<A> list) where A : CodeName<B>
            => list
                .Select(a => new SelectListItem
                {
                    Value = a.Code?.ToString(),
                    Text = a.Name,
                })
                .ToImmutableList();
    }
}
