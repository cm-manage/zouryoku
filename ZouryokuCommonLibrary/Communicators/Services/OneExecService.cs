using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Data;

namespace ZouryokuCommonLibrary.Communicators.Services
{
    public abstract class OneExecService<MySelf> : Communicator<MySelf>
    {
        /// <summary>
        /// 実行フラグ
        /// </summary>
        protected bool Executed = false;

        /// <summary>
        /// 基準日時
        /// </summary>
        protected DateTime BaseDate;

        /// <summary>
        /// 次回実行日時
        /// </summary>
        protected DateTime NextExecuteDate;

        /// <summary>
        /// 処理実行判定
        /// </summary>
        protected abstract bool DoesExecute();

        /// <summary>
        /// 一日一回行う処理を実装してください。
        /// </summary>
        protected abstract void ExecuteByOnce(ZouContext db);

        protected OneExecService(IServiceScopeFactory scopeFactory, IOptions<AppConfig> optionsAccessor, ILogger<MySelf> logger)
            : base(scopeFactory, optionsAccessor, logger)
        {
            IsOutputCommunicatorLog = false;
        }

        protected override void Execute(ZouContext db)
        {
            if (DoesExecute())
            {
                logger.LogInformation($"{TaskName}を開始。");
                ExecuteByOnce(db);
                logger.LogInformation($"{TaskName}を終了。");
            }
        }

        protected DateTime CombineDateTime(DateTime date, TimeSpan time)
            => new(date.Year, date.Month, date.Day, time.Hours, time.Minutes, 0);

        protected DateTime CombineDateTime(DateTime date, int minute)
            => new(date.Year, date.Month, date.Day, date.Hour, minute, 0);
    }
}
