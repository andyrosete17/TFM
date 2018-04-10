using Emgu.CV;
using System.Text;

namespace LicensePlateRecognition.OCR_Method
{
    public class TesseractOCR
    {
        /// <summary>
        /// Tesseract OCR
        /// </summary>
        /// <param name="filteredPlate"></param>
        /// <param name="_ocr"></param>
        /// <returns></returns>
        public static StringBuilder GetText(UMat filteredPlate, Emgu.CV.OCR.Tesseract _ocr)
        {
            Emgu.CV.OCR.Tesseract.Character[] words;
            StringBuilder strBuilder = new StringBuilder();
            using (UMat tmp = filteredPlate.Clone())
            {
                _ocr.SetImage(tmp);
                _ocr.Recognize();

                strBuilder.Append(_ocr.GetUTF8Text());

                words = _ocr.GetCharacters();

                //if (words.Length == 0) continue;

                //for (int i = 0; i < words.Length; i++)
                //{
                //    strBuilder.Append(words[i].Text);
                //}
            }

            return strBuilder;
        }
    }
}