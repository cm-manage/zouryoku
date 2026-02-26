using Amazon;
using Amazon.S3;
using LanguageExt;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Model.Data;
using Model.Enums;
using Model.Model;
using Newtonsoft.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Zouryoku.Middleware;
using Zouryoku.Services;
using ZouryokuCommonLibrary;
using ZouryokuCommonLibrary.Attributes;

namespace Zouryoku
{
    public class Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        protected IConfiguration Configuration { get; } = configuration;
        protected IWebHostEnvironment Env { get; } = env;

        private bool IsAWSEnvironment()
            => new[] { DevelopmentMode.AWS本番, DevelopmentMode.AWS開発 }
                .Contains(Configuration.GetValue<DevelopmentMode>("AppSettings:WebApplication:DevelopmentMode"));

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Entra ID認証の設定
            string[] initialScopes = Configuration.GetValue<string>("DownstreamApi:Scopes")?.Split(' ') ?? Array.Empty<string>();

            services.AddAuthentication(Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    options.Instance = Configuration["AzureAd:Instance"];
                    options.Domain = Configuration["AzureAd:Domain"];
                    options.CallbackPath = Configuration["AzureAd:CallbackPath"];
                    options.SignedOutCallbackPath = Configuration["AzureAd:SignedOutCallbackPath"];

                    // DB から秘密情報取得
                    using var scope = services.BuildServiceProvider().CreateScope();
                    var dbCtx = scope.ServiceProvider.GetRequiredService<ZouContext>();
                    var cfg = dbCtx.ApplicationConfigs.AsNoTracking().FirstOrDefault()
                        ?? throw new InvalidOperationException("app_config が未登録です。");

                    options.TenantId = cfg.MsTenantId;
                    options.ClientId = cfg.MsClientId;
                    options.ClientSecret = cfg.MsClientSecret;

                    // 認証成功後のイベントをカスタマイズ
                    options.Events.OnTokenValidated = async context =>
                    {
                        // デフォルトの処理を完了させる（認証Cookieを設定）
                        await Task.CompletedTask;
                    };
                    
                    // 認証完了後にカスタムページにリダイレクト
                    options.Events.OnTicketReceived = context =>
                    {
                        // 認証Cookieが設定された後にリダイレクト
                        context.ReturnUri = "/EntraCallback";
                        return Task.CompletedTask;
                    };
                    
                    // 認証失敗時のエラーハンドリング
                    options.Events.OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Startup>>();
                        logger.LogError(context.Exception, "Entra ID認証に失敗しました");
                        
                        context.Response.Redirect("/Error");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    };
                    
                    // リモート認証失敗時のハンドリング
                    options.Events.OnRemoteFailure = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Startup>>();
                        logger.LogError(context.Failure, "Entra IDリモート認証に失敗しました");
                        
                        context.Response.Redirect("/Error");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    };
                })
                // ダウンストリームAPI（Microsoft Graph等）を呼び出すためのトークン取得を有効化
                // initialScopes: 初期同意が必要なスコープ（例: "user.read"）
                .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
                // Microsoft Graph APIクライアントをDIコンテナに登録
                // appsettings.jsonの"DownstreamApi"セクションから設定を読み込み
                // これにより、PageModelやコントローラーでGraphServiceClientを注入して使用可能
                .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
                // トークンをメモリ内にキャッシュ（本番環境では分散キャッシュの使用を推奨）
                // ユーザーごとにアクセストークン・リフレッシュトークンを一時保存し、
                // API呼び出し時のトークン再取得を最小限に抑える
                .AddInMemoryTokenCaches();

            services.AddRazorPages()
                .AddMvcOptions(options =>
                {
                    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
                        fieldName => $"{fieldName}は数値で入力してください。");
                })
                .AddMicrosoftIdentityUI();

            // Add the UI support to handle claims challenges
            services.AddServerSideBlazor()
               .AddMicrosoftIdentityConsentHandler();

            services.AddMvc().AddMvcOptions(options =>
            {
                // stringのnull自動変換をOFF
                options.ModelMetadataDetailsProviders.Add(new CustomMetadataProvider());
            });

            services.AddDateOnlyTimeOnlyStringConverters()
                .AddControllers();

            services.AddDbContextPool<ZouContext>((service, options) =>
            {
                var logger = service.GetRequiredService<ILogger<ZouContext>>();
                var connectionString = Configuration.GetConnectionString("Database") ?? throw new ArgumentNullException("Database is null");

                if (IsAWSEnvironment())
                {
                    // AWS環境では、Parameter Storeからパスワードを取得し、プレースホルダーと置換する
                    var pass = Configuration["rds-senason-password"];
                    connectionString = connectionString.Replace("{PASSWORD}", pass);
                }
                logger.LogDebug("Connect To：{connectionString}", connectionString);

                options.UseNpgsql(connectionString);

                // 開発環境のみパラメータをログに出力する
                if (Env.IsDevelopment())
                { 
                    options
                        .EnableSensitiveDataLogging()
                        .EnableDetailedErrors();
                }
            },
            Configuration.GetValue<int>("MaxDbContextPoolSize"));

            // 設定ファイルをDI　各コントローラから取得可能になる
            services.Configure<Zouryoku.AppConfig>(Configuration);
            services.AddScoped<MenuService>();

            // テスト時に FakeTimeProvider へ差し替え可能にするため DI 登録
            services.AddSingleton(TimeProvider.System);

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // クッキー認証ミドルウェアの設定
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => !(context.User.Identity?.IsAuthenticated ?? false);
                options.MinimumSameSitePolicy = SameSiteMode.None;
                // Releaseでは、SSL時のみクッキーを送信
