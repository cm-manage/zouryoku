using Model.Enums;
using Model.Model;

namespace Zouryoku.Pages.RoleDefaultKengen
{
    public partial class IndexModel
    {
        /// <summary>
        /// ユーザーロールのビューモデル
        /// </summary>
        public class RoleViewModel(UserRole userRole)
        {
            private UserRole _userRole = userRole;

            /// <value>ロールID</value>
            public long Id => _userRole.Id;

            /// <value>ロール名称</value>
            public string Name => _userRole.Name;

            /// <value>ロールが所持している社員権限</value>
            public EmployeeAuthority Kengen => _userRole.Kengen;
        }
    }
}
