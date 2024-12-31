using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MunicipalityPortal
{
    public class ImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is byte[] imageData && imageData.Length > 0)
            {
                var image = new BitmapImage();
                using (var mem = new MemoryStream(imageData))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                return image;
            }

            // If no image is available, use a default system icon (e.g., Information)
            using (var icon = SystemIcons.Information)
            {
                using (var bitmap = icon.ToBitmap())
                {
                    var stream = new MemoryStream();
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png); // Save as PNG to stream
                    stream.Position = 0;

                    var defaultImage = new BitmapImage();
                    defaultImage.BeginInit();
                    defaultImage.StreamSource = stream;
                    defaultImage.CacheOption = BitmapCacheOption.OnLoad;
                    defaultImage.EndInit();
                    defaultImage.Freeze();

                    return defaultImage;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
