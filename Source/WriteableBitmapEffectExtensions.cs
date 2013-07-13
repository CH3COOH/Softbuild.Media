﻿//
// WriteableBitmapEffectExtensions.cs
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

using Softbuild.Media.Effects;
using System;
using System.Collections.Generic;
using System.Reflection;

#if WINDOWS_STORE_APPS
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using Softbuild.Media.Effects.GiCoCu;
#elif WINDOWS_PHONE
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Media;
#endif

namespace Softbuild.Media
{
    public static class  WriteableBitmapEffectExtensions
    {
        /// <summary>
        /// パラメータ無しの画像処理をおこなう
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="effector">処理させるIEffectオブジェクト</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        private static WriteableBitmap ProcessEffect(WriteableBitmap bmp, IEffect effector)
        {
            // WriteableBitmapのピクセルデータをバイト配列に変換する
            var srcPixels = bmp.GetPixels();

            // パラメータ無しの画像処理をおこなう
            var dstPixels = effector.Effect(bmp.PixelWidth, bmp.PixelHeight, srcPixels);

            // バイト配列からピクセルを作成する
            return WriteableBitmapLoadExtensions.FromArray(bmp.PixelWidth, bmp.PixelHeight, dstPixels);
        }

        /// <summary>
        /// パラメータ無しの画像処理をおこなう
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="effectors">処理させるIEffectオブジェクト配列</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        private static WriteableBitmap ProcessEffect(WriteableBitmap bmp, IEnumerable<IEffect> effectors)
        {
            var pixels = bmp.GetPixels();
            var width = bmp.PixelWidth;
            var height = bmp.PixelHeight;

            foreach (var effector in effectors)
            {
                pixels = effector.Effect(width, height, pixels);
            }

            return WriteableBitmapLoadExtensions.FromArray(width, height, pixels);
        }

