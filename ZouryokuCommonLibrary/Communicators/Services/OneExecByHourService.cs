using System;
using System.Collections.Generic;
using System.Linq;
using CommonLibrary.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Data;

namespace ZouryokuCommonLibrary.Communicators.Services
{
    public abstract class OneExecByHourService<MySelf> : OneExecService<MySelf>
    {
        /// <summary>
        /// 処理を実行する時間(分指定カンマ区切り)
        /// </summary>
        protected abstract List<int> ExecMinutes { get; }

        protected override int SpanTime => 1;

        /// <summary>
        /// 指定時間ごとに行う処理を実装してください。
        /// </summary>
        protected abstract void ExecuteByHour(ZouContext db);

        protected OneExecByHourService(IServiceScopeFactory scopeFactory, IOptions<AppConfig> optionsAccessor, ILogger<MySelf> logger)
            : base(scopeFactory, optionsAccessor, logger)
        {
            BaseDate = DateTime.Now.ToDateByYYYYMMDDSlashHHmm();
            NextExecuteDate = GetNextExecuteDate(BaseDate);
            if (NextExecuteDate <= BaseDate)
            {
                Executed = true;
            }
        }

        protected override void ExecuteByOnce(ZouContext db)
        {
            ExecuteByHour(db);
        }

        /// <summary>
        /// 1時間1回の処理を行うかを判断します
        /// </summary>
        protected override bool DoesExecute()
        {
            var date = DateTime.Now.ToDateByYYYYMMDDSlashHHmm();
            if (BaseDate.Minute != date.Minute)
            {
                // 翌日になったら処理対象日時､と処理済みフラグの再設定
                BaseDate = date;
                NextExecuteDate = GetNextExecuteDate(BaseDate);
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

        private DateTime GetNextExecuteDate(DateTime date)
            => ExecMinutes.FirstOption(x => x >= date.Minute)
                .Some(x => CombineDateTime(date, x))
                .None(() => CombineDateTime(date.AddHours(1), ExecMinutes.First()));
    }
}
