WriteableBitmapEffector
=======================

SoftbuildLibrary is WriteableBitmap Effector for Metro Style apps.

Using SoftbuildLibrary on Metro Style Apps
----------------------------------------

 *  This source repository includes WriteableBitmap effector so that you can get started easily! More documents you can see on [this article(English)](http://d.hatena.ne.jp/ch3cooh393/20120810/1344588939) or [Japanese article](http://d.hatena.ne.jp/ch3cooh393/20120810/1344587748).

Examples
----------------------------------------

Please add the following to the using directive.

    using Softbuild.Media;

## Resize

    var bitmap = await GetTestImageAsync();
    var resizedBmp = bitmap.Resize(640, 480);

## Effects

### Grayscale

    private async void btnGrayscale_Click(object sender, RoutedEventArgs e) {
        var bitmap = await GetTestImageAsync();
        imageDst.Source = bitmap.EffectGrayscale();
    }

### Sepia

    private async void btnSepia_Click(object sender, RoutedEventArgs e) {
        var bitmap = await GetTestImageAsync();
        imageDst.Source = bitmap.EffectSepia();
    }

### Contrast

    private async void btnContrast_Click(object sender, RoutedEventArgs e) {
        var bitmap = await GetTestImageAsync();
        imageDst.Source = bitmap.EffectContrast(1.0);
    }

### Saturation

    private async void btnSaturation_Click(object sender, RoutedEventArgs e) {
        var bitmap = await GetTestImageAsync();
        imageDst.Source = bitmap.EffectSaturation(1.0);
    }

### Vignetting

    private async void btnVignetting_Click(object sender, RoutedEventArgs e) {
        var bitmap = await GetTestImageAsync();
        imageDst.Source = await bitmap.EffectVignettingAsync(1.0);
    }

### Bakumatsu Effect

    private async void btnBakumatsu_Click(object sender, RoutedEventArgs e) {
        var bitmap = await GetTestImageAsync();
        imageDst.Source = await bitmap.EffectBakumatsuAsync();
    }

![alt text](http://cdn-ak.f.st-hatena.com/images/fotolife/c/ch3cooh393/20120810/20120810172415.png)

### Toycamera Effect

    private async void btnToycamera_Click(object sender, RoutedEventArgs e) {
        var bitmap = await GetTestImageAsync();
        imageDst.Source = await bitmap.EffectToycameraAsync();
    }

![alt text](http://cdn-ak.f.st-hatena.com/images/fotolife/c/ch3cooh393/20120810/20120810172410.png)

### Auto Coloring

    private async void btnAutoColoring_Click(object sender, RoutedEventArgs e) {
        var bitmap = await GetTestMonochromeImageAsync();
        imageDst.Source = await bitmap.EffectAutoColoringAsync();
    }

## Save

### Save Jpeg to Pictures Library

    private async void btnSaveJpeg_Click(object sender, RoutedEventArgs e) {
        var bitmap = imageDst.Source as WriteableBitmap;
        await bitmap.SaveAsync(ImageFormat.Jpeg, ImageDirectories.PicturesLibrary, "effect_sample", 320, 280);
    }

### Save PNG to local in application

    private async void btnSavePng_Click(object sender, RoutedEventArgs e) {
        var bitmap = imageDst.Source as WriteableBitmap;
        await bitmap.SaveAsync(ImageFormat.Png, ImageDirectories.InApplicationLocal, "effect_sample");
    }

### Save Gif to Roaming in application

    private async void btnSavePng_Click(object sender, RoutedEventArgs e) {
        var bitmap = imageDst.Source as WriteableBitmap;
        await bitmap.SaveAsync(ImageFormat.Gif, ImageDirectories.InApplicationRoaming, "effect_sample");
    }

Change Log
----------------------------------------

 *  2012/08/16 - Release this project with Visual Studio 2012 and Windows 8
 *  2012/08/03 - Release this project with Visual Studio 2012 RC and Windows 8 Release Preview

