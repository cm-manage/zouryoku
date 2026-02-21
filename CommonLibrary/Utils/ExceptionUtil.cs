using System;
using log4net;

namespace CommonLibrary.Utils
{
    public class ExceptionUtil
    {
        /// <summary>
        /// Exceptionキャッチ時にMessageBoxを表示するかどうかのフラグ。サービスで稼働中にUIを呼び出すと下記のエラーが発生する。
        /// System.InvalidOperationException: アプリケーションが UserInteractive モードで実行されていないときに、モーダル ダイアログまたはフォームを表示することは有効な操作ではありません。 
        /// </summary>
        public static bool messageBoxOn = true;

        public static string GetMessage(Exception e)
        {
            var info = e.ToString() + ":" + e.Message + ":" + e.StackTrace;
            if (e.InnerException != null)
            {
                info = info + Environment.NewLine + GetMessage(e.InnerException);
            }
            return info;
        }

        public static int Using(Action action)
        {
            try
            {
                action.Invoke();

                return 0;
            }
            //catch (EntityCommandExecutionException e)
            //{
            //    var message = ExceptionUtil.GetMessage(e);
            //    LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType).Error(message);
            //    if (messageBoxOn)
            //    {
            //        if (e.InnerException != null && e.InnerException is SqlException
            //                && ((SqlException)e.InnerException).Number == -2)
            //        {
            //            MessageBox.Show("現在、更新処理をしています。しばらくしてから再度実行してください。", string.Empty, MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        }
            //        else
            //        {
            //            MessageBox.Show(message);
            //        }
            //    }
            //    return 1;
            //}
            catch (Exception e)
            {
                var message = GetMessage(e);
                LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType?.Name).Error(message);
                //if (messageBoxOn) { MessageBox.Show(message); }
                return 1;
            }
        }
    }
}
