using System.Collections.Generic;
using System.Text;

namespace XamarinAndroidFFmpeg
{


	public abstract class VideoFilter
	{

		public abstract string FilterString {get;}

		public static string Build(List<VideoFilter> listFilters)
		{
			StringBuilder result = new StringBuilder();

			IEnumerator<VideoFilter> it = listFilters.GetEnumerator();
			VideoFilter vf;

			while (it.MoveNext())
			{
				vf = it.Current;
				result.Append(vf.FilterString).Append(", ");
			}

			var res = result.ToString();
			if (res.Substring (res.Length - 2) == ", ") {
				res = res.Substring (0, res.Length - 2);
			}

			//return @"'" + res + @"'";
			return res;
		}
	}

}