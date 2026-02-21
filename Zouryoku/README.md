# Zouryoku プロジェクト - 認証システムガイド

## 概要

Zouryokuプロジェクトは、ASP.NET Core Razor Pagesを使用したWebアプリケーションで、**Entra ID認証（旧Azure AD）**によるシングルサインオンを使用しています。

---
    
## Entra ID認証の処理フロー

### 1. 認証開始フロー

```
[ユーザー] 
    ↓ アクセス
[ログインページ]
    ↓ Microsoftアカウントでサインインをクリック
[Microsoft Identity]
    ↓ リダイレクト
[Entra ID認証画面]
    ↓ 認証情報入力
[認証成功・トークン発行]
    ↓ コールバック
[EntraCallback ページ (/EntraCallback)]
    ↓
[ユーザー情報取得（Graph API）]
    ↓
[データベース検索（syains.email）]
    ↓
[LoginInfo作成・セッション保存]
    ↓
[トップページ表示]
```

---

### 2. 詳細処理フロー

#### 2.1 認証開始（ログインページ）

**ファイル**: `Pages/Shared/_EntraLoginPartial.cshtml`

**処理ステップ**:

1. ユーザーが「Microsoftアカウントでサインイン」ボタンをクリック
2. Microsoft.Identity.Webが提供するサインインエンドポイントに遷移
   ```razor
   <a asp-area="MicrosoftIdentity" 
      asp-controller="Account" 
      asp-action="SignIn" 
      class="btn btn-primary btn-lg">
   ```
3. OpenID ConnectプロトコルでEntra IDへリダイレクト

**設定ファイル**: `appsettings.json`
```json
"AzureAd": {
  "Instance": "https://login.microsoftonline.com/",
  "CallbackPath": "/signin-oidc"
}
```

---

#### 2.2 Entra ID認証

**処理内容**:

1. **認証画面表示**
   - ユーザーがEntra IDの認証情報（メールアドレス・パスワード）を入力
   - 多要素認証（MFA）が有効な場合は追加認証を実施

2. **トークン発行**
   - 認証成功時、Entra IDがIDトークンとアクセストークンを発行
   - トークンに含まれるClaim情報：
     - `oid`: Entra IDユーザーオブジェクトID
     - `preferred_username`: メールアドレス
     - `name`: 表示名

---

#### 2.3 コールバック処理

**ファイル**: `Pages/EntraCallback.cshtml.cs`

**主要処理メソッド**:

```csharp
public async Task<IActionResult> OnGetAsync()
{
    // 1. 認証状態確認
    if (!User.Identity?.IsAuthenticated)
        return RedirectToPage("/Index");

    // 2. LoginInfo作成
    var loginInfo = await EntraAuthHelper.CreateLoginInfoFromEntraAsync(
        User, db, graphServiceClient);

    // 3. セッション保存
    HttpContext.Session.Set(loginInfo);

    // 4. アクセスログ作成
    await LoginUtil.CreateAccessLogAsync(Request, db, loginInfo);

    // 5. トップページリダイレクト
    return RedirectToPage("/Tops/Index");
}
```

**処理ステップ詳細**:

**Step 1: 認証状態確認**
```csharp
if (!User.Identity?.IsAuthenticated)
    return RedirectToPage("/Index");
```
- Entra ID認証が完了しているか確認
- 未認証の場合はログインページにリダイレクト

**Step 2: LoginInfo作成**

**ファイル**: `Utils/EntraAuthHelper.cs`

```csharp
public static async Task<LoginInfo?> CreateLoginInfoFromEntraAsync(
    ClaimsPrincipal principal, 
    ZouContext db,
    GraphServiceClient? graphServiceClient = null)
{
    // a. ClaimからEntra ID情報を取得
    var oid = principal.FindFirst("oid")?.Value;
    var email = principal.FindFirst("preferred_username")?.Value;
    var displayName = principal.FindFirst("name")?.Value;

    // b. Graph APIでユーザー詳細情報取得（オプション）
    if (graphServiceClient != null) {
        var user = await graphServiceClient.Me.GetAsync();
        displayName = user.DisplayName ?? displayName;
        email = user.Mail ?? user.UserPrincipalName ?? email;
    }

    // c. メールアドレスでSyain検索
    var syain = await db.Syains
        .Include(s => s.Syainbase)
        .Include(s => s.Busyo)
        .FirstOrDefaultAsync(s => s.Email == email);

    if (syain == null) {
        return null; // ユーザーが見つからない
    }

    // d. LoginInfo作成
    return new LoginInfo {
        User = syain,
        EntraUserId = oid,
        EntraDisplayName = displayName,
        EntraEmail = email,
        AuthenticationMethod = "Entra"
    };
}
```

**重要ポイント**:
- **Entra IDのメールアドレスと`syains.email`を照合してユーザーを特定**
- 該当するSyainが存在しない場合はnullを返却し、エラーメッセージを表示

**Step 3: セッション保存**
```csharp
HttpContext.Session.Set(loginInfo);
```
- セッション拡張メソッドにより、JSON形式でシリアライズして保存

