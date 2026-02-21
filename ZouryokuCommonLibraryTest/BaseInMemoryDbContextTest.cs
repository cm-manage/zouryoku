using CommonLibrary.Extensions;
using ZouryokuCommonLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model;
using Moq;
using Model.Data;

namespace ZouryokuCommonLibraryTest
{
    [TestClass]
    public class BaseInMemoryDbContextTest
    {
        protected IServiceScopeFactory scopeFactory = null!;
        protected IOptions<AppConfig> options = null!;
        protected ZouContext db = null!;

        protected static ILogger<T> GetLogger<T>() where T : class => new Mock<ILogger<T>>().Object;

        protected DateOnly D(string s) => s.ToDateOnly();
        protected DateTime DT(string s) => DateTime.Parse(s);

        [TestInitialize]
        public virtual void Setup()
        {
            var dboptions = new DbContextOptionsBuilder<ZouContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var context = new ZouContext(dboptions);

            // トランザクションのモック
            var tranMock = new Mock<IDbContextTransaction>();

            // DatabaseFacade のモック
            var databaseMock = new Mock<DatabaseFacade>(context) { CallBase = true };
            databaseMock.Setup(x => x.BeginTransaction()).Returns(tranMock.Object);

            // Database プロパティを差し替え
            var contextMock = new Mock<ZouContext>(dboptions) { CallBase = true };
            contextMock.Setup(x => x.Database).Returns(databaseMock.Object);

            // IServiceProvider モック
            var providerMock = new Mock<IServiceProvider>();
            providerMock
             .Setup(x => x.GetService(typeof(ZouContext)))
             .Returns(() =>
             {
                 // 
                 var contextMock = new Mock<ZouContext>(dboptions) { CallBase = true };
                 contextMock.Setup(x => x.Database).Returns(databaseMock.Object);
                 return contextMock.Object;
             });

            // IServiceScope モック
            var scopeMock = new Mock<IServiceScope>();
            scopeMock.SetupGet(x => x.ServiceProvider).Returns(providerMock.Object);

            // IServiceScopeFactory モック
            var scopeFactoryMock = new Mock<IServiceScopeFactory>();
            scopeFactoryMock.Setup(x => x.CreateScope()).Returns(scopeMock.Object);

            scopeFactory = scopeFactoryMock.Object;
            db = contextMock.Object;
            options = GetIOptions();
        }

        protected IOptions<AppConfig> GetIOptions(AppSettings? app = null)
        {
            var op = new Mock<IOptions<AppConfig>>();
            var conf = new AppConfig()
            {
                AppSettings = new AppSettings()
                {
                    WebApplication = new WebApplication(), // 必須プロパティを初期化
                    MailPath = new MailPath()
                    {
                        Host = "smtp.example.com",
                        Port = 587,
                        FromMail = "test@example.com",
                        RequestHost = "http://localhost",
                    },
                }
            };
            if (app is not null)
            {
                conf.AppSettings = app;
            }
            op.SetupGet(x => x.Value).Returns(conf);
            return op.Object;
        }
    }
}
