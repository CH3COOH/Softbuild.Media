﻿//
// ConstrastEffect.cs
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
    public class ContrastEffect : IEffect
    {
        private byte[] ContrastTable { get; set; }
        private double Contrast { get; set; }

        public ContrastEffect(double contrast)
        {
            Contrast = contrast * 2;

            // コントラストの変換テーブルを作成する
            ContrastTable = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                double value = ((double)i - 0.5) * Contrast + 0.5;
                ContrastTable[i] = (byte)Math.Min(255, Math.Max(0, value));
            }
        }

        public byte[] Effect(int width, int height, byte[] source)
        {
            int pixelCount = width * height;
            var dest = new byte[source.Length];

            for (int i = 0; i < pixelCount; i++)
            {
                var index = i * 4;

                var b = source[index + 0];
                var g = source[index + 1];
                var r = source[index + 2];
                var a = source[index + 3];

                // 変換テーブル
                b = ContrastTable[b];
                g = ContrastTable[g];
                r = ContrastTable[r];

                dest[index + 0] = b;
                dest[index + 1] = g;
                dest[index + 2] = r;
                dest[index + 3] = a;
            }

            return dest;
        }
    }
}
