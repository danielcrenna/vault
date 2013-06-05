using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace ArtApp
{
	// http://stackoverflow.com/questions/2757489/uisplitviewcontroller-programmtically-without-nib-xib-thank-you
	// http://lsd.luminis.nl/building-ipad-applications-using-monotouch-the-uisplitview/
	// http://stackoverflow.com/questions/7883375/uisplitviewcontroller-with-monotouch-dialog
	// http://stackoverflow.com/questions/5691062/in-monotouch-how-do-i-change-the-detail-view-controller-based-on-row-selection
		
	public class DetailViewController : UIViewController
    {
        private UIPopoverController _pc;
        private string _detail;

        public UIPopoverController Popover
		{
            get { return _pc; }
            set { _pc = value; }
        }

        public string Detail
		{
            get
			{ 
				return _detail;
			}
            set 
			{
                _detail = value;
                
                if (_pc != null)
				{
                    _pc.Dismiss(true);
                }                
            }
        }

        public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
        {
            return true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
			this.NavigationController.NavigationBar.TopItem.Title = "Detail";
        }

        public void AddNavBarButton(UIBarButtonItem button)
        {
            button.Title = "Master List";
			
			//button.Clicked += delegate {
				//Popover.PresentFromBarButtonItem(button, UIPopoverArrowDirection.Down, true);	
			//};
			
			this.NavigationController.NavigationBar.TopItem.SetLeftBarButtonItem (button, false);
        }

        public void RemoveNavBarButton()
        {
            this.NavigationController.NavigationBar.TopItem.SetLeftBarButtonItem (null, false);
        }        
    }
}

