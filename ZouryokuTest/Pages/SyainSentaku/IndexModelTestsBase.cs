using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.SyainSentaku;
using static Zouryoku.Utils.Const;

namespace ZouryokuTest.Pages.SyainSentaku
{
    [TestClass]
    public abstract class IndexModelTestsBase : BaseInMemoryDbContextTest
    {
        /// <summary>
        /// ログインユーザーの部署ID
        /// </summary>
        private const long LoggedInUserBusyoID = 111;

        /// <summary>
        /// テスト実施日の日付
        /// </summary>
        public static readonly DateTime TestDate = new(2026, 2, 2);

        /// <summary>
        /// 順序確認用配列
        /// </summary>
        public static readonly long[] OrderList = [2, 3, 1];

        /// <summary>
        /// 複数選択時のPartialファイル名
        /// </summary>
        public const string PartialMultiple = "_SyainSentakuMultiplePartial";

        /// <summary>
        /// 単数選択時のPartialファイル名
        /// </summary>
        public const string PartialSingle = "_SyainSentakuSinglePartial";

        /// <summary>
        /// 保存するセッション名
        /// </summary>
        public const string SaveSessionName = "selectedBusyoId";

        /// <summary>
        /// 社員名未入力時のエラーメッセージ
        /// </summary>
        public readonly string ErrorMsgSyainNameRequired = string.Format(ErrorRequired, "社員名");

        /// <summary>
        /// 社員未選択時のエラーメッセージ
        /// </summary>
        public readonly string ErrorMsgSyainSelectRequired = string.Format(ErrorSelectRequired, "社員");

        /// <summary>
        /// IndexModel(社員選択)のユニットテスト
        /// </summary>
        /// <param name="loginUser">ログインユーザー</param>
        /// <returns>Model</returns>
        protected IndexModel CreateModel(Syain? loginUser = null)
        {
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine, fakeTimeProvider){
                PageContext = GetPageContext(),
                TempData = GetTempData()
            };
            SetLoggedInUser(model);
            return model;
        }

        /// <summary>
        /// モデルのセッションにログイン情報を作成する
        /// </summary>
        /// <param name="model">ログイン情報を作成するモデル</param>
        /// <returns>ログイン情報が格納されたモデル</returns>
        protected IndexModel SetLoggedInUser(IndexModel model)
        {
            var emp = new Syain(){
                BusyoId = LoggedInUserBusyoID,
            };
            db.Syains.Add(emp);

            // LoginInfoを作成
            var loginInfo = new LoginInfo { User = emp };

            // セッションに格納
            model.HttpContext.Session.Set(loginInfo);

            return model;
        }

        protected static List<BusyoViewModel> DeserializeNodes(JsonResult json)
        {
            if (json == null) throw new ArgumentNullException(nameof(json));

            if (json.Value is List<BusyoViewModel> list) return list;
            if (json.Value is IEnumerable<BusyoViewModel> enumerable) return enumerable.ToList();

            return new List<BusyoViewModel>();
        }

        // ================================================
        // Helper Method
        // ================================================

        /// <summary>
        /// シード処理
        /// </summary>
        protected void SeedEntities(params object[] entities)
        {
            foreach (var e in entities)
            {
                if (e is IEnumerable<object> list)
                {
                    db.AddRange(list);
                }
                else
                {
                    db.Add(e);
                }
            }
            db.SaveChanges();
        }

        /// <summary>
        /// 応答のアサーション
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        protected static JsonResult AssertJson(IActionResult result)
        {
            Assert.IsInstanceOfType<JsonResult>(result);
            return (JsonResult)result;
        }

        /// <summary>
        /// 社員情報の辞書作成
        /// </summary>
        /// <returns>辞書化した社員情報</returns>
        public static Dictionary<long, SyainViewModel> CreatePreSelectedSyain()
        {
            Syain[] syains =
            [
                new Syain { Id = 10, Name = "社員1", BusyoId = 1, SyainBaseId = 1, Code = "01", Retired = false },
                new Syain { Id = 20, Name = "社員2", BusyoId = 1, SyainBaseId = 2, Code = "02", Retired = false },
                new Syain { Id = 30, Name = "社員3", BusyoId = 1, SyainBaseId = 3, Code = "03", Retired = false }
            ];

            var dict = syains
                .Select(s => new SyainViewModel(s))
                .ToDictionary(vm => vm.SyainBaseId, vm => vm);

            return dict;
        }

        /// <summary>
        /// 所属社員数をキーとした辞書作成
        /// </summary>
        /// <returns>所属社員数をキーとした辞書</returns>
        public static Dictionary<long, int> CreatePreSelectedBusyoCounts()
        {
            Syain[] syains =
            [
                new Syain { Id = 10, Name = "社員1", BusyoId = 1, SyainBaseId = 1, Code = "01", Retired = false },
                new Syain { Id = 20, Name = "社員2", BusyoId = 1, SyainBaseId = 2, Code = "02", Retired = false },
                new Syain { Id = 30, Name = "社員3", BusyoId = 1, SyainBaseId = 3, Code = "03", Retired = false }
            ];

            var dict = syains
                .GroupBy(s => s.BusyoId)
                .ToDictionary(g => g.Key, g => g.Count());

            return dict;
        }
    }
}
