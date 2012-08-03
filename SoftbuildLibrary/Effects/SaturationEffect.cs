﻿//
// SaturationEffect.cs
//
// Copyright (c) 2012 Kenji Wada, http://ch3cooh.jp/
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files 
// (the "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Softbuild.Media.Effects
{
    public class SaturationEffect : IEffect
    {
        private double Saturation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="saturation">濃さを表現する(0.0～1.0 標準:0.5)</param>
        public SaturationEffect(double saturation)
        {
            Saturation = saturation * 2;
        }

        public byte[] Effect(int width, int height, byte[] source)
        {
            int pixelCount = width * height;
            var dest = new byte[source.Length];

            for (int i = 0; i < pixelCount; i++)
            {
                var index = i * 4;

                double b = source[index + 0];
                double g = source[index + 1];
                double r = source[index + 2];
                double a = source[index + 3];

                // 単純平均法で輝度を求める
                double y = (b + g + r) / 3;

                b = y + Saturation * (b - y);
                g = y + Saturation * (g - y);
                r = y + Saturation * (r - y);

                dest[index + 0] = (byte)Math.Min(255, Math.Max(0, b));
                dest[index + 1] = (byte)Math.Min(255, Math.Max(0, g));
                dest[index + 2] = (byte)Math.Min(255, Math.Max(0, r));
                dest[index + 3] = source[index + 3];
            }

            return dest;
        }
    }
}
