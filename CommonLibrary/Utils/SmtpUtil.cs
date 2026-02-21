using CommonLibrary.Extensions;
using MailKit.Net.Smtp;
using MimeKit;
using log4net;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CommonLibrary.Utils
{
    public static class SmtpUtil
    {
        public static int RetryCount { get { return _retryCount; } set { _retryCount = value; } }
        private static int _retryCount;

        /// <summary>
        /// メール送信
        /// </summary>
        /// <param name="host">ホストアドレス</param>
        /// <param name="port">ポート</param>
        /// <param name="user">Smtpユーザー</param>
        /// <param name="password">Smtpパスワード</param>
        /// <param name="isDevelopEnviroment">開発環境かどうか</param>
        /// <param name="isText">テキスト形式かHTML形式か</param>
        public static void MailSend(string host, int port, string user, string password, List<MailContent> contents, bool isAuthenticate = true, bool isAllWithInToMailAddress = false)
        {
            //.netCoreで"iso-2022-jp"のエンコードするために必要
            //https://www.sukerou.com/2018/11/net-core-shift-jisiso-2022-jp.html
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ExceptionUtil.Using(() =>
            {
                // SSL証明書がない場合は以下の処理を、別の1度しか処理されない場所で宣言する必要あり
                //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
                //{
                //    return true;
                //};
                try
                {
                    using (var sc = new SmtpClient())
                    {
                        sc.Connect(host, port, false);
                        // パスワード認証を行うためXOAUTH2認証を削除
                        sc.AuthenticationMechanisms.Remove("XOAUTH2");
                        // 本番環境では認証を行っており、開発環境では認証を行っていないため条件分岐
                        if (isAuthenticate)
                        {
                            sc.Authenticate(Encoding.GetEncoding("iso-2022-jp"), user, password);
                        }

                        contents.ForEach(content =>
                        {
                            if (isAllWithInToMailAddress)
                            {
                                var msg = GetMimeMessage(content, user);
                                sc.Send(msg);
                            }
                            else
                            {
                                content.ToMailAddress.ForEach(to =>
                                {
                                    var tmpContent = new MailContent()
                                    {
                                        Subject = content.Subject,
                                        Contents = content.Contents,
                                        FromMailAddress = content.FromMailAddress,
                                        ToMailAddress = new string[] { to },
                                        FilePaths = content.FilePaths,
                                        FileStreams = content.FileStreams,
                                        TextFormat = content.TextFormat,
                                    };
                                    var msg = GetMimeMessage(tmpContent, user);
                                    sc.Send(msg);
                                });
                            }
                        });

                        sc.Disconnect(true);
                    }
                }
                catch (MailKit.Security.AuthenticationException e)
                {
                    // 通信環境が悪く送信前にネットワークが切れてしまうと認証エラーになるため、その場合はリトライ
                    var message = ExceptionUtil.GetMessage(e);
                    LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name).Error(message);

                    if (_retryCount <= 5)
                    {
                        _retryCount++;
                        MailSend(host, port, user, password, contents, isAuthenticate);
                        return;
                    }
                }
            });
        }

        private static MimeMessage GetMimeMessage(MailContent mailContent, string user)
        {
            var fromAd = string.IsNullOrWhiteSpace(mailContent.FromMailAddress) ? user : mailContent.FromMailAddress;
            // メールのオブジェクトを作成する
            var msg = new MimeMessage();
            // メール送信元の名前とメールアドレスを指定する
            msg.From.Add(new MailboxAddress("", user));
            // メール送信先を指定する
            msg.To.AddRange(mailContent.ToMailAddress.Select(address => new MailboxAddress("", address)));
            // メールの件名を設定する
            msg.Subject = mailContent.Subject;
            var mailType = mailContent.TextFormat;
            var body = new TextPart(mailType) { Text = mailContent.Contents };
            // メールの本文を指定する（テキストメッセージ）
            if (mailContent.FilePaths.NotEmpty() || mailContent.FileStreams.NotEmpty())
            {
                var multi = new Multipart { body };

                if (mailContent.FilePaths.NotEmpty())
                {
                    multi.AddRange(mailContent
                        .FilePaths
                        .Where(fileName => File.Exists(fileName))
                        .Select(fileName => new MimePart()
                        {
                            Content = new MimeContent(File.OpenRead(fileName), ContentEncoding.Default),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = Path.GetFileName(fileName)
                        })
                        .ToList());
                }
                if (mailContent.FileStreams.NotEmpty())
                {
                    multi.AddRange(mailContent
                        .FileStreams
                        .Select(x => new MimePart()
                        {
                            Content = new MimeContent(x.FileStream),
                            ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                            ContentTransferEncoding = ContentEncoding.Base64,
                            FileName = Path.GetFileName(x.Name)
                        })
                        .ToList());
                }
                msg.Body = multi;
            }
            else
            {
                msg.Body = body;
            }
            return msg;
        }

        public class MailContent
        {
            public string Subject { get; set; } = "";
            public string Contents { get; set; } = "";
            public string FromMailAddress { get; set; } = "";
            public string[] ToMailAddress { get; set; } = new string[0];
            public string[] FilePaths { get; set; } = new string[0];
            public List<SendFileStream> FileStreams { get; set; } = new();
            public MimeKit.Text.TextFormat TextFormat { get; set; } = MimeKit.Text.TextFormat.Plain;
        }

        public class SendFileStream
        {
            public string Name { get; set; } = "";
            public byte[] File { get; set; } = new byte[0];
            public Stream FileStream
            {
                get => new MemoryStream(File);
                set { }
            }
        }
    }
}
