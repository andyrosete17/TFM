//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Emgu.RPC
{
   ///<summary>
   ///A simple implementation of the IDuplexCaptureCallback interface
   ///</summary>
   public class DuplexCaptureCallback : IDuplexCaptureCallback
   {
      private Image<Bgr, Byte> _img;

      ///<summary>
      ///Get the image that has been captured.
      ///</summary>
      public Image<Bgr, Byte> CapturedImage { get { return _img; } }

      ///<summary>
      ///Function to call when an image is received.
      ///</summary>
      public virtual void ReceiveFrame(Image<Bgr, Byte> img)
      {
         if (_img != null) _img.Dispose(); //release the old frame if there is any
         _img = img;
         onFrameReceived(this, new EventArgs());
      }

      ///<summary>
      ///Handler events when image is received.
      ///</summary>
      public event EventHandler onFrameReceived;

   }
}
