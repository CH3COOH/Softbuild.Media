﻿//
// MainPage.xaml.cs
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
using System.Threading.Tasks;
using Softbuild.Media; // for Bitmap Effect
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;

namespace EffectSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            slider.Value = 50;
        }

        private async Task<WriteableBitmap> GetTestImageAsync()
        {
            return await GetTestImageAsync("lenna.PNG");
        }

        private async Task<WriteableBitmap> GetTestImageAsync(string name)
        {
            // クラスライブラリ内の画像をリソースを読み出す
            var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/" + name));
            // StorageFileからWriteableBitmapを生成する
            return await WriteableBitmapExtensions.FromStreamAsync(await imageFile.OpenReadAsync());
        }

        private async Task<WriteableBitmap> GetTestMonochromeImageAsync()
        {
            return await GetTestImageAsync("lenna_monochrome.jpg");
        }

        private async void btnNegative_Click(object sender, RoutedEventArgs e)
        {
            // テスト用の画像を非同期で取得する
            WriteableBitmap bitmap = await GetTestImageAsync();

            // ネガポジ反転してImageコントロールのSourceプロパティに設定する
            imageDst.Source = bitmap.EffectNegative();
        }

        private async void btnGrayscale_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = await GetTestImageAsync();
            imageDst.Source = bitmap.EffectGrayscale();
        }

        private async void btnSaturation_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = await GetTestImageAsync();
            imageDst.Source = bitmap.EffectSaturation(1.0);
        }

        private async void btnBakumatsu_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = await GetTestImageAsync();
            imageDst.Source = await bitmap.EffectBakumatsuAsync();
        }

        private async void btnVignetting_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = await GetTestImageAsync();
            imageDst.Source = await bitmap.EffectVignettingAsync(1.0);
        }

        private async void btnSepia_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = await GetTestImageAsync();
            imageDst.Source = bitmap.EffectSepia();
        }

        private async void btnBrightness_Click(object sender, RoutedEventArgs e)
        {
            var value = slider.Value / 100.0;

            var bitmap = await GetTestImageAsync();
            imageDst.Source = bitmap.EffectBrightness(value);
        }

        private async void btnContrast_Click(object sender, RoutedEventArgs e)
        {
            var value = slider.Value / 100.0;

            var bitmap = await GetTestImageAsync();
            imageDst.Source = bitmap.EffectContrast(value);
        }

        private async void btnToycamera_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = await GetTestImageAsync();
            imageDst.Source = await bitmap.EffectToycameraAsync();
        }

        private async void btnPosterize_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = await GetTestImageAsync();
            imageDst.Source = bitmap.EffectPosterize(255);
        }

        private async void btnAutoColoring_Click(object sender, RoutedEventArgs e)
        {
            // グレースケールのテスト画像を取得する
            var bitmap = await GetTestMonochromeImageAsync();
            // 自動で疑似着色した画像をImageコントロールに表示する
            imageDst.Source = bitmap.EffectAutoColoring();
        }

        private async void btnSaveJpeg_Click(object sender, RoutedEventArgs e)
        {
            // Imageコントロールに表示されているWriteableBitmapオブジェクトを取り出す
            var bitmap = imageDst.Source as WriteableBitmap;
            if (bitmap != null)
            {
                // ピクチャーライブラリから「effect_sample」という名前のJPEGファイルを
                // 幅320x高さ280のサイズに収まるサイズで保存します
                await bitmap.SaveAsync(ImageDirectories.PicturesLibrary, 
                    ImageFormat.Jpeg, "effect_sample", 320, 280);
            }
        }

        private async void btnSavePng_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = imageDst.Source as WriteableBitmap;
            if (bitmap != null)
            {
                await bitmap.SaveAsync(ImageDirectories.InApplicationLocal, ImageFormat.Png, "effect_sample");
                var bmp = await WriteableBitmapExtensions.LoadAsync(ImageDirectories.InApplicationLocal, ImageFormat.Png, "effect_sample");
            }
        }

        private async void btnPixelate_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = await GetTestImageAsync();
            Debug.WriteLine("st: " + DateTime.Now.ToString("mm:ss ffff"));
            for (int i = 0; i < 100; i++)
            {
                //var hoge = bitmap.EffectPixelate(10, 10, 400, 200, 50);
            }
            Debug.WriteLine("en: " + DateTime.Now.ToString("mm:ss ffff"));

            imageDst.Source = bitmap.EffectPixelate(50, 50, 300, 250, 40);
        }

        private async void btnThinning_Click(object sender, RoutedEventArgs e)
        {
            var slide = int.Parse(textValue.Text);
            var value = slide;

            var bitmap = await GetTestImageAsync();
            imageDst.Source = bitmap.EffectThinning((int)value, 100);
        }

        private async void btnBinarization_Click(object sender, RoutedEventArgs e)
        {
            var slide = int.Parse(textValue.Text);
            var value = slide;

            var bitmap = await GetTestImageAsync();
            imageDst.Source = bitmap.EffectBinarization((int)value);
        }

        private async void btnComic_Click(object sender, RoutedEventArgs e)
        {
            var slide = int.Parse(textValue.Text);
            var value = slide;

            var bitmap = await GetTestImageAsync();
            imageDst.Source = bitmap.EffectCartoonize((int)value, 255);
        }

        private void btnEmbossment_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Slider_ValueChanged_1(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (textValue != null)
                textValue.Text = string.Format("{0}", e.NewValue);
        }

    }
}
