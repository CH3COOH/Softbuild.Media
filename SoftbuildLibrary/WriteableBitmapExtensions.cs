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
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Softbuild.Media
{
    public static class WriteableBitmapExtensions
    {
        /// <summary>
        /// IRandomAccessStreamからWriteableBitmapを生成する
        /// </summary>
        /// <param name="stream">ランダムアクセスストリーム</param>
        /// <returns>WriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> FromStreamAsync(IRandomAccessStream stream)
        {
            // ストリームからピクセルデータを読み込む
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var transform = new BitmapTransform();
            var pixelData = await decoder.GetPixelDataAsync(decoder.BitmapPixelFormat, decoder.BitmapAlphaMode,
                transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.ColorManageToSRgb);
            var pixels = pixelData.DetachPixelData();

            // ピクセルデータからWriteableBitmapオブジェクトを生成する
            return WriteableBitmapExtensions.FromArray((int)decoder.OrientedPixelWidth, (int)decoder.OrientedPixelHeight, pixels);
        }

        /// <summary>
        /// バイト配列からWriteableBitmapを生成する
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <param name="array">ピクセルデータ</param>
        /// <returns>WriteableBitmapオブジェクト</returns>
        public static WriteableBitmap FromArray(int width, int height, byte[] array)
        {
            // 出力用のWriteableBitmapオブジェクトを生成する
            var bitmap = new WriteableBitmap(width, height);
            // WriteableBitmapへバイト配列のピクセルデータをコピーする
            using (var pixelStream = bitmap.PixelBuffer.AsStream())
            {
                pixelStream.Seek(0, SeekOrigin.Begin);
                pixelStream.Write(array, 0, array.Length);
            }
            return bitmap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmp">WriteableBitmapオブジェクト</param>
        /// <param name="destWidth">変形後の幅</param>
        /// <param name="destHeight">変形後の高さ</param>
        /// <returns></returns>
        public static WriteableBitmap Resize(this WriteableBitmap bmp, int destWidth, int destHeight)
        {
            // 加工前のWriteableBitampオブジェクトからピクセルデータ等を取得する
            var srcWidth = bmp.PixelWidth;
            var srcHeight = bmp.PixelHeight;
            if ((srcWidth == destWidth) && (srcHeight == destHeight))
            {
                // リサイズする必要がないのでそのままビットマップを返す
                return bmp;
            }

            var srcPixels = bmp.PixelBuffer.ToArray();
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
            return WriteableBitmapExtensions.FromArray(destWidth, destHeight, destPixels);
        }
    }
}
