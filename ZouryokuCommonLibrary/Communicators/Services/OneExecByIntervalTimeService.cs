using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Data;

namespace ZouryokuCommonLibrary.Communicators.Services
{
    public abstract class OneExecByIntervalTimeService<MySelf> : Communicator<MySelf>
    {
        private DateTime execTime;

        /// <summary>
        /// 処理を実行する間隔[HH:mm:ss]or[HH:mm]
        /// </summary>
        /// <remarks>
        /// 設定値[00:00:20]
        /// 処理時間[5秒]
        /// 上記の条件の場合、以下の流れの実行となる
        /// 1.起動時、即時実行
        /// 2.処理開始、5秒後、終了
        /// 3.20-5＝15秒後、再度処理実行
        /// </remarks>
        protected abstract TimeSpan ExecInterval { get; }

        /// <summary>
        /// 処理を実行しない時間(FROM)[HH:mm:ss]or[HH:mm]
        /// </summary>
        protected abstract TimeSpan? ExecExclusionFrom { get; }

        /// <summary>
        /// 処理を実行しない時間(TO)[HH:mm:ss]or[HH:mm]
        /// </summary>
        protected abstract TimeSpan? ExecExclusionTo { get; }
        protected override int SpanTime => 1;

        /// <summary>
        /// 指定時間ごとに行う処理を実装してください。
        /// </summary>
        protected abstract void ExecuteByTime(ZouContext db);

        protected OneExecByIntervalTimeService(IServiceScopeFactory scopeFactory, IOptions<AppConfig> optionsAccessor, ILogger<MySelf> logger) : base(scopeFactory, optionsAccessor, logger)
        {
            execTime = DateTime.Now;
            IsOutputCommunicatorLog = false;
        }

        protected override void Execute(ZouContext db)
        {
            if (DoesExecute())
            {
                logger.LogInformation($"{TaskName}を開始。");
                ExecuteByTime(db);
                logger.LogInformation($"{TaskName}を終了。");
            }
        }

        /// <summary>
        /// 前回の実行から指定時間以上間隔が空いているか確認
        /// </summary>
        private bool DoesExecute()
        {
            var date = DateTime.Now;
            var time = new TimeSpan(date.Hour, date.Minute, date.Second);
            // 実行時間外かどうか
            var isExclusion = (ExecExclusionFrom.HasValue && ExecExclusionTo.HasValue)
                ? ExecExclusionFrom > ExecExclusionTo
                    ? ExecExclusionFrom <= time || time <= ExecExclusionTo
                    : ExecExclusionFrom <= time && time <= ExecExclusionTo
                : false;

            if (date - execTime >= ExecInterval && !isExclusion)
            {
                // 指定時間以上経過していれば実行する｡
                execTime = date;

                return true;
            }
            return false;
        }
    }
}
