using Emgu.CV;
using Emgu.CV.CvEnum;
using System.Drawing;

namespace LicensePlateRecognition.Utils
{
    public class SaveImageClass
    {
        public static void SaveImage(Mat img, string name)
        {
            var bitmap = img.Bitmap;
            bitmap.Save("salvas\\" + name);
        }

        public static void SaveImage(UMat img, string name)
        {
            var bitmap = img.Bitmap;
            bitmap.Save("salvas\\" + name);
        }

        public static void SaveImage(IInputArray img, string name)
        {
            var mat = new Mat();
            var newSize = new Size
            {
                Height = 240,
                Width = 180
            };
            CvInvoke.Resize(img, mat, newSize, 0, 0, Inter.Cubic);
            var bitmap = mat.Bitmap;
            bitmap.Save("salvas\\" + name);
        }
    }
}