﻿//
// WriteableBitmapLoaderExtensions.cs
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
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
#elif WINDOWS_PHONE
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
#endif
#if NETFX_CORE
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
#endif

namespace Softbuild.Media
{
    public static class WriteableBitmapLoadExtensions
    {
        /// <summary>
        /// バイト配列からWriteableBitmapを生成する
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <param name="array">ピクセルデータ</param>
        /// <returns>WriteableBitmapオブジェクト</returns>
        public static WriteableBitmap FromArray(int width, int height, byte[] array, ImageFileTypes type = ImageFileTypes.Normal)
        {
            // 出力用のWriteableBitmapオブジェクトを生成する
            var bitmap = new WriteableBitmap(width, height);
#if WINDOWS_STORE_APPS
            if (type == ImageFileTypes.GifWithStrokes)
            {
                // WriteableBitmapへバイト配列のピクセルデータをコピーする
                using (var pixelStream = bitmap.PixelBuffer.AsStream())
                {
                    pixelStream.Seek(0, SeekOrigin.Begin);

                    var pixelCount = array.Length / 4;
                    for (var i = 0; i < pixelCount; i++)
                    {
                        var index = i * 4;
                        var r = array[index + 0];
                        var g = array[index + 1];
                        var b = array[index + 2];
                        var a = array[index + 3];

                        pixelStream.WriteByte(b);
                        pixelStream.WriteByte(g);
                        pixelStream.WriteByte(r);
                        pixelStream.WriteByte(a);
                    }
                }
            }
            else
            {
                // WriteableBitmapへバイト配列のピクセルデータをコピーする
                using (var pixelStream = bitmap.PixelBuffer.AsStream())
                {
                    pixelStream.Seek(0, SeekOrigin.Begin);
                    pixelStream.Write(array, 0, array.Length);
                }

            }
#elif WINDOWS_PHONE
            int max = width * height;
            for (int i = 0; i < max; i++)
            {
                int index = i * 4;
                int a = array[index + 3];
                int r = array[index + 2];
                int g = array[index + 1];
                int b = array[index + 0];

                bitmap.Pixels[i] = (a << 24 | r << 16 | g << 8 | b);
            }
#endif
            return bitmap;
        }

#if NETFX_CORE

        /// <summary>
        /// ストリームからWriteableBitmapオブジェクトを生成する
        /// </summary>
        /// <param name="stream">Streamストリーム</param>
        /// <returns>WriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> FromStreamAsync(System.IO.Stream stream, ImageFileTypes type = ImageFileTypes.Normal)
        {
            WriteableBitmap retBitmap = null;
#if WINDOWS_STORE_APPS
            // ストリームからbyte配列に読み込む
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            var buffe = bytes.AsBuffer();

            using (var ras = new InMemoryRandomAccessStream())
            {
                await ras.WriteAsync(buffe);
                ras.Seek(0);
                retBitmap = await FromRandomAccessStreamAsync(ras, type);
            }
#else
            var bitmapSource = new BitmapImage();
            bitmapSource.SetSource(stream);

            retBitmap = new WriteableBitmap(bitmapSource);
#endif
            return retBitmap;
        }

        public static async Task<WriteableBitmap> FromFileAsync(StorageFile file, ImageFileTypes type = ImageFileTypes.Normal)
        {
            var bitmap = default(WriteableBitmap);
            using (var strm = await file.OpenStreamForReadAsync())
            {
                bitmap = await WriteableBitmapLoadExtensions.FromStreamAsync(strm, type);
            }
            return bitmap;
        }

        public static async Task<WriteableBitmap> FromUriAsync(Uri uri)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            return await FromFileAsync(file);
        }
#endif

#if WINDOWS_STORE_APPS


        /// <summary>
        /// IRandomAccessStreamストリームからWriteableBitmapオブジェクトを生成する
        /// </summary>
        /// <param name="stream">IRandomAccessStreamストリーム</param>
        /// <returns>WriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> FromStreamAsync(IRandomAccessStream stream, ImageFileTypes type = ImageFileTypes.Normal)
        {
            return await FromRandomAccessStreamAsync(stream, type);
        }

        /// <summary>
        /// IRandomAccessStreamストリームからWriteableBitmapオブジェクトを生成する
        /// </summary>
        /// <param name="stream">IRandomAccessStreamストリーム</param>
        /// <returns>WriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> FromRandomAccessStreamAsync(IRandomAccessStream stream, ImageFileTypes type = ImageFileTypes.Normal)
        {
            // ストリームからピクセルデータを読み込む
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var transform = new BitmapTransform();
            var pixelData = await decoder.GetPixelDataAsync(decoder.BitmapPixelFormat, decoder.BitmapAlphaMode,
                transform, ExifOrientationMode.RespectExifOrientation, ColorManagementMode.ColorManageToSRgb);
            var pixels = pixelData.DetachPixelData();

            // ピクセルデータからWriteableBitmapオブジェクトを生成する
            return WriteableBitmapLoadExtensions.FromArray((int)decoder.OrientedPixelWidth, (int)decoder.OrientedPixelHeight, pixels, type);
        }


        /// <summary>
        /// 指定したフォーマットでストレージに保存されている画像を読み出しWriteableBitmapオブジェクトを生成する
        /// </summary>
        /// <param name="directory">読み取り先のディレクトリ種別</param>
        /// <param name="format">画像フォーマット種別</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く画像ファイル名</param>
        /// <returns>読みだしたWriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> LoadAsync(ImageDirectories directory, ImageFormat format, string fileNameWithoutExtension)
        {
            var extension = format.GetExtension();
            var fileName = string.Format(@"{0}{1}", fileNameWithoutExtension, extension);
            return await LoadAsync(directory, fileName);
        }

        /// <summary>
        /// ストレージに保存されている画像を読み出しWriteableBitmapオブジェクトを生成する
        /// </summary>
        /// <param name="directory">読み取り先のディレクトリ種別</param>
        /// <param name="fileNameWithExtension">拡張子を含む画像ファイル名</param>
        /// <returns>読みだしたWriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> LoadAsync(ImageDirectories directory, string fileNameWithExtension)
        {
            var bmp = default(WriteableBitmap);
            var folder = directory.GetStorageFolder();
            using (var strm = await folder.LoadFileAsync(fileNameWithExtension))
            {
                bmp = await WriteableBitmapLoadExtensions.FromRandomAccessStreamAsync(strm);
            }
            return bmp;
        }

#endif
    }
}
