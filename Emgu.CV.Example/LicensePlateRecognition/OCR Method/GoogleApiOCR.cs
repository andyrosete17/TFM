using Emgu.CV;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicensePlateRecognition.OCR_Method
{
    public class GoogleApiOCR
    {
        /// <summary>
        /// Google api OCR
        /// </summary>
        /// <param name="filteredPlate"></param>
        /// <returns></returns>
        public static StringBuilder GetText(UMat filteredPlate)
        {
            var client = ImageAnnotatorClient.Create();
            // load the image file into memory
            var filteredImageBitmap = filteredPlate.Bitmap;
            filteredImageBitmap.Save(@"path + test.jpg");
            var image = Image.FromFile(@"path + test.jpg");
            // performs label detection on the image file
            var response = client.DetectText(image);
            StringBuilder strBuilder = new StringBuilder();
            foreach (var annotation in response)
            {
                if (annotation.Description != null)
                {
                    strBuilder.Append(annotation.Description);
                    break;
                }
            }

            return strBuilder;
        }
    }
}