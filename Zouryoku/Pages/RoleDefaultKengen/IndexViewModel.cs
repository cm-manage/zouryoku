using Microsoft.AspNetCore.Mvc.Rendering;
using Model.Enums;
using System.ComponentModel.DataAnnotations;
using static Model.Enums.EmployeeAuthority;

namespace Zouryoku.Pages.RoleDefaultKengen
{
    /// <summary>
    /// ロールデフォルト権限設定のViewModelです。
    /// </summary>
    public class IndexViewModel
    {
        /// <summary>選択中ロールID</summary>
        public long SelectedRoleId { get; set; }

        /// <summary>ロールドロップダウン一覧</summary>
        public List<SelectListItem> UserRoles { get; set; } = [];

        /// <summary>画面表示用の権限</summary>
        public EmployeeAuthority Kengen { get; set; }

        /// <summary>更新用の権限値</summary>
        public long KengenValue { get; set; }

        /// <summary>
        /// 同時実行制御用のバージョン
        /// </summary>
        [Timestamp]
        public uint Version { get; set; }

        /// <summary>
        /// 社員権限Enumの一覧を返します。Noneは含みません。
        /// </summary>
        public IEnumerable<EmployeeAuthority> AllAuthorities =>
            Enum.GetValues<EmployeeAuthority>()
                .Where(authority => authority != None);
    }
}