**Step 4: アクセスログ作成**
```csharp
await LoginUtil.CreateAccessLogAsync(Request, db, loginInfo);
```
- ブラウザ情報や端末情報も記録

**Step 5: リダイレクト**
```csharp
return RedirectToPage("/Tops/Index");
```
- 認証成功後、トップページにリダイレクト

---

#### 2.4 認証後の画面アクセス

**ファイル**: `Pages/Shared/BasePageModel.cs`

```csharp
public LoginInfo LoginInfo => HttpContext.Session.LoginInfo();
```

- `BasePageModel`を継承したPageModelでは、`LoginInfo`プロパティでログインユーザー情報にアクセス可能
- セッションタイムアウト時は、`SessionTimeoutMiddleware`により自動的にログインページにリダイレクト

---

### 3. ログアウトフロー

```
[ユーザー]
    ↓ ログアウトクリック
[SignOutページ (/SignOut)]
    ↓
[セッションからLoginInfo取得]
    ↓
[セッションクリア]
    ↓
[Entra IDサインアウト]
[Cookieサインアウト]
    ↓
[ログインページにリダイレクト]
```

**ファイル**: `Pages/SignOut.cshtml.cs`

**主要処理メソッド**:

```csharp
public async Task<IActionResult> OnGetAsync()
{
    // 1. セッションからLoginInfo取得
    var loginInfoOption = HttpContext.Session.Get<LoginInfo>();

    // 2. セッションクリア
    HttpContext.Session.Clear();

    // 3. 認証方式別サインアウト
    if (EntraAuthHelper.IsEntraAuthentication(User)) {
        // Entra IDサインアウト
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    } else {
        // Cookie認証サインアウト
        await HttpContext.SignOutAsync("Cookies");
    }

    // 4. リダイレクト
    return RedirectToPage("/Index");
}
```

---

### 4. 主要コンポーネント

#### 4.1 設定ファイル（Startup.cs）

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Entra ID認証設定
    string[] initialScopes = Configuration
        .GetValue<string>("DownstreamApi:Scopes")?.Split(' ') 
        ?? Array.Empty<string>();

    services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
        .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
        .AddInMemoryTokenCaches();
}
```

**重要ポイント**:
- `AddMicrosoftIdentityWebApp`: OpenID Connect認証を設定
- `EnableTokenAcquisitionToCallDownstreamApi`: トークン取得を有効化
- `AddMicrosoftGraph`: Microsoft Graph API呼び出しを有効化
- `AddInMemoryTokenCaches`: トークンをメモリにキャッシュ

---

#### 4.2 データモデル（LoginInfo）

**ファイル**: `Data/LoginInfo.cs`

```csharp
public class LoginInfo
{
    /// <summary>ログインユーザー情報</summary>
    public required Syain User { get; set; }

    /// <summary>Entra ID ユーザーID</summary>
    public string? EntraUserId { get; set; }

    /// <summary>Entra ID ユーザー表示名</summary>
    public string? EntraDisplayName { get; set; }

    /// <summary>Entra ID メールアドレス</summary>
    public string? EntraEmail { get; set; }

    /// <summary>認証方法（Entra or Cookie）</summary>
    public string AuthenticationMethod { get; set; } = "Cookie";
}
```

---

#### 4.3 ヘルパークラス（EntraAuthHelper）

**ファイル**: `Utils/EntraAuthHelper.cs`

**主要メソッド**:

1. **CreateLoginInfoFromEntraAsync**
   - Entra IDのClaimsPrincipalからLoginInfoを作成
   - Graph APIでユーザー詳細情報を取得（オプション）
   - メールアドレスでSyainを検索

2. **IsEntraAuthentication**
   - ClaimsPrincipalがEntra ID認証かどうかを判定
   - `oid` Claimの存在で判定

---

## セットアップ手順

### 1. Azure Portal での設定

1. [Azure Portal](https://portal.azure.com/) にアクセス
2. 「Microsoft Entra ID」に移動
3. 「アプリの登録」→「新規登録」を選択
4. アプリケーション名を入力（例: Zouryoku）
5. リダイレクトURIを設定:
   - 種類: Web
   - 開発環境: `https://localhost:7158/signin-oidc`
   - 本番環境: `https://your-domain.com/signin-oidc`
6. 「証明書とシークレット」からクライアントシークレットを作成
7. 「APIのアクセス許可」で `User.Read` を追加

### 2. appsettings.json の設定

**ファイル**: `appsettings.Development.json`

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "your-domain.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc",
    "ClientCapabilities": [ "cp1" ]
  },
  "DownstreamApi": {
    "BaseUrl": "https://graph.microsoft.com/v1.0",
    "Scopes": "user.read"
  }
}
```

**取得方法**:
- `TenantId`: Azure Portal > Microsoft Entra ID > 概要 > テナントID
- `ClientId`: アプリの登録 > アプリケーション（クライアント）ID
- `ClientSecret`: アプリの登録 > 証明書とシークレット > 新しいクライアントシークレット

### 3. データベース設定

Entra ID認証を使用するには、`syains`テーブルの`email`フィールドに、Entra IDのメールアドレスを設定してください。

```sql
-- メールアドレスを設定
UPDATE syains
SET email = 'user@your-domain.com'
WHERE code = '社員番号';

