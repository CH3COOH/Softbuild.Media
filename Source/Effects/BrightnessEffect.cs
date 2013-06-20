﻿//
// BrightnessEffect.cs
//
// Copyright (c) 2013 Kenji Wada, http://ch3cooh.jp/
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
    public class BrightnessEffect : IEffect
    {

        /// <summary>
        /// ブライトネス値をベースに事前に計算した変換テーブル
        /// </summary>
        private byte[] BrightnessTable { get; set; }
        
        /// <summary>
        /// 調整するブライトネス値
        /// </summary>
        private double Brightness { get; set; }

        /// <summary>
        /// BrightnessEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="brightness">ブライトネス値を表現する(0.0～1.0 標準:0.5)</param>
        public BrightnessEffect(double brightness)
        {
            Brightness = brightness * 2;

            // ブライトネス変換テーブルを作成する
            BrightnessTable = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                double value = ((double)i - 0.5) * Brightness + 0.5;
                BrightnessTable[i] = (byte)Math.Min(255, Math.Max(0, value));
            }
        }

        public byte[] Effect(int width, int height, byte[] source)
        {
            int pixelCount = width * height;
            var dest = new byte[source.Length];

            for (int i = 0; i < pixelCount; i++)
            {
                var index = i * 4;

                // 処理前のピクセルの各ARGB要素を取得する
                var b = source[index + 0];
                var g = source[index + 1];
                var r = source[index + 2];
                var a = source[index + 3];

                // 変換テーブルでブライトネス調整する
                b = BrightnessTable[b];
                g = BrightnessTable[g];
                r = BrightnessTable[r];

                // 処理後のバッファへピクセル情報を保存する
                dest[index + 0] = b;
                dest[index + 1] = g;
                dest[index + 2] = r;
                dest[index + 3] = a;
            }

            return dest;
        }
    }
}
