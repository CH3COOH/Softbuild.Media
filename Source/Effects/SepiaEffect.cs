﻿//
// SepiaEffect.cs
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
    /// セピア調変換処理をおこなうクラス
    /// </summary>
    public class SepiaEffect : IEffect
    {
        /// <summary>
        /// セピア調変換処理をおこなう
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
#if !IOS
                var b = source[index + 0];
                var g = source[index + 1];
                var r = source[index + 2];
                var a = source[index + 3];
#else
                var r = source[index + 0];
                var g = source[index + 1];
                var b = source[index + 2];
                var a = source[index + 3];
#endif

                // 単純平均法で輝度を求める
                var y = (double)(r + g + b) / 3;

                // 輝度に対して指定の比率を掛けてセピア調に変換する
                var db = y * 0.4;
                var dg = y * 0.7;
                var dr = y;

#if !IOS
                // 処理後のバッファへピクセル情報を保存する
                dest[index + 0] = (byte)Math.Min(255, Math.Max(0, db));
                dest[index + 1] = (byte)Math.Min(255, Math.Max(0, dg));
                dest[index + 2] = (byte)Math.Min(255, Math.Max(0, dr));
                dest[index + 3] = a;
#else
                dest[index + 0] = (byte)Math.Min(255, Math.Max(0, dr));
                dest[index + 1] = (byte)Math.Min(255, Math.Max(0, dg));
                dest[index + 2] = (byte)Math.Min(255, Math.Max(0, db));
                dest[index + 3] = a;
#endif
            }

            return dest;
        }
    }
}
