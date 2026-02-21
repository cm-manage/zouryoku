# Model プロジェクト開発者向け README

本ドキュメントは Zouryoku ソリューション内の `Model` プロジェクトの構造・生成方針・拡張手順をまとめた開発者向けガイドです。すべて UTF-8 (BOMなし) で保存します。

## 概要

`Model` プロジェクトはデータアクセス層で利用する:
- Entity Framework Core の DbContext (`ZouContext`)
- データベーステーブルに対応するエンティティクラス
- 列挙型 (Enums)
- 自動生成コードを拡張するための `Partial` クラス群

PostgreSQL を前提に設計されており、リレーション/制約/コメントは DB メタ情報から取得しています。

## コード生成: EF Core Power Tools

エンティティと `ZouContext` は **EF Core Power Tools** の Reverse Engineering 機能で生成されています。自動生成ファイルには次のヘッダーが付与されます:

### 再生成手順 (開発環境)

1. Visual Studio 拡張機能「EF Core Power Tools」を起動  
2. 右クリック → EF Core Power Tools → Reverse Engineer  
3. 接続文字列を選択  
4. 対象テーブルを選択 (必要に応じて View, Stored Procedure も)  
5. 推奨オプション  
   - Use DataAnnotations: ON  
   - Use pluralizer: OFF  
   - Include table & column comments: ON  
   - Install EF Core provider: 必要に応じ  
6. 出力先  
   - DbContext: `Model/Data`  
   - Entities: `Model/Model`  
7. 生成後に差分レビュー  

### 自動生成コードに対する運用ルール

| 項目 | ルール |
|------|--------|
| 直接編集 | 原則禁止 (再生成で失われる) |
| ビジネスロジック | `Model/Partial` の部分クラスで拡張 |
| 追加列/変更 | 先に DB スキーマ更新 → 再生成 |
| ナビゲーション修正 | Fluent API を `OnModelCreatingPartial` に記述 |
| バージョン管理 | 大きな差分は PR 説明に再生成日時を記載 |

## プロジェクト構成

(省略: 実際のフォルダ構成はソース参照)

## DbContext: `ZouContext`

- エンティティは `DbSet<T>` で公開
- コメントは `HasComment` で反映
- 追加設定は `partial void OnModelCreatingPartial(...)` を利用

### OnModelCreatingPartial 例 (拡張)

```csharp
// 例: Product エンティティのモデル生成を拡張
partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Product>(entity =>
    {
        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100)
            .HasComment("商品名");
        
        entity.HasIndex(e => e.CategoryId)
            .HasName("IX_Products_CategoryId");
    });
}
```

この形なら再生成時に競合せず利用可能。

## 列挙型運用

`Enums` 配下に列挙型を定義し `[Display]` 属性で表示名を付与。UI では `Html.GetEnumSelectList<T>()` を利用。

### ★ 列挙体カラムの生成方針（重要）

EF Core Power Tools の自動生成では「列挙体として扱いたい数値カラム」は **自動生成対象から除外し**、部分クラス (`Partial`) 側で列挙型プロパティとして再定義します。  
これにより:
- 逆生成による上書き時に型が int/short 等へ戻る事故を防止
- 列挙値管理を C# 側で一元化
- 列挙追加時の差分が最小化

#### 実装例

DB: `pc_logs.operation` (smallint / int 想定) を `PcOperationType` 列挙型で扱う。

1. 逆生成時に `operation` 列は除外 (Power Tools の「Advanced > Object filters」等で Exclude)
2. 自動生成された `PcLog` (Model/Model/PcLog.cs) は **編集しない**
3. `Model/Partial/Partial.cs` に列挙型プロパティを追加:

```csharp
public partial class PcLog
{
    // 列挙型プロパティの追加
    public PcOperationType OperationType
    {
        get => (PcOperationType)operation;
        set => operation = (short)value;
    }

    // 列挙型の明示的な定義
    [NotMapped]
    public enum PcOperationType : short
    {
        [Display(Name = "不明")]
        Unknown = 0,
        
        [Display(Name = "ログイン")]
        Login = 1,
        
        [Display(Name = "ログアウト")]
        Logout = 2,
        
        // 必要に応じて追加
    }
}
```

4. 列挙定義:

```csharp
// 例: Db/Model/PcLog.cs を基にしたカスタム列挙型定義
public enum PcOperationType // 列挙型名はクラス名と合わせる
{
    [Display(Name = "不明")]
    Unknown = 0,
    
    [Display(Name = "ログイン")]
    Login = 1,
    
    [Display(Name = "ログアウト")]
    Logout = 2,
    
    // 必要に応じて追加
}

````````

## よくある質問 (FAQ)

| 質問 | 回答 |
|------|------|
| 自動生成に enum カラムを残しても良い? | 非推奨。再生成リスク増 |
| enum 名称変更時の注意は? | 値(数値)を変えず名前のみ変更 |
| 値追加は? | DB 既存データへの影響を調査後に実施 |
| 列挙型を文字列マップにしたい | DB 型を text/varchar にし `ValueConverter` 導入を検討 |

## 注意事項

- 物理削除より論理削除を優先 (必要なら設計段階で列追加)
- 競合制御が必要な場合は xmin / RowVersion の採用を検討
- `#nullable disable` 禁止 (nullable 解析を維持)

## 参考

- EF Core Power Tools: https://github.com/ErikEJ/EFCorePowerTools
- EF Core Docs: https://learn.microsoft.com/ja-jp/ef/core/
- PostgreSQL Provider: https://www.npgsql.org/efcore/

---

最終更新日: 2025-10-19
