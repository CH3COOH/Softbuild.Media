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
using Softbuild.Media.Effects;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Softbuild.Media
{
    static public class WriteableBitmapExtensions
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
            var srcPixels = bmp.PixelBuffer.ToArray();
            var srcWidth = bmp.PixelWidth;
            var srcHeight = bmp.PixelHeight;

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

        /// <summary>
        /// パラメータ無しの画像処理をおこなう
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="effecter"></param>
        /// <returns></returns>
        private static WriteableBitmap Effect(WriteableBitmap bmp, IEffect effecter)
        {
            // WriteableBitampのピクセルデータをバイト配列に変換する
            var srcPixels = bmp.PixelBuffer.ToArray();

            // パラメータ無しの画像処理をおこなう
            var dstPixels = effecter.Effect(bmp.PixelWidth, bmp.PixelHeight, srcPixels);

            // バイト配列からピクセルを作成する
            return WriteableBitmapExtensions.FromArray(bmp.PixelWidth, bmp.PixelHeight, dstPixels);
        }

        /// <summary>
        /// 白黒反転処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bitmap">元になるWriteableBitampオブジェクト</param>
        /// <returns>WriteableBitampオブジェクト</returns>
        public static WriteableBitmap EffectNegative(this WriteableBitmap bmp)
        {
            return Effect(bmp, new NegativeEffect());
        }

        /// <summary>
        /// グレイスケール処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bitmap">元になるWriteableBitampオブジェクト</param>
        /// <returns>WriteableBitampオブジェクト</returns>
        public static WriteableBitmap EffectGrayscale(this WriteableBitmap bmp)
        {
            return Effect(bmp, new GrayscaleEffect());
        }

        /// <summary>
        /// ぼかし処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns>WriteableBitampオブジェクト</returns>
        public static WriteableBitmap EffectBlur(this WriteableBitmap bitmap, int range)
        {
            var pixels = bitmap.PixelBuffer.ToArray();
            int pixelCount = bitmap.PixelWidth * bitmap.PixelHeight;
            for (int i = 0; i < pixelCount; i++)
            {
                var index = i * 4;

                // 平均なんとか法
                var sum = pixels[index + 0] + pixels[index + 1] + pixels[index + 2];
                var p = (double)sum / 3;

                pixels[index + 0] = (byte)Math.Min(255, Math.Max(0, p));
                pixels[index + 1] = (byte)Math.Min(255, Math.Max(0, p));
                pixels[index + 2] = (byte)Math.Min(255, Math.Max(0, p));
            }

            return WriteableBitmapExtensions.FromArray(bitmap.PixelWidth, bitmap.PixelHeight, pixels);
        }

        /// <summary>
        /// セピア調処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bitmap">元になるWriteableBitampオブジェクト</param>
        /// <returns>WriteableBitampオブジェクト</returns>
        public static WriteableBitmap EffectSepia(this WriteableBitmap bmp)
        {
            return Effect(bmp, new SepiaEffect());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="contrast"></param>
        /// <returns></returns>
        public static WriteableBitmap EffectContrast(this WriteableBitmap bmp, double contrast)
        {
            return Effect(bmp, new ContrastEffect(contrast));
        }

        /// <summary>
        /// 幕末写真風処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bitmap">元になるWriteableBitampオブジェクト</param>
        /// <returns>WriteableBitampオブジェクト</returns>
        public static async Task<WriteableBitmap> EffectBakumatsuAsync(this WriteableBitmap bmp)
        {
            // クラスライブラリ内の画像をリソースを読み出す
            var maskFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SoftbuildLibrary/Images/bakumatsu.jpg"));
            // StorageFileからWriteableBitampを生成する
            var maskBitamp = await WriteableBitmapExtensions.FromStreamAsync(await maskFile.OpenReadAsync());
            // 元画像とサイズと合わせる
            var resizedBmp = maskBitamp.Resize(bmp.PixelWidth, bmp.PixelHeight);

            // 幕末画像を作成する
            return Effect(bmp, new BakumatsuEffect(resizedBmp));
        }

        /// <summary>
        /// 低周辺光量風処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bitmap">元になるWriteableBitampオブジェクト</param>
        /// <returns>WriteableBitampオブジェクト</returns>
        public static async Task<WriteableBitmap> EffectVignettingAsync(this WriteableBitmap bmp, double vignetting)
        {
            // クラスライブラリ内の画像をリソースを読み出す
            var maskFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SoftbuildLibrary/Images/vignetting_gradation.png"));
            // StorageFileからWriteableBitampを生成する
            var maskBitamp = await WriteableBitmapExtensions.FromStreamAsync(await maskFile.OpenReadAsync());
            // 元画像とサイズと合わせる
            var resizedBmp = maskBitamp.Resize(bmp.PixelWidth, bmp.PixelHeight);

            // 幕末画像を作成する
            return Effect(bmp, new VignettingEffect(resizedBmp, vignetting));
        }

        public static WriteableBitmap EffectSaturation(this WriteableBitmap bmp, double saturation)
        {
            return Effect(bmp, new SaturationEffect(saturation));
        }

        /// <summary>
        /// トイカメラ風処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bitmap">元になるWriteableBitampオブジェクト</param>
        /// <returns>WriteableBitampオブジェクト</returns>
        public static async Task<WriteableBitmap> EffectToycameraAsync(this WriteableBitmap bmp)
        {
            // クラスライブラリ内の画像をリソースを読み出す
            var maskFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///SoftbuildLibrary/Images/vignetting_gradation.png"));
            // StorageFileからWriteableBitampを生成する
            var maskBitamp = await WriteableBitmapExtensions.FromStreamAsync(await maskFile.OpenReadAsync());
            // 元画像とサイズと合わせる
            var resizedBmp = maskBitamp.Resize(bmp.PixelWidth, bmp.PixelHeight);

            var srcPixels = bmp.PixelBuffer.ToArray();
            byte[] dstPixels;
            dstPixels = new ContrastEffect(0.8).Effect(bmp.PixelWidth, bmp.PixelHeight, srcPixels);
            dstPixels = new SaturationEffect(0.8).Effect(bmp.PixelWidth, bmp.PixelHeight, dstPixels);
            dstPixels = new VignettingEffect(resizedBmp, 0.7).Effect(bmp.PixelWidth, bmp.PixelHeight, dstPixels);

            // バイト配列からピクセルを作成する
            return WriteableBitmapExtensions.FromArray(bmp.PixelWidth, bmp.PixelHeight, dstPixels);
        }
    }
}
