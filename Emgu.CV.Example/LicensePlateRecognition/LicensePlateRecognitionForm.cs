//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.
//----------------------------------------------------------------------------

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using LicensePlateRecognition.Enums;
using LicensePlateRecognition.OCR_Method;
using LicensePlateRecognition.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LicensePlateRecognition
{
    public partial class LicensePlateRecognitionForm : Form
    {
        private LicensePlateDetector _licensePlateDetector;
        private Mat img;

        public LicensePlateRecognitionForm()
        {
            InitializeComponent();
            _licensePlateDetector = new LicensePlateDetector("x64/");
            //Mat m = new Mat("license-plate.jpg");
            //UMat um = m.GetUMat(AccessType.ReadWrite);
            //imageBox1.Image = um;
            //ProcessImage(m);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="ocr_mode"></param>
        /// <param name="count"></param>
        /// <param name="canny_thres">Canny threshold will take 3 values 20, 30, 40, 50</param>
        /// <returns></returns>
        private bool ProcessImage(IInputOutputArray image, int ocr_mode)
        {
            Stopwatch watch = Stopwatch.StartNew(); // time the detection process
            List<IInputOutputArray> licensePlateImagesList = new List<IInputOutputArray>();
            List<IInputOutputArray> filteredLicensePlateImagesList = new List<IInputOutputArray>();
            List<RotatedRect> licenseBoxList = new List<RotatedRect>();
            List<string> words = new List<string>();
            var result = false;
            bool validValue = false;
            UMat filteredPlate = new UMat();
            StringBuilder strBuilder = new StringBuilder();
            CvInvoke.CvtColor(img, filteredPlate, ColorConversion.Bgr2Gray);

            words = _licensePlateDetector.DetectLicensePlate(
                        image,
                        licensePlateImagesList,
                        filteredLicensePlateImagesList,
                        licenseBoxList,
                        ocr_mode);

            if (ocr_mode == 3)
            {
                strBuilder = ComputerVisionOCR.GetText(filteredPlate);
                if (strBuilder != null)
                {
                    words.Clear();
                    List<String> licenses = new List<String>
                        {
                            strBuilder.ToString()
                        };
                    licenses.ForEach(
                        x =>
                        {
                            words.Add(x);
                        });
                }
            }

            var validWords = new List<string>();
            var validLicencePlates = new List<IInputOutputArray>();
            for (int w = 0; w < words.Count; w++)
            {
                string replacement2 = Regex.Replace(words[w], @"\t|\n|\r", "");
                string replacement = Regex.Replace(replacement2, "[^0-9a-zA-Z]+", "");
                if (replacement.Length >= 6 && replacement != null)
                {
                    var filteredLicence = FilterLicenceSpain(replacement);
                    if (!string.IsNullOrWhiteSpace(filteredLicence))
                    {
                        validValue = true;
                        if (!validWords.Contains(replacement))
                        {
                            validWords.Add(filteredLicence);
                            validLicencePlates.Add(licensePlateImagesList[w]);
                        }
                    }
                }
            }

            if (validValue)
            {
                ShowResults(image, watch, validLicencePlates, filteredLicensePlateImagesList, licenseBoxList, validWords);
            }
            else
            {
                ShowResults(image, watch, licensePlateImagesList, filteredLicensePlateImagesList, licenseBoxList, words);
            }


            result = true;
            return result;
        }

        /// <summary>
        /// Check if the license has a valid value
        /// </summary>
        /// <param name="replacement"></param>
        /// <returns></returns>
        private string FilterLicenceSpain(string replacement)
        {
            var result = "";
            var mask = new List<String>();
            var charList = replacement.ToCharArray();
            foreach (var character in charList)
            {
                try
                {
                    int.Parse(character.ToString());
                    mask.Add("0");
                }
                catch (Exception)
                {
                    mask.Add("1");
                }
            }

            if (string.Join("", mask).IndexOf("111") > 0 && mask.Count >= 8)
            {
                replacement = replacement.Substring(0, string.Join("", mask).IndexOf("111") + 3);
                mask = GerenateMask(mask, 8, true);
            }

            if (mask.Count >= 8)
            {
                if (string.Join("", mask).Substring(mask.Count - 6) == "100001" )
                {
                    replacement = replacement.Substring(replacement.Length - 6);
                    mask = GerenateMask(mask, 6, false);
                }
                else if (string.Join("", mask).IndexOf("100001") > 0)
                {
                    replacement = replacement.Substring(string.Join("", mask).IndexOf("100001"), 6);
                    mask = GerenateMask(mask, 6, false, "100001");
                }
                else if (string.Join("", mask).Substring(mask.Count - 7) == "1100001"
                        || string.Join("", mask).Substring(mask.Count - 7) == "1000011"
                        || string.Join("", mask).Substring(mask.Count - 7) == "0000111")
                {
                    replacement = replacement.Substring(replacement.Length - 7);
                    mask = GerenateMask(mask, 7, false);
                }
                else if (string.Join("", mask).IndexOf("1100001") > 0)
                {
                    replacement = replacement.Substring(string.Join("", mask).IndexOf("1100001"),7);
                    mask = GerenateMask(mask, 7, false, "1100001");
                }
                else if (string.Join("", mask).IndexOf("1000011") > 0)
                {
                    replacement = replacement.Substring(string.Join("", mask).IndexOf("1000011"), 7);
                    mask = GerenateMask(mask, 7, false, "1000011");
                }
                else if (string.Join("", mask).IndexOf("0000111") > 0)
                {
                    replacement = replacement.Substring(string.Join("", mask).IndexOf("0000111"), 7);
                    mask = GerenateMask(mask, 7, false, "0000111");
                }
                else if (string.Join("", mask).Substring(mask.Count - 8) == "11000011"
                      || string.Join("", mask).Substring(mask.Count - 8) == "10000011")
                {
                    replacement = replacement.Substring(replacement.Length - 8);
                    mask = GerenateMask(mask, 8,false);
                }
                else if (string.Join("", mask).IndexOf("11000011") > 0)
                {
                    replacement = replacement.Substring(string.Join("", mask).IndexOf("11000011"), 8);
                    mask = GerenateMask(mask, 8, false, "11000011");
                }
                else if (string.Join("", mask).IndexOf("10000011") > 0)
                {
                    replacement = replacement.Substring(string.Join("", mask).IndexOf("10000011"), 8);
                    mask = GerenateMask(mask, 8, false, "10000011");
                }

            }


            switch (mask.Count)
            {
                case 6:
                    {
                        if (string.Join("", mask) == "100001")
                        {
                            if (CheckProvinceEnum(replacement.Substring(0, 1)))
                            {
                                result = replacement;
                            }
                        }
                        break;
                    }
                case 7:
                    {
                        if (string.Join("", mask) == "0000111")
                        {
                            result = replacement;
                        }

                        if (string.Join("", mask) == "1100001")
                        {
                            if (CheckProvinceEnum(replacement.Substring(0, 2)))
                            {
                                result = replacement;
                            }
                        }

                        if (string.Join("", mask) == "1000011")
                        {
                            if (CheckProvinceEnum(replacement.Substring(0, 1)))
                            {
                                result = replacement;
                            }
                        }
                        break;
                    }
                case 8:
                    {
                        if (string.Join("", mask) == "11000011")
                        {
                            if (CheckProvinceEnum(replacement.Substring(0, 2)))
                            {
                                result = replacement;
                            }
                        }
                        else if (string.Join("", mask) == "10000011")
                        {
                            if (CheckProvinceEnum(replacement.Substring(0, 1)))
                            {
                                result = replacement;
                            }
                        }
                        break;
                    }
                default:
                    break;

            }


            return result;
        }

        private static List<string> GerenateMask(List<string> mask, int limit, bool direction, string maskForce = "")           
        {
            var maskTemp = new List<String>();
            if (!string.IsNullOrWhiteSpace(maskForce))
            {
                var maskForceCharArray = maskForce.ToCharArray();
                foreach (var item in maskForceCharArray)
                {
                    maskTemp.Add(item.ToString());
                }
            }
            else
            {
                if (direction)
                {
                    for (int i = 0; i < limit; i++)
                    {
                        maskTemp.Add(mask[i]);
                    }
                }
                else
                {
                    for (int i = mask.Count - limit; i < mask.Count; i++)
                    {
                        maskTemp.Add(mask[i]);
                    }
                }
            }
            
            return maskTemp;
        }

        private bool CheckProvinceEnum(string provinceSufix)
        {
            var result = false;
            try
            {
                //string province = Enum.GetName(typeof(ProvincesEnum), provinceSufix);
                ProvincesEnum province = (ProvincesEnum)Enum.Parse(typeof(ProvincesEnum), provinceSufix);

                result = true;
            }
            catch (Exception)
            { }

            return result;
        }

        private void ShowResults(IInputOutputArray image, Stopwatch watch, List<IInputOutputArray> licensePlateImagesList, List<IInputOutputArray> filteredLicensePlateImagesList, List<RotatedRect> licenseBoxList, List<string> words)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            var refinnedWords = new List<string>();
            watch.Stop(); //stop the timer
            processTimeLabel.Text = String.Format("License Plate Recognition time: {0} milli-seconds", watch.Elapsed.TotalMilliseconds);

            panel1.Controls.Clear();
            Point startPoint = new Point(10, 10);
            logger.Trace("License Plate Recognition time: {0} milli-seconds", watch.Elapsed.TotalMilliseconds);
            logger.Trace("Licence plate: {0} \nLicence detected: \n", nameB.Text);
            for (int i = 0; i < licensePlateImagesList.Count; i++)
            {
                if (licensePlateImagesList.Count > 0)
                {
                    Mat dest = new Mat();
                    CvInvoke.VConcat(licensePlateImagesList[i], filteredLicensePlateImagesList[i], dest);
                    string replacement2 = Regex.Replace(words[i], @"\t|\n|\r", "");
                    string replacement = Regex.Replace(replacement2, "[^0-9a-zA-Z]+", "");
                    AddLabelAndImage(
                       ref startPoint,
                       String.Format("License: {0}", replacement),
                       dest);
                    PointF[] verticesF = licenseBoxList[i].GetVertices();
                    Point[] vertices = Array.ConvertAll(verticesF, Point.Round);
                    using (VectorOfPoint pts = new VectorOfPoint(vertices))
                        CvInvoke.Polylines(image, pts, true, new Bgr(Color.Red).MCvScalar, 2);
                    logger.Trace("{0}- {1} \n", i, replacement);
                    refinnedWords.Add(replacement);
                }
            }
            LicenceQuality(nameB.Text, refinnedWords);
        }

        private void AddLabelAndImage(ref Point startPoint, String labelText, IImage image)
        {
            Label label = new Label();
            panel1.Controls.Add(label);
            label.Text = labelText;
            label.Width = 100;
            label.Height = 30;
            label.Location = startPoint;
            startPoint.Y += label.Height;

            ImageBox box = new ImageBox();
            panel1.Controls.Add(box);
            box.ClientSize = image.Size;
            box.Image = image;
            box.Location = startPoint;
            startPoint.Y += box.Height + 10;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ///Clear multiple image 
            inputTextBox.Text = "";
            MyGlobal.imageClassList.Clear();
            MyGlobal.imageList.Clear();

            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    img = CvInvoke.Imread(openFileDialog1.FileName);
                    imageBox1.Image = img;
                    textBox1.Text = System.IO.Path.GetFileName(openFileDialog1.FileName);
                }
                catch
                {
                    MessageBox.Show(String.Format("Invalide File: {0}", openFileDialog1.FileName));
                    return;
                }

                //UMat uImg = img.GetUMat(AccessType.ReadWrite);
                //ProcessImage(uImg);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void TesseractBtn_Click_1(object sender, EventArgs e)
        {
            TesseractSimple();

        }

        private void TesseractSimple()
        {
            CheckInputType();
            UMat uImg = img.GetUMat(AccessType.ReadWrite);
            ProcessImageMethod(uImg, 1);
            ResetImage();
        }

        private void CheckInputType()
        {
            if (textBox1.Text.Length == 0)
            {
                int.TryParse(indiceB.Text, out int value);
                var root = SetImageDescription(value);
                CheckLicenceDataBase(value);
                img = CvInvoke.Imread(root);
            }
        }

        private void ProcessImageMethod(UMat uImg, int ocr_Method)
        {
            ProcessImage(uImg, ocr_Method);
        }

        private void GoogleBtn_Click(object sender, EventArgs e)
        {
            GoogleApiSimple();
        }

        private void GoogleApiSimple()
        {
            CheckInputType();
            UMat uImg = img.GetUMat(AccessType.ReadWrite);
            ProcessImageMethod(uImg, 2);
            ResetImage();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            ResetImage();
        }

        private void ResetImage()
        {
            imageBox1.Image = img;
        }

        private void CVisionButton_Click(object sender, EventArgs e)
        {
            CompVisionSimple();
        }

        private void CompVisionSimple()
        {
            CheckInputType();
            UMat uImg = img.GetUMat(AccessType.ReadWrite);
            ProcessImageMethod(uImg, 3);
            ResetImage();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            var value = default(int);
            int.TryParse(indiceB.Text, out value);
            var x = SetImageDescription(value);

            NextImage();

            DeleteLicenceDataBase(value);
            File.Delete(x);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //remove one image texbox text
            textBox1.Text = "";
            inputTextBox.Text = "";
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = @"C:\_MWS\TFM\Imagenes\Coches\";
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    inputB.Text = fbd.SelectedPath;
                    var files = Directory.GetFiles(inputB.Text).OrderBy(f=>f);
                    var count = 0;
                    foreach (var file in files)
                    {
                        var split = inputB.Text.Split(new string[] { "\\" }, StringSplitOptions.None);
                        var ini = split[split.Length - 1];
                        var fileText = file.Substring(file.IndexOf(ini) + ini.Length + 1, file.Length - file.IndexOf(ini) - ini.Length - 1);
                        inputTextBox.Text += count.ToString() + " -- " + fileText + "\r\n";

                        ImageStructure imageStructure = GetImageStructure(count, fileText);
                        MyGlobal.imageClassList.Add(imageStructure);
                        count++;
                    }

                    if (MyGlobal.imageClassList.Count != 0)
                    {
                        SetImageDescription(0);
                        CheckLicenceDataBase(0);
                    }
                }
            }
        }

        public static class MyGlobal
        {
            public static List<string> imageList = new List<string>();

            public static List<ImageStructure> imageClassList = new List<ImageStructure>();

            public static List<string> CorrectDetection = new List<string>();
        }

        private static ImageStructure GetImageStructure(int count, string fileText)
        {
            return new ImageStructure
            {
                Id = count,
                ImageName = fileText
            };
        }
        private string SetImageDescription(int indexImage)
        {
            var first = MyGlobal.imageClassList[indexImage];
            var index = first.ImageName.IndexOf(".");
            var name = first.ImageName.Substring(0, index);
            var extension = first.ImageName.Substring(index + 1, first.ImageName.Length - index - 1);
            nameB.Text = "";
            indiceB.Text = first.Id.ToString();
            var result = inputB.Text + "\\" + first.ImageName;
            imageBox1.Load(result);
            imageBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            return result;
        }

        public void CheckLicenceDataBase(int index)
        {
            var first = MyGlobal.imageClassList[index];
            try
            {
                using (var context = new ImageDataBaseEntities())
                {
                    IQueryable<ImagesLicence> imageSelected = GetImageDetails(first, context);
                    if (imageSelected.Any())
                    {
                        nameB.Text = imageSelected.FirstOrDefault().carLicence;
                    }
                }
            }
            catch (Exception es)

            {
                MessageBox.Show(es.Message);
            }
        }

        public void DeleteLicenceDataBase(int index)
        {
            var first = MyGlobal.imageClassList[index];
            try
            {
                using (var context = new ImageDataBaseEntities())
                {
                    IQueryable<ImagesLicence> imageSelected = GetImageDetails(first, context);
                    if (imageSelected.Any())
                    {
                        imageSelected.FirstOrDefault().active = false;
                        context.ImagesLicences.Remove(imageSelected.FirstOrDefault());
                        context.SaveChanges();
                    }
                }
            }
            catch (Exception es)

            {
                MessageBox.Show(es.Message);
            }
        }

        private static IQueryable<ImagesLicence> GetImageDetails(ImageStructure first, ImageDataBaseEntities context)
        {
            return from img in context.ImagesLicences
                   where img.name == first.ImageName
                   select img;
        }

        private static IQueryable<ImagesLicence> GetImageDetails(string name, ImageDataBaseEntities context)
        {
            return from img in context.ImagesLicences
                   where img.name == name
                   select img;
        }

        private void beforeB_Click(object sender, EventArgs e)
        {
            if (indiceB.Text.Length > 0 && indiceB.Text != String.Empty && indiceB.Text != "0")
            {
                try
                {
                    var value = default(int);
                    int.TryParse(indiceB.Text, out value);
                    if (value != 0)
                    {
                        value--;
                        SetImageDescription(value);
                        var root = SetImageDescription(value);
                        img = CvInvoke.Imread(root);
                        CheckLicenceDataBase(value);
                    }
                }
                catch (Exception)
                { }
            }
        }

        private void nextB_Click(object sender, EventArgs e)
        {
            NextImage();
        }

        private void NextImage()
        {
            if (indiceB.Text.Length > 0 && indiceB.Text != String.Empty)
            {
                try
                {
                    var value = default(int);
                    int.TryParse(indiceB.Text, out value);

                    value++;
                    var root = SetImageDescription(value);
                    img = CvInvoke.Imread(root);
                    CheckLicenceDataBase(value);
                }
                catch (Exception)
                { }
            }
        }

        private void SaveDbButton_Click(object sender, EventArgs e)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            try
            {
                //String str = "Data Source=ESBA9336;Initial Catalog=ImageDataBase;Persist Security Info=True;User ID=sa;Password=Sunshine123";

                //SqlConnection con = new SqlConnection(str);

                var imageDto = new ImagesLicence
                {
                    dateAdded = DateTime.Now,
                    active = true,
                    carLicence = nameB.Text,
                    localRoute = inputB.Text,
                    name = MyGlobal.imageClassList[int.Parse(indiceB.Text)].ImageName
                };



                using (var context = new ImageDataBaseEntities())
                {
                    IQueryable<ImagesLicence> imageSelected = GetImageDetails(imageDto.name, context);
                    if (imageSelected.Any())
                    {
                        imageSelected.FirstOrDefault().carLicence = nameB.Text;
                    }
                    else
                    {
                        context.ImagesLicences.Add(imageDto);
                    }

                    context.SaveChanges();
                    logger.Trace("New car licence saved name: {0}, car licence: {1}", imageDto.name, imageDto.carLicence);
                }

            }
            catch (Exception es)

            {
                MessageBox.Show(es.Message);
            }
        }

        private void indiceB_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (indiceB.Text.Length > 0 && indiceB.Text != String.Empty)
                {
                    try
                    {
                        var value = default(int);
                        int.TryParse(indiceB.Text, out value);
                        if (value != 0)
                        {
                            var root = SetImageDescription(value);
                            img = CvInvoke.Imread(root);
                            CheckLicenceDataBase(value);
                        }
                    }
                    catch (Exception)
                    { }
                }
            }
        }

        private void LicenceQuality(string correctLicence, List<string> detectedLicence)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            bool valid = false;
            var recall = 0;
            //detectedLicence.ForEach(licence =>
            //{
            //    if (string.Compare(correctLicence.ToLower(), licence.ToLower()).Equals(0))
            //    {
            //        valid = true;
            //        count++;
            //    }
            //});
            var validCount = detectedLicence.Where(a => a.ToLower() == correctLicence.ToLower()).AsQueryable().Count();
            if (validCount>0)
            {
                logger.Trace("Licence plate successfully detected\n");
                MyGlobal.CorrectDetection.Add(correctLicence);
                recall = 100;
            }
            //if (valid)
            //{
            //    logger.Trace("Licence plate successfully detected\n");
            //    MyGlobal.CorrectDetection.Add(correctLicence);
            //    recall = 100;
            //}
            else
            {
                logger.Trace("Licence plate wrong detection \n", nameB.Text);
            }
            double effectivty = 0;
            if (detectedLicence.Count > 0)
            {
                effectivty = (double)validCount / (double)detectedLicence.Count * 100;
            }            
            logger.Trace("Licences detected: {0} Detection Precision: {1}% --- Recall = {2}%\n", detectedLicence.Count, Math.Round(effectivty, 2), recall);
        }

        private void TesseractGBtn_Click(object sender, EventArgs e)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            Stopwatch globalWatch = Stopwatch.StartNew(); // time the detection process
            var totalImages = MyGlobal.imageClassList.Where(i => i.ImageName.IndexOf(".jpg") > 0).AsQueryable().ToArray();
            for (int i = 0; i < totalImages.Count(); i++)
            {
                if (totalImages[i].ImageName.IndexOf(".jpg") > 0)
                {
                    TesseractSimple();
                    logger.Trace("Image name: {0}\n\n\n", totalImages[i].ImageName);
                }
                NextImage();
            }
            globalWatch.Stop();
            double percent = (double)MyGlobal.CorrectDetection.Count / (double)totalImages.Count() * 100;
            logger.Trace("License Plate Recognition time Tesseract Global: {0} milli-seconds ",globalWatch.Elapsed.TotalMilliseconds.ToString());
            logger.Trace("License Plate Recognition time Tesseract Global:Founded licence: {0}\n", MyGlobal.CorrectDetection.Count.ToString());
            logger.Trace("License Plate Recognition time Tesseract Global:Total licences {0}\n", totalImages.Count().ToString());
            logger.Trace("License Plate Recognition time Tesseract Global:Detection percent: {0}%\n", percent.ToString());
        }

        private void GoogleApiGBtn_Click(object sender, EventArgs e)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            Stopwatch globalWatch = Stopwatch.StartNew(); // time the detection process
            var totalImages = MyGlobal.imageClassList.Where(i => i.ImageName.IndexOf(".jpg") > 0).AsQueryable().ToArray();
            for (int i = 0; i < totalImages.Count(); i++)
            {
                if (totalImages[i].ImageName.IndexOf(".jpg") > 0)
                {
                    GoogleApiSimple();
                    logger.Trace("Image name: {0}", totalImages[i].ImageName);
                }
                NextImage();
            }
            globalWatch.Stop();
            double percent = (double)MyGlobal.CorrectDetection.Count / (double)totalImages.Count() * 100;
            logger.Trace("License Plate Recognition time Google Api Global: {0} milli-seconds ", globalWatch.Elapsed.TotalMilliseconds.ToString());
            logger.Trace("License Plate Recognition time Google Api Global:Founded licence: {0}\n", MyGlobal.CorrectDetection.Count.ToString());
            logger.Trace("License Plate Recognition time Google Api Global:Total licences {0}\n", totalImages.Count().ToString());
            logger.Trace("License Plate Recognition time Google Api Global:Detection percent: {0}%\n", percent.ToString());

        }

        private void GoogleVisionGBtn_Click(object sender, EventArgs e)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            Stopwatch globalWatch = Stopwatch.StartNew(); // time the detection process
            var totalImages = MyGlobal.imageClassList.Where(i => i.ImageName.IndexOf(".jpg") > 0).AsQueryable().ToArray();
            for (int i = 0; i < totalImages.Count(); i++)
            {
                if (totalImages[i].ImageName.IndexOf(".jpg") > 0)
                {
                    CompVisionSimple();
                    logger.Trace("Image name: {0}", totalImages[i].ImageName);
                }
                NextImage();
            }
            globalWatch.Stop();
            double percent = (double)MyGlobal.CorrectDetection.Count / (double)totalImages.Count() * 100;
            logger.Trace("License Plate Recognition time Computer Vision Api Global: {0} milli-seconds ", globalWatch.Elapsed.TotalMilliseconds.ToString());
            logger.Trace("License Plate Recognition time Computer Vision Api Global:Founded licence: {0}\n", MyGlobal.CorrectDetection.Count.ToString());
            logger.Trace("License Plate Recognition time Computer Vision Api Global:Total licences {0}\n", totalImages.Count().ToString());
            logger.Trace("License Plate Recognition time Computer Vision Api Global:Detection percent: {0}%\n", percent.ToString());

        }
    }
}
