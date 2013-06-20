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
    /// <summary>
    /// コントラスト調整処理をおこなうクラス
    /// </summary>
    public class ContrastEffect : IEffect
    {
        /// <summary>
        /// コントラスト値をベースに事前に計算した変換テーブル
        /// </summary>
        private byte[] ContrastTable { get; set; }
        
        /// <summary>
        /// 調整するコントラスト値
        /// </summary>
        private double Contrast { get; set; }

        /// <summary>
        /// ContrastEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="contrast">コントラスト値を表現する(0.0～1.0 標準:0.5)</param>
        public ContrastEffect(double contrast)
        {
            double contrastValue = contrast * 2;

            Contrast = contrastValue;

            // コントラストの変換テーブルを作成する
            ContrastTable = new byte[256];
            for (int i = 0; i < 256; i++)
            {

                double value2 = 0.0;
                value2 = (double)i / 255.0;
                value2 -= 0.5;
                value2 *= Contrast;
                value2 += 0.5;
                value2 *= 255;

                ContrastTable[i] = (byte)Math.Min(255, Math.Max(0, value2));
            }
        }

        /// <summary>
        /// コントラスト調整処理をおこなう
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
                var b = source[index + 0];
                var g = source[index + 1];
                var r = source[index + 2];
                var a = source[index + 3];

                // 変換テーブルでコントラストを調整する
                b = ContrastTable[b];
                g = ContrastTable[g];
                r = ContrastTable[r];

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
