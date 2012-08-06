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
    /// </summary>
    /// <remarks>
    /// 元ネタ：http://blogs.msdn.com/b/shintak/archive/2012/06/14/10319480.aspx
    /// </remarks>
    public class BakumatsuEffect : IEffect
    {
        /// <summary>
        /// マスク画像
        /// </summary>
        private WriteableBitmap MaskBitamp { get; set; }

        /// <summary>
        /// BakumatsuEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="maskBitamp">加算合成するビットマップ(古紙のビットマップを推奨)</param>
        public BakumatsuEffect(WriteableBitmap maskBitamp)
        {
            MaskBitamp = maskBitamp;
        }

        /// <summary>
        /// 幕末風処理をおこなう
        /// </summary>
        /// <param name="width">ビットマップの幅</param>
        /// <param name="height">ビットマップの高さ</param>
        /// <param name="source">処理前のピクセルデータ</param>
        /// <returns>処理後のピクセルデータ</returns>
        public byte[] Effect(int width, int height, byte[] source)
        {
            // マスク画像のピクセルデータを取得する
            var mask = MaskBitamp.PixelBuffer.ToArray();

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

                // 単純平均法で輝度を求める
                var y =(double)(r + g + b) / 3;

                // ハイコントラストの計算
                if (y > 170) y = 255;
                else if (y < 85) y = 0;
                else y = (y - 85) * 3;

                // マスク画像を透明度80%で被せる
                var db = y + mask[index + 0] * 0.8;
                var dg = y + mask[index + 1] * 0.8;
                var dr = y + mask[index + 2] * 0.8;

                // 処理後のピクセルの各ARGB要素を取得する
                dest[index + 0] = (byte)Math.Min(255, Math.Max(0, db));
                dest[index + 1] = (byte)Math.Min(255, Math.Max(0, dg));
                dest[index + 2] = (byte)Math.Min(255, Math.Max(0, dr));
                dest[index + 3] = a;
            }

            return dest;
        }
    }
}
