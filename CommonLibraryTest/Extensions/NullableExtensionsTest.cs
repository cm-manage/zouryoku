using CommonLibrary.Extensions;

namespace CommonLibraryTest.Extensions
{
    [TestClass]
    public class NullableExtensionsTest
    {
        [TestMethod]
        public void HasValue()
        {
            // 参照値
            int? no = 100;
            User? user = new(200, "ユーザー");

            // struct => struct
            var nextNo = no.IfHasValue(x => x + 1);
            Assert.AreEqual(101, nextNo);

            // class => class
            var nextUser = user.IfHasValue(x => new User(x.No + 1, x.Name));
            Assert.AreEqual(new User(201, "ユーザー").ToString(), nextUser?.ToString());

            // struct => class
            var newUser = no.IfHasValue(x => new User(x, "新規ユーザー"));
            Assert.AreEqual(new User(100, "新規ユーザー").ToString(), newUser?.ToString());

            // class => struct
            var newNo = user.IfHasValue(x => x.No);
            Assert.AreEqual(200, newNo);
        }

        [TestMethod]
        public void HasNoValue()
        {
            // 参照値
            int? no = null;
            User? user = null;

            // struct => struct
            var nextNo = no.IfHasValue(x => x++);
            Assert.IsNull(nextNo);

            // class => class
            var nextUser = user.IfHasValue(x =>
            {
                x.No += 1;
                return x;
            });
            Assert.IsNull(nextUser);

            // struct => class
            var newUser = no.IfHasValue(x => new User(x, "新規ユーザー"));
            Assert.IsNull(newUser);

            // class => struct
            var newNo = user.IfHasValue(x => x.No);
            Assert.IsNull(newNo);
        }

        public class User
        {
            public int No { get; set; }
            public string Name { get; set; }
            public User(int no, string name)
            {
                No = no;
                Name = name;
            }
            public override string ToString() => $"${No}: {Name}";
        }
    }
}