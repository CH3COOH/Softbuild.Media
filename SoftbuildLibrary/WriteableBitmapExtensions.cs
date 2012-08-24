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

using Softbuild.Storage;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace Softbuild.Media
{
    public static class WriteableBitmapExtensions
    {

        public static async Task<WriteableBitmap> FromStreamAsync(System.IO.Stream stream)
        {
            // 
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            // 
            var buffe = bytes.AsBuffer();
            var ras = new InMemoryRandomAccessStream();
            await ras.WriteAsync(buffe);
            ras.Seek(0);

            return await FromRandomAccessStreamAsync(ras);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task<WriteableBitmap> FromStreamAsync(IRandomAccessStream stream)
        {
            return await FromRandomAccessStreamAsync(stream);
        }

        /// <summary>
        /// IRandomAccessStreamからWriteableBitmapを生成する
        /// </summary>
        /// <param name="stream">ランダムアクセスストリーム</param>
        /// <returns>WriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> FromRandomAccessStreamAsync(IRandomAccessStream stream)
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
        /// リサイズする
        /// </summary>
        /// <param name="bmp">WriteableBitmapオブジェクト</param>
        /// <param name="destWidth">変形後の幅</param>
        /// <param name="destHeight">変形後の高さ</param>
        /// <returns>リサイズ後のWriteableBitmapオブジェクト</returns>
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
        /// 指定したフォーマット種別からデコーダーのGUIDを取得する
        /// </summary>
        /// <param name="format">画像フォーマット種別</param>
        /// <returns>デコーダーのGUID</returns>
        private static Guid GetDecodeId(ImageFormat format)
        {
            var imageFormatId = default(Guid);
            switch (format)
            {
                case ImageFormat.Jpeg:
                    imageFormatId = BitmapDecoder.JpegDecoderId;
                    break;
                case ImageFormat.JpegXR:
                    imageFormatId = BitmapDecoder.JpegXRDecoderId;
                    break;
                case ImageFormat.Gif:
                    imageFormatId = BitmapDecoder.GifDecoderId;
                    break;
                case ImageFormat.Bitmap:
                    imageFormatId = BitmapDecoder.BmpDecoderId;
                    break;
                case ImageFormat.Png:
                    imageFormatId = BitmapDecoder.PngDecoderId;
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
        /// 画像をJPEGフォーマットでピクチャーライブラリへ保存する
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
                await folder.SaveFileAsync(fileName, strm);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="format"></param>
        /// <param name="fileNameWithoutExtension"></param>
        /// <returns></returns>
        public static async Task<WriteableBitmap> LoadAsync(ImageDirectories directory, ImageFormat format, string fileNameWithoutExtension)
        {
            var extension = GetExtension(format);
            var fileName = string.Format(@"{0}{1}", fileNameWithoutExtension, extension);
            return await LoadAsync(directory, fileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileNameWithExtension"></param>
        /// <returns></returns>
        public static async Task<WriteableBitmap> LoadAsync(ImageDirectories directory, string fileNameWithExtension)
        {
            var bmp = default(WriteableBitmap);
            var folder = GetStorageFolder(directory);
            using (var strm = await folder.LoadFileAsync(fileNameWithExtension))
            {
                bmp = await FromRandomAccessStreamAsync(strm);
            }
            return bmp;
        }
    }
}
