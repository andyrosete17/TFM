﻿//----------------------------------------------------------------------------
//  Copyright (C) 2004-2018 by EMGU Corporation. All rights reserved.       
//----------------------------------------------------------------------------

#if NETFX_CORE || NETSTANDARD1_4
namespace System
{
   public delegate TOutput Converter<TInput, TOutput>(TInput input);
}

namespace Emgu.CV
{
   public static class Extensions
   {
      public static TOutput[] ConvertAll<TInput, TOutput>(TInput[] array, System.Converter<TInput, TOutput> converter)
      {
         TOutput[] result = new TOutput[array.Length];
         for (int i = 0; i < result.Length; i++)
         {
            result[i] = converter(array[i]);
         }
         return result;
      }
   }
}
#endif