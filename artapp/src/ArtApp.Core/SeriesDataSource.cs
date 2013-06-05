using System;
using MonoTouch.UIKit;
using System.Linq;
using System.Drawing;
using MonoTouch.MessageUI;
using System.Collections.Generic;
using System.Diagnostics;
using MonoTouch.Foundation;

namespace ArtApp
{
	public class SeriesDataSource
	{		
		public Series Series { get;	private set; }
		
		private SeriesViewController _parent;
		
		public SeriesDataSource(Series series, SeriesViewController parent)
		{
			Series = series;	
			_parent = parent;	
		}
				
		public PieceViewController GetPage(int i)
		{
			var coll = Series;
			var piece = coll.Pieces[i];
			
			RectangleF canvasFrame;
			var imageView = ImageFactory.BuildGalleryPage(piece, i, coll.Pieces.Count, out canvasFrame);
			imageView.ClipsToBounds = false;
							
			PieceViewController pvc = new PieceViewController(piece, canvasFrame, _parent, imageView);
			pvc.View.Frame = imageView.Frame;
			pvc.View.AddSubview(imageView);
			pvc.View.ClipsToBounds = false;
			
			RectangleF frame = GetGalleryDimensions();
			imageView.Bounds = frame;
			imageView.Frame = frame;
						
			return pvc;
		}

		private RectangleF GetGalleryDimensions()
		{
			RectangleF frame;
			if(Util.IsPortrait())
			{
				frame = new RectangleF(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - DimensionSet.StatusBarHeight);
			}
			else
			{
				frame = new RectangleF(0, 0, UIScreen.MainScreen.Bounds.Height, UIScreen.MainScreen.Bounds.Width - DimensionSet.StatusBarHeight);
			}
			return frame;
		}		

		public int Pages { get { return Series.Pieces.Count; } }
	}
}

