using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Zouryoku.Utils
{
    public static class ImageUtil
    {
        public static string GetImageSrc(string fileName, byte[] image)
        {
            var mimeType = FileUtil.GetContentType(fileName);
            var imageByte = Convert.ToBase64String(image);
            return $"data:{mimeType};base64,{imageByte}";
        }

        public static string GetImageSrc(byte[] image, string contentType)
        {
            var imageByte = Convert.ToBase64String(image);
            return $"data:{contentType};base64,{imageByte}";
        }

        public static byte[] FormFileToByte(IFormFile file)
        {
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                bytes = stream.ToArray();
            }
            return bytes;
        }

        public static Image ByteArrayToImage(byte[] byteArrayIn)
        {
            using var ms = new MemoryStream(byteArrayIn);
            return Image.Load(ms);
        }

        public static byte[] GetCompresseImage(byte[] imageByte)
        {
            var image = ByteArrayToImage(imageByte);

            var encoder = new JpegEncoder
            {
                Quality = 50
            };

            using var memoryStream = new MemoryStream();

            image.Save(memoryStream, encoder);
            return memoryStream.ToArray();
        }

        public static string FormFileToSrc(IFormFile file)
            => GetImageSrc(file.FileName, FormFileToByte(file));

        public static (int Width, int Height) KeepAspect(double aspect, int? width = null, int? height = null)
        {
            // height を優先して合わせる
            switch (width.HasValue, height.HasValue)
            {
                case (false, false):
                    height = 120;
                    width = (int)(height / aspect);
                    break;
                case (false, true):
                    width = (int?)(height / aspect);
                    break;
                case (true, false):
                    height = (int?)(width * aspect);
                    break;
                case (true, true):
                    width = (int?)(height / aspect);
                    break;
            }
            return (width ?? int.MinValue, height ?? int.MinValue);
        }
    }

    public class BindImage
    {
        public long? Id { get; set; }
        public string? Name { get; set; }
        public string? Src { get; set; }
    }
}
