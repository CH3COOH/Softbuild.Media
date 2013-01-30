﻿//
// CartoonizeEffect.cs
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
    public class CartoonizeEffect : IEffect
    {
        /// <summary>
        /// 二値化をおこなう際の閾値
        /// </summary>
        private int Threshold { get; set; }

        private byte Stroke { get; set; }

        /// <summary>
        /// ThinningEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        public CartoonizeEffect()
            : this(50, 255)
        {
        }

        /// <summary>
        /// ThinningEffect クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="threshold"></param>
        /// <param name="stroke"></param>
        public CartoonizeEffect(int threshold, byte stroke)
        {
            //Threshold = threshold;
            //Stroke = stroke;

            Threshold = 50;
            Stroke = 255;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="source"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="pixel"></param>
        private void SetPixel(int width, int height, ref byte[] source, int x, int y, byte pixel)
        {
            int index = x * 4 + y * (width * 4);

            source[index] = pixel;
            source[index + 1] = pixel;
            source[index + 2] = pixel;
            source[index + 3] = 255;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="source"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private byte GetPixel(int width, int height, byte[] source, int x, int y)
        {
            int index = x * 4 + y * (width * 4);
            byte pixel = source[index];
            return pixel;
        }

        byte THRESHOLD_1 = 50;
        byte THRESHOLD_2 = 70;
        byte THRESHOLD_3 = 100;
        byte THRESHOLD_4 = 110;

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

            var toneMap = new byte[pixelCount];

            // まずは画像を閾値をThresholdで二値化する
            var temp = new byte[source.Length];
            for (int i = 0; i < pixelCount; i++)
            {
                var index = i * 4;

                // 処理前のピクセルから各BGAR要素を取得する
                var b = source[index + 0];
                var g = source[index + 1];
                var r = source[index + 2];
                var a = source[index + 3];

                // 輝度を求める
                byte y = ((byte)(r * 0.2126 + g * 0.7152 + b * 0.0722));
                // 二値化する
                byte y2 = (y < Threshold) ? byte.MinValue : byte.MaxValue; 

                // トーンを貼る用のマップを作成する
                byte tone = 0;
                if ((0 <= y) && (y < THRESHOLD_1))
                {
                    tone = 1;
                }
                else if ((THRESHOLD_1 <= y) && (y < THRESHOLD_2))
                {
                    tone = 2;
                }
                else if ((THRESHOLD_2 <= y) && (y < THRESHOLD_3))
                {
                    tone = 3;
                }
                else if ((THRESHOLD_3 <= y) && (y < THRESHOLD_4))
                {
                    //tone = 4;
                }
                toneMap[i] = tone;

                // 処理後のピクセルデータを出力用バッファへ格納する
                temp[index + 0] = y2;
                temp[index + 1] = y2;
                temp[index + 2] = y2;
                temp[index + 3] = a;
            }

            var dest = new byte[source.Length];
            var ia = new int[9];
            var ic = new int[9];

            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    // 処理前のピクセルから各BGAR要素を取得する
                    var gray = GetPixel(width, height, temp, x, y);
                    if (gray != byte.MaxValue) continue;

                    ia[0] = GetPixel(width, height, temp, x + 1, y);
                    ia[1] = GetPixel(width, height, temp, x + 1, y - 1);
                    ia[2] = GetPixel(width, height, temp, x, y - 1);
                    ia[3] = GetPixel(width, height, temp, x - 1, y - 1);
                    ia[4] = GetPixel(width, height, temp, x - 1, y);
                    ia[5] = GetPixel(width, height, temp, x - 1, y + 1);
                    ia[6] = GetPixel(width, height, temp, x, y + 1);
                    ia[7] = GetPixel(width, height, temp, x + 1, y + 1);

                    for (int i = 0; i < 8; i++)
                    {
                        if (ia[i] == Stroke)
                        {
                            ia[i] = byte.MaxValue;
                            ic[i] = 0;
                        }
                        else
                        {
                            if (ia[i] < byte.MaxValue)
                            {
                                ia[i] = 0;
                            }
                            ic[i] = ia[i];
                        }
                    }

                    ia[8] = ia[0];
                    ic[8] = ic[0];

                    if ((ia[0] + ia[2] + ia[4] + ia[6]) == byte.MaxValue * 4) continue;

                    int iv = 0, iw = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        if (ia[i] == byte.MaxValue) iv++;
                        if (ic[i] == byte.MaxValue) iw++;
                    }

                    if (iv <= 1) continue;
                    if (iw == 0) continue;

                    if (cconc(ia) != 1) continue;

                    var p = GetPixel(width, height, temp, x, y - 1);
                    if (p == Stroke)
                    {
                        ia[2] = 0;
                        if (cconc(ia) != 1) continue;
                        ia[2] = byte.MaxValue;
                    }
                    p = GetPixel(width, height, temp, x - 1, y);
                    if (p == Stroke)
                    {
                        ia[4] = 0;
                        if (cconc(ia) != 1) continue;
                        ia[4] = byte.MaxValue;
                    }
                    
                    // 処理後のピクセルデータを出力用バッファへ格納する
                    SetPixel(width, height, ref dest, x, y, Stroke);
                }
            }

            // トーン貼り付け処理
            for (int i = 0; i < pixelCount; i++)
            {
                int index = i * 4;

                byte ho = toneMap[i];
                switch (ho)
                {
                    case 0:
                        dest[index] = 255;
                        dest[index + 1] = 255;
                        dest[index + 2] = 255;
                        break;
                    case 1:
                        dest[index] = 0;
                        dest[index + 1] = 0;
                        dest[index + 2] = 0;
                        break;
                    case 2:
                        dest[index] = 70;
                        dest[index + 1] = 70;
                        dest[index + 2] = 70;
                        break;
                    case 3:
                        dest[index] = 140;
                        dest[index + 1] = 140;
                        dest[index + 2] = 140;
                        break;
                    case 4:
                        //dest[index] = 210;
                        //dest[index + 1] = 210;
                        //dest[index + 2] = 210;
                        break;
                }
                dest[index + 3] = 255;
            }

            return dest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inb"></param>
        /// <returns></returns>
        private int cconc(int[] inb)
        {          
            int icn = 0;

            for (int i = 0; i < 8; i += 2)
            {
                if ((inb[i] == 0) && 
                    (inb[i + 1] == byte.MaxValue || inb[i + 2] == byte.MaxValue))
                {
                    icn++;
                }
            }
            return icn;
        }
    }
}