#if DEBUG
#else
                            options.Secure = CookieSecurePolicy.Always;
#endif
                // httpOnly属性設定（Jsから触れなくする）
                options.HttpOnly = HttpOnlyPolicy.Always;
            });

            //X-CSRF-TOKEN
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
            // 同期操作の許可
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            //Razorの日本語文字化け対策
            services.AddSingleton(HtmlEncoder.Create(UnicodeRanges.All));

            services.AddHttpContextAccessor();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }

            app.Use(async (context, next) =>
            {
                //レスポンスヘッダのミドルウェア
                //https://blog.beachside.dev/entry/2020/06/10/183000

                //X-Content-Type-Options
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                //X-Frame-Options
                context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
                //X-XSS-Protection
                context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
                //Content-Security-Policy: 外部スクリプトをブロックし、自サイトのリソースのみ許可
                //Microsoft Entra ID認証に必要なドメインを許可
                var cspDefaultSrc = "default-src 'self'";
                var cspScriptSrc = "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://*.msauth.net https://*.msftauth.net https://login.microsoftonline.com";
                var cspStyleSrc = "style-src 'self' 'unsafe-inline'";
                var cspImgSrc = "img-src 'self' data: https://*.msftauth.net https://*.msauthimages.net";
                var cspFontSrc = "font-src 'self'";
                var cspFrameSrc = "frame-src 'self' https://*.msauth.net https://*.msftauth.net https://login.microsoftonline.com";
                var cspFrameAncestors = "frame-ancestors 'self'";
                // 開発環境のみ localhost を追加（Visual Studio のホットリロードやBrowser Link用）
                var cspConnectSrc = env.IsDevelopment() || env.IsEnvironment("ITC")
                    ? "connect-src 'self' ws://localhost:* http://localhost:* https://*.msauth.net https://*.msftauth.net https://login.microsoftonline.com"
                    : "connect-src 'self' https://*.msauth.net https://*.msftauth.net https://login.microsoftonline.com";

                context.Response.Headers.Append("Content-Security-Policy",
                    $"{cspDefaultSrc}; {cspScriptSrc}; {cspStyleSrc}; {cspImgSrc}; {cspFontSrc}; {cspConnectSrc}; {cspFrameSrc}; {cspFrameAncestors}");

                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        Private = true,
                        NoCache = true,
                        NoStore = true,
                    };
                context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
                    new string[] { "Accept-Encoding" };

                await next();
                if (context.Response.StatusCode == 404)
                {
                    context.Request.Path = "/page404";
                    await next();
                }
                else if (context.Response.StatusCode == 403)
                {
                    context.Request.Path = "/page403";
                    await next();
                }
            });

            ///http→httpsにリダイレクトする構文
            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.UseMiddleware<SessionTimeoutMiddleware>();
            app.UseMiddleware<SessionRefreshMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });
        }
    }
}