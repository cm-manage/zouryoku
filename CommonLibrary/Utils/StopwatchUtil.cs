using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CommonLibrary.Utils
{
    public class StopwatchUtil<MySelf>(ILogger<MySelf> logger)
    {
        protected readonly ILogger<MySelf> logger = logger;

        public void Using(string label, Action action, LogLevel logLevel = LogLevel.Debug)
        {
            logger.Log(logLevel, "{label}の処理開始", label);
            var watch = new Stopwatch();

            watch.Start();
            try
            {
                action();
            }
            finally
            {
                watch.Stop();
                logger.Log(logLevel, "{label}の処理時間 = {time}", label, watch.Elapsed);
            }
        }

        public A Using<A>(string label, Func<A> func, LogLevel logLevel = LogLevel.Debug)
        {
            logger.Log(logLevel, "{label}の処理開始", label);
            var watch = new Stopwatch();

            watch.Start();
            try
            {
                return func();
            }
            finally
            {
                watch.Stop();
                logger.Log(logLevel, "{label}の処理時間 = {time}", label, watch.Elapsed);
            }
        }

        public async Task<A> UsingAsync<A>(string label, Func<Task<A>> func, LogLevel logLevel = LogLevel.Debug)
        {
            logger.Log(logLevel, "{label}の処理開始", label);
            var watch = new Stopwatch();

            watch.Start();
            try
            {
                var result = func();
                return await result;
            }
            finally
            {
                watch.Stop();
                logger.Log(logLevel, "{label}の処理時間 = {time}", label, watch.Elapsed);
            }
        }

        public async Task UsingAsync(string label, Func<Task> func, LogLevel logLevel = LogLevel.Debug)
        {
            logger.Log(logLevel, "{label}の処理開始", label);
            var watch = new Stopwatch();

            watch.Start();
            try
            {
                await func();
            }
            finally
            {
                watch.Stop();
                logger.Log(logLevel, "{label}の処理時間 = {time}", label, watch.Elapsed);
            }
        }
    }
}
