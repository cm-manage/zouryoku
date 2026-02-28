using ZouryokuCommonLibrary.Utils;
namespace ZouryokuCommonLibraryTest;

[TestClass]
public class TimeTests
{
    [TestMethod]
    public void ToHHmmWithColon_Test()
    {
        // 0分 -> "00:00"
        Assert.AreEqual("00:00", Time.ToHHmmWithColon(0));

        // 1分 -> "00:01"
        Assert.AreEqual("00:01", Time.ToHHmmWithColon(1));

        // 10分 -> "00:10"
        Assert.AreEqual("00:10", Time.ToHHmmWithColon(10));

        // 60分 -> "01:00"
        Assert.AreEqual("01:00", Time.ToHHmmWithColon(60));

        // 65分 -> "01:05"
        Assert.AreEqual("01:05", Time.ToHHmmWithColon(65));

        // 125分 -> "02:05"
        Assert.AreEqual("02:05", Time.ToHHmmWithColon(125));

        // 1445分 -> "24:05"
        Assert.AreEqual("24:05", Time.ToHHmmWithColon(1445));
    }
}
