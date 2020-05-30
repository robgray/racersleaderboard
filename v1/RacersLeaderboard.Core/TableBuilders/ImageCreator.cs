using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace RacersLeaderboard.Core.TableBuilders
{
    public class ImageCreator
    {
        private Bitmap _bitmap;
        public ImageCreator(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }

        public Image Create()
        {
            using (var stream = new MemoryStream())
            {
                _bitmap.Save(stream, ImageFormat.Png);
                stream.Seek(0, 0);

                return Image.FromStream(stream);
            }
        }
    }
}
