using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq.Expressions;

namespace Zouryoku.Extensions
{
    public static class ModelStateExtensions
    {
        /// <summary>
        /// ModelStateにエラーを追加します。
        /// </summary>
        /// <param name="target">検証エラーを追加する対象のプロパティ</param>
        /// <param name="errorMessage">検証エラーのメッセージ</param>
        public static void AddError<A>(this ModelStateDictionary modelState, Expression<Func<A>> target, string errorMessage)
        {
            Expression? expr = target.Body;
            var stack = new Stack<string>();

            while (expr is not null)
            {
                switch (expr)
                {
                    case MemberExpression memberExpr:
                        stack.Push(memberExpr.Member.Name);
                        expr = memberExpr.Expression;
                        break;
                    case MethodCallExpression methodCallExpr:
                        if (methodCallExpr.Method.Name == "get_Item")
                        {
                            var indexExpr = methodCallExpr.Arguments[0];
                            var indexValue = Expression.Lambda(indexExpr).Compile().DynamicInvoke();
                            expr = methodCallExpr.Object;
                            stack.Push($"[{indexValue}]");
                        }
                        break;
                    case ParameterExpression:
                        expr = null;
                        break;
                    case UnaryExpression unaryExpr:
                        expr = unaryExpr.Operand;
                        break;
                    default:
                        expr = null;
                        break;
                }
            }

            var propertyName = string.Join(".", stack).Replace(".[]", "[");
            if (!string.IsNullOrWhiteSpace(propertyName))
            {
                modelState.AddModelError(propertyName, errorMessage);
            }
        }

        /// <summary>
        /// ModelStateによるValidationエラーのキーとメッセージを返します。
        /// </summary>
        public static Dictionary<string, string[]> Errors(this ModelStateDictionary modelState, params Expression<Func<KeyValuePair<string, ModelStateEntry?>, bool>>[] wheres)
        {
            if (!modelState.IsValid)
            {
                var query = modelState.AsQueryable();
                wheres.ForEach(where =>
                {
                    query = query.Where(where);
                });

                return query
                    .AsEnumerable()
                    .Where(x => x.Value?.Errors.Any() ?? false)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Value?.Errors.Select(y => y.ErrorMessage).Distinct().ToArray() ?? []
                    );
            }
            
            return [];
        }

        /// <summary>
        /// ModelStateによるValidationエラーのキーとメッセージをJsonResultで返します。
        /// ※エラーがない時は、nullが返ります。
        /// </summary>
        public static JsonResult? ErrorJson(this ModelStateDictionary modelState, params Expression<Func<KeyValuePair<string, ModelStateEntry?>, bool>>[] wheres)
        {
            var errors = modelState.Errors(wheres);

            if (errors.Any())
            {
                return new(new
                {
                    Errors = errors
                });
            }

            return null;
        }
    }
}