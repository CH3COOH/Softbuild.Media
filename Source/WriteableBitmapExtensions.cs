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
    public static class WriteableBitmapExtensions
    {
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
#if WINDOWS_STORE_APPS
            // WriteableBitmapへバイト配列のピクセルデータをコピーする
            using (var pixelStream = bitmap.PixelBuffer.AsStream())
            {
                pixelStream.Seek(0, SeekOrigin.Begin);
                pixelStream.Write(array, 0, array.Length);
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

#if WINDOWS_STORE_APPS
        /// <summary>
        /// ストリームからWriteableBitmapオブジェクトを生成する
        /// </summary>
        /// <param name="stream">Streamストリーム</param>
        /// <returns>WriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> FromStreamAsync(System.IO.Stream stream)
        {
            // ストリームからbyte配列に読み込む
            stream.Seek(0, SeekOrigin.Begin);
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            
            var buffe = bytes.AsBuffer();

            WriteableBitmap retBitmap = null;
            using (var ras = new InMemoryRandomAccessStream())
            {
                await ras.WriteAsync(buffe);
                ras.Seek(0);
                retBitmap = await FromRandomAccessStreamAsync(ras);
            }
            return retBitmap;
        }

        /// <summary>
        /// IRandomAccessStreamストリームからWriteableBitmapオブジェクトを生成する
        /// </summary>
        /// <param name="stream">IRandomAccessStreamストリーム</param>
        /// <returns>WriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> FromStreamAsync(IRandomAccessStream stream)
        {
            return await FromRandomAccessStreamAsync(stream);
        }

        /// <summary>
        /// IRandomAccessStreamストリームからWriteableBitmapオブジェクトを生成する
        /// </summary>
        /// <param name="stream">IRandomAccessStreamストリーム</param>
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

        public static async Task<WriteableBitmap> FromFileAsync(StorageFile file)
        {
            var bitmap = default(WriteableBitmap);
            using (var strm = await file.OpenStreamForReadAsync())
            {
                bitmap = await WriteableBitmapExtensions.FromStreamAsync(strm);
            }
            return bitmap;
        }

        public static async Task<WriteableBitmap> FromUriAsync(Uri uri)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            return await FromFileAsync(file);
        }

#endif
        
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

            return WriteableBitmapExtensions.FromArray(bmp.PixelWidth, bmp.PixelHeight, pixels);
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
            return WriteableBitmapExtensions.FromArray(destWidth, destHeight, destPixels);
        }

#if WINDOWS_STORE_APPS

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
        /// 
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static async Task SaveAsync(this WriteableBitmap bmp, StorageFile file)
        {
            // 保存したい画像のピクセルデータを取り出す
            var bytes = new byte[bmp.PixelBuffer.Length];
            using (var strm = bmp.PixelBuffer.AsStream())
            {
                strm.Position = 0;
                strm.Read(bytes, 0, bytes.Length);
            }

            // ユーザーが指定したファイルのストリームを開く
            using (var writeStrm = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                // JPEGのエンコーダーを使ってピクセルデータをエンコードして、
                // ストリームへ書き出す
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, writeStrm);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight,
                    (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, 96, 96, bytes);
                await encoder.FlushAsync();
            }
        }

        /// <summary>
        /// 画像をJPEGフォーマットでピクチャーライブラリへ保存する
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <returns>無し</returns>
        public static async Task SaveAsync(this WriteableBitmap bmp, string fileNameWithoutExtension)
        {
            await SaveAsync(bmp, ImageDirectories.PicturesLibrary, ImageFormat.Jpeg, 
                fileNameWithoutExtension, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, true, 96, 96);
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
            await SaveAsync(bmp, directory, format, fileNameWithoutExtension, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, true, 96, 96);
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
            await SaveAsync(bmp, directory, format, fileNameWithoutExtension, encodeWidth, encodeHeight, true, 96, 96);
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
        public static async Task SaveAsync(this WriteableBitmap bmp, ImageDirectories directory, ImageFormat format, 
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

#region Obsolete バージョン2.0にて削除する

        /// <summary>
        /// 画像を指定したフォーマットでストレージへ保存する(互換性のため維持：バージョン2.0にて削除予定)
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="format">画像フォーマット種別</param>
        /// <param name="directory">保存先のディレクトリ種別</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <returns>無し</returns>
        [Obsolete("use SaveAsync(ImageDirectories,ImageFormat,string)")]
        public static async Task SaveAsync(this WriteableBitmap bmp, ImageFormat format, ImageDirectories directory,
            string fileNameWithoutExtension)
        {
            await SaveAsync(bmp, directory, format, fileNameWithoutExtension, (uint)bmp.PixelWidth, (uint)bmp.PixelHeight, true, 96, 96);
        }
        /// <summary>
        /// 画像を指定したフォーマットでストレージへ保存する(互換性のため維持：バージョン2.0にて削除予定)
        /// </summary>
        /// <param name="bmp">保存するWriteableBitmapオブジェクト</param>
        /// <param name="format">画像フォーマット種別</param>
        /// <param name="directory">保存先のディレクトリ種別</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く保存ファイル名</param>
        /// <param name="encodeWidth">エンコード後の画像の幅</param>
        /// <param name="encodeHeight">エンコード後の画像の高さ</param>
        /// <returns>無し</returns>
        [Obsolete("use SaveAsync(ImageDirectories,ImageFormat,string,uint,uint)")]
        public static async Task SaveAsync(this WriteableBitmap bmp, ImageFormat format, ImageDirectories directory,
            string fileNameWithoutExtension, uint encodeWidth, uint encodeHeight)
        {
            await SaveAsync(bmp, directory, format, fileNameWithoutExtension, encodeWidth, encodeHeight, true, 96, 96);
        }

        /// <summary>
        /// 画像を指定したフォーマットでストレージへ保存する(互換性のため維持：バージョン2.0にて削除予定)
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
        [Obsolete("use SaveAsync(ImageDirectories,ImageFormat,string,uint,uint,bool,double,double)")]
        public static async Task SaveAsync(this WriteableBitmap bmp, ImageFormat format, ImageDirectories directory,
            string fileNameWithoutExtension, uint encodeWidth, uint encodeHeight, bool isAspectRatio, double dpiX, double dpiY)
        {
            await SaveAsync(bmp, directory, format, fileNameWithoutExtension, encodeWidth, encodeHeight, isAspectRatio, dpiX, dpiY);
        }

#endregion

        /// <summary>
        /// 指定したフォーマットでストレージに保存されている画像を読み出しWriteableBitmapオブジェクトを生成する
        /// </summary>
        /// <param name="directory">読み取り先のディレクトリ種別</param>
        /// <param name="format">画像フォーマット種別</param>
        /// <param name="fileNameWithoutExtension">拡張子を除く画像ファイル名</param>
        /// <returns>読みだしたWriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> LoadAsync(ImageDirectories directory, ImageFormat format, string fileNameWithoutExtension)
        {
            var extension = GetExtension(format);
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
            var folder = GetStorageFolder(directory);
            using (var strm = await folder.LoadFileAsync(fileNameWithExtension))
            {
                bmp = await FromRandomAccessStreamAsync(strm);
            }
            return bmp;
        }

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
