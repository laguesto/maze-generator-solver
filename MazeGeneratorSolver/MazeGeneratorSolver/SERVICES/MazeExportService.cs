
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MazeGeneratorSolver.SERVICES
{
    public class MazeExportService
    {
        // Експорт лабіринту у формат PNG
        public void ExportToPng(FrameworkElement element, string filePath)
        {
            // Перевіряємо розміри зображення
            double width = element.ActualWidth;
            double height = element.ActualHeight;

            if (width <= 0 || height <= 0)
            {
                throw new InvalidOperationException(
                    "Неможливо експортувати порожнє зображення.");
            }

            // Масштаб для збереження зображення у високій якості
            int scale = 2;

            int pixelWidth = (int)Math.Ceiling(width * scale);
            int pixelHeight = (int)Math.Ceiling(height * scale);

            // Створюємо растрове зображення
            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                pixelWidth,
                pixelHeight,
                96 * scale,
                96 * scale,
                PixelFormats.Pbgra32);

            // Переносимо лабіринт на зображення
            bitmap.Render(element);

            // Створюємо PNG-файл
            PngBitmapEncoder encoder = new PngBitmapEncoder();

            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            // Зберігаємо файл
            using (FileStream stream = new FileStream(
                filePath, FileMode.Create))
            {
                encoder.Save(stream);
            }
        }
    }
}
