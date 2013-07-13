﻿//
// WriteableBitmapSaverExtensions.cs
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
    public static class WriteableBitmapSaveExtensions
    {
        public static async Task SaveAsync(this WriteableBitmap bmp, StorageFile file)
        {
            await SaveAsync(bmp, ImageFormat.Jpeg, file, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight);
        }

        public static async Task SaveAsync(this WriteableBitmap bmp, StorageFile file, ImageFormat format)
        {
            await SaveAsync(bmp, format, file, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight);
        }

        /// <summary>
        /// 画像をJPEGフォーマットでピクチャーライブラリへ保存する
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <returns>無し</returns>
        public static async Task SaveAsync(this WriteableBitmap bmp, string fileNameWithoutExtension)
        {
            var folder = ImageDirectories.PicturesLibrary.GetStorageFolder();
            var extension = ImageFormat.Jpeg.GetExtension();
            var fileName = string.Format(@"{0}{1}", fileNameWithoutExtension, extension);

            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await SaveAsync(bmp, ImageFormat.Jpeg, file, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight);
        }

        /// <summary>
        /// 画像を指定したフォーマットでストレージへ保存する
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="directory">保存先のディレクトリ種別</param>
        /// <param name="format">画像フォーマット種別</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <returns>無し</returns>
        public static async Task SaveAsync(this WriteableBitmap bmp, ImageDirectories directory, ImageFormat format,
            string fileNameWithoutExtension)
        {
            var folder = directory.GetStorageFolder();
            var extension = format.GetExtension();
            var fileName = string.Format(@"{0}{1}", fileNameWithoutExtension, extension);

            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await SaveAsync(bmp, format, file, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight);
        }

        /// <summary>
        /// 画像を指定したフォーマットでストレージへ保存する
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="directory">保存先のディレクトリ種別</param>
        /// <param name="format">画像フォーマット種別</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <param name="encodeWidth">エンコード後の画像の幅</param>
        /// <param name="encodeHeight">エンコード後の画像の高さ</param>
        /// <returns>無し</returns>
        public static async Task SaveAsync(this WriteableBitmap bmp, ImageDirectories directory, ImageFormat format,
            string fileNameWithoutExtension, uint encodeWidth, uint encodeHeight)
        {
            var folder = directory.GetStorageFolder();
            var extension = format.GetExtension();
            var fileName = string.Format(@"{0}{1}", fileNameWithoutExtension, extension);

            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await SaveAsync(bmp, format, file, encodeWidth, encodeHeight);
        }

        public static async Task SaveAsync(this WriteableBitmap bmp, StorageFile file, uint encodeWidth, uint encodeHeight)
        {
            var format = ImageFormat.Jpeg;

            // TODO: フォーマット別の切り分け処理が必要

            await SaveAsync(bmp, format, file, encodeWidth, encodeHeight);
        }

        /// <summary>
        /// 画像を指定したフォーマットでストレージへ保存する
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="directory">保存先のディレクトリ種別</param>
        /// <param name="format">画像フォーマット種別</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <param name="encodeWidth">エンコード後の画像の幅</param>
        /// <param name="encodeHeight">エンコード後の画像の高さ</param>
        /// <param name="isAspectRatio">エンコード後の画像のサイズのアスペクト比を維持する</param>
        /// <param name="dpiX">保存後の水平方向の解像度(dpi)</param>
        /// <param name="dpiY">保存後の直立方向の解像度(dpi)</param>
        /// <returns>無し</returns>
        public static async Task SaveAsync(this WriteableBitmap bmp, ImageFormat format, StorageFile file,
            uint encodeWidth, uint encodeHeight, bool isAspectRatio = true, double dpiX = 96.0, double dpiY = 96.0)
        {
            var encodeId = format.GetEncodertId();

            // 最終的に保存する画角がビットマップと異なる場合リサイズする
            var width = bmp.PixelWidth;
            var height = bmp.PixelHeight;
            var dstSize = new Size(encodeWidth, encodeHeight);
            if (isAspectRatio)
            {
                // 元画像の比率を維持する場合は、比率を求める
                dstSize = WriteableBitmapCoreExtensions.GetAspectRatio(width, height, encodeWidth, encodeHeight);
            }
            bmp = bmp.Resize((int)dstSize.Width, (int)dstSize.Height);

            // エンコーダーを生成し、ストリームへエンコード後の画像データを書き込む
            using (var strm = new InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(encodeId, strm);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight,
                    (uint)dstSize.Width, (uint)dstSize.Height, dpiX, dpiY, bmp.PixelBuffer.ToArray());
                await encoder.FlushAsync();

                strm.Seek(0);

                // ストリームを保存する
                await file.SaveAsync(strm);
            }
        }
    }
}
