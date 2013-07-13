using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using Softbuild.Media;

namespace EffectSample.WinPhone71
{
    public partial class MainPage : PhoneApplicationPage
    {
        // コンストラクター
        public MainPage()
        {
            InitializeComponent();
        }

        private WriteableBitmap GetSampleImage()
        {
            //
            var info = Application.GetResourceStream(new Uri("lenna.PNG", UriKind.Relative));

            var image = new BitmapImage();
            using (var strm = info.Stream)
            {
                image.SetSource(strm);
            }
            WriteableBitmap bitmap = new WriteableBitmap(image);
            return bitmap;
        }

        private void buttonGrayscale_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = GetSampleImage();
            image1.Source = bitmap.EffectGrayscale();
        }

        private void buttonPixelate_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = GetSampleImage();
            image1.Source = bitmap.EffectPixelate(new Rect(350, 300, 200, 100), 20);
        }

        private void buttonCartoonize_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = GetSampleImage();
            image1.Source = bitmap.EffectCartoonize();
        }

        private void buttonPosterize_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = GetSampleImage();
            image1.Source = bitmap.EffectPosterize(100);
        }

        private void buttonSepia_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = GetSampleImage();
            image1.Source = bitmap.EffectSepia();
        }

        private void buttonSaturation_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = GetSampleImage();
            image1.Source = bitmap.EffectSaturation(0.8);
        }

        private void buttonThinning_Click(object sender, RoutedEventArgs e)
        {
            var bitmap = GetSampleImage();
            image1.Source = bitmap.EffectThinning(20);
        }
    }
}