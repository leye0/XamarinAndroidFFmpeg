using System.Text;

namespace XamarinAndroidFFmpeg
{

	public class FadeVideoFilter : VideoFilter
	{

		private string mAction; //in our out
		private int mStart;
		private int mLength;

		public FadeVideoFilter(string action, int start, int length)
		{
			mAction = action;
			mStart = start;
			mLength = length;
		}

		public override string FilterString
		{
			get
			{
    
				StringBuilder result = new StringBuilder();
				result.Append("fade=");
				result.Append (mAction).Append (':').Append (mStart).Append (":").Append (mLength);
    
				return result.ToString();
			}
		}
	}

	///fade=in:0:25, fade=out:975:25


}