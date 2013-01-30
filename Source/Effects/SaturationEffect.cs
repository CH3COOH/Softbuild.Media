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
    /// <summary>
    /// 彩度調整処理をおこなうクラス
    /// </summary>
    public class SaturationEffect : IEffect
    {
        /// <summary>
        /// 彩度の調整値
        /// </summary>
        private double Saturation { get; set; }

        /// <summary>
        /// SaturationEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="saturation">彩度を表現する(0.0～1.0 標準:0.5)</param>
        public SaturationEffect(double saturation)
        {
            Saturation = saturation * 2;
        }

        /// <summary>
        /// 彩度調整処理をおこなう
        /// </summary>
        /// <param name="width">ビットマップの幅</param>
        /// <param name="height">ビットマップの高さ</param>
        /// <param name="source">処理前のピクセルデータ</param>
        /// <returns>処理後のピクセルデータ</returns>
        public byte[] Effect(int width, int height, byte[] source)
        {
            int pixelCount = width * height;
            var dest = new byte[source.Length];

            for (int i = 0; i < pixelCount; i++)
            {
                var index = i * 4;

                // 処理前のピクセルの各ARGB要素を取得する
                double b = source[index + 0];
                double g = source[index + 1];
                double r = source[index + 2];
                double a = source[index + 3];

                // 色空間をRGBからHSVへ変換する
                var hsv = HSV.FromRGB(r, g, b);

                // 彩度の調整をおこなう
                hsv.Saturation *= Saturation;

                // 色空間をHSVからRGBへ変換する
                var rgb = hsv.ToRGB();
                int db = rgb.Blue;
                int dg = rgb.Green;
                int dr = rgb.Red;

                // 処理後のバッファへピクセル情報を保存する
                dest[index + 0] = (byte)Math.Min(255, Math.Max(0, db));
                dest[index + 1] = (byte)Math.Min(255, Math.Max(0, dg));
                dest[index + 2] = (byte)Math.Min(255, Math.Max(0, dr));
                dest[index + 3] = source[index + 3];
            }

            return dest;
        }
    }

}
