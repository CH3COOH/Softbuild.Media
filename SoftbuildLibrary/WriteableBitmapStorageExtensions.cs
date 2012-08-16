﻿//
// WriteableBitmapStorageExtensions.cs
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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Softbuild.Storage;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Softbuild.Media
{
    public static class WriteableBitmapStorageExtensions
    {
        /// <summary>
        /// 指定したフォーマット種別からエンコーダーのGUIDを取得する
        /// </summary>
        /// <param name="format">画像フォーマット種別</param>
        /// <returns>エンコーダーのGUID</returns>
        private static Guid GetEncodertId(ImageFormat format)
        {
            var imageFormatId = default(Guid);
            switch (format)
            {
                case ImageFormat.Jpeg:
                    imageFormatId = BitmapEncoder.JpegEncoderId;
                    break;
                case ImageFormat.JpegXR:
                    imageFormatId = BitmapEncoder.JpegXREncoderId;
                    break;
                case ImageFormat.Gif:
                    imageFormatId = BitmapEncoder.GifEncoderId;
                    break;
                case ImageFormat.Bitmap:
                    imageFormatId = BitmapEncoder.BmpEncoderId;
                    break;
                case ImageFormat.Png:
                    imageFormatId = BitmapEncoder.PngEncoderId;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return imageFormatId;
        }

        /// <summary>
        /// 指定したフォーマット種別からファイル拡張子を取得する
        /// </summary>
        /// <param name="format">画像フォーマット種別</param>
        /// <returns>ファイル拡張子</returns>
        private static string GetExtension(ImageFormat format)
        {
            var extension = default(string);
            switch (format)
            {
                case ImageFormat.Jpeg:
                    extension = ".jpg";
                    break;
                case ImageFormat.JpegXR:
                    extension = ".wdp";
                    break;
                case ImageFormat.Gif:
                    extension = ".gif";
                    break;
                case ImageFormat.Bitmap:
                    extension = ".bmp";
                    break;
                case ImageFormat.Png:
                    extension = ".png";
                    break;
                default:
                    throw new NotImplementedException();
            }

            return extension;
        }

        /// <summary>
        /// 画像比率を維持したまま指定されたサイズに収まる最大画像サイズを計算する
        /// </summary>
        /// <param name="srcWidth">元画像の幅</param>
        /// <param name="srcHeight">元画像の高さ</param>
        /// <param name="dstWidth">出力画像の幅</param>
        /// <param name="dstHeight">出力画像の高さ</param>
        /// <returns>画像比率が維持された状態</returns>
        private static Size GetAspectRatio(double srcWidth, double srcHeight, double dstWidth, double dstHeight)
        {
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
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private static IStorageFolder GetStorageFolder(ImageDirectories directory)
        {
            var folder = default(IStorageFolder);
            switch (directory)
            {
                case ImageDirectories.PicturesLibrary:
                    folder = KnownFolders.PicturesLibrary;
                    break;
                case ImageDirectories.DocumentsLibrary:
                    folder = KnownFolders.DocumentsLibrary;
                    break;
                case ImageDirectories.InApplicationLocal:
                    folder = ApplicationData.Current.LocalFolder;
                    break;
                case ImageDirectories.InApplicationRoaming:
                    folder = ApplicationData.Current.RoamingFolder;
                    break;
                case ImageDirectories.InApplicationTemporary:
                    folder = ApplicationData.Current.TemporaryFolder;
                    break;
                default:
                    throw new ArgumentException();
            }
            return folder;
        }

        /// <summary>
        /// 画像を指定したフォーマットでストレージへ保存する
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="format">画像フォーマット種別</param>
        /// <param name="directory">保存先のディレクトリ種別</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <returns>無し</returns>
        public static async Task SaveAsync(this WriteableBitmap bmp, string fileNameWithoutExtension)
        {
            await SaveAsync(bmp, ImageFormat.Jpeg, ImageDirectories.PicturesLibrary, fileNameWithoutExtension, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, true, 96, 96);
        }

        /// <summary>
        /// 画像を指定したフォーマットでストレージへ保存する
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="format">画像フォーマット種別</param>
        /// <param name="directory">保存先のディレクトリ種別</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <returns>無し</returns>
        public static async Task SaveAsync(this WriteableBitmap bmp, ImageFormat format, ImageDirectories directory, 
            string fileNameWithoutExtension)
        {
            await SaveAsync(bmp, format, directory, fileNameWithoutExtension, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, true, 96, 96);
        }

        /// <summary>
        /// 画像を指定したフォーマットでストレージへ保存する
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="format">画像フォーマット種別</param>
        /// <param name="directory">保存先のディレクトリ種別</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <param name="encodeWidth">エンコード後の画像の幅</param>
        /// <param name="encodeHeight">エンコード後の画像の高さ</param>
        /// <returns>無し</returns>
        public static async Task SaveAsync(this WriteableBitmap bmp, ImageFormat format, ImageDirectories directory, 
            string fileNameWithoutExtension, uint encodeWidth, uint encodeHeight)
        {
            await SaveAsync(bmp, format, directory, fileNameWithoutExtension, encodeWidth, encodeHeight, true, 96, 96);
        }

        /// <summary>
        /// 画像を指定したフォーマットでストレージへ保存する
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="format">画像フォーマット種別</param>
        /// <param name="directory">保存先のディレクトリ種別</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <param name="encodeWidth">エンコード後の画像の幅</param>
        /// <param name="encodeHeight">エンコード後の画像の高さ</param>
        /// <param name="isAspectRatio">エンコード後の画像のサイズのアスペクト比を維持する</param>
        /// <param name="dpiX">保存後の水平方向の解像度(dpi)</param>
        /// <param name="dpiY">保存後の直立方向の解像度(dpi)</param>
        /// <returns>無し</returns>
        public static async Task SaveAsync(this WriteableBitmap bmp, ImageFormat format, ImageDirectories directory, 
            string fileNameWithoutExtension, uint encodeWidth, uint encodeHeight, bool isAspectRatio, double dpiX, double dpiY)
        {
            var encodeId = GetEncodertId(format);
            var extension = GetExtension(format);
            var fileName = string.Format(@"{0}{1}", fileNameWithoutExtension, extension);

            // 最終的に保存する画角がビットマップと異なる場合リサイズする
            var width = bmp.PixelWidth;
            var height = bmp.PixelHeight;
            var dstSize = new Size(encodeWidth, encodeHeight);
            if (isAspectRatio)
            {
                // 元画像の比率を維持する場合は、比率を求める
                dstSize = GetAspectRatio(width, height, encodeWidth, encodeHeight);
            }
            bmp = bmp.Resize((int)dstSize.Width, (int)dstSize.Height);

            // エンコーダーを生成し、ストリームへエンコード後の画像データを書き込む
            using (var strm = new InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(encodeId, strm);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint)dstSize.Width, (uint)dstSize.Height, dpiX, dpiY, bmp.PixelBuffer.ToArray());
                await encoder.FlushAsync();

                strm.Seek(0);

                // 保存先のストレージフォルダを取得する
                var folder = GetStorageFolder(directory);
                await StorageExtensions.SaveToFolderAsync(folder, fileName, strm);
            }
        }
    }
}
