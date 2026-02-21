using ZouryokuCommonLibrary.Utils;
using Model.Enums;
using System;

namespace ZouryokuCommonLibrary
{
    /// <summary>
    /// appsettings.jsonの情報をバインドするクラス
    /// </summary>
    /// <remarks>
    /// このクラスはZouryokuCommonLibraryで定義される基底クラスです。
    /// 各実行プロジェクト（Zouryoku等）で継承し、プロジェクト固有の設定を追加できます。
    /// </remarks>
    public class AppConfig
    {
        // UnitTestでMoqがoverrideするためにvirtualが必要
        public virtual required AppSettings AppSettings { get; set; }
    }

    /// <summary>
    /// アプリケーション設定の基底クラス
    /// </summary>
    /// <remarks>
    /// このクラスは共通ライブラリ層で定義され、全プロジェクトで共通の設定項目を持ちます。
    /// 実行プロジェクト（Zouryoku等）では、このクラスを継承してプロジェクト固有の設定項目を追加してください。
    /// 例: public class AppSettings : ZouryokuCommonLibrary.AppSettings { ... }
    /// </remarks>
    public class AppSettings
    {
        public required WebApplication WebApplication { get; set; }

        public required MailPath MailPath { get; set; } 
    }
    
    /// <summary>
    /// Webアプリケーション設定
    /// </summary>
    public class WebApplication
    {
        /// <summary>開発モード</summary>
        public DevelopmentMode? DevelopmentMode { get; set; }
    }

    /// <summary>
    /// メール送信設定
    /// </summary>
    public class MailPath
    {
        /// <summary>SMTPホスト</summary>
        public string? Host { get; set; }
        
        /// <summary>SMTPポート</summary>
        public int Port { get; set; }
        
        /// <summary>送信元メールアドレス</summary>
        public string? FromMail { get; set; }
        
        /// <summary>リクエストホスト（メール本文のリンク生成用）</summary>
        public string? RequestHost { get; set; }
    }
}
