using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using Zouryoku.Pages.Maintenance.PhotoList;

namespace ZouryokuTest.Pages.Maintenance.PhotoList
{
    /// <summary>
    /// DetailsModel (顔写真詳細ページ) のユニットテスト
    /// </summary>
    [TestClass]
    public class DetailsModelTests : BaseInMemoryDbContextTest
    {
        private DetailsModel CreateModel()
        {
            var model = new DetailsModel(db, GetLogger<DetailsModel>(), options);
            model.PageContext = GetPageContext();
            model.TempData = GetTempData();
            return model;
        }

        /// <summary>
        /// 正常: 対象の社員Baseレコードが存在する場合 PageResult が返り Syain が設定されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Returns_Page_When_SyainBase_Exists()
        {
            // Arrange
            var busyo = new Busyo
            {
                Name = "テスト部署",
                IsActive = true,
                StartYmd = DateOnly.FromDateTime(System.DateTime.Today.AddYears(-1)),
                EndYmd = DateOnly.FromDateTime(System.DateTime.Today.AddYears(1)),
                // ↓ 必須プロパティを適当なダミー値で埋める
                Code = "001",
                KaikeiCode = "001",
                KanaName = "テストブショ",
                KasyoCode = "001",
                OyaCode = "000"
            };
            db.Busyos.Add(busyo);

            var syainBase = new SyainBasis
            {
                Name = "テスト太郎",
                Code = "T001"
            };
            db.SyainBases.Add(syainBase);

            var syain = new Syain
            {
                SyainBase = syainBase,
                Code = "T001",
                Name = "テスト太郎",
                KanaName = "テストタロウ",
                Busyo = busyo,
                NyuusyaYmd = DateOnly.FromDateTime(System.DateTime.Today.AddYears(-5)),
                EMail = "t001@test.com",
                PhoneNumber = "0000000000",
                Retired = false,
                BusyoCode = busyo.Code,
                KingsSyozoku = "1"
            };
            db.Syains.Add(syain);

            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync(syainBase.Id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsNotNull(model.Syain);
            Assert.AreEqual(syain.Code, model.Syain.SyainNumber);
            Assert.AreEqual(syain.Name, model.Syain.Name);
            Assert.AreEqual(syain.KanaName, model.Syain.NameKana);
            Assert.AreEqual(busyo.Name, model.Syain.BusyoName);
            Assert.AreEqual(syain.NyuusyaYmd, model.Syain.NyusyaYmd);
            Assert.AreEqual(syain.EMail, model.Syain.MailAddress);
            Assert.AreEqual(syain.PhoneNumber, model.Syain.TelNumber);
        }

        /// <summary>
        /// 異常: id が null の場合 NotFoundResult が返ること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Returns_NotFound_When_Id_Is_Null()
        {
            // Arrange
            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync(null);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult), "ID 未指定時は NotFoundResult が返るべきです。");
        }

