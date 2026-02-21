using EnumsNET;
using Model.Enums;
using Model.Model;
using Zouryoku.Enums;
using Zouryoku.Extensions;

namespace Zouryoku.Utils
{
    /// <summary>
    /// 権限系ユーティリティ
    /// </summary>
    public class AuthUtil
    {
        /// <summary>
        /// ログインユーザーが指定されたメニューコードに対して権限を持っているか確認する
        /// </summary>
        /// <param name="menuCode">権限の確認を行うメニューコード</param>
        /// <param name="loginUser">ログインユーザー情報</param>
        /// <returns>true: 権限あり, false: 権限なし</returns>
        public static bool IsAuth(MenuCode menuCode, Syain loginUser)
        {
            var menuKengen = menuCode.GetKengen();

            // メニュー権限が設定されていない場合、
            // ログインユーザーの権限に関わらず表示
            if (menuKengen == EmployeeAuthority.None) return true;

            // メニュー権限のうち、社員がどれか一つでも権限を持っていればtrue
            return loginUser.Kengen.HasAnyFlags(menuKengen);
        }
    }
}
