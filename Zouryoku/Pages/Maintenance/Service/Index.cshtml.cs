using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Model.Extensions;
using Model.Model;
using System.ComponentModel.DataAnnotations;
using Zouryoku.Attributes;
using Zouryoku.Extensions;
using Zouryoku.Pages.Shared;

namespace Zouryoku.Pages.Maintenance.Service
{
    [FunctionAuthorizationAttribute]
    public class IndexModel(ZouContext db, ILogger<IndexModel> logger, IOptions<AppConfig> optionsAccessor, TimeProvider? timeProvider = null) : BasePageModel<IndexModel>(db, logger, optionsAccessor, timeProvider)
    {
        /// <summary>
        /// 入力画面用共通CSS/JSをレイアウトで読み込むかどうかのフラグ
        /// </summary>
        public override bool UseInputAssets { get; } = true;

        [BindProperty]
        public ServiceExecuteModel ServiceExecuteData { get; set; } = new ServiceExecuteModel();

        /// <summary>
        /// 画面初期表示
        /// </summary>
        public async Task<IActionResult> OnGetAsync()
        {
            // サービス稼働状況を一度のDBアクセスで取得
            var serviceRecords = await db.ServiceExecutes
                .AsNoTracking()
                .Where(s =>
                    s.Type == ServiceClassification.連携プログラム稼働 ||
                    s.Type == ServiceClassification.過労運転防止 ||
                    s.Type == ServiceClassification.有給未取得アラート ||
                    s.Type == ServiceClassification.チャット連携)
                .ToListAsync();

            foreach (var record in serviceRecords)
            {
                switch (record.Type)
                {
                    case ServiceClassification.連携プログラム稼働:
                        ServiceExecuteData.KingsIntegrationProgram = record.Used;
                        break;
                    case ServiceClassification.過労運転防止:
                        ServiceExecuteData.DriverFatiguePrevention = record.Used;
                        break;
                    case ServiceClassification.有給未取得アラート:
                        ServiceExecuteData.RestPrevention = record.Used;
                        break;
                    case ServiceClassification.チャット連携:
                        ServiceExecuteData.ChatPrevention = record.Used;
                        break;
                }
            }

            return Page();
        }

        /// <summary>
        /// 入力送信（更新）
        /// </summary>
        public async Task<IActionResult> OnPostRegisterAsync()
        {
            var errorJson = ModelState.ErrorJson();
            if (errorJson is not null)
            {
                return errorJson;
            }

            var serviceRecords = await db.ServiceExecutes
                .Where(s =>
                    s.Type == ServiceClassification.連携プログラム稼働 ||
                    s.Type == ServiceClassification.過労運転防止 ||
                    s.Type == ServiceClassification.有給未取得アラート ||
                    s.Type == ServiceClassification.チャット連携)
                .ToListAsync();

            // 各Typeのレコードを取得または作成
            var kingsRecord = serviceRecords.FirstOrDefault(s => s.Type == ServiceClassification.連携プログラム稼働)
                ?? db.ServiceExecutes.AddReturn(new ServiceExecute
                {
                    Type = ServiceClassification.連携プログラム稼働
                });
            var fatigueRecord = serviceRecords.FirstOrDefault(s => s.Type == ServiceClassification.過労運転防止)
                ?? db.ServiceExecutes.AddReturn(new ServiceExecute
                {
                    Type = ServiceClassification.過労運転防止
                });
            var restRecord = serviceRecords.FirstOrDefault(s => s.Type == ServiceClassification.有給未取得アラート)
                ?? db.ServiceExecutes.AddReturn(new ServiceExecute
                {
                    Type = ServiceClassification.有給未取得アラート
                });
            var chatRecord = serviceRecords.FirstOrDefault(s => s.Type == ServiceClassification.チャット連携)
                ?? db.ServiceExecutes.AddReturn(new ServiceExecute
                {
                    Type = ServiceClassification.チャット連携
                });

            // 値を更新
            kingsRecord.Used = ServiceExecuteData.KingsIntegrationProgram;
            fatigueRecord.Used = ServiceExecuteData.DriverFatiguePrevention;
            restRecord.Used = ServiceExecuteData.RestPrevention;
            chatRecord.Used = ServiceExecuteData.ChatPrevention;

            await db.SaveChangesAsync();

            return Success();
        }

        /// <summary>
        /// サービス実行入力用ビュー / バインドモデル
        /// </summary>
        public class ServiceExecuteModel
        {
            /// <summary>KINGS連携プログラム稼働</summary>
            [Display(Name = "KINGS連携プログラム稼働")]
            public bool KingsIntegrationProgram { get; set; }

            /// <summary>過労運転防止</summary>
            [Display(Name = "過労運転防止")]
            public bool DriverFatiguePrevention { get; set; }

            /// <summary>有給未取得アラート</summary>
            [Display(Name = "有給未取得アラート")]
            public bool RestPrevention { get; set; }

            /// <summary>チャット連携</summary>
            [Display(Name = "チャット連携")]
            public bool ChatPrevention { get; set; }
        }
    }
}