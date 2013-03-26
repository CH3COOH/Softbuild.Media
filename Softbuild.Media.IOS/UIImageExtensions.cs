using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using Softbuild.Media.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Softbuild.Media
{
    public static class UIImageExtensions
    {
        /// <summary>
        /// ビットマップの2-Dテクスチャを表す配列を取得します
        /// </summary>
        /// <param name="bmp">WriteableBitmapオブジェクト</param>
        /// <returns>ピクセルデータ</returns>
        public static byte[] GetPixels(this UIImage bmp)
        {
            var bytes = default(byte[]);

            // データプロバイダを取得する
            using (var cgImage = bmp.CGImage)
            using (var dataProvider = cgImage.DataProvider)
            using (var data = dataProvider.CopyData())
            {
                // ビットマップデータを取得する
                var buffer = data.Bytes;
                bytes = new byte[data.Length];
                Marshal.Copy(buffer, bytes, 0, bytes.Length);
            }

            return bytes;
        }

        /// <summary>
        /// バイト配列からWriteableBitmapを生成する
        /// </summary>
        /// <param name="width">幅</param>
        /// <param name="height">高さ</param>
        /// <param name="array">ピクセルデータ</param>
        /// <returns>WriteableBitmapオブジェクト</returns>
        public static UIImage FromArray(this UIImage bmp, byte[] array)
        {
            var cgImage = bmp.CGImage;

	        int width = cgImage.Width;
	        int height = cgImage.Height;
	        int bitsPerComponent = cgImage.BitsPerComponent;
	        int bitsPerPixel = cgImage.BitsPerPixel;
	        int bytesPerRow = cgImage.BytesPerRow;
	        var colorSpace = cgImage.ColorSpace;
	        var bitmapInfo = cgImage.BitmapInfo;
	        var shouldInterpolate = cgImage.ShouldInterpolate;
	        var intent = cgImage.RenderingIntent; 

            // 画像処理後のbyte配列を元にデータプロバイダーを作成する
            CGImage effectedCgImage;

            using (var effectedDataProvider = new CGDataProvider(array, 0, array.Length))
            {
                // データプロバイダーからCGImageを作成し、CGImageからUIImageを作成する
                effectedCgImage = new CGImage(
                    width, height, bitsPerComponent, bitsPerPixel, bytesPerRow,
                    colorSpace, bitmapInfo, effectedDataProvider,
                    null, shouldInterpolate, intent);
            }
            
            return new UIImage(effectedCgImage);
        }

        public static int GetWidth(this UIImage bmp)
        {
            int width;
            using (var cgImage = bmp.CGImage)
            {
                width = cgImage.Width;
            }
            return width;
        }

        public static int GetHeight(this UIImage bmp)
        {
            int height;
            using (var cgImage = bmp.CGImage)
            {
                height = cgImage.Height;
            }
            return height;
        }


        /// <summary>
        /// パラメータ無しの画像処理をおこなう
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="effector">処理させるIEffectオブジェクト</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        private static UIImage ProcessEffect(UIImage bmp, IEffect effector)
        {
            // WriteableBitmapのピクセルデータをバイト配列に変換する
            var srcPixels = bmp.GetPixels();

            // パラメータ無しの画像処理をおこなう
            var dstPixels = effector.Effect(bmp.GetWidth(), bmp.GetHeight(), srcPixels);

            // バイト配列からピクセルを作成する
            return UIImageExtensions.FromArray(bmp, dstPixels);
        }

        /// <summary>
        /// グレイスケール処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static UIImage EffectGrayscale(this UIImage bmp)
        {
            return ProcessEffect(bmp, new GrayscaleEffect());
        }

        /// <summary>
        /// セピア調処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static UIImage EffectSepia(this UIImage bmp)
        {
            return ProcessEffect(bmp, new SepiaEffect());
        }

    }
}
