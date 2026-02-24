using CommonLibrary.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Model.Model;
using SixLabors.ImageSharp;
using Zouryoku.Data;
using Zouryoku.Extensions;
using Zouryoku.Pages.Maintenance.Syains;

namespace ZouryokuTest.Pages.Maintenance.Syains
{
    /// <summary>
    /// 顔写真入力（新規登録）ページモデルのユニットテスト
    /// </summary>
    [TestClass]
    public class InputModelTests : BaseInMemoryDbContextTest
    {
        private const long SyainBaseId = 1;

        private InputModel CreateModel()
        {
            // Arrange
            var model = new InputModel(db, GetLogger<InputModel>(), options, fakeTimeProvider);
            model.PageContext = GetPageContext();
            model.TempData = GetTempData();

            model.PageContext.HttpContext.Session.Set(new LoginInfo
            {
                User = new Syain
                {
                    Id = 1,
                    SyainBaseId = SyainBaseId
                }
            });

            return model;
        }

        /// <summary>
        /// 画面表示_新規_正常
        /// </summary>
        [TestMethod]
        public async Task 画面表示_新規_正常()
        {
            // Arrange
            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsFalse(model.SyainPhoto.IsUpdate);
            Assert.AreEqual(0, model.SyainPhoto.Id);
        }

        /// <summary>
        /// 画面表示_編集_正常
        /// </summary>
        [TestMethod]
        public async Task 画面表示_編集_正常()
        {
            // Arrange
            var syainPhoto = new SyainPhoto
            {
                SyainBaseId = SyainBaseId,
                Seq = 1,
                Selected = true,
                Deleted = false,
                UploadTime = fakeTimeProvider.Now(),
                PhotoName = "Photo1"
            };
            db.SyainPhotos.Add(syainPhoto);
            db.PhotoDatas.Add(new PhotoData
            {
                SyainPhoto = syainPhoto,
                Photo = [1, 2, 3]
            });
            await db.SaveChangesAsync();

            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync(syainPhoto.Id);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(PageResult));
            Assert.IsTrue(model.SyainPhoto.IsUpdate);
            Assert.AreEqual(syainPhoto.Id, model.SyainPhoto.Id);
            Assert.AreEqual("Photo1", model.SyainPhoto.PhotoName);
            Assert.IsFalse(string.IsNullOrEmpty(model.SyainPhoto.CurrentPhotoBase64));
        }

        /// <summary>
        /// 画面表示_編集_写真が見つからない
        /// </summary>
        [TestMethod]
        public async Task 画面表示_編集_写真が見つからない()
        {
            // Arrange
            var model = CreateModel();

            // Act
            var result = await model.OnGetAsync(999);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        /// <summary>
        /// 登録_バリデーション_新規でファイル未指定
        /// </summary>
        [TestMethod]
        public async Task 登録_バリデーション_新規でファイル未指定()
        {
            // Arrange
            var model = CreateModel();
            model.SyainPhoto = new InputModel.SyainPhotoModel
            {
                Id = 0,
                IsUpdate = false,
                CropX = 0,
                CropY = 0,
                CropWidth = 1,
                CropHeight = 1
            };
            var beforeSyainPhotos = await db.SyainPhotos.CountAsync();

            // Act
            var result = await model.OnPostRegisterAsync(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            var afterSyainPhotos = await db.SyainPhotos.CountAsync();
            Assert.AreEqual(beforeSyainPhotos, afterSyainPhotos);
        }

        /// <summary>
        /// 登録_正常_新規
        /// </summary>
        [TestMethod]
        public async Task 登録_正常_新規()
        {
            // Arrange
            var model = CreateModel();
            model.SyainPhoto = new InputModel.SyainPhotoModel
            {
                Id = 0,
                IsUpdate = false,
                CropX = 0,
                CropY = 0,
                CropWidth = 1,
                CropHeight = 1
            };
            var beforeSyainPhotos = await db.SyainPhotos.CountAsync();
            var beforePhotoDatas = await db.PhotoDatas.CountAsync();
            var beforeAfterProcess = await db.PhotoAfterProcessTnDatas.CountAsync();

            using (var stream = new MemoryStream())
            {
                using (var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(1, 1))
                {
                    image[0, 0] = new SixLabors.ImageSharp.PixelFormats.Rgba32(255, 255, 255, 255);
                    image.SaveAsJpeg(stream);
                }

                stream.Position = 0;
                var file = new FormFile(stream, 0, stream.Length, "photoFile", "test.jpg");

                // Act
                var result = await model.OnPostRegisterAsync(file);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ObjectResult));
                Assert.AreEqual(beforeSyainPhotos + 1, await db.SyainPhotos.CountAsync());
                Assert.AreEqual(beforePhotoDatas + 1, await db.PhotoDatas.CountAsync());
                Assert.AreEqual(beforeAfterProcess + 1, await db.PhotoAfterProcessTnDatas.CountAsync());
            }
        }

        /// <summary>
        /// 登録_正常_更新_ファイル未指定でもサムネイルが更新される
        /// </summary>
        [TestMethod]
        public async Task 登録_正常_更新_ファイル未指定でもサムネイルが更新される()
        {
            // Arrange
            var syainPhoto = new SyainPhoto
            {
                SyainBaseId = SyainBaseId,
                Seq = 1,
                Selected = true,
                Deleted = false,
                UploadTime = fakeTimeProvider.Now(),
                PhotoName = "Before"
            };
            db.SyainPhotos.Add(syainPhoto);

            byte[] originalBytes;
            using (var ms = new MemoryStream())
            using (var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(1, 1))
            {
                image[0, 0] = new SixLabors.ImageSharp.PixelFormats.Rgba32(255, 255, 255, 255);
                image.SaveAsJpeg(ms);
                originalBytes = ms.ToArray();
            }

            db.PhotoDatas.Add(new PhotoData
            {
                SyainPhoto = syainPhoto,
                Photo = originalBytes
            });

            db.PhotoAfterProcessTnDatas.Add(new PhotoAfterProcessTnData
            {
                SyainPhoto = syainPhoto,
                Photo = new byte[] { 9, 9, 9 }
            });
            await db.SaveChangesAsync();

            var beforePhotoData = await db.PhotoDatas.AsNoTracking()
                .FirstAsync(x => x.SyainPhotoId == syainPhoto.Id);
            var beforeAfter = await db.PhotoAfterProcessTnDatas.AsNoTracking()
                .FirstAsync(x => x.SyainPhotoId == syainPhoto.Id);

            var model = CreateModel();
            model.SyainPhoto = new InputModel.SyainPhotoModel
            {
                Id = syainPhoto.Id,
                IsUpdate = true,
                CropX = 0,
                CropY = 0,
                CropWidth = 1,
                CropHeight = 1
            };

            // Act
            var result = await model.OnPostRegisterAsync(null);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));

            var afterPhotoData = await db.PhotoDatas.AsNoTracking()
                .FirstAsync(x => x.SyainPhotoId == syainPhoto.Id);
            var afterAfter = await db.PhotoAfterProcessTnDatas.AsNoTracking()
                .FirstAsync(x => x.SyainPhotoId == syainPhoto.Id);

            CollectionAssert.AreEqual(beforePhotoData.Photo!, afterPhotoData.Photo!);
            CollectionAssert.AreNotEqual(beforeAfter.Photo!, afterAfter.Photo!);
        }

        /// <summary>
        /// 登録_正常_更新_ファイル指定で元画像とサムネイルが更新される
        /// </summary>
        [TestMethod]
        public async Task 登録_正常_更新_ファイル指定で元画像とサムネイルが更新される()
        {
            // Arrange
            var syainPhoto = new SyainPhoto
            {
                SyainBaseId = SyainBaseId,
                Seq = 1,
                Selected = true,
                Deleted = false,
                UploadTime = fakeTimeProvider.Now(),
                PhotoName = "Before"
            };
            db.SyainPhotos.Add(syainPhoto);
            db.PhotoDatas.Add(new PhotoData
            {
                SyainPhoto = syainPhoto,
                Photo = [9, 9, 9]
            });
            db.PhotoAfterProcessTnDatas.Add(new PhotoAfterProcessTnData
            {
                SyainPhoto = syainPhoto,
                Photo = [8, 8, 8]
            });
            await db.SaveChangesAsync();

            var beforePhotoData = await db.PhotoDatas.AsNoTracking().FirstAsync(x => x.SyainPhotoId == syainPhoto.Id);
            var beforeAfter = await db.PhotoAfterProcessTnDatas.AsNoTracking().FirstAsync(x => x.SyainPhotoId == syainPhoto.Id);

            var model = CreateModel();
            model.SyainPhoto = new InputModel.SyainPhotoModel
            {
                Id = syainPhoto.Id,
                IsUpdate = true,
                CropX = 0,
                CropY = 0,
                CropWidth = 1,
                CropHeight = 1
            };

            using (var stream = new MemoryStream())
            {
                using (var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(1, 1))
                {
                    image[0, 0] = new SixLabors.ImageSharp.PixelFormats.Rgba32(255, 255, 255, 255);
                    image.SaveAsJpeg(stream);
                }

                stream.Position = 0;
                var file = new FormFile(stream, 0, stream.Length, "photoFile", "new.jpg");

                // Act
                var result = await model.OnPostRegisterAsync(file);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ObjectResult));

                var afterPhotoData = await db.PhotoDatas.AsNoTracking().FirstAsync(x => x.SyainPhotoId == syainPhoto.Id);
                var afterAfter = await db.PhotoAfterProcessTnDatas.AsNoTracking().FirstAsync(x => x.SyainPhotoId == syainPhoto.Id);

                CollectionAssert.AreNotEqual(beforePhotoData.Photo!, afterPhotoData.Photo!);
                CollectionAssert.AreNotEqual(beforeAfter.Photo!, afterAfter.Photo!);
            }
        }

        /// <summary>
        /// 削除_正常_指定IDの写真が論理削除されSelectedも外れる
        /// </summary>
        [TestMethod]
        public async Task 削除_正常_指定IDの写真が論理削除されSelectedも外れる()
        {
            // Arrange
            var syainPhoto = new SyainPhoto
            {
                SyainBaseId = SyainBaseId,
                Seq = 0,
                Selected = true,
                Deleted = false,
                UploadTime = fakeTimeProvider.Now(),
                PhotoName = "ToDelete"
            };
            db.SyainPhotos.Add(syainPhoto);
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SyainPhoto = new InputModel.SyainPhotoModel
            {
                Id = syainPhoto.Id
            };

            // Act
            var result = await model.OnPostDeleteAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ObjectResult));

            var deleted = await db.SyainPhotos.AsNoTracking()
                .FirstAsync(x => x.Id == syainPhoto.Id);

            Assert.IsTrue(deleted.Deleted);
            Assert.IsFalse(deleted.Selected);
        }

        /// <summary>
        /// 削除_異常_存在しないIDはエラーJsonを返す
        /// </summary>
        [TestMethod]
        public async Task 削除_異常_存在しないIDはエラーJsonを返す()
        {
            // Arrange
            var model = CreateModel();
            model.SyainPhoto = new InputModel.SyainPhotoModel
            {
                Id = 9999
            };

            // Act
            var result = await model.OnPostDeleteAsync();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(JsonResult));
            Assert.AreEqual(0, await db.SyainPhotos.CountAsync());
        }

        /// <summary>
        /// 登録_正常_新規追加時_SeqとSelectedの挙動_既存無し
        /// </summary>
        [TestMethod]
        public async Task 登録_正常_新規追加時_SeqとSelected_既存無し()
        {
            // Arrange
            var model = CreateModel();
            model.SyainPhoto = new InputModel.SyainPhotoModel
            {
                Id = 0,
                IsUpdate = false,
                CropX = 0,
                CropY = 0,
                CropWidth = 1,
                CropHeight = 1
            };

            using (var stream = new MemoryStream())
            {
                using (var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(1, 1))
                {
                    image[0, 0] = new SixLabors.ImageSharp.PixelFormats.Rgba32(255, 255, 255, 255);
                    image.SaveAsJpeg(stream);
                }

                stream.Position = 0;
                var file = new FormFile(stream, 0, stream.Length, "photoFile", "test.jpg");

                // Act
                var result = await model.OnPostRegisterAsync(file);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ObjectResult));

                var photos = await db.SyainPhotos
                    .Where(x => x.SyainBaseId == SyainBaseId)
                    .OrderBy(x => x.Seq)
                    .ToListAsync();

                Assert.AreEqual(1, photos.Count);
                Assert.AreEqual(0, photos[0].Seq);
                Assert.IsTrue(photos[0].Selected);
            }
        }

        /// <summary>
        /// 登録_正常_新規追加時_SeqとSelectedの挙動_既存有り
        /// </summary>
        [TestMethod]
        public async Task 登録_正常_新規追加時_SeqとSelected_既存有り()
        {
            // Arrange
            var existing1 = new SyainPhoto
            {
                SyainBaseId = SyainBaseId,
                Seq = 0,
                Selected = true,
                Deleted = false,
                UploadTime = fakeTimeProvider.Now(),
                PhotoName = "Existing1"
            };
            var existing2 = new SyainPhoto
            {
                SyainBaseId = SyainBaseId,
                Seq = 1,
                Selected = false,
                Deleted = false,
                UploadTime = fakeTimeProvider.Now(),
                PhotoName = "Existing2"
            };
            db.SyainPhotos.AddRange(existing1, existing2);
            await db.SaveChangesAsync();

            var model = CreateModel();
            model.SyainPhoto = new InputModel.SyainPhotoModel
            {
                Id = 0,
                IsUpdate = false,
                CropX = 0,
                CropY = 0,
                CropWidth = 1,
                CropHeight = 1
            };

            using (var stream = new MemoryStream())
            {
                using (var image = new Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(1, 1))
                {
                    image[0, 0] = new SixLabors.ImageSharp.PixelFormats.Rgba32(255, 255, 255, 255);
                    image.SaveAsJpeg(stream);
                }

                stream.Position = 0;
                var file = new FormFile(stream, 0, stream.Length, "photoFile", "test.jpg");

                // Act
                var result = await model.OnPostRegisterAsync(file);

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ObjectResult));

                var photos = await db.SyainPhotos
                    .Where(x => x.SyainBaseId == SyainBaseId && !x.Deleted)
                    .OrderBy(x => x.Seq)
                    .ToListAsync();

                Assert.AreEqual(3, photos.Count);
                // 既存Seqはそのまま、Max+1 が新規に割り当てられること
                Assert.AreEqual(0, photos[0].Seq);
                Assert.AreEqual(1, photos[1].Seq);
                Assert.AreEqual(2, photos[2].Seq);

                // Selected の挙動: 既存実装は "Seq == 0 のときだけ Selected = true" なので、
                // 新規追加時は既存の Selected を外さないことを確認
                Assert.IsTrue(photos[0].Selected);
                Assert.IsFalse(photos[1].Selected);
                Assert.IsFalse(photos[2].Selected);
            }
        }
    }
}