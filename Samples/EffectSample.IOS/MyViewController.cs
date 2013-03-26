using System;
using MonoTouch.UIKit;
using System.Drawing;
using Softbuild.Media;

namespace EffectSample.IOS
{
    public class MyViewController : UIViewController
    {
        UIButton button;
        UIImageView imageView;
        float buttonWidth = 200;
        float buttonHeight = 50;

        public MyViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.Frame = UIScreen.MainScreen.Bounds;
            View.BackgroundColor = UIColor.White;
            View.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;

           
            var imageViewFrame = new Rectangle(
                0, 0, 200, 200

                );
            imageView = new UIImageView(imageViewFrame);
            using (var image = UIImage.FromFile("nekoicon114.png"))
            {
                imageView.Image = image;
            }
            View.AddSubview(imageView);

            button = UIButton.FromType(UIButtonType.RoundedRect);

            button.Frame = new RectangleF(
                View.Frame.Width / 2 - buttonWidth / 2,
                View.Frame.Height / 2 - buttonHeight / 2,
                buttonWidth,
                buttonHeight);

            button.SetTitle("Click me", UIControlState.Normal);

            button.TouchUpInside += (object sender, EventArgs e) =>
            {
                //button.SetTitle(String.Format("clicked {0} times", numClicks++), UIControlState.Normal);

                using (var image = UIImage.FromFile("nekoicon114.png"))
                {
                    imageView.Image = image.EffectSepia();
                }
            };

            button.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin |
                UIViewAutoresizing.FlexibleBottomMargin;

            View.AddSubview(button);
        }

    }
}

