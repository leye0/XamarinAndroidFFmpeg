using System.Text;
using Java.IO;

namespace XamarinAndroidFFmpeg
{

	public class DrawTextVideoFilter : VideoFilter
	{

		private string mX;
		private string mY;
		private string mText;
		private string mFontColor;
		private int mFontSize;
		private File mFileFont;
		private int mBox;
		private string mBoxColor;

		public const string X_CENTERED = "(w-text_w)/2";
		public const string Y_CENTERED = "(h-text_h-line_h)/2";

		public const string X_LEFT = "0";
		public const string Y_BOTTOM = "(h-text_h-line_h)";

		public DrawTextVideoFilter(string text)
		{
			mX = X_CENTERED;
			mY = Y_CENTERED;

			mText = text;
			mFontColor = "white";
			mFontSize = 36;
			mFileFont = new File("/system/fonts/Roboto-Regular.ttf");
			if (!mFileFont.Exists())
			{
				mFileFont = new File("/system/fonts/DroidSerif-Regular.ttf");
			}

			mBox = 1;
			mBoxColor = "black@0.5"; //0x00000000@1

		}

		public DrawTextVideoFilter(string text, string x, string y, string fontColor, int fontSize, File fontFile, bool showBox, string boxColor, string boxOpacity)
		{
			mX = x;
			mY = y;

			mText = text;
			mFontColor = fontColor;
			mFontSize = fontSize;

			mFileFont = fontFile;

			mBox = showBox? 1 : 0;
			mBoxColor = boxColor + '@' + boxOpacity;

		}

		public override string FilterString
		{
			get
			{
    
				StringBuilder result = new StringBuilder();
				result.Append("drawtext=");
				result.Append("fontfile='").Append(mFileFont.AbsolutePath).Append("':");
				result.Append("text='").Append(mText).Append("':");
				result.Append("x=").Append(mX).Append(":");
				result.Append("y=").Append(mY).Append(":");
				result.Append("fontcolor=").Append(mFontColor).Append(":");
				result.Append("fontsize=").Append(mFontSize).Append(":");
				result.Append("box=").Append(mBox).Append(":");
				result.Append("boxcolor=").Append(mBoxColor);
    
				return result.ToString();
			}
		}

	}

	/*
	 * 	//mdout.videoFilter = "drawtext=fontfile=/system/fonts/DroidSans.ttf: text='this is awesome':x=(w-text_w)/2:y=H-60 :fontcolor=white :box=1:boxcolor=0x00000000@1";
	    			
	    	File fontFile = new File("/system/fonts/Roboto-Regular.ttf");
	    	if (!fontFile.exists())
	    		fontFile = new File("/system/fonts/DroidSans.ttf");
	    	
	    	mdout.videoFilter = "drawtext=fontfile='" + fontFile.getAbsolutePath() + "':text='this is awesome':x=(main_w-text_w)/2:y=50:fontsize=24:fontcolor=white";
	    	*/

	/// 
	/// 
	/// <summary>
	/// /system/fonts
	/// 
	/// AndroidClock.ttf
	/// AndroidClock_Highlight.ttf
	/// AndroidClock_Solid.ttf
	/// AndroidEmoji.ttf
	/// AnjaliNewLipi-light.ttf
	/// Clockopia.ttf
	/// DroidNaskh-Regular-SystemUI.ttf
	/// DroidNaskh-Regular.ttf
	/// DroidSans-Bold.ttf
	/// DroidSans.ttf
	/// DroidSansArmenian.ttf
	/// DroidSansDevanagari-Regular.ttf
	/// DroidSansEthiopic-Regular.ttf
	/// DroidSansFallback.ttf
	/// DroidSansGeorgian.ttf
	/// DroidSansHebrew-Bold.ttf
	/// DroidSansHebrew-Regular.ttf
	/// DroidSansMono.ttf
	/// DroidSansTamil-Bold.ttf
	/// DroidSansTamil-Regular.ttf
	/// DroidSansThai.ttf
	/// DroidSerif-Bold.ttf
	/// DroidSerif-BoldItalic.ttf
	/// DroidSerif-Italic.ttf
	/// DroidSerif-Regular.ttf
	/// Lohit-Bengali.ttf
	/// Lohit-Kannada.ttf
	/// Lohit-Telugu.ttf
	/// MTLmr3m.ttf
	/// Roboto-Bold.ttf
	/// Roboto-BoldItalic.ttf
	/// Roboto-Italic.ttf
	/// Roboto-Light.ttf
	/// Roboto-LightItalic.ttf
	/// Roboto-Regular.ttf
	/// RobotoCondensed-Bold.ttf
	/// RobotoCondensed-BoldItalic.ttf
	/// RobotoCondensed-Italic.ttf
	/// RobotoCondensed-Regular.ttf
	/// </summary>
}