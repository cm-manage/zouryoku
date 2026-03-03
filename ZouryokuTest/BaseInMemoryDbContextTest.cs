using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Time.Testing;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Model.Data;
using Model.Enums;
using Moq;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Zouryoku;
using Zouryoku.Pages.Shared;
using ZouryokuCommonLibrary.ModelsPartial;
using ZouryokuCommonLibrary.Utils;
using static Model.Enums.ResponseStatus;

namespace ZouryokuTest
{
    [TestClass]
    public class BaseInMemoryDbContextTest
    {
        protected IServiceScopeFactory serviceScopeFactory = null!;
        protected IOptions<AppConfig> options = null!;
        protected ZouContext db = null!;
        protected ICompositeViewEngine viewEngine = null!;
        protected FakeTimeProvider fakeTimeProvider = null!;

        protected static ILogger<T> GetLogger<T>() where T : class => new Mock<ILogger<T>>().Object;

        protected DateOnly D(string s) => s.ToDateOnly();
        protected DateTime DT(string s) => DateTime.Parse(s);

        [TestInitialize]
        public virtual void Setup()
        {
            var dboptions = new DbContextOptionsBuilder<ZouContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning)) // トランザクション警告を無視
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

            // ICompositeViewEngine モック
            var viewEngineMock = new Mock<ICompositeViewEngine>();
            viewEngineMock
                .Setup(engine => engine.FindView(It.IsAny<ActionContext>(), It.IsAny<string>(), It.IsAny<bool>()))
             .Returns(ViewEngineResult.Found("TestView", new Mock<IView>().Object));

            db = contextMock.Object;
            serviceScopeFactory = scopeFactoryMock.Object;
            options = GetIOptions();
            viewEngine = viewEngineMock.Object;
            fakeTimeProvider = new FakeTimeProvider(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));
            fakeTimeProvider.SetLocalNow(new DateTime(2025, 7, 1, 0, 0, 0));
        }

        protected IOptions<AppConfig> GetIOptions()
        {
            var op = new Mock<IOptions<AppConfig>>();
            var ap = new Mock<AppConfig>();
            var aps = new AppSettings()
            {
                WebApplication = new ZouryokuCommonLibrary.WebApplication(), // 必須プロパティを追加
                MailPath = new ZouryokuCommonLibrary.MailPath()
                {
                    Host = "smtp.example.com",
                    Port = 587,
                    FromMail = "test@example.com",
                    RequestHost = "http://localhost",
                },
            };
            ap.SetupGet(x => x.AppSettings).Returns(aps);
            op.SetupGet(x => x.Value).Returns(ap.Object);
            return op.Object;
        }


        protected PageContext GetPageContext()
        {
            var mockSession = new Mock<ISession>();

            // セッションの内部ストレージを模倣
            var sessionStorage = new Dictionary<string, byte[]>();

            mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) =>
                {
                    sessionStorage[key] = value;
                });

            mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny))
                .Returns((string key, out byte[]? value) =>
                {
                    if (sessionStorage.TryGetValue(key, out var storedValue))
                    {
                        value = storedValue;
                        return true;
                    }
                    value = null;
                    return false;
                });

            // HttpContext のモック
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new ISessionMockAdapter(mockSession);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "TestUser")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            httpContext.User = new ClaimsPrincipal(identity);

            // PageContext に HttpContext を設定
            var pageContext = new PageContext
            {
                HttpContext = httpContext,
                ActionDescriptor = new CompiledPageActionDescriptor(),
                RouteData = new RouteData(new RouteValueDictionary()),
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new()),
            };
            return pageContext;
        }

        // システムメンテナンスのユニットテストを作成のために、追加
        // 既存コードとの整合性については田中さんに相談済みで、修正してもらう予定
        //protected static PageContext GetPageContext()
        //    => GetPageContext(null, null);

        protected static PageContext GetPageContext(QueryCollection queryCollection)
            => GetPageContext(queryCollection, null);

        protected static PageContext GetPageContext(FormCollection formCollection)
            => GetPageContext(null, formCollection);

        protected static PageContext GetPageContext(QueryCollection? queryCollection = null, FormCollection? formCollection = null)
        {
            var mockSession = new Mock<ISession>();

            // セッションの内部ストレージを模倣
            var sessionStorage = new Dictionary<string, byte[]>();

            mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) =>
                {
                    sessionStorage[key] = value;
                });

            mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny))
                .Returns((string key, out byte[]? value) =>
                {
                    if (sessionStorage.TryGetValue(key, out var storedValue))
                    {
                        value = storedValue;
                        return true;
                    }
                    value = null;
                    return false;
                });

            // HttpContext のモック
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(r => r.Session).Returns(new ISessionMockAdapter(mockSession));

            // HttpRequest のモック
            if (queryCollection is not null || formCollection is not null)
            {
                var httpRequestMock = new Mock<HttpRequest>();

                if (queryCollection is not null)
                {
                    httpRequestMock.Setup(r => r.Query).Returns(queryCollection);
                }

                if (formCollection is not null)
                {
                    httpRequestMock.Setup(r => r.Form).Returns(formCollection);
                }

                httpContextMock.Setup(r => r.Request).Returns(httpRequestMock.Object);
            }


            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, "TestUser")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            httpContextMock.Setup(r => r.User).Returns(new ClaimsPrincipal(identity));

            // PageContext に HttpContext を設定
            var pageContext = new PageContext
            {
                HttpContext = httpContextMock.Object,
                ActionDescriptor = new CompiledPageActionDescriptor(),
                RouteData = new RouteData([]),
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new()),
            };
            return pageContext;
        }

        protected ITempDataDictionary GetTempData()
        {
            var tempDataMock = new Mock<ITempDataDictionary>();
            tempDataMock.SetupSet(t => t["Message"] = It.IsAny<object>());
            return tempDataMock.Object;
        }

        /// <summary>
        /// レスポンスから<see cref="ResponseStatus"/>の値を取得する。
        /// </summary>
        /// <param name="result">モデルが返却したレスポンス</param>
        /// <returns>レスポンス内の<see cref="ResponseStatus"/>の値</returns>
        /// <exception cref="ArgumentException">ResponseStatusの取得に失敗したとき</exception>
        protected ResponseStatus GetResponseStatus(ObjectResult result)
        {
            var val = result.Value ?? throw new ArgumentException("レスポンスのValueがnullです。");
            var propType = val.GetType().GetProperty("Status") ?? throw new ArgumentException("Statusプロパティが存在しません。");
            if (propType.GetValue(val) is ResponseStatus respStat)
            {
                return respStat;
            }
            else
            {
                throw new ArgumentException("StatusプロパティがResponseStatus型ではありません。");
            }
        }

        /// <summary>
        /// レスポンスからメッセージを取得する。
        /// </summary>
        /// <param name="result">モデルが返却したレスポンス</param>
        /// <returns>レスポンス内のメッセージの値</returns>
        /// <exception cref="ArgumentException">ResponseStatusの取得に失敗したとき</exception>
        protected string? GetMessage(ObjectResult result)
        {
            var val = result.Value ?? throw new ArgumentException("Valueが設定されていないObjectResultです。");
            var propType = val.GetType().GetProperty("Message") ?? throw new ArgumentException("Messageプロパティが存在しません。");
            return propType.GetValue(val) as string;
        }

        /// <summary>
        /// レスポンスから<see cref="ResponseStatus"/>の値を取得する。
        /// </summary>
        /// <param name="result">モデルが返却したレスポンス</param>
        /// <returns>レスポンス内の<see cref="ResponseStatus"/>の値</returns>
        /// <exception cref="ArgumentException">ResponseStatusの取得に失敗したとき</exception>
        protected ResponseStatus GetResponseStatus(JsonResult result)
        {
            var val = result.Value ?? throw new ArgumentException("レスポンスのValueがnullです。");
            var propType = val.GetType().GetProperty("Status");
            propType ??= val.GetType().GetProperty("status") ?? throw new ArgumentException("Status (status)プロパティが存在しません。");
            if (propType.GetValue(val) is ResponseStatus respStat)
            {
                return respStat;
            }
            else
            {
                throw new ArgumentException("StatusプロパティがResponseStatus型ではありません。");
            }
        }

        /// <summary>
        /// レスポンスからメッセージを取得する。
        /// </summary>
        /// <param name="result">モデルが返却したレスポンス</param>
        /// <returns>レスポンス内のメッセージの値</returns>
        /// <exception cref="ArgumentException">ResponseStatusの取得に失敗したとき</exception>
        protected string? GetMessage(JsonResult result)
        {
            var val = result.Value ?? throw new ArgumentException("Valueが設定されていないObjectResultです。");
            var propType = val.GetType().GetProperty("Message");
            propType ??= val.GetType().GetProperty("message") ?? throw new ArgumentException("Message (message)プロパティが存在しません。");
            return propType.GetValue(val) as string;
        }

        /// <summary>
        /// レスポンスから指定したキーのエラー配列を取得する。
        /// </summary>
        /// <param name="result">モデルが返却したレスポンス</param>
        /// <param name="key">メッセージが格納されているキー</param>
        /// <returns>レスポンス内のエラー配列</returns>
        /// <exception cref="ArgumentException">エラーメッセージ配列の取得に失敗した時</exception>
        protected string[]? GetErrors(JsonResult result, string key)
        {
            var val = result.Value ?? throw new ArgumentException("Valueが設定されていないJsonResultです。");
            var propType = val.GetType().GetProperty("Errors");
            propType ??= val.GetType().GetProperty("errors") ?? throw new ArgumentException("Errors (errors)プロパティが存在しません。");
            
            var errorsDict = propType.GetValue(val) as Dictionary<string, string[]> ?? throw new ArgumentException("ErrorsプロパティがDictionary<string, string[]>型ではありません。");
            return errorsDict.ContainsKey(key) ? errorsDict[key] : null;
        }

        /// <summary>
        /// 指定したモデルのバリデーションを実行し、結果を取得する。
        /// </summary>
        /// <param name="model">バリデーション対象モデル</param>
        /// <returns>(<see cref="Validator.TryValidateObject"/> の戻り値, エラーリスト)</returns>
        protected static (bool isValid, List<ValidationResult> results) ValidateModel(object model)
        {
            var context = new ValidationContext(model, null, null);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return (isValid, results);
        }

        /// <summary>
        /// <paramref name="result"/> が成功レスポンス (<see cref="SuccessMessage"/>) であることを検証します。
        /// </summary>
        /// <param name="result">検証する <see cref="IActionResult"/></param>
        protected static void AssertSuccess(IActionResult result)
        {
            var objectResult = Assert.IsInstanceOfType<ObjectResult>(result, "ObjectResult が返るべきです。");
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode, "ステータスコードが一致しません。");

            var successMessage = Assert.IsInstanceOfType<SuccessMessage>(objectResult.Value, "SuccessMessage が返るべきです。");
            Assert.IsNull(successMessage.Message, "メッセージが一致しません。");
        }

        /// <summary>
        /// <paramref name="result"/> が成功レスポンス (<see cref="正常"/>) であることを検証します。
        /// </summary>
        /// <param name="result">検証する <see cref="IActionResult"/></param>
        protected static void AssertSuccessJson(IActionResult result)
        {
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result, "JsonResult が返るべきです。");
            var responseJson = Assert.IsInstanceOfType<ResponseJson>(jsonResult.Value, "ResponseJson が返るべきです。");
            Assert.AreEqual(正常, responseJson.Status, "ステータスが一致しません。");
        }

        /// <summary>
        /// <paramref name="result"/> がエラーレスポンス (<see cref="ErrorMessage"/>) かつ
        /// 期待するエラーメッセージであることを検証します。
        /// </summary>
        /// <param name="result">検証する <see cref="IActionResult"/></param>
        /// <param name="expectedMessage">期待するエラーメッセージ</param>
        protected static void AssertError(IActionResult result, string expectedMessage)
        {
            var objectResult = Assert.IsInstanceOfType<ObjectResult>(result, "ObjectResult が返るべきです。");
            Assert.AreEqual(StatusCodes.Status200OK, objectResult.StatusCode, "ステータスコードが一致しません。");

            var errorMessage = Assert.IsInstanceOfType<ErrorMessage>(objectResult.Value, "ErrorMessage が返るべきです。");
            Assert.AreEqual(expectedMessage, errorMessage.Message, "エラーメッセージが一致しません。");
        }

        /// <summary>
        /// <paramref name="result"/> がエラーレスポンス (<see cref="エラー"/>) かつ
        /// 期待するエラーメッセージであることを検証します。
        /// </summary>
        /// <param name="result">検証する <see cref="IActionResult"/></param>
        /// <param name="expectedMessage">期待するエラーメッセージ</param>
        protected static void AssertErrorJson(IActionResult result, string expectedMessage)
        {
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result, "JsonResult が返るべきです。");
            var responseJson = Assert.IsInstanceOfType<ResponseJson>(jsonResult.Value, "ResponseJson が返るべきです。");
            Assert.AreEqual(エラー, responseJson.Status, "ステータスが一致しません。");
            Assert.AreEqual(expectedMessage, responseJson.Message, "エラーメッセージが一致しません。");
        }

        /// <summary>
        /// <paramref name="result"/> が期待するエラーメッセージであることを検証します。
        /// </summary>
        /// <param name="result">検証する <see cref="IActionResult"/></param>
        /// <param name="expectedErrors">期待するエラーメッセージ配列</param>
        protected void AssertErrors(IActionResult result, params string[] expectedErrors)
        {
            var jsonResult = Assert.IsInstanceOfType<JsonResult>(result, "JsonResult が返るべきです。");
            var errors = GetErrors(jsonResult, string.Empty);
            CollectionAssert.AreEqual(expectedErrors, errors, "エラーメッセージが一致しません。");
        }
    }
}
