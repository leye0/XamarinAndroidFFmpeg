using System.Text;

namespace XamarinAndroidFFmpeg
{

	public class ColorVideoFilter : VideoFilter
	{

		//			//		   mp=eq2=gamma:contrast:brightness:saturation:rg:gg:bg:weight
		//			//			cmd.Add ("'mp=eq2=0.5:0.68:0.6:0.46:1:0.96:1'");
		public ColorVideoFilter(decimal gamma = 1.0m, decimal contrast = 1.0m, decimal brightness = 0.0m, decimal saturation = 1.0m, decimal redGamma = 1.0m, decimal greenGamma = 1.0m, decimal blueGamma = 1.0m, decimal weight = 1.0m)
		{
			_filterString = string.Format ("mp=eq2={0}:{1}:{2}:{3}:{4}:{5}", gamma, contrast, brightness, saturation, redGamma, greenGamma, blueGamma, weight);
		}

		string _filterString = "";

		public override string FilterString
		{ 
			get { 
				return _filterString;
			}
		}
	}

	///fade=in:0:25, fade=out:975:25


}