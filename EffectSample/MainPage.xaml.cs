﻿﻿//
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
using Softbuild.Media;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

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

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private async Task<WriteableBitmap> GetTestImageAsync()
        {
            // クラスライブラリ内の画像をリソースを読み出す
            var imageFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/lena.PNG"));
            // StorageFileからWriteableBitampを生成する
            return await WriteableBitmapExtensions.FromStreamAsync(await imageFile.OpenReadAsync());
        }

        private async void btnNegative_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = await GetTestImageAsync();
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

        private async void btnContrast_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = await GetTestImageAsync();
            imageDst.Source = bitmap.EffectContrast(1.0);
        }

        private async void btnToycamera_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = await GetTestImageAsync();
            imageDst.Source = await bitmap.EffectToycameraAsync();
        }
    }
}
