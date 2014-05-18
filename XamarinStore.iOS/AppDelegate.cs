using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.MessageUI;
using BigTed;
using MonoTouch.Twitter;

namespace XamarinStore.iOS
{
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		public static AppDelegate Shared;

		UIWindow window;
		UINavigationController navigation;

		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			Shared = this;
			FileCache.SaveLocation = System.IO.Directory.GetParent (Environment.GetFolderPath (Environment.SpecialFolder.Personal)).ToString () + "/tmp";

			UIApplication.SharedApplication.SetStatusBarStyle (UIStatusBarStyle.LightContent, false);

			window = new UIWindow (UIScreen.MainScreen.Bounds);
			UINavigationBar.Appearance.SetTitleTextAttributes (new UITextAttributes {
				TextColor = UIColor.White
			});

			var productVc = new ProductListViewController ();
			productVc.ProductTapped += ShowProductDetail;
			navigation = new UINavigationController (productVc);

			navigation.NavigationBar.TintColor = UIColor.White;
			navigation.NavigationBar.BarTintColor = Color.Blue;

			window.RootViewController = navigation;
			window.MakeKeyAndVisible ();
			return true;
		}

		public void ShowProductDetail (Product product)
		{
			var productDetails = new ProductDetailViewController (product);
			productDetails.AddToBasket += p => {
				WebService.Shared.CurrentOrder.Add (p);
				UpdateProductsCount();
			};
			navigation.PushViewController (productDetails, true);
		}

		public void ShowBasket ()
		{
			var basketVc = new BasketViewController (WebService.Shared.CurrentOrder);
			basketVc.Checkout += (object sender, EventArgs e) => ShowLogin ();
			navigation.PushViewController (basketVc, true);
		}

		public void ShowLogin ()
		{
			var loginVc = new LoginViewController ();
			loginVc.LoginSucceeded += () => ShowAddress ();
			navigation.PushViewController (loginVc, true);
		}

		public void ShowAddress ()
		{
			var addreesVc = new ShippingAddressViewController (WebService.Shared.CurrentUser);
			addreesVc.ShippingComplete += (object sender, EventArgs e) => ProccessOrder ();
			navigation.PushViewController (addreesVc, true);
		}

		public void ProccessOrder()
		{
			var processing = new ProcessingViewController (WebService.Shared.CurrentUser);
			processing.OrderPlaced += (object sender, EventArgs e) => {
				OrderCompleted ();
			};
			navigation.PresentViewController (new UINavigationController(processing), true, null);
		}

		public void OrderCompleted ()
		{
			navigation.PopToRootViewController (true);
		}

		BasketButton button;
		public UIBarButtonItem CreateBasketButton ()
		{
			if (button == null) {
				button = new BasketButton () {
					Frame = new RectangleF (0, 0, 44, 44),
				};
				button.TouchUpInside += (sender, args) => ShowBasket ();
			}
			button.ItemsCount = WebService.Shared.CurrentOrder.Products.Count;
			return new UIBarButtonItem (button);
		}

		public void UpdateProductsCount()
		{
			button.UpdateItemsCount(WebService.Shared.CurrentOrder.Products.Count);
		}

		public void SelfieShoot()
		{
			UIImagePickerController imagePickerController = new UIImagePickerController ();
			imagePickerController.FinishedPickingMedia += HandleFinishedPickingMedia;
			imagePickerController.Canceled += (object sender, EventArgs e) => navigation.DismissViewController (true, null);
			imagePickerController.SourceType = UIImagePickerControllerSourceType.Camera;
			imagePickerController.AllowsEditing = false;
			if(UIImagePickerController.IsCameraDeviceAvailable(UIImagePickerControllerCameraDevice.Front))
				imagePickerController.CameraDevice = UIImagePickerControllerCameraDevice.Front;

			while (UIDevice.CurrentDevice.GeneratesDeviceOrientationNotifications)
				UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
			navigation.PresentViewController(imagePickerController, false, null);
			while (UIDevice.CurrentDevice.GeneratesDeviceOrientationNotifications)
				UIDevice.CurrentDevice.EndGeneratingDeviceOrientationNotifications();
		}

		void HandleFinishedPickingMedia (object sender, UIImagePickerMediaPickedEventArgs e)
		{
			navigation.DismissViewController (true, ()=>SendTweet(e.OriginalImage));
			BTProgressHUD.Show ();
		}

		private void SendMail(UIImage imageAttachment)
		{
			MFMailComposeViewController mailController = new MFMailComposeViewController ();
			mailController.SetToRecipients (new string[]{"hello@xamarin.com"});
			mailController.SetSubject ("Xamarin shirt selfie");
			mailController.SetMessageBody ("Hi Xamarin\n\nTake a look at my brandnew xamarin Shirt!", false);
			mailController.AddAttachmentData (imageAttachment.AsJPEG(), "jpeg", "Xamarin-Selfie.jpg");

			mailController.Finished += ( object s, MFComposeResultEventArgs args) => {
				Console.WriteLine (args.Result.ToString ());
				args.Controller.DismissViewController (true, null);
			};

			BTProgressHUD.Dismiss ();
			navigation.PresentViewController (mailController, true, null);

		}

		private void SendTweet(UIImage imageAttachment)
		{
			var tvc = new TWTweetComposeViewController();
			tvc.SetInitialText("Got a brandnew C# shirt from Xamarin.\nThank you @xamarinhq");
			tvc.AddImage (imageAttachment);
			BTProgressHUD.Dismiss ();

			tvc.SetCompletionHandler((TWTweetComposeViewControllerResult r)=>{
				navigation.DismissViewController(true,null);
				if (r == TWTweetComposeViewControllerResult.Cancelled){
					BTProgressHUD.ShowErrorWithStatus("Cancelled");
				} else {
					BTProgressHUD.ShowSuccessWithStatus("Sent");
				}
			});

			navigation.PresentViewController(tvc, true, null);
		}

		SelfieButton selfieButton;
		public UIBarButtonItem CreateSelfieButton ()
		{
			if (selfieButton == null) {
				selfieButton = new SelfieButton () {
					Frame = new RectangleF (0, 0, 44, 44),
				};
				selfieButton.TouchUpInside += (sender, e) => SelfieShoot();
			}
			return new UIBarButtonItem (selfieButton);
		}
	}
}
