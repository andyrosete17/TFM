using Emgu.CV;
using Microsoft.ProjectOxford.Vision;
using Microsoft.ProjectOxford.Vision.Contract;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LicensePlateRecognition.OCR_Method
{
    public class ComputerVisionOCR
    {
        public static StringBuilder GetText(UMat filteredPlate)
        {
            // load the image file into memory
            var filteredImageBitmap = filteredPlate.Bitmap;
            filteredImageBitmap.Save(@"test.jpg");
            // performs label detection on the image file

            var response = new OcrResults();

            Task.Run(async () =>
            {
                var cognitiveService = new ImageToTextInterpreter
                {
                    ImageFilePath = @"test.jpg",
                    SubscriptionKey = "473349f3782e424f91d1b37e3403fa84",
                    ApiRoot = "https:///westcentralus.api.cognitive.microsoft.com/vision/v1.0/ocr"
                };
                try
                {
                    response = await cognitiveService.ConvertImageToStreamAndExtractText();
                }
                catch (System.Exception)
                {
                    response = null;
                }
            }).Wait();

            var strBuilder = new StringBuilder();
            if (response != null)
            {
                strBuilder = GetResults(response);
            }
            return strBuilder;
        }

        private static StringBuilder GetResults(OcrResults response)
        {
            StringBuilder strBuilder = new StringBuilder();
            foreach (var region in response.Regions)
            {
                foreach (var line in region.Lines)
                {
                    foreach (var item in line.Words)
                    {
                        strBuilder.Append(item.Text);
                    }
                }
            }

            return strBuilder;
        }

        public class ImageToTextInterpreter
        {
            public string ImageFilePath { get; set; }

            public string SubscriptionKey { get; set; }
            public string ApiRoot { get; set; }

            private const string UNKNOWN_LANGUAGE = "unk";

            public async Task<OcrResults> ConvertImageToStreamAndExtractText()
            {
                VisionServiceClient VisionServiceClient = new VisionServiceClient(SubscriptionKey, "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");
                using (Stream imageFileStream = File.OpenRead(ImageFilePath))
                {
                    return await VisionServiceClient.RecognizeTextAsync(imageFileStream, UNKNOWN_LANGUAGE);
                }
            }
        }
    }
}