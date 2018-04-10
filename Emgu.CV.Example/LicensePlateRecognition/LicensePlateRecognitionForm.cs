//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.
//----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;

using System.Diagnostics;
using Emgu.CV.Util;
using System.Text.RegularExpressions;

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

        private bool ProcessImage(IInputOutputArray image, int ocr_mode, int count)
        {
            Stopwatch watch = Stopwatch.StartNew(); // time the detection process
            List<IInputOutputArray> licensePlateImagesList = new List<IInputOutputArray>();
            List<IInputOutputArray> filteredLicensePlateImagesList = new List<IInputOutputArray>();
            List<RotatedRect> licenseBoxList = new List<RotatedRect>();
            List<string> words = new List<string>();
            var result = false;
            bool validValue = false;

            words = _licensePlateDetector.DetectLicensePlate(
               image,
               licensePlateImagesList,
               filteredLicensePlateImagesList,
               licenseBoxList,
               ocr_mode);

            words.ForEach(w =>
            {
                string replacement = Regex.Replace(w, @"\t|\n|\r", "");
                if (replacement.Length >= 6 && replacement.Length <= 8 && replacement != null && FilterLicenceSpain(replacement))
                {
                    validValue = true;
                }
            });

            if (validValue || count == 5)
            {
                ShowResults(image, watch, licensePlateImagesList, filteredLicensePlateImagesList, licenseBoxList, words, count);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Check if the license has a valid value
        /// </summary>
        /// <param name="replacement"></param>
        /// <returns></returns>
        private bool FilterLicenceSpain(string replacement)
        {
            var result = false;
            int countInterger = 0;
            int countChar = 0;
            var charList = replacement.ToCharArray();
            foreach (var character in charList)
            {
                try
                {
                    int.Parse(character.ToString());
                    countInterger++;
                }
                catch (Exception)
                {
                    countChar++;
                }
            }

            if (countInterger >= 3 && countChar >= 2)
            {
                result = true;
            }

            return result;
        }

        private void ShowResults(IInputOutputArray image, Stopwatch watch, List<IInputOutputArray> licensePlateImagesList, List<IInputOutputArray> filteredLicensePlateImagesList, List<RotatedRect> licenseBoxList, List<string> words, int count)
        {
            watch.Stop(); //stop the timer
            processTimeLabel.Text = String.Format("License Plate Recognition time: {0} milli-seconds \nIteration number = {1}", watch.Elapsed.TotalMilliseconds, count);

            panel1.Controls.Clear();
            Point startPoint = new Point(10, 10);
            for (int i = 0; i < words.Count; i++)
            {
                Mat dest = new Mat();
                CvInvoke.VConcat(licensePlateImagesList[i], filteredLicensePlateImagesList[i], dest);
                AddLabelAndImage(
                   ref startPoint,
                   String.Format("License: {0}", words[i]),
                   dest);
                PointF[] verticesF = licenseBoxList[i].GetVertices();
                Point[] vertices = Array.ConvertAll(verticesF, Point.Round);
                using (VectorOfPoint pts = new VectorOfPoint(vertices))
                    CvInvoke.Polylines(image, pts, true, new Bgr(Color.Red).MCvScalar, 2);
            }
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
            UMat uImg = img.GetUMat(AccessType.ReadWrite);
            ProcessImageMethod(uImg, 1);
        }

        private void ProcessImageMethod(UMat uImg, int ocr_Method)
        {
            for (int count = 1; count <= 5; count++)
            {
                if (ProcessImage(uImg, ocr_Method, count))
                {
                    break;
                }
            }
        }

        private void GoogleBtn_Click(object sender, EventArgs e)
        {
            UMat uImg = img.GetUMat(AccessType.ReadWrite);
            ProcessImageMethod(uImg, 2);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            img = CvInvoke.Imread(openFileDialog1.FileName);
            imageBox1.Image = img;
        }

        private void CVisionButton_Click(object sender, EventArgs e)
        {
            UMat uImg = img.GetUMat(AccessType.ReadWrite);
            ProcessImageMethod(uImg, 3);
        }
    }
}