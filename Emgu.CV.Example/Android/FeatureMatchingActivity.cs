﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using System.IO;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using FeatureMatchingExample;

namespace AndroidExamples
{
   [Activity(Label = "Feature Matching")]
   public class FeatureMatchingActivity : ButtonMessageImageActivity
   {
      public FeatureMatchingActivity()
         : base("Feature Matching")
      {
      }

      protected override void OnCreate(Bundle bundle)
      {
         base.OnCreate(bundle);

         AppPreference preference = new AppPreference();


         OnButtonClick += delegate
         {
            CvInvoke.UseOpenCL = preference.UseOpenCL;
            String oclDeviceName = preference.OpenClDeviceName;
            if (!String.IsNullOrEmpty(oclDeviceName))
            {
               CvInvoke.OclSetDefaultDevice(oclDeviceName);
            }

            long time;
            using (Mat box = new Mat(Assets, "box.png"))
            using (Mat boxInScene = new Mat(Assets, "box_in_scene.png"))
            using (Mat result = DrawMatches.Draw(box, boxInScene, out time))
            {
               SetImageBitmap(result.ToBitmap(Bitmap.Config.Rgb565));

               String computeDevice = CvInvoke.UseOpenCL ? "OpenCL: " + Emgu.CV.Ocl.Device.Default.Name : "CPU";
               SetMessage(String.Format("Matched with '{0}' in {1} milliseconds.", computeDevice, time));
            }
         };
      }
   }
}

