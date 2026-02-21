using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model.Model;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.SyainSentaku;
using ZouryokuTest.Builder;
using ZouryokuTest.Pages.Builder;
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
            var model = new IndexModel(db, GetLogger<IndexModel>(), options, viewEngine){
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
            var emp = new SyainBuilder()
                .WithBusyoId(LoggedInUserBusyoID)
                .Build();
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
        /// 部署データを追加するヘルパーメソッド
        /// </summary>
        /// <param name="id">部署ID</param>
        /// <param name="name">部署名</param>
        /// <param name="jyunjyo">並び順序</param>
        /// <param name="active">アクティブフラグ</param>
        /// <param name="oyaId">親ID</param>
        /// <param name="start">有効開始日</param>
        /// <param name="end">有効終了日</param>
        /// <returns></returns>
        protected static Busyo AddBusyo(int id, string name, short jyunjyo, bool active, long? oyaId = null, int? start = null, int? end = null)
        {
            var today = DateTime.Now.ToDateOnly();
            // start || endがnullである場合、null, nullではない場合数値分の日付を追加
            DateOnly? startYmd = start == null ? null : today.AddDays(start.Value);
            DateOnly? endYmd = end == null ? null : today.AddDays(end.Value);

            var busyo = new BusyoBuilder()
                .WithId(id)
                .WithName(name)
                .WithJyunjyo(jyunjyo)
                .WithOyaId(oyaId)
                .WithIsActive(active)
                .WithStartYmd(startYmd)
                .WithEndYmd(endYmd)
                .Build();

            return busyo;
        }

        /// <summary>
        /// 社員データを追加するヘルパーメソッド
        /// </summary>
        /// <param name="id">社員ID</param>
        /// <param name="name">社員名</param>
        /// <param name="code">社員コード</param>
        /// <param name="jyunjyo">並び順序</param>
        /// <param name="retired">退職フラグ</param>
        /// <param name="syainBaseId">社員BaseID</param>
        /// <param name="busyoId">部署ID</param>
        /// <param name="start">有効開始日</param>
        /// <param name="end">有効終了日</param>
        /// <returns></returns>
        protected static Syain AddSyain(long id, string name, string code, short? jyunjyo, 
            bool? retired, long syainBaseId, long busyoId, int? start = null, int? end = null)
        {
            var today = DateTime.Now.ToDateOnly();
            DateOnly? startYmd = start == null ? null : today.AddDays(start.Value);
            DateOnly? endYmd = end == null ? null : today.AddDays(end.Value);

            var syain = new SyainBuilder()
                .WithId(id)
                .WithName(name)
                .WithCode(code)
                .WithStartYmd(startYmd)
                .WithEndYmd(endYmd)
                .WithJyunjyo(jyunjyo)
                .WithRetired(retired)
                .WithSyainBaseId(syainBaseId)
                .WithBusyoId(busyoId)
                .Build();

            return syain;
        }

        /// <summary>
        /// 社員Baseデータを追加するメソッド
        /// </summary>
        /// <param name="id">社員BaseID</param>
        /// <param name="name">社員名</param>
        /// <param name="code">社員コード</param>
        /// <returns></returns>
        protected static SyainBasis AddSyainBase(long id, string name, string code)
        {
            var syain = new SyainBasisBuilder()
                .WithId(id)
                .WithName(name)
                .WithCode(code)
                .Build();

            return syain;
        }

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
