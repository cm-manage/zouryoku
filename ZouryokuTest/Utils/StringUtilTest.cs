using static Zouryoku.Utils.StringUtil;

namespace ZouryokuTest.Utils
{
    [TestClass]
    public class StringUtilTest
    {
        /// <summary>
        /// 半角カナを全角カナに変換する
        /// </summary>
        [TestMethod]
        public void WhenHarfWdthKana_Normalized()
        {
            // Act
            var result = NormalizeString(
                "ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜｦﾝｧｨｩｪｫｯｬｭｮｰ｡｢｣､･ｶﾞｷﾞｸﾞｹﾞｺﾞｻﾞｼﾞｽﾞｾﾞｿﾞﾀﾞﾁﾞﾂﾞﾃﾞﾄﾞﾊﾞﾋﾞﾌﾞﾍﾞﾎﾞﾊﾟﾋﾟﾌﾟﾍﾟﾎﾟ");

            // Assert
            Assert.AreEqual(
                "アイウエオカキクケコサシスセソタチツテトナニヌネノハヒフヘホマミムメモヤユヨラリルレロワヲンァィゥェォッャュョー。「」、・ガギグゲゴザジズゼゾダヂヅデドバビブベボパピプペポ",
                result, "全角カナに変換されないケースが存在します。");
        }

        /// <summary>
        /// 英字を半角大文字に変換する
        /// </summary>
        /// <param name="input">変換する文字列</param>
        [TestMethod]
        [DataRow("abcdefghijklmnopqrstuvwxyz", DisplayName = "半角小文字")]
        [DataRow("ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ", DisplayName = "全角小文字")]
        [DataRow("ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ", DisplayName = "全角大文字")]
        public void WhenEnglish_Normalized(string input)
        {
            // Act
            var result = NormalizeString(input);

            // Assert
            Assert.AreEqual("ABCDEFGHIJKLMNOPQRSTUVWXYZ", result, "半角英字に変換されないケースが存在します。");
        }

        /// <summary>
        /// 数字を半角数字に変換する
        /// </summary>
        [TestMethod]
        public void WhenHalfWidthNumber_Normalized()
        {
            // Act
            var result = NormalizeString("０１２３４５６７８９");

            // Assert
            Assert.AreEqual("0123456789", result, "全角数字に変換されないケースが存在します。");
        }

        /// <summary>
        /// 全角スペースを半角スペースに変換する
        /// </summary>
        [TestMethod]
        public void WhenFullWidthSpace_Normalized()
        {
            // Act
            var result = NormalizeString("　");

            // Assert
            Assert.AreEqual(" ", result, "全角スペースに変換されないケースが存在します。");
        }

        /// <summary>
        /// その他文字を変換しない
        /// </summary>
        [TestMethod]
        public void WhenOthers_NoNormalization()
        {
            // Arrange
            var input = "Aは1番目のアルファベットで、エーと発音します。";

            // Act
            var result = NormalizeString(input);

            // Assert
            Assert.AreEqual(input, result, "変換されるべきでない箇所が変換されています。");
        }

        /// <summary>
        /// NULL引数のときArgumentNullExceptionが発生すること
        /// </summary>
        [TestMethod]
        public void WhenNull_ThrowArgumentNullException()
        {
#pragma warning disable CS8625
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => NormalizeString(null), "NULL引数を許容しています。");
#pragma warning restore CS8625
        }

        [TestMethod]
        public void NormalizeString_混在文字列_適切に変換()
        {
            // Arrange
            var input = "案件ABC123テストｱｲｳ";

            // Act
            var result = NormalizeString(input);

            // Assert
            Assert.AreEqual("案件ABC123テストアイウ", result);
        }

        [TestMethod]
        public void NormalizeString_空文字列_空文字列を返す()
        {
            // Arrange
            var input = "";

            // Act
            var result = NormalizeString(input);

            // Assert
            Assert.AreEqual("", result);
        }
    }
}
