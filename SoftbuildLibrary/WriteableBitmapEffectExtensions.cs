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
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace Softbuild.Media
{
    public static class  WriteableBitmapEffectExtensions
    {
        /// <summary>
        /// パラメータ無しの画像処理をおこなう
        /// </summary>
        /// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        /// <param name="effector">処理させるIEffectオブジェクト</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        private static WriteableBitmap Effect(WriteableBitmap bmp, IEffect effector)
        {
            // WriteableBitampのピクセルデータをバイト配列に変換する
            var srcPixels = bmp.PixelBuffer.ToArray();

            // パラメータ無しの画像処理をおこなう
            var dstPixels = effector.Effect(bmp.PixelWidth, bmp.PixelHeight, srcPixels);

            // バイト配列からピクセルを作成する
            return WriteableBitmapExtensions.FromArray(bmp.PixelWidth, bmp.PixelHeight, dstPixels);
        }

        /// <summary>
        /// パラメータ無しの画像処理をおこなう
        /// </summary>
        /// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        /// <param name="effectors">処理させるIEffectオブジェクト配列</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        private static WriteableBitmap EffectArray(WriteableBitmap bmp, IEnumerable<IEffect> effectors)
        {
            var pixels = bmp.PixelBuffer.ToArray();
            var width = bmp.PixelWidth;
            var height = bmp.PixelHeight;

            foreach (var effector in effectors)
            {
                pixels = effector.Effect(width, height, pixels);
            }

            return WriteableBitmapExtensions.FromArray(width, height, pixels);
        }

        /// <summary>
        /// アセンブリ内のリソースのストリームを取得する
        /// </summary>
        /// <param name="resourceName">リソース名</param>
        /// <returns>ストリーム</returns>
        private static System.IO.Stream GetResourceStream(string resourceName)
        {
            var asm = Assembly.Load(new AssemblyName("SoftbuildLibrary"));
            return asm.GetManifestResourceStream(resourceName);
        }

        /// <summary>
        /// 白黒反転処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        public static WriteableBitmap EffectNegative(this WriteableBitmap bmp)
        {
            return Effect(bmp, new NegativeEffect());
        }

        /// <summary>
        /// グレイスケール処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        public static WriteableBitmap EffectGrayscale(this WriteableBitmap bmp)
        {
            return Effect(bmp, new GrayscaleEffect());
        }

        ///// <summary>
        ///// ぼかし処理をしたWriteableBitampオブジェクトを返す
        ///// </summary>
        ///// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        ///// <param name="range">ぼかしの強さ</param>
        ///// <returns>処理後のWriteableBitampオブジェクト</returns>
        //public static WriteableBitmap EffectBlur(this WriteableBitmap bitmap, int range)
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// セピア調処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        public static WriteableBitmap EffectSepia(this WriteableBitmap bmp)
        {
            return Effect(bmp, new SepiaEffect());
        }

        /// <summary>
        /// コントラストの調整処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        /// <param name="contrast">コントラストの調整量(0.0～1.0 標準:0.5)</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        public static WriteableBitmap EffectContrast(this WriteableBitmap bmp, double contrast)
        {
            return Effect(bmp, new ContrastEffect(contrast));
        }

        /// <summary>
        /// 幕末写真風処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        public static async Task<WriteableBitmap> EffectBakumatsuAsync(this WriteableBitmap bmp)
        {
            // クラスライブラリ内の画像をリソースを読み出す
            var maskBitamp = default(WriteableBitmap);
            using (var strm = GetResourceStream("Softbuild.Images.bakumatsu.jpg"))
            {
                // StreamからWriteableBitampを生成する
                maskBitamp = await WriteableBitmapExtensions.FromStreamAsync(strm);
            }
            // 元画像とサイズと合わせる
            var resizedBmp = maskBitamp.Resize(bmp.PixelWidth, bmp.PixelHeight);

            // 幕末画像を作成する
            return Effect(bmp, new BakumatsuEffect(resizedBmp));
        }

        /// <summary>
        /// 口径食風の処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        /// <param name="vignetting">口径食の強さ(0.0～1.0 標準:1.0)</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        public static async Task<WriteableBitmap> EffectVignettingAsync(this WriteableBitmap bmp, double vignetting)
        {
            // クラスライブラリ内の画像をリソースを読み出す
            var maskBitamp = default(WriteableBitmap);
            using (var strm = GetResourceStream("Softbuild.Images.vignetting_gradation.png"))
            {
                // StreamからWriteableBitampを生成する
                maskBitamp = await WriteableBitmapExtensions.FromStreamAsync(strm);
            }
            // 元画像とサイズと合わせる
            var resizedBmp = maskBitamp.Resize(bmp.PixelWidth, bmp.PixelHeight);

            // 口径食による周辺光量の低下風の処理をしたビットマップを作成する
            return Effect(bmp, new VignettingEffect(resizedBmp, vignetting));
        }

        /// <summary>
        /// ポスタライズ処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        /// <param name="level">レベル</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        public static WriteableBitmap EffectPosterize(this WriteableBitmap bmp, byte level)
        {
            return Effect(bmp, new PosterizeEffect(level));
        }

        /// <summary>
        /// 彩度の調整処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        /// <param name="saturation">彩度の調整量(0.0～1.0 標準:0.5)</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        public static WriteableBitmap EffectSaturation(this WriteableBitmap bmp, double saturation)
        {
            return Effect(bmp, new SaturationEffect(saturation));
        }


        /// <summary>
        /// トイカメラ風処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        public static async Task<WriteableBitmap> EffectToycameraAsync(this WriteableBitmap bmp)
        {
            return await EffectToycameraAsync(bmp, 0.8, 0.8, 0.7);
        }

        /// <summary>
        /// トイカメラ風処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bmp">元になるWriteableBitampオブジェクト</param>
        /// <param name="contrast">コントラストの調整量(0.0～1.0 標準:0.5)</param>
        /// <param name="saturation">彩度の調整量(0.0～1.0 標準:0.5)</param>
        /// <param name="vignetting">口径食の強さの調整量(0.0～1.0 標準:1.0)</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        public static async Task<WriteableBitmap> EffectToycameraAsync(this WriteableBitmap bmp, double contrast, double saturation, double vignetting)
        {
            int width = bmp.PixelWidth;
            int height = bmp.PixelHeight;

            // クラスライブラリ内の画像をリソースを読み出す
            var maskBitamp = default(WriteableBitmap);
            using (var strm = GetResourceStream("Softbuild.Images.vignetting_gradation.png"))
            {
                // StreamからWriteableBitampを生成する
                maskBitamp = await WriteableBitmapExtensions.FromStreamAsync(strm);
            }
            // 元画像とサイズと合わせる
            var resizedBmp = maskBitamp.Resize(width, height);

            var effectors = new List<IEffect>();
            effectors.Add(new ContrastEffect(contrast));
            effectors.Add(new SaturationEffect(saturation));
            effectors.Add(new VignettingEffect(resizedBmp, vignetting));

            return EffectArray(bmp, effectors);
        }

        /// <summary>
        /// 自動着色処理をしたWriteableBitampオブジェクトを返す
        /// </summary>
        /// <param name="bitmap">元になるWriteableBitampオブジェクト</param>
        /// <returns>処理後のWriteableBitampオブジェクト</returns>
        public static WriteableBitmap EffectAutoColoring(this WriteableBitmap bmp)
        {
            var effect = default(IEffect);
            using (var strm = GetResourceStream("Softbuild.Files.default_hosei.cur"))
            {
                effect = new AutoColoringEffect(strm, CurveTypes.Gimp);
            }
            return Effect(bmp, effect);
        }
    }
}
