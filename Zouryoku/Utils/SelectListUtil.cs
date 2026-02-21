using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Zouryoku.Utils
{
    /// <summary>
    /// valueがTrue,Falseのみのセレクトボックス用SelectListItem
    /// </summary>
    public class SelectListUtil
    {
        public static List<SelectListItem> GenerateSelectList(IEnumerable<(string Value, string Text)> items)
        {
            var selectList = new List<SelectListItem>();

            items.ForEach(item =>
            {
                selectList.Add(new SelectListItem
                {
                    Value = item.Value,
                    Text = item.Text
                });
            });

            return selectList;
        }


        public static string GetText(List<SelectListItem> selectList, string value)
            => selectList.FirstOption(x => x.Value == value).Map(x => x.Text).IfNone(() => "");

        public static List<SelectListItem> StoreTopPageGraphViewList()
            => GenerateSelectList(new List<(string, string)>
            {
                ("True", "A(グラフあり)"),
                ("False", "B(グラフなし)")
            });

        public static List<SelectListItem> YesNoSelectList()
            => GenerateSelectList(new List<(string, string)>
            {
                ("True", "はい"),
                ("False", "いいえ")
            });

        public static List<SelectListItem> ExistenceSelectList()
            => GenerateSelectList(new List<(string, string)>
            {
                ("True", "あり"),
                ("False", "なし")
            });

        public static List<SelectListItem> SwitchSelectList()
            => GenerateSelectList(new List<(string, string)>
            {
                ("True", "ON"),
                ("False", "OFF")
            });

        public static List<SelectListItem> DemandAlertMailSendEnabledList()
            => GenerateSelectList(new List<(string, string)>
            {
                ("True", "送信許可"),
                ("False", "送信不可")
            });

        public static List<SelectListItem> EventNotEnteredList()
            => GenerateSelectList(new List<(string, string)>
            {
                ("False", "参加"),
                ("True", "不参加")
            });
    }
}
