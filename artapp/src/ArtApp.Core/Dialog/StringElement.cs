using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using System.Drawing;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	public class StringElement : Element {
		static NSString skey = new NSString ("StringElement");
		static NSString skeyvalue = new NSString ("StringElementValue");
		public UITextAlignment Alignment = UITextAlignment.Left;
		public string Value;
		
		public StringElement (string caption) : base (caption) {}
		
		public StringElement (string caption, string value) : base (caption)
		{
			this.Value = value;
		}
		
		public StringElement (string caption,  NSAction tapped) : base (caption)
		{
			Tapped += tapped;
		}
		
		public event NSAction Tapped;
				
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (Value == null ? skey : skeyvalue);
			if (cell == null)
			{
				cell = new UITableViewCell (Value == null ? UITableViewCellStyle.Default : UITableViewCellStyle.Value1, skey);
				cell.SelectionStyle = (Tapped != null) ? UITableViewCellSelectionStyle.Blue : UITableViewCellSelectionStyle.None;
			}
			cell.Accessory = UITableViewCellAccessory.None;
			cell.TextLabel.Text = Caption;
			cell.TextLabel.TextAlignment = Alignment;
			
			// The check is needed because the cell might have been recycled.
			if (cell.DetailTextLabel != null)
			{
				cell.DetailTextLabel.Text = Value == null ? "" : Value;
			}
			
			return cell;
		}

		public override string Summary ()
		{
			return Caption;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath indexPath)
		{
			if (Tapped != null)
			{
				Tapped();
			}
			tableView.DeselectRow (indexPath, true);
		}
		
		public override bool Matches (string text)
		{
			return (Value != null ? Value.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1: false) || base.Matches (text);
		}
	}
	
	/// <summary>
	///   This interface is implemented by Element classes that will have
	///   different heights
	/// </summary>
	
	/// <summary>
	///   This interface is implemented by Elements that needs to update
	///   their cells Background properties just before they are displayed
	///   to the user.   This is an iOS 3 requirement to properly render
	///   a cell.
	/// </summary>
	
	/// <summary>
	///   This element can be used to insert an arbitrary UIView
	/// </summary>
	/// <remarks>
	///   There is no cell reuse here as we have a 1:1 mapping
	///   in this case from the UIViewElement to the cell that
	///   holds our view.
	/// </remarks>
	
	/// <summary>
	/// Sections contain individual Element instances that are rendered by MonoTouch.Dialog
	/// </summary>
	/// <remarks>
	/// Sections are used to group elements in the screen and they are the
	/// only valid direct child of the RootElement.    Sections can contain
	/// any of the standard elements, including new RootElements.
	/// 
	/// RootElements embedded in a section are used to navigate to a new
	/// deeper level.
	/// 
	/// You can assign a header and a footer either as strings (Header and Footer)
	/// properties, or as UIViews to be shown (HeaderView and FooterView).   Internally
	/// this uses the same storage, so you can only show one or the other.
	/// </remarks>
	
	/// <summary>
	/// Used by root elements to fetch information when they need to
	/// render a summary (Checkbox count or selected radio group).
	/// </summary>
	
	/// <summary>
	/// Captures the information about mutually exclusive elements in a RootElement
	/// </summary>
	
	/// <summary>
	///    RootElements are responsible for showing a full configuration page.
	/// </summary>
	/// <remarks>
	///    At least one RootElement is required to start the MonoTouch.Dialogs
	///    process.   
	/// 
	///    RootElements can also be used inside Sections to trigger
	///    loading a new nested configuration page.   When used in this mode
	///    the caption provided is used while rendered inside a section and
	///    is also used as the Title for the subpage.
	/// 
	///    If a RootElement is initialized with a section/element value then
	///    this value is used to locate a child Element that will provide
	///    a summary of the configuration which is rendered on the right-side
	///    of the display.
	/// 
	///    RootElements are also used to coordinate radio elements.  The
	///    RadioElement members can span multiple Sections (for example to
	///    implement something similar to the ring tone selector and separate
	///    custom ring tones from system ringtones).
	/// 
	///    Sections are added by calling the Add method which supports the
	///    C# 4.0 syntax to initialize a RootElement in one pass.
	/// </remarks>
	
}
