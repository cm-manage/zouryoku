using System;
using CommonLibrary.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Data;

namespace ZouryokuCommonLibrary.Communicators.Services
{
    public abstract class OneExecByMonthService<MySelf> : OneExecService<MySelf>
    {
        /// <summary>
        /// 処理を実行する日（月末は99）
        /// </summary>
        protected abstract int ExecDay { get; }

        /// <summary>
        /// 処理を実行する時間[HH:mm]
        /// </summary>
        protected abstract TimeSpan? ExecTime { get; }

        protected override int SpanTime => 60;
        /// <summary>
        /// 一日一回行う処理を実装してください。
        /// </summary>
        protected abstract void ExecuteByMonth(ZouContext db);

        protected OneExecByMonthService(IServiceScopeFactory scopeFactory, IOptions<AppConfig> optionsAccessor, ILogger<MySelf> logger)
            : base(scopeFactory, optionsAccessor, logger)
        {
            BaseDate = DateTime.Today;
            NextExecuteDate = GetJudgeDateTime(BaseDate);
            // 今月未実行であれば実行する｡
            if (NextExecuteDate <= BaseDate)
            {
                Executed = true;
            }
        }

        protected override void ExecuteByOnce(ZouContext db)
        {
            ExecuteByMonth(db);
        }

        /// <summary>
        /// 1月に1回の処理を行うかを判断します
        /// </summary>
        protected override bool DoesExecute()
        {
            var date = DateTime.Now.ToDateByYYYYMMDDSlashHHmm();
            if (BaseDate.Month != date.Month)
            {
                // 翌月になったら処理対象日時､と処理済みフラグの再設定
                BaseDate = date.Date;
                NextExecuteDate = GetJudgeDateTime(BaseDate);
                Executed = false;
            }
            // 今月未実行であれば実行する｡
            if (!Executed && NextExecuteDate <= date)
            {
                Executed = true;
                return true;
            }
            return false;
        }

        private DateTime GetJudgeDateTime(DateTime date)
        {
            var targetDate = ExecDay == 99
                ? new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month))
                : new DateTime(date.Year, date.Month, ExecDay);
            return CombineDateTime(targetDate, ExecTime ?? "00:00".ToTimeSpan());
        }
    }
}
