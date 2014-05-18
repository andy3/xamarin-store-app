using System;
using MonoTouch.UIKit;

namespace XamarinStore
{
	public class SelfieButton : UIControl
	{
		static Lazy<UIImage> SelfieButtonImage = new Lazy<UIImage>(() => UIImage.FromBundle("cam"));
		UIImageView imageView;

		public SelfieButton()
		{
			imageView = new UIImageView(SelfieButtonImage.Value.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate)){
				TintColor = UIColor.White,
			};
			this.AddSubview(imageView);
		}

		const float padding = 5;
		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			var bounds = this.Bounds;

			bounds.Y += padding;
			bounds.Width -= padding*2;
			bounds.Height -= padding*2;

			imageView.Frame = bounds;
		}
	}
}

