﻿//
// WriteableBitmapExtensions.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;

#if WINDOWS_STORE_APPS
using Softbuild.Data;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
#elif WINDOWS_PHONE
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
#endif

namespace Softbuild.Media
{
    /// <summary>
    /// 画像の拡張メソッド
    /// </summary>
    public static class WriteableBitmapCoreExtensions
    {

        public static Size GetSize(this WriteableBitmap bmp)
        {
            var size = new Size();
            size.Width = bmp.PixelWidth;
            size.Height = bmp.PixelHeight;
            return size;
        }

        public static Rect GetRect(this WriteableBitmap bmp)
        {
            var rect = new Rect();
            rect.X = 0;
            rect.Y = 0;
            rect.Width = bmp.PixelWidth;
            rect.Height = bmp.PixelHeight;
            return rect;
        }

        /// <summary>
        /// ビットマップの2-Dテクスチャを表す配列を取得します
        /// </summary>
        /// <param name="bmp">WriteableBitmapオブジェクト</param>
        /// <returns>ピクセルデータ</returns>
        public static byte[] GetPixels(this WriteableBitmap bmp)
        {
            var bytes = default(byte[]);
#if WINDOWS_STORE_APPS
            bytes = bmp.PixelBuffer.ToArray();
#elif WINDOWS_PHONE
            bytes = new byte[bmp.Pixels.Length * 4];

            int max = bmp.Pixels.Length;
            for (int i = 0; i < max; i++)
            {
                int pixel = bmp.Pixels[i];

                int index = i * 4;
                bytes[index + 0] = (byte)(pixel & 0xff);
                bytes[index + 1] = (byte)((pixel >> 8) & 0xff); 
                bytes[index + 2] = (byte)((pixel >> 16) & 0xff);
                bytes[index + 3] = (byte)((pixel >> 24) & 0xff);
            }

            //bmp.Pixels.CopyTo(bytes, 0);
#endif
            return bytes;
        }


        
        /// <summary>
        /// 指定した複数の矩形を指定した色で塗りつぶす
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="drawRects"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static WriteableBitmap DrawRect(this WriteableBitmap bmp, IEnumerable<Rect> drawRects, Color color)
        {
            var pixels = bmp.GetPixels();

            foreach (var drawRect in drawRects)
            {
                for (int x = (int)drawRect.X; x < drawRect.X + drawRect.Width; x++)
                {
                    for (int y = (int)drawRect.Y; y < drawRect.Y + drawRect.Height; y++)
                    {
                        int index = x * 4 + y * (bmp.PixelWidth * 4);

                        pixels[index] = color.B;
                        pixels[index + 1] = color.G;
                        pixels[index + 2] = color.R;
                        pixels[index + 3] = color.A;
                    }
                }
            }

            return WriteableBitmapLoadExtensions.FromArray(bmp.PixelWidth, bmp.PixelHeight, pixels);
        }

        /// <summary>
        /// 指定した矩形を指定した色で塗りつぶす
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="drawRect"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static WriteableBitmap DrawRect(this WriteableBitmap bmp, Rect drawRect, Color color)
        {
            return DrawRect(bmp, new[] { drawRect }, color);
        }

        /// <summary>
        /// 画像比率を維持したまま指定されたサイズに収まる最大画像サイズを計算する
        /// </summary>
        /// <param name="srcWidth">元画像の幅</param>
        /// <param name="srcHeight">元画像の高さ</param>
        /// <param name="dstWidth">出力画像の幅</param>
        /// <param name="dstHeight">出力画像の高さ</param>
        /// <returns>画像比率が維持された状態</returns>
        internal static Size GetAspectRatio(double srcWidth, double srcHeight, double dstWidth, double dstHeight)
        {
            if ((srcWidth == dstWidth) && (srcHeight == dstHeight))
            {
                return new Size(srcWidth, srcHeight);
            }

            // 幅を1として考えた場合、高さから見た幅の比率
            var srcRatio = srcWidth / srcHeight;
            var dstRatio = dstWidth / dstHeight;

            double width, height;
            if (srcRatio < dstRatio)
            {
                height = Math.Round(dstHeight);
                width = Math.Round(dstHeight * srcRatio);
            }
            else
            {
                height = Math.Round(dstWidth * srcRatio);
                width = Math.Round(dstWidth);
            }
            return new Size(width, height);
        }

        /// <summary>
        /// リサイズする
        /// </summary>
        /// <param name="bmp">WriteableBitmapオブジェクト</param>
        /// <param name="destWidth">変形後の幅</param>
        /// <param name="destHeight">変形後の高さ</param>
        /// <returns>リサイズ後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap Resize(this WriteableBitmap bmp, int destWidth, int destHeight)
        {
            // 加工前のWriteableBitmapオブジェクトからピクセルデータ等を取得する
            var srcWidth = bmp.PixelWidth;
            var srcHeight = bmp.PixelHeight;
            if ((srcWidth == destWidth) && (srcHeight == destHeight))
            {
                // リサイズする必要がないのでそのままビットマップを返す
                return bmp;
            }

            var srcPixels = bmp.GetPixels();
            int pixelCount = destWidth * destHeight;
            var destPixels = new byte[4 * pixelCount];

            var xs = (float)srcWidth / destWidth;
            var ys = (float)srcHeight / destHeight;

            for (var y = 0; y < destHeight; y++)
                for (var x = 0; x < destWidth; x++)
                {
                    var index = (y * destWidth + x) * 4;

                    var sx = x * xs;
                    var sy = y * ys;
                    var x0 = (int)sx;
                    var y0 = (int)sy;

                    var srcIndex = (y0 * srcWidth + x0) * 4;

                    destPixels[index + 0] = srcPixels[srcIndex + 0];
                    destPixels[index + 1] = srcPixels[srcIndex + 1];
                    destPixels[index + 2] = srcPixels[srcIndex + 2];
                    destPixels[index + 3] = srcPixels[srcIndex + 3];
                }

            // ピクセルデータからWriteableBitmapオブジェクトを生成する
            return WriteableBitmapLoadExtensions.FromArray(destWidth, destHeight, destPixels);
        }

#if WINDOWS_STORE_APPS



#elif WINDOWS_PHONE

        /// <summary>
        /// 画像をJPEGフォーマットでピクチャーライブラリへ保存する
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <returns>無し</returns>
        public static void SaveToCameraRoll(this WriteableBitmap bmp, string fileNameWithoutExtension)
        {
           MemoryStream memStrm = new MemoryStream();
           bmp.SaveJpeg(memStrm, bmp.PixelWidth, bmp.PixelHeight, 0, 90);
           Softbuild.Data.StorageExtensions.SaveToCameraRoll(memStrm, fileNameWithoutExtension + ".jpg");
           memStrm.Close();
        }

        /// <summary>
        /// 画像をJPEGフォーマットでピクチャーライブラリへ保存する
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <returns>無し</returns>
        public static void Save(this WriteableBitmap bmp, string fileNameWithoutExtension)
        {
            MemoryStream memStrm = new MemoryStream();
            bmp.SaveJpeg(memStrm, bmp.PixelWidth, bmp.PixelHeight, 0, 90);
            Softbuild.Data.StorageExtensions.SaveToPicturesLibrary(memStrm, fileNameWithoutExtension + ".jpg");
            memStrm.Close();
        }

#endif
    }
}
