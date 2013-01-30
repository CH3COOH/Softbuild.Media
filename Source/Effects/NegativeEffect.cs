﻿//
// NegativeEffect.cs
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
    /// ネガティブ処理をおこなうクラス
    /// </summary>
    public class NegativeEffect : IEffect
    {
        /// <summary>
        /// ネガティブ処理をおこなう
        /// </summary>
        /// <param name="width">ビットマップの幅</param>
        /// <param name="height">ビットマップの高さ</param>
        /// <param name="source">処理前のピクセルデータ</param>
        /// <returns>処理後のピクセルデータ</returns>
        public byte[] Effect(int width, int height, byte[] source)
        {
            // ピクセルデータの数を計算する
            int pixelCount = width * height;

            // 処理後のピクセルデータを格納するためのバッファを生成する
            var dest = new byte[source.Length];

            for (int i = 0; i < pixelCount; i++)
            {
                var index = i * 4;

                // 処理前のピクセルの各ARGB要素を取得し反転する
                var b = 255 - source[index + 0];
                var g = 255 - source[index + 1];
                var r = 255 - source[index + 2];
                var a = source[index + 3];

                // 処理後のバッファへピクセル情報を保存する
                dest[index + 0] = (byte)Math.Min(255, Math.Max(0, b));
                dest[index + 1] = (byte)Math.Min(255, Math.Max(0, g));
                dest[index + 2] = (byte)Math.Min(255, Math.Max(0, r));
                dest[index + 3] = a;
            }

            return dest;
        }
    }
}
