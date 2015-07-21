using System;
using Java.IO;

namespace XamarinAndroidFFmpeg
{

	/// <summary>
	/// @class overlay overlay one image or video on top of another
	/// 
	/// @desc x is the x coordinate of the overlayed video on the main video, 
	/// y is the y coordinate. The parameters are expressions containing 
	/// the following parameters:
	/// <pre>
	///      	main_w, main_h
	///          main input width and height
	/// 
	///       W, H
	///           same as main_w and main_h
	/// 
	///       overlay_w, overlay_h
	///           overlay input width and height
	/// 
	///       w, h
	///           same as overlay_w and overlay_h
	/// </pre>          
	/// @examples
	/// <pre>draw the overlay at 10 pixels from the bottom right
	/// corner of the main video.
	/// 		main_w-overlay_w-10
	/// 		main_h-overlay_h-10
	/// draw the overlay in the bottom left corner of the input
	///  10
	///  main_h-overlay_h-10 [out]</pre>
	/// 
	/// </summary>
	public class OverlayVideoFilter : VideoFilter
	{

		public File overlayFile;
		public string xParam, yParam;

		public OverlayVideoFilter()
		{

		}

		public OverlayVideoFilter(File fileMovieOverlay, int x, int y)
		{
			this.overlayFile = fileMovieOverlay;
			this.xParam = Convert.ToString(x);
			this.yParam = Convert.ToString(y);
		}

		public OverlayVideoFilter(File fileMovieOverlay, string xExpression, string yExpression)
		{
			this.overlayFile = fileMovieOverlay;
			this.xParam = xExpression;
			this.yParam = yExpression;
		}

		public override string FilterString
		{
			get
			{
				if (overlayFile != null)
				{
					return "movie=" + overlayFile.AbsolutePath + " [logo];[in][logo] " + "overlay=" + xParam + ":" + yParam + " [out]";
				}
				else
				{
					return "";
				}
    
			}
		}
	}

	//"\"movie="+ overlayImage.getPath() +" [logo];[in][logo] overlay=0:0 [out]\"",
}