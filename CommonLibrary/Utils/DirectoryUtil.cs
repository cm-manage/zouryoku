using System.IO;
using CommonLibrary.Extensions;
using NPOI.Util.ArrayExtensions;

namespace CommonLibrary.Utils
{
    public static class DirectoryUtil
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool BuildDirectories(string path)
        {
            // フォルダ存在チェック なければ作成する
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 指定されたパスとその中身を再帰的にすべて削除する
        /// </summary>
        /// <param name="path"></param>
        public static void Delete(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }
            // ディレクトリ以下のファイルを削除
            var filePaths = Directory.GetFiles(path);
            filePaths.ForEach(filePath =>
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            });

            // ディレクトリ内ディレクトリも再帰的に削除
            var directoryPaths = Directory.GetDirectories(path);
            directoryPaths.ForEach(directoryPath =>
            {
                Delete(directoryPath);
            });
            // 中身が空になったら自分自身も削除
            Directory.Delete(path, false);
        }
    }
}
