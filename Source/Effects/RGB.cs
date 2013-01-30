﻿//
// RGB.cs
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
    /// ピクセルデータをRGB色空間で表したクラス
    /// </summary>
    public class RGB
    {
        /// <summary>
        /// RGB クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="r">赤成分</param>
        /// <param name="g">緑成分</param>
        /// <param name="b">青成分</param>
        public RGB(byte r, byte g, byte b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }

        /// <summary>
        /// RGB クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="r">赤成分</param>
        /// <param name="g">緑成分</param>
        /// <param name="b">青成分</param>
        public RGB(double r, double g, double b)
        {
            Red = (byte)r;
            Green = (byte)g;
            Blue = (byte)b;
        }

        /// <summary>
        /// 赤成分
        /// </summary>
        public byte Red { get; set; }

        /// <summary>
        /// 緑成分
        /// </summary>
        public byte Green { get; set; }

        /// <summary>
        /// 青成分
        /// </summary>
        public byte Blue { get; set; }
    }
}
