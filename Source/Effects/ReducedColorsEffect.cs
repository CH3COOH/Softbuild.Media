﻿//
// ReducedColorsEffect.cs
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
    /// <summary>
    /// 減色処理をおこなうクラス
    /// </summary>
    public class ReducedColorsEffect : IEffect
    {
        /// <summary>
        /// 変換テーブル
        /// </summary>
        private byte[] Table { get; set; }

        /// <summary>
        /// 減色する色数
        /// </summary>
        private byte Level { get; set; }

        /// <summary>
        /// ReducedColorsEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="level">ポスタライズレベルの値(0～255 標準:1)</param>
        public ReducedColorsEffect(byte level)
        {
            Level = level;

            // 変換テーブルを作成する
            Table = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                var baseValue = 256.0 / level;
                var value = Math.Round((double)i / baseValue);
                Table[i] = (byte)Math.Min(255, Math.Max(0, value * baseValue));
            }
        }

        /// <summary>
        /// 減色処理をおこなう
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

                // 変換テーブルでRGB要素ごとに値を変換する
                b = Table[b];
                g = Table[g];
                r = Table[r];

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
