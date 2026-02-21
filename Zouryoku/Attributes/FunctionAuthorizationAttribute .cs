using Zouryoku.Extensions;
using Zouryoku.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Model.Data;
using Model.Enums;
using Zouryoku.Data;
using EnumsNET;

namespace Zouryoku.Attributes
{
    /// <summary>
    /// 機能認可属性
    /// </summary>
    /// <remarks>
    /// この属性をPageModelに付与することで、以下の認可チェックを行います：
    /// <list type="bullet">
    /// <item><description>セッションにLoginInfoが存在するかチェック（認証チェック）</description></item>
    /// <item><description>ユーザーが対象機能にアクセスする権限を持っているかチェック（認可チェック）</description></item>
    /// </list>
    /// セッションが存在しない場合はログイン画面にリダイレクトします。
    /// 権限がない場合は403エラーページにリダイレクトします。
    /// </remarks>
    /// <example>
    /// 使用例：
    /// <code>
    /// [FunctionAuthorization]
    /// public class IndexModel : BasePageModel&lt;IndexModel&gt;
    /// {
    ///     public void OnGet() { ... }
    /// }
    /// </code>
    /// </example>
    public class FunctionAuthorizationAttribute : Attribute, IResourceFilter
    {
        private readonly EmployeeAuthority[] authorities;

        /// <summary>
        /// FunctionAuthorizationAttributeのコンストラクタ
        /// </summary>
        /// <param name="authorities">
        /// 機能を使用するために必要な権限
        /// 
        /// 設定しない場合は全ユーザーのアクセスを許可
        /// OR条件にしたい場合 : "|"を使用する
        /// AND条件にしたい場合  : 配列を使用する
        /// </param>
        /// <example>
        /// 使用例 : PCログ出力または勤怠データ出力の権限を持つユーザー、かつ、指示最終承認者の権限を持つユーザー
        /// <code>
        /// [FunctionAuthorization(
        ///     EmployeeAuthority.PCログ出力 | EmployeeAuthority.勤怠データ出力,
        ///     EmployeeAuthority.指示最終承認者)]
        /// </code>
        /// </example>
        public FunctionAuthorizationAttribute(params EmployeeAuthority[] authorities)
        {
            this.authorities = authorities ?? [];
        }

        /// <summary>
        /// リソース実行前に認証・認可チェックを実行します
        /// </summary>
        /// <param name="context">リソース実行コンテキスト</param>
        /// <remarks>
        /// モデルバインドされる前に実行され、以下の処理を行います：
        /// <list type="number">
        /// <item><description>セッションからLoginInfoを取得</description></item>
        /// <item><description>LoginInfoが存在する場合、権限チェックを実施（現在は常に許可）</description></item>
        /// <item><description>LoginInfoが存在しない場合、ログイン画面にリダイレクト</description></item>
        /// <item><description>権限がない場合、403エラーページにリダイレクト</description></item>
        /// </list>
        /// </remarks>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            context.HttpContext.Session.Get<LoginInfo>()
                .Some(x =>
                {
                    // 遷移する画面にて権限の設定がなかった場合は、全ユーザーに対してアクセス許可
                    if (authorities.Length == 0)
                        return;

                    // 権限チェック処理
                    var result = authorities.All(a => x.User.Kengen.HasAnyFlags(a));
                    
                    if (result)
                    {
                        // 権限あり：処理を継続
                        return;
                    }
                    else
                    {
                        // 権限なし：403エラーページにリダイレクト
                        context.Result = new RedirectResult("/page403/");
                    }
                })
                .None(() =>
                {
                    // セッションが存在しない（未認証）：ログイン画面にリダイレクト
                    context.Result = new RedirectResult("~/Logins/Index");
                });
        }

        /// <summary>
        /// リソース実行後の処理（現在は未使用）
        /// </summary>
        /// <param name="context">リソース実行完了コンテキスト</param>
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            // 現在は実装なし
        }
    }
}
