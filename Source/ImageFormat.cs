﻿//
// ImageFormat.cs
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
#if NETFX_CORE && WINDOWS_STORE_APPS
using Windows.Graphics.Imaging;
#endif

namespace Softbuild.Media
{
    /// <summary>
    /// 画像のフォーマット種別
    /// </summary>
    public enum ImageFormat
    {
        /// <summary>
        /// ビットマップ フォーマット
        /// </summary>
        Bitmap,

        /// <summary>
        /// JPEG フォーマット
        /// </summary>
        Jpeg,

        /// <summary>
        /// JPEG XR フォーマット
        /// </summary>
        JpegXR,

        /// <summary>
        /// PNG フォーマット
        /// </summary>
        Png,

        /// <summary>
        /// GIF フォーマット
        /// </summary>
        Gif
    }

    public static class ImageFormatExtensions
    {
        /// <summary>
        /// 指定したフォーマット種別からファイル拡張子を取得する
        /// </summary>
        /// <param name="format">画像フォーマット種別</param>
        /// <returns>ファイル拡張子</returns>
        public static string GetExtension(this ImageFormat format)
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

#if WINDOWS_STORE_APPS
        /// <summary>
        /// 指定したフォーマット種別からエンコーダーのGUIDを取得する
        /// </summary>
        /// <param name="format">画像フォーマット種別</param>
        /// <returns>エンコーダーのGUID</returns>
        public static Guid GetEncodertId(this ImageFormat format)
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
        public static Guid GetDecodeId(this ImageFormat format)
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
#endif
    }
}
