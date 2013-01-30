﻿//
// PixelateEffect.cs
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

using Softbuild.Media.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Softbuild.Media.Effects
{
    public class PixelateEffect : IEffect
    {
        private Rect Block;
        private int BlockSize { set; get; }

        public PixelateEffect(Rect blockRect, int blockSize)
        {
            Block = blockRect;
            BlockSize = blockSize;
        }

        public PixelateEffect(int x, int y, int width, int height, int blockSize)
            : this(new Rect(x, y, width, height), blockSize)
        {
        }

        private void SetPixel(int width, int height, ref byte[] source, int x, int y, byte[] pixel)
        {
            int index = x * 4 + y * (width * 4);

            source[index] = pixel[0];
            source[index + 1] = pixel[1];
            source[index + 2] = pixel[2];
            source[index + 3] = pixel[3];
        }

        private byte[] GetPixel(int width, int height, byte[] source, int x, int y)
        {
            byte[] pixel = new byte[4];

            int index = x * 4 + y * (width * 4);
            pixel[0] = source[index];
            pixel[1] = source[index + 1];
            pixel[2] = source[index + 2];
            pixel[3] = source[index + 3];

            return pixel;
        }

        // これmaxとmin逆のほうがよくね……？
        private double Clamp(double value, double max, double min)
        {
            if (value < min)
            {
                return min;
            }
            if (max < value)
            {
                return max;
            }
            if (min <= value && value <= max)
            {
                return value;
            }

            throw new Exception();
        }

        private byte[] GetFillPixel(int MinX, int MaxX, int MinY, int MaxY, byte[] source, int width, int height)
        {
            // モザイクの中身を埋めるピクセルを取得する
            // アルゴリズムは範囲内の全ピクセルの平均値
            byte[] pixel = new byte[4];

            int RValue = 0;
            int GValue = 0;
            int BValue = 0;

            int count = 0;
            for (int x = MinX; x < MaxX; ++x)
            {
                for (int y = MinY; y < MaxY; ++y)
                {
                    var Pixel = GetPixel(width, height, source, x, y);
                    BValue += Pixel[0];
                    GValue += Pixel[1];
                    RValue += Pixel[2];
                    count++;
                }
            }

            pixel[0] = (byte)(BValue / count);
            pixel[1] = (byte)(GValue / count);
            pixel[2] = (byte)(RValue / count);
            pixel[3] = 255;

            return pixel;
        }

        private byte[] GetFillPixelLight(int MinX, int MaxX, int MinY, int MaxY, byte[] source, int width, int height)
        {
            // モザイクの中身を埋めるピクセルを取得する
            // アルゴリズムは範囲内の四隅の平均値

            byte[] pixel = new byte[4];

            int RValue = 0;
            int GValue = 0;
            int BValue = 0;

            int count = 0;

            {
                var Pixel = GetPixel(width, height, source, MinX, MinY);
                BValue += Pixel[0];
                GValue += Pixel[1];
                RValue += Pixel[2];
            }
            {
                var Pixel = GetPixel(width, height, source, MaxX-1, MinY);
                BValue += Pixel[0];
                GValue += Pixel[1];
                RValue += Pixel[2];
            }
            {
                var Pixel = GetPixel(width, height, source, MinX, MaxY-1);
                BValue += Pixel[0];
                GValue += Pixel[1];
                RValue += Pixel[2];
            }
            {
                var Pixel = GetPixel(width, height, source, MaxX-1, MaxY-1);
                BValue += Pixel[0];
                GValue += Pixel[1];
                RValue += Pixel[2];
            }

            pixel[0] = (byte)(BValue / 4);
            pixel[1] = (byte)(GValue / 4);
            pixel[2] = (byte)(RValue / 4);
            pixel[3] = 255;

            return pixel;
        }

        public byte[] Effect(int width, int height, byte[] source)
        {
            // ピクセルデータの数を計算する
            int pixelCount = width * height * 4;

            var dest = new byte[pixelCount];
            Array.Copy(source, dest, pixelCount);

            int XLimit = (int)Clamp(Block.X + Block.Width, width, 0);
            int YLimit = (int)Clamp(Block.Y + Block.Height, height, 0);

            for (int x = 0; x < Block.Width; x += BlockSize)
            {
                int MinX = (int)Clamp(x, XLimit, 0);
                int MaxX = (int)Clamp(x + BlockSize, XLimit, 0);

                for (int y = 0; y < Block.Height; y += BlockSize)
                {
                    int MinY = (int)Clamp(y, YLimit, 0);
                    int MaxY = (int)Clamp(y + BlockSize, YLimit, 0);

                    var FillPixel = GetFillPixel(MinX, MaxX, MinY, MaxY, source, width, height);

                    // 軽量版
                    //var FillPixel = GetFillPixelLight(MinX, MaxX, MinY, MaxY, source, width, height);

                    for (int x2 = MinX; x2 < MaxX; ++x2)
                    {
                        for (int y2 = MinY; y2 < MaxY; ++y2)
                        {
                            SetPixel(width, height, ref dest, x2, y2, FillPixel);
                        }
                    }
                }
            }

            return dest;
        }
    }
}
