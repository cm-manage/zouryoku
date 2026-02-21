# Zouryoku コーディング規約 — v2.2（要点集約・完成版）

最終更新: 2026-01-16  
適用範囲: C# (.NET 10) / Razor Pages / HTML / JavaScript

---

## 0. 優先順位と参照

- 本規約は Zouryoku プロジェクト内の全アプリケーションに適用する。
- .NET/C# 公式コーディング規約・命名規約・Bootstrap は参考とし、  
  **相反する場合は本規約を優先する。**

---

## 1. 基本原則

1. **一貫性**  
   同種の処理には同じパターンを使う。

2. **可読性**  
   第三者が即座に理解できるコードを最優先。

3. **安全性**  
   脆弱性やデータ不整合を招く処理を禁止。

---

## 2. 日付・時刻・範囲

### 2.1 現在時刻

- **`DateTime.Now` を共通関数を含めて常に使用してよい。**

### 2.2 型（DateOnly/TimeOnly/DateTime）

- 現時点では未決。

### 2.3 範囲の正規化

- `From > To` の場合は **必ず自動入れ替え**。
- 対象: 日付／時刻／数値／金額／ID など。
- ログ・通知は不要。

---

## 3. Entity Framework Core（EF）

### 3.1 射影（Select／返却ポリシー）

- **原則 ViewModel / DTO に射影して返却する。**
- **Entity を View にそのまま渡すことは禁止。**
- **匿名型の外部返却は禁止。**

### 3.2 N+1／Include

- ループ内でクエリを発行しない。
- **多段 Include は原則禁止**。必要な場合は `AsSplitQuery` または DTO 射影で対応。

### 3.3 IQueryable

- **複数回 SQL が走る状態は誤り。**  
  再利用する場合は `ToListAsync()` で早期実行。

### 3.4 Tracking / Update

- **Tracked 状態の Entity に対して `Update()` を呼び出すことは禁止。**
- Detached の詳細な扱いは規定しない。

### 3.5 クエリ最適化

- 読み取り専用 → `AsNoTracking`
- 多段 Include → `AsSplitQuery`
- `SaveChanges` のループ呼び出し禁止

---

## 4. Razor / HTML / JavaScript

### 4.1 Razor

- ファイル先頭順は **`@page → @model → @using`**。
- `@model` を明示し、ロジックは PageModel/サービス層へ。
- **部分更新は Partial/ViewComponent を優先**。

### 4.2 DOM / HTML

- **`innerHTML` 全置換は禁止（例外なし）。**
- **C# 内で HTML を直接生成しない。**
- **`Html.Raw` は例外なく禁止。**
- HTML 属性順は固定：  
  **`id → class → name → data-* → aria-* → その他`**

### 4.3 フォーム / CSRF

- `name=""` 空は不可。不要なら付けない。
- POST フォームでは AntiForgeryToken 必須。
- AJAX の CSRF トークンはヘッダーで送信。

### 4.4 アクセシビリティ

- `label` と `input` を関連付ける。
- 画像には `alt`、必要に応じ `aria-*`。

### 4.5 JavaScript

#### ● jQuery 優先ルール（新規）

- **使用可能な場面では jQuery を優先する。**
- DOM 操作・イベント・AJAX は jQuery API を使用すること。
- `fetch` や ESM は jQuery で代替できない場合に限り使用。
- AJAX は HTML Partial の返却を標準とし、JSON は最小用途で使用。

#### その他 JS 規約

- インラインイベント禁止（`onclick="..."` 等）。
- グローバル汚染を避けるため即時関数/IIFE/ESM を利用。

---

## 5. PageModel

### 5.1 構成（推奨順）

using
namespace
クラス
定数
DI（コンストラクタ）
[BindProperty] 入力 DTO
表示用プロパティ
Handler（OnGet/OnPost）
private メソッド

### 5.2 アンチパターン

- `[BindProperty]` の乱用禁止。入力専用 DTO を使う。
- GET に副作用を入れない。
- 非同期内で同期 EF 呼び出し禁止。

### 5.3 必須

- ハンドラは `OnGetAsync` / `OnPostAsync`
- DTO は入力専用として分離

---

## 6. 命名規則（C# / ASP.NET Core）

### 6.1 C#命名

| 要素           | 規則           | 例                       |
| -------------- | -------------- | ------------------------ |
| Namespace      | PascalCase     | `Company.Project.Module` |
| Class / Record | PascalCase     | `UserService`            |
| Interface      | I + PascalCase | `IUserService`           |
| Method         | PascalCase     | `GetUser`                |
| Property       | PascalCase     | `UserName`               |
| private Field  | \_camelCase    | `_userRepo`              |
| const Field    | PascalCase     | `DefaultSize`            |
| Parameter      | camelCase      | `userId`                 |
| Local 変数     | camelCase      | `count`                  |

### 6.2 ASP.NET

- Razor Page: PascalCase（`UserList.cshtml`）
- ルートパラメータ: **kebab-case**（`/user-detail/{user-id}`）

---

## 7. 例外処理・ログ

### 7.1 例外処理

- 非業務例外は可能な限りキャッチしない。
- キャッチする場合は **ログ → 必要に応じて再スロー**。
- 業務エラー（入力・排他）は IActionResult で返す。

### 7.2 ログ

- クラス名・メソッド名・ユーザー ID など**コンテキストを含めてログ出力**。
- 情報漏洩防止のため PII は必要最小限でマスク。

---

## 8. 非同期

- メソッド名は **Async サフィックス必須**。
- `.Result` / `.Wait()` の使用禁止。
- 戻り値は Task / Task\<T\>。

---

## 9. DI（依存性注入）

- **インターフェース経由で DI**。
- **コンストラクタインジェクションを原則**とし、プロパティ注入禁止。

---

## 10. 構造・ファイル配置・整形

- 1 ファイル 1 クラス（DTO/小型型は例外可）。
- クラス名とファイル名は一致。
- using は **System → 外部 → 自社** の順でアルファベット順。
- 行長 120、スペース 4、末尾空白禁止。
- LINQ チェーンは **ドット前改行（固定）**。
- 三項演算子は **1 段まで**。
- 制御構文（if/switch/foreach）は常に `{}` を付ける。

---

## 11. null 許容 / 可読性補助

- `?` は必要十分に使う。
- プロパティに初期値を記載。
- ガード後の `?.` の濫用は避ける。

---

## 12. パフォーマンス / セキュリティ

- LINQ の `ToList()` / `FirstOrDefault()` は必要箇所のみ。
- パラメータ化クエリを徹底。
- 入力値はサニタイズ＋バリデーション必須。
- 外部リソース読み込みは原則禁止（CSP 詳細は別途方針）。

---

## 13. メッセージ / 定数 / その他

- 定数は Const クラスまたは enum で管理。
- コメントは最新状態を維持し、不要コメントは削除。
- TODO は担当者・期限つきで記載。
- 重複ロジックは禁止（共通化・拡張メソッド化）。

---

## 14. Enum

- DB カラム型に合わせる。
- 飛び番号はコメント必須。
- 末尾カンマ必須。
- Flags は `1 << 0` 開始。

---

## 付録 A: PageModel/Controller の肥大化防止

- 1 クラス 500 行を超えたら責務分割を検討する。

---

# End of Document