-- 設定確認
SELECT code, name, email FROM syains WHERE email IS NOT NULL;
```

### 4. ログインページへの統合

**ファイル**: `Pages/Logins/Index.cshtml`

ログインフォームの後に以下を追加:

```razor
<partial name="_EntraLoginPartial" />
```

---

## トラブルシューティング

### よくあるエラー

#### エラー: "ユーザー情報の取得に失敗しました"

**原因**: 
- Entra IDのメールアドレスと`syains.email`が一致しない
- `syains.email`が未設定

**対処方法**:
```sql
-- メールアドレスを確認
SELECT code, name, email FROM syains WHERE code = '社員番号';

-- メールアドレスを更新
UPDATE syains
SET email = 'user@your-domain.com'
WHERE code = '社員番号';
```

---

#### エラー: "AADSTS50011: リダイレクトURIが一致しません"

**原因**: Azure Portal のリダイレクトURI設定が不正

**対処方法**:
1. Azure Portal > アプリの登録 > 認証
2. リダイレクトURIを確認・追加:
   - 開発: `https://localhost:7158/signin-oidc`
   - 本番: `https://your-domain.com/signin-oidc`

---

#### エラー: "AADSTS7000215: 無効なクライアントシークレットが指定されました"

**原因**: ClientSecretの有効期限切れまたは誤り

**対処方法**:
1. Azure Portal > アプリの登録 > 証明書とシークレット
2. 新しいクライアントシークレットを作成
3. `appsettings.json`のClientSecretを更新

---

### デバッグ方法

1. **ログ確認**
   ```csharp
   logger.LogInformation("Entra認証成功: UserId={UserId}, Email={Email}",
       loginInfo.EntraUserId, loginInfo.EntraEmail);
   ```

2. **Claim情報確認**
   ```csharp
   foreach (var claim in User.Claims)
   {
       logger.LogInformation($"{claim.Type}: {claim.Value}");
   }
   ```

3. **Graph API レスポンス確認**
   ```csharp
   var user = await graphServiceClient.Me.GetAsync();
   logger.LogInformation($"DisplayName: {user.DisplayName}, Mail: {user.Mail}");
   ```

---

## セキュリティ考慮事項

### 1. ClientSecretの管理

- **開発環境**: User Secretsを使用
  ```bash
  dotnet user-secrets set "AzureAd:ClientSecret" "your-secret"
  ```

- **本番環境**: Azure Key VaultまたはAWS Systems Manager Parameter Storeを使用

### 2. トークンの保護

- トークンはメモリキャッシュに保存（`AddInMemoryTokenCaches`）
- HTTPSの使用を強制
- Cookie属性: `HttpOnly`, `Secure`, `SameSite`

### 3. セッション管理

- セッションタイムアウト: 1時間（`Startup.cs`で設定）
- `SessionTimeoutMiddleware`で自動的にログインページにリダイレクト

### 4. CSRF対策

- Razor Pagesでは自動的にアンチフォージェリトークンを検証
- AJAX通信時は`XSRF-TOKEN`ヘッダーを使用

---

## ファイル構成

```
Zouryoku/
├── Pages/
│   ├── Logins/
│   │   ├── Index.cshtml                  # ログインページ（Cookie認証フォーム + Entra IDボタン）
│   │   └── Index.cshtml.cs               # ログインページモデル
│   ├── EntraCallback.cshtml              # Entra ID認証コールバックページ
│   ├── EntraCallback.cshtml.cs           # コールバックページモデル
│   ├── SignOut.cshtml                    # ログアウトページ
│   ├── SignOut.cshtml.cs                 # ログアウトページモデル
│   └── Shared/
│       ├── _Layout_NoSession.cshtml      # 未ログイン状態用レイアウト
│       └── _EntraLoginPartial.cshtml     # Entra IDログイン部分ビュー
├── Utils/
│   └── EntraAuthHelper.cs                # Entra ID認証ヘルパー
├── Data/
│   └── LoginInfo.cs                      # ログイン情報モデル（拡張済み）
├── Startup.cs                             # 認証設定（更新済み）
└── appsettings.json                       # Entra ID設定
```

---

## 関連ドキュメント

- [Microsoft Identity Web ドキュメント](https://learn.microsoft.com/ja-jp/azure/active-directory/develop/microsoft-identity-web)
- [Microsoft Graph API リファレンス](https://learn.microsoft.com/ja-jp/graph/api/overview)
- [OpenID Connect プロトコル](https://learn.microsoft.com/ja-jp/azure/active-directory/develop/v2-protocols-oidc)
- [ASP.NET Core 認証](https://learn.microsoft.com/ja-jp/aspnet/core/security/authentication/)

