using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageCropper.API.PomocneController
{
    public interface IPomocnaZaProcesiranjeSlike
    {
        byte[] CropImage(byte[] imageData, int x, int y, int width, int height, float? scale = null);
        byte[] ApplyLogo(byte[] imageData, byte[] logoData, string logoPosition, float scaleDown);
    }

    public class PomocnaZaProcesiranjeSlike : IPomocnaZaProcesiranjeSlike
    {
        public byte[] CropImage(byte[] imageData, int x, int y, int width, int height, float? scale = null)
        {
            using var image = Image.Load(imageData);

            if (x < 0 || y < 0 || width <= 0 || height <= 0)
                throw new ArgumentException("Invalid crop coordinates");

            if (x + width > image.Width || y + height > image.Height)
                throw new ArgumentException("Crop area exceeds image bounds");

            image.Mutate(img => img.Crop(new Rectangle(x, y, width, height)));

            if (scale.HasValue && scale.Value > 0 && scale.Value < 1)
            {
                int newWidth = (int)(image.Width * scale.Value);
                int newHeight = (int)(image.Height * scale.Value);
                image.Mutate(img => img.Resize(newWidth, newHeight));
            }

            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return ms.ToArray();
        }

        public byte[] ApplyLogo(byte[] imageData, byte[] logoData, string logoPosition, float scaleDown)
        {
            using var image = Image.Load<Rgba32>(imageData);
            using var logo = Image.Load<Rgba32>(logoData);

            // Skaliraj logo
            int logoWidth = (int)(image.Width * scaleDown);
            int logoHeight = (int)(logo.Height * ((float)logoWidth / logo.Width));
            logo.Mutate(l => l.Resize(logoWidth, logoHeight));

            // Odredi poziciju loga
            int x = 0, y = 0;
            switch (logoPosition.ToLower())
            {
                case "top-left":
                    x = 10; y = 10;
                    break;
                case "top-right":
                    x = image.Width - logoWidth - 10; y = 10;
                    break;
                case "bottom-left":
                    x = 10; y = image.Height - logoHeight - 10;
                    break;
                case "bottom-right":
                    x = image.Width - logoWidth - 10;
                    y = image.Height - logoHeight - 10;
                    break;
                default:
                    x = 10; y = 10;
                    break;
            }

            image.Mutate(img => img.DrawImage(logo, new Point(x, y), 1f));

            using var ms = new MemoryStream();
            image.SaveAsPng(ms);
            return ms.ToArray();
        }
    }
}