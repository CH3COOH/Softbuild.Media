﻿//
// BakumatsuEffect.cs
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
    /// <summary>
    /// 幕末写真風エフェクト処理をおこなうクラス
    /// 元ネタ：http://blogs.msdn.com/b/shintak/archive/2012/06/14/10319480.aspx
    /// </summary>
    public class BakumatsuEffect : IEffect
    {
        private WriteableBitmap MaskBitamp { get; set; }

        public BakumatsuEffect(WriteableBitmap maskBitamp)
        {
            MaskBitamp = maskBitamp;
        }

        public byte[] Effect(int width, int height, byte[] source)
        {
            var mask = MaskBitamp.PixelBuffer.ToArray();

            int pixelCount = width * height;
            var dest = new byte[source.Length];

            for (int i = 0; i < pixelCount; i++)
            {
                var index = i * 4;

                // 単純平均法で輝度を求める
                var sum = source[index + 0] + source[index + 1] + source[index + 2];
                var y = (double)sum / 3;

                // ハイコントラストの計算
                if (y > 170) y = 255;
                else if (y < 85) y = 0;
                else y = (uint)((y - 85) * 3);

                // マスク画像を透明度80%で被せる
                var b = y + mask[index + 0] * 0.8;
                var g = y + mask[index + 1] * 0.8;
                var r = y + mask[index + 2] * 0.8;

                dest[index + 0] = (byte)Math.Min(255, Math.Max(0, b));
                dest[index + 1] = (byte)Math.Min(255, Math.Max(0, g));
                dest[index + 2] = (byte)Math.Min(255, Math.Max(0, r));
                dest[index + 3] = source[index + 3];
            }

            return dest;
        }
    }
}