        /// <summary>
        /// 異常: 対象の社員Baseレコードが存在しない場合 NotFoundResult が返ること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Returns_NotFound_When_SyainBase_Not_Found()
        {
            // Arrange
            var model = CreateModel();
            var notExistsId = 9999L;

            // Act
            var result = await model.OnGetAsync(notExistsId);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult), "存在しないID指定時は NotFoundResult が返るべきです。");
            Assert.IsNull(model.Syain, "レコード未取得時に Syain が設定されてはなりません。");
        }

        /// <summary>
        /// 異常: 退職済み社員の場合 NotFoundResult が返ること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Returns_NotFound_When_Syain_Is_Retired()
        {
            // Arrange
            var busyo = new Busyo
            {
                Name = "退職部署",
                IsActive = true,
                StartYmd = DateOnly.FromDateTime(System.DateTime.Today.AddYears(-1)),
                EndYmd = DateOnly.FromDateTime(System.DateTime.Today.AddYears(1)),
                Code = "002",
                KaikeiCode = "002",
                KanaName = "タイショクブショ",
                KasyoCode = "002",
                OyaCode = "000"
            };
            db.Busyos.Add(busyo);

            var syainBase = new SyainBasis
            {
                Name = "退職太郎",
                Code = "R001"
            };
            db.SyainBases.Add(syainBase);

            var retiredSyain = new Syain
            {
                SyainBase = syainBase,
                Code = "R001",
                Name = "退職太郎",
                KanaName = "タイショクタロウ",
                Busyo = busyo,
                NyuusyaYmd = DateOnly.FromDateTime(System.DateTime.Today.AddYears(-10)),
                EMail = "r001@test.com",
                PhoneNumber = "0000000001",
                Retired = true,
                BusyoCode = busyo.Code,
                KingsSyozoku = "1"
            };
            db.Syains.Add(retiredSyain);

            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync(syainBase.Id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundResult), "退職済み社員指定時は NotFoundResult が返るべきです。");
            Assert.IsNull(model.Syain, "退職済み社員の場合に Syain が設定されてはなりません。");
        }

        /// <summary>
        /// 正常: 有効な写真データが存在する場合 PhotoBase64 が設定されること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_Sets_PhotoBase64_When_Valid_Photo_Exists()
        {
            // Arrange
            var busyo = new Busyo
            {
                Name = "写真部署",
                IsActive = true,
                StartYmd = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
                EndYmd = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
                Code = "010",
                KaikeiCode = "010",
                KanaName = "シャシンブショ",
                KasyoCode = "010",
                OyaCode = "000"
            };
            db.Busyos.Add(busyo);

            var syainBase = new SyainBasis
            {
                Name = "写真太郎",
                Code = "P001"
            };
            db.SyainBases.Add(syainBase);

            var syain = new Syain
            {
                SyainBase = syainBase,
                Code = "P001",
                Name = "写真太郎",
                KanaName = "シャシンタロウ",
                Busyo = busyo,
                NyuusyaYmd = DateOnly.FromDateTime(DateTime.Today.AddYears(-3)),
                EMail = "p001@test.com",
                PhoneNumber = "0000000010",
                Retired = false,
                BusyoCode = busyo.Code,
                KingsSyozoku = "1"
            };
            db.Syains.Add(syain);

            var photoBytes = new byte[] { 1, 2, 3, 4 };

            var photoData = new PhotoAfterProcessTnData
            {
                Photo = photoBytes
            };

            var syainPhoto = new SyainPhoto
            {
                SyainBase = syainBase,
                Selected = true,
                Deleted = false,
                PhotoAfterProcessTnData = new List<PhotoAfterProcessTnData> { photoData },
                PhotoName = "valid-photo.jpg"
            };

            syainBase.SyainPhotos = new List<SyainPhoto> { syainPhoto };

            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync(syainBase.Id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsNotNull(model.Syain);
            var expectedBase64 = Convert.ToBase64String(photoBytes);
            Assert.AreEqual(expectedBase64, model.Syain.PhotoBase64, "有効な写真データが存在する場合、PhotoBase64 に Base64 文字列が設定されるべきです。");
        }

        /// <summary>
        /// 異常: SyainBase に写真が紐付いていない場合 PhotoBase64 が空文字列であること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_PhotoBase64_Empty_When_No_Photos_Linked()
        {
            // Arrange
            var busyo = new Busyo
            {
                Name = "写真無し部署",
                IsActive = true,
                StartYmd = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
                EndYmd = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
                Code = "011",
                KaikeiCode = "011",
                KanaName = "シャシンナシブショ",
                KasyoCode = "011",
                OyaCode = "000"
            };
            db.Busyos.Add(busyo);

            var syainBase = new SyainBasis
            {
                Name = "写真無し太郎",
                Code = "NP001",
                SyainPhotos = new List<SyainPhoto>()
            };
            db.SyainBases.Add(syainBase);

            var syain = new Syain
            {
                SyainBase = syainBase,
                Code = "NP001",
                Name = "写真無し太郎",
                KanaName = "シャシンナシタロウ",
                Busyo = busyo,
                NyuusyaYmd = DateOnly.FromDateTime(DateTime.Today.AddYears(-2)),
                EMail = "np001@test.com",
                PhoneNumber = "0000000011",
                Retired = false,
                BusyoCode = busyo.Code,
                KingsSyozoku = "1"
            };
            db.Syains.Add(syain);

            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync(syainBase.Id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsNotNull(model.Syain);
            Assert.AreEqual(string.Empty, model.Syain.PhotoBase64, "写真が紐付いていない場合、PhotoBase64 は空文字列であるべきです。");
        }

        /// <summary>
        /// 異常: 写真が Selected=false または Deleted=true の場合 PhotoBase64 が空文字列であること
        /// </summary>
        [TestMethod]
        public async Task OnGetAsync_PhotoBase64_Empty_When_Photo_Not_Selected_Or_Deleted()
        {
            // Arrange
            var busyo = new Busyo
            {
                Name = "無効写真部署",
                IsActive = true,
                StartYmd = DateOnly.FromDateTime(DateTime.Today.AddYears(-1)),
                EndYmd = DateOnly.FromDateTime(DateTime.Today.AddYears(1)),
                Code = "012",
                KaikeiCode = "012",
                KanaName = "ムコウシャシンブショ",
                KasyoCode = "012",
                OyaCode = "000"
            };
            db.Busyos.Add(busyo);

            var syainBase = new SyainBasis
            {
                Name = "無効写真太郎",
                Code = "IP001"
            };
            db.SyainBases.Add(syainBase);

            var syain = new Syain
            {
                SyainBase = syainBase,
                Code = "IP001",
                Name = "無効写真太郎",
                KanaName = "ムコウシャシンタロウ",
                Busyo = busyo,
                NyuusyaYmd = DateOnly.FromDateTime(DateTime.Today.AddYears(-4)),
                EMail = "ip001@test.com",
                PhoneNumber = "0000000012",
                Retired = false,
                BusyoCode = busyo.Code,
                KingsSyozoku = "1"
            };
            db.Syains.Add(syain);

            var invalidPhotoBytes = new byte[] { 9, 9, 9 };

            var invalidPhotoData = new PhotoAfterProcessTnData
            {
                Photo = invalidPhotoBytes
            };

            var notSelectedPhoto = new SyainPhoto
            {
                SyainBase = syainBase,
                Selected = false,
                Deleted = false,
                PhotoAfterProcessTnData = new List<PhotoAfterProcessTnData> { invalidPhotoData },
                PhotoName = "valid-photo.jpg"
            };

            var deletedPhoto = new SyainPhoto
            {
                SyainBase = syainBase,
                Selected = true,
                Deleted = true,
                PhotoAfterProcessTnData = new List<PhotoAfterProcessTnData> { invalidPhotoData },
                PhotoName = "valid-photo.jpg"
            };

            syainBase.SyainPhotos = new List<SyainPhoto> { notSelectedPhoto, deletedPhoto };

            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync(syainBase.Id);

            // Assert
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsNotNull(model.Syain);
            Assert.AreEqual(string.Empty, model.Syain.PhotoBase64, "Selected=false または Deleted=true の写真しかない場合、PhotoBase64 は空文字列であるべきです。");
        }
    }
}