        /// <summary>
        /// アセンブリ内のリソースのストリームを取得する
        /// </summary>
        /// <param name="resourceName">リソース名</param>
        /// <returns>ストリーム</returns>
        private static System.IO.Stream GetResourceStream(string resourceName)
        {
            var asm = Assembly.Load(new AssemblyName("Softbuild.Media"));
            return asm.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// 白黒反転処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectNegative(this WriteableBitmap bmp)
        {
            return ProcessEffect(bmp, new NegativeEffect());
        }

        /// <summary>
        /// グレイスケール処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectGrayscale(this WriteableBitmap bmp)
        {
            return ProcessEffect(bmp, new GrayscaleEffect());
        }

        ///// <summary>
        ///// ぼかし処理をしたWriteableBitmapオブジェクトを返す
        ///// </summary>
        ///// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        ///// <param name="range">ぼかしの強さ</param>
        ///// <returns>処理後のWriteableBitmapオブジェクト</returns>
        //public static WriteableBitmap EffectBlur(this WriteableBitmap bitmap, int range)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// セピア調処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectSepia(this WriteableBitmap bmp)
        {
            return ProcessEffect(bmp, new SepiaEffect());
        }

        /// <summary>
        /// コントラストの調整処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="contrast">コントラストの調整量(0.0～1.0 標準:0.5)</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectContrast(this WriteableBitmap bmp, double contrast)
        {
            return ProcessEffect(bmp, new ContrastEffect(contrast));
        }

        /// <summary>
        /// ブライトネスの調整処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="brightness">ブライトネスの調整量(0.0～1.0 標準:0.5)</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectBrightness(this WriteableBitmap bmp, double brightness)
        {
            return ProcessEffect(bmp, new BrightnessEffect(brightness));
        }

        /// <summary>
        /// 漫画風処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectCartoonize(this WriteableBitmap bmp)
        {
            return ProcessEffect(bmp, new CartoonizeEffect());
        }

        /// <summary>
        /// 漫画風処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="contrast">コントラストの調整量(0.0～1.0 標準:0.5)</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectCartoonize(this WriteableBitmap bmp, int threshold, byte stroke)
        {
            return ProcessEffect(bmp, new CartoonizeEffect(threshold, stroke));
        }

#if WINDOWS_STORE_APPS
        /// <summary>
        /// 幕末写真風処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> EffectBakumatsuAsync(this WriteableBitmap bmp)
        {
            // クラスライブラリ内の画像をリソースを読み出す
            var maskBitmap = default(WriteableBitmap);
            using (var strm = GetResourceStream("Softbuild.Media.Images.bakumatsu.jpg"))
            {
                // StreamからWriteableBitmapを生成する
                maskBitmap = await WriteableBitmapLoadExtensions.FromStreamAsync(strm);
            }
            // 元画像とサイズと合わせる
            var resizedBmp = maskBitmap.Resize(bmp.PixelWidth, bmp.PixelHeight);

            // 幕末画像を作成する
            return ProcessEffect(bmp, new BakumatsuEffect(resizedBmp));
        }

        /// <summary>
        /// 口径食風の処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="vignetting">口径食の強さ(0.0～1.0 標準:1.0)</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> EffectVignettingAsync(this WriteableBitmap bmp, double vignetting)
        {
            // クラスライブラリ内の画像をリソースを読み出す
            var maskBitmap = default(WriteableBitmap);
            using (var strm = GetResourceStream("Softbuild.Media.Images.vignetting_gradation.png"))
            {
                // StreamからWriteableBitmapを生成する
                maskBitmap = await WriteableBitmapLoadExtensions.FromStreamAsync(strm);
            }
            // 元画像とサイズと合わせる
            var resizedBmp = maskBitmap.Resize(bmp.PixelWidth, bmp.PixelHeight);

            // 口径食による周辺光量の低下風の処理をしたビットマップを作成する
            return ProcessEffect(bmp, new VignettingEffect(resizedBmp, vignetting));
        }
#endif

        /// <summary>
        /// ポスタライズ処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="level">レベル</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectPosterize(this WriteableBitmap bmp, byte level)
        {
            return ProcessEffect(bmp, new PosterizeEffect(level));
        }

        /// <summary>
        /// 彩度の調整処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="saturation">彩度の調整量(0.0～1.0 標準:0.5)</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectSaturation(this WriteableBitmap bmp, double saturation)
        {
            return ProcessEffect(bmp, new SaturationEffect(saturation));
        }

#if WINDOWS_STORE_APPS
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static WriteableBitmap EffectBinarization(this WriteableBitmap bmp)
        {
            return EffectBinarization(bmp, 85);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bmp"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static WriteableBitmap EffectBinarization(this WriteableBitmap bmp, int threshold)
        {
            return ProcessEffect(bmp, new BinarizationEffect(threshold));
        }

        /// <summary>
        /// トイカメラ風処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> EffectToycameraAsync(this WriteableBitmap bmp)
        {
            return await EffectToycameraAsync(bmp, 0.8, 0.8, 0.7);
        }

#endif

#region EffectPixelate

        /// <summary>
        /// 指定した領域をピクセル化処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="blockRects">ピクセル化したい領域</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectPixelate(this WriteableBitmap bmp, IEnumerable<Rect> blockRects)
        {
            var effector = new List<IEffect>();
            foreach (var rect in blockRects)
            {
                var blockSize = rect.Height * 0.6;
                blockSize = Math.Round(Math.Max(blockSize, 1));
                effector.Add(new PixelateEffect(rect, (int)blockSize));
            }
            return ProcessEffect(bmp, effector);
        }

        /// <summary>
        /// 指定した領域をピクセル化処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="blockRects">ピクセル化したい領域</param>
        /// <param name="blockSize">ピクセル化の時のブロックサイズ</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectPixelate(this WriteableBitmap bmp, IEnumerable<Rect> blockRects, int blockSize)
        {
            var effector = new List<IEffect>();
            foreach (var rect in blockRects)
            {
                effector.Add(new PixelateEffect(rect, blockSize));
            }
            return ProcessEffect(bmp, effector);
        }

        /// <summary>
        /// 指定した領域をピクセル化処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="blockRect">ピクセル化したい領域</param>
        /// <param name="blockSize">ピクセル化の時のブロックサイズ</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectPixelate(this WriteableBitmap bmp, Rect blockRect, int blockSize)
        {
            return EffectPixelate(bmp, new[] { blockRect }, blockSize);
        }

        /// <summary>
        /// 指定した領域をピクセル化処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="blockX"></param>
        /// <param name="blockY"></param>
        /// <param name="blockWidth"></param>
        /// <param name="blockHeight"></param>
        /// <param name="blockSize">ピクセル化の時のブロックサイズ</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectPixelate(this WriteableBitmap bmp, int blockX, int blockY, int blockWidth, int blockHeight, int blockSize)
        {
            var blockRect = new Rect(blockX, blockY, blockWidth, blockHeight);
            return EffectPixelate(bmp, new[] { blockRect }, blockSize);
        }

#endregion
      
        /// <summary>
        /// 輪郭の抽出をおこない細線化したWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="threshold">輪郭検出時の閾値</param>
        /// <param name="stroke">線の濃さ(薄0～255濃)</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectThinning(this WriteableBitmap bmp, int threshold = 60, byte stroke = 0)
        {
            return ProcessEffect(bmp, new ThinningEffect(threshold, stroke));
        }

#if WINDOWS_STORE_APPS
        
        /// <summary>
        /// トイカメラ風処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectPixelate(this WriteableBitmap bmp, int blockSize)
        {
            var rect = new Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight);
            return EffectPixelate(bmp, rect, blockSize);
        }


        /// <summary>
        /// トイカメラ風処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitmapオブジェクト</param>
        /// <param name="contrast">コントラストの調整量(0.0～1.0 標準:0.5)</param>
        /// <param name="saturation">彩度の調整量(0.0～1.0 標準:0.5)</param>
        /// <param name="vignetting">口径食の強さの調整量(0.0～1.0 標準:1.0)</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static async Task<WriteableBitmap> EffectToycameraAsync(this WriteableBitmap bmp, double contrast, double saturation, double vignetting)
        {
            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;

            // クラスライブラリ内の画像をリソースを読み出す
            var maskBitmap = default(WriteableBitmap);
            using (var strm = GetResourceStream("Softbuild.Media.Images.vignetting_gradation.png"))
            {
                // StreamからWriteableBitmapを生成する
                maskBitmap = await WriteableBitmapLoadExtensions.FromStreamAsync(strm);
            }
            // 元画像とサイズと合わせる
            var resizedBmp = maskBitmap.Resize(width, height);

            var effectors = new List<IEffect>();
            effectors.Add(new ContrastEffect(contrast));
            effectors.Add(new SaturationEffect(saturation));
            effectors.Add(new VignettingEffect(resizedBmp, vignetting));

            return ProcessEffect(bmp, effectors);
        }

        /// <summary>
        /// 自動着色処理をしたWriteableBitmapオブジェクトを返す
        /// </summary>
        /// <param name="bitmap">元になるWriteableBitmapオブジェクト</param>
        /// <returns>処理後のWriteableBitmapオブジェクト</returns>
        public static WriteableBitmap EffectAutoColoring(this WriteableBitmap bmp)
        {
            var effect = default(IEffect);
            using (var strm = GetResourceStream("Softbuild.Media.Files.default_hosei.cur"))
            {
                effect = new AutoColoringEffect(strm, CurveTypes.Gimp);
            }
            return ProcessEffect(bmp, effect);
        }  
#endif
    }
}
