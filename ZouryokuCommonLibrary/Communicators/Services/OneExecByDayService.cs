using System;
using CommonLibrary.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Data;

namespace ZouryokuCommonLibrary.Communicators.Services
{
    public abstract class OneExecByDayService<MySelf> : OneExecService<MySelf>
    {
        /// <summary>
        /// 処理を実行する時間[HH:mm]
        /// </summary>
        protected abstract TimeSpan ExecTime { get; }
        protected override int SpanTime => 60;

        /// <summary>
        /// 一日一回行う処理を実装してください。
        /// </summary>
        protected abstract void ExecuteByDay(ZouContext db);

        protected OneExecByDayService(IServiceScopeFactory scopeFactory, IOptions<AppConfig> optionsAccessor, ILogger<MySelf> logger)
            : base(scopeFactory, optionsAccessor, logger)
        {
            BaseDate = DateTime.Now.ToDateByYYYYMMDDSlashHHmm();
            NextExecuteDate = CombineDateTime(BaseDate, ExecTime);

            if (NextExecuteDate <= BaseDate)
            {
                Executed = true;
            }
        }

        protected override void ExecuteByOnce(ZouContext db)
        {
            ExecuteByDay(db);
        }

        /// <summary>
        /// 1日1回の処理を行うかを判断します
        /// </summary>
        protected override bool DoesExecute()
        {
            var date = DateTime.Now.ToDateByYYYYMMDDSlashHHmm();
            if (BaseDate.Date != date.Date)
            {
                // 翌日になったら処理対象日時､と処理済みフラグの再設定
                BaseDate = date;
                NextExecuteDate = CombineDateTime(BaseDate, ExecTime);
                Executed = false;
            }
            // 本日未実行であれば実行する｡
            if (!Executed && NextExecuteDate <= date)
            {
                Executed = true;
                return true;
            }
            return false;
        }
    }
}
