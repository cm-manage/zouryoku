using System;
using System.Threading;
using System.Threading.Tasks;
using CommonLibrary.Utils;
using ZouryokuCommonLibrary.Extensions;
using ZouryokuCommonLibrary.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Data;

namespace ZouryokuCommonLibrary.Communicators.Services
{
    public abstract class Communicator<MySelf> : BackgroundService
    {
        protected readonly IServiceScopeFactory scopeFactory;
        protected readonly AppSettings appSettings;
        protected readonly ILogger<MySelf> logger;

        /// <summary>
        /// Communicatorのログを出力するか
        /// </summary>
        protected bool IsOutputCommunicatorLog { get; set; } = true;
        /// <summary>
        /// 処理名
        /// </summary>
        protected abstract string TaskName { get; }
        /// <summary>
        /// 実行間隔（秒を数値指定）
        /// </summary>
        protected abstract int SpanTime { get; }

        /// <summary>
        /// 処理を実装してください。
        /// </summary>
        protected abstract void Execute(ZouContext db);

        protected virtual Task ExecuteServiceAsync() { return Task.CompletedTask; }

        protected Communicator(IServiceScopeFactory scopeFactory, IOptions<AppConfig> optionsAccessor, ILogger<MySelf> logger)
        {
            this.scopeFactory = scopeFactory;
            appSettings = optionsAccessor.Value.AppSettings;
            this.logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation($"「{TaskName}」処理サービス起動");

            stoppingToken.Register(() =>
                logger.LogDebug($"{TaskName} background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                if (IsOutputCommunicatorLog) { logger.LogInformation($"{TaskName} 処理を開始します。"); }

                try
                {
                    scopeFactory.Using(db =>
                    {
                        Execute(db);
                    });
                    await ExecuteServiceAsync();
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"{TaskName} エラーが発生しました。");
                }
                finally
                {
                    if (IsOutputCommunicatorLog) { logger.LogInformation($"{TaskName} 処理を終了しました。"); }
                }

                await Task.Delay(SpanTime * 1000, stoppingToken);
            }
        }
    }
}
