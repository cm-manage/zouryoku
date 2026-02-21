using System.Text;

namespace Zouryoku.Utils
{
    public class StringUtil
    {
        /// <summary>
        /// 正規化形式KCで変換し、さらに大文字に変換する
        /// </summary>
        /// <param name="word">変換したい文字列</param>
        /// <returns>変換された文字列</returns>
        /// <exception cref="ArgumentNullException"><paramref name="word"/>が<c>null</c>の場合</exception>
        public static string NormalizeString(string word)
            => word is not null 
                ? word.Normalize(NormalizationForm.FormKC).ToUpper() 
                : throw new ArgumentNullException(nameof(word), "正規化する文字列が null です。");
    }
}
