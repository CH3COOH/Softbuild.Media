﻿//
// HSV.cs
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
    /// ピクセルデータをHSV色空間で表したクラス
    /// </summary>
    public class HSV
    {
        /// <summary>
        /// HSV クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="h">色相</param>
        /// <param name="s">彩度</param>
        /// <param name="v">明度</param>
        public HSV(double h, double s, double v)
        {
            Hue = h;
            Saturation = s;
            Value = v;
        }

        /// <summary>
        /// HSV クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="hsv">HSV各要素の値が格納されたdouble配列</param>
        public HSV(double[] hsv)
        {
            Hue = hsv[0];
            Saturation = hsv[1];
            Value = hsv[2];
        }

        /// <summary>
        /// 色相
        /// </summary>
        public double Hue { get; set; }

        /// <summary>
        /// 彩度
        /// </summary>
        public double Saturation { get; set; }

        /// <summary>
        /// 明度
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// RGB値を元に HSV クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="rgb">RGBオブジェクト</param>
        /// <returns>HSVオブジェクト</returns>
        public static HSV FromRGB(RGB rgb)
        {
            return FromRGB(rgb.Red, rgb.Green, rgb.Blue);
        }

        /// <summary>
        /// RGB値を元に HSV クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="r">R要素の値</param>
        /// <param name="g">G要素の値</param>
        /// <param name="b">B要素の値</param>
        /// <returns>HSVオブジェクト</returns>
        public static HSV FromRGB(double r, double g, double b)
        {
            // R、GおよびBが0.0を最小量、1.0を最大値とする0.0から1.0の範囲にある
            r /= 255;
            g /= 255;
            b /= 255;

            var max = Math.Max(Math.Max(r, g), b);
            var min = Math.Min(Math.Min(r, g), b);
            var sub = max - min;

            double h = 0, s = 0, v = 0;

            // Calculate Hue
            if (sub == 0)
            {
                // MAX = MIN(例・S = 0)のとき、 Hは定義されない。
                h = 0;
            }
            else
            {
                if (max == r)
                {
                    h = (60 * (g - b) / sub) + 0;
                }
                else if (max == g)
                {
                    h = (60 * (b - r) / sub) + 120;
                }
                else if (max == b)
                {
                    h = (60 * (r - g) / sub) + 240;
                }

                // さらに H += 360 if H < 0
                if (h < 0)
                {
                    h += 360;
                }
            }

            // Calculate Saturation
            if (max > 0)
            {
                s = sub / max;
            }

            // Calculate Value
            v = max;

            return new HSV(h, s, v);
        }

        /// <summary>
        /// RGBへ変換する
        /// </summary>
        /// <returns>RGBオブジェクト</returns>
        public RGB ToRGB()
        {
            // まず、もしSが0.0と等しいなら、最終的な色は無色もしくは灰色である。
            if (Saturation == 0)
            {
                return new RGB(Value * 255, Value * 255, Value * 255);
            }

            double r = 0, g = 0, b = 0;
            double f = 0;
            double p = 0, q = 0, t = 0;

            //var h = Math.Min(360.0, Math.Max(0, Hue));
            // 角座標系で、Hの範囲は0から360までであるが、その範囲を超えるHは360.0で
            // 割った剰余（またはモジュラ演算）でこの範囲に対応させることができる。
            // たとえば-30は330と等しく、480は120と等しくなる。
            var h = Hue % 360;
            var s = Math.Min(1.0, Math.Max(0, Saturation));
            var v = Math.Min(1.0, Math.Max(0, Value));

            var hi = (int)(h / 60);
            f = (h / 60) - hi;
            p = v * (1 - s);
            q = v * (1 - f * s);
            t = v * (1 - (1 - f) * s);

            if (hi == 0)
            {
                r = v; g = t; b = p;
            }
            else if (hi == 1)
            {
                r = q; g = v; b = p;
            }
            else if (hi == 2)
            {
                r = p; g = v; b = t;
            }
            else if (hi == 3)
            {
                r = p; g = q; b = v;
            }
            else if (hi == 4)
            {
                r = t; g = p; b = v;
            }
            else if (hi == 5)
            {
                r = v; g = p; b = q;
            }

            return new RGB(r * 255, g * 255, b * 255);
        }
    }
}
