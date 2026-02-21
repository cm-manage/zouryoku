using Model.Model;

namespace Zouryoku.Data
{
    /// <summary>
    /// ログイン情報を記載したクラスです
    /// </summary>
    public class LoginInfo
    {
        /// <summary>ログインユーザー情報</summary>
        public required Syain User { get; set; }

        /// <summary>Entra ID ユーザーID</summary>
        public string? EntraUserId { get; set; }

        /// <summary>Entra ID ユーザー表示名</summary>
        public string? EntraDisplayName { get; set; }

        /// <summary>Entra ID メールアドレス</summary>
        public string? EntraEmail { get; set; }

        /// <summary>認証方法（Entra or Cookie）</summary>
        public string AuthenticationMethod { get; set; } = "Cookie";

        /// <summary>セッション情報の最終更新日時（DB再取得タイミングの判定用）</summary>
        public DateTime LastRefreshedAt { get; set; }
    }
}
