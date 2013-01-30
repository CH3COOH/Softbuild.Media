﻿//
// BinarizationEffect.cs
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

namespace Softbuild.Media.Effects
{
    /// <summary>
    /// 二値化をおこなうクラス
    /// </summary>
    public class BinarizationEffect : IEffect
    {
        /// <summary>
        /// 二値化をおこなう際の閾値
        /// </summary>
        private int Threshold { get; set; }

        /// <summary>
        /// コントラスト値をベースに事前に計算した変換テーブル
        /// </summary>
        private byte[] LookUpTable { get; set; }

        /// <summary>
        /// BinarizationEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        public BinarizationEffect()
            : this(85)
        {
        }

        /// <summary>
        /// BinarizationEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="threshold"></param>
        public BinarizationEffect(int threshold)
        {
            Threshold = threshold;

            LookUpTable = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                LookUpTable[i] = (byte)((i < threshold) ? 0 : 255);
            }
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
            int pixelCount = width * height;
            var dest = new byte[source.Length];

            // まずはグレースケール化する
            for (int i = 0; i < pixelCount; i++)
            {
                var index = i * 4;

                // 処理前のピクセルから各BGAR要素を取得する
                var b = source[index + 0];
                var g = source[index + 1];
                var r = source[index + 2];
                var a = source[index + 3];

                //// 単純平均法で輝度を
                int y = ((byte)(r * 0.2126 + g * 0.7152 + b * 0.0722));

                byte y2 = LookUpTable[y];

                // 処理後のピクセルデータを出力用バッファへ格納する
                dest[index + 0] = y2;
                dest[index + 1] = y2;
                dest[index + 2] = y2;
                dest[index + 3] = a;
            }

            return dest;
        }
    }
}
