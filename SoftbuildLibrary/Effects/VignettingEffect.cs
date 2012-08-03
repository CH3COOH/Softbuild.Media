﻿//
// VignettingEffect.cs
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
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Media.Imaging;

namespace Softbuild.Media.Effects
{
    public class VignettingEffect : IEffect
    {
        private WriteableBitmap MaskBitamp { get; set; }
        private double Opacity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maskBitamp">周辺光を表現するマスク画像</param>
        /// <param name="opacity">濃さを表現する(0.0～1.0 不透明:1.0)</param>
        public VignettingEffect(WriteableBitmap maskBitamp, double opacity)
        {
            MaskBitamp = maskBitamp;
            Opacity = opacity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public byte[] Effect(int width, int height, byte[] source)
        {
            var mask = MaskBitamp.PixelBuffer.ToArray();

            int pixelCount = width * height;
            var dest = new byte[source.Length];

            for (int i = 0; i < pixelCount; i++)
            {
                var index = i * 4;
                
                // 元画像側のピクセル
                double b = source[index + 0];
                double g = source[index + 1];
                double r = source[index + 2];
                double a = source[index + 3];

                // マスク画像側のピクセル
                double mb = mask[index + 0];
                double mg = mask[index + 1];
                double mr = mask[index + 2];
                double ma = mask[index + 3];

                // 出力側のピクセル
                double db, dg, dr, da;

                // マスク画像に適用する透明率を算出する
                double ax = (ma / 255) * Opacity;

                // 指定値画像のピクセルのアルファ値をチェック
                if (ax == 0)
                {
                    // マスク画像が透明なので元画像のARGB値をそのまま代入
                    db = b;
                    dg = g;
                    dr = r;
                    da = a;
                }
                else
                {
                    // アルファ値を元に合成後のRGB値を算出
                    db = b * (1.0 - ax) + mb * ax;
                    dg = g * (1.0 - ax) + mg * ax;
                    dr = r * (1.0 - ax) + mr * ax;
                    da = a;
                }

                dest[index + 0] = (byte)Math.Min(255, Math.Max(0, db));
                dest[index + 1] = (byte)Math.Min(255, Math.Max(0, dg));
                dest[index + 2] = (byte)Math.Min(255, Math.Max(0, dr));
                dest[index + 3] = (byte)Math.Min(255, Math.Max(0, da));
            }

            return dest;
        }
    }
}
