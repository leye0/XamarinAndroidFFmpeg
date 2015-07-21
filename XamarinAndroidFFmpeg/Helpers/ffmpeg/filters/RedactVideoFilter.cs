using Java.IO;

namespace XamarinAndroidFFmpeg
{

	public class RedactVideoFilter : VideoFilter
	{

		private File fileRedactList;

		public RedactVideoFilter(File fileRedactList)
		{
			this.fileRedactList = fileRedactList;
		}

		public override string FilterString
		{
			get
			{
				if (fileRedactList != null)
				{
					return "redact=" + fileRedactList.AbsolutePath;
				}
				else
				{
					return "";
				}
    
			}
		}
	}

	//redact=blurbox.txt [out] [d], [d]nullsink
	//"redact=" + Environment.getExternalStorageDirectory().getPath() + "/" + PACKAGENAME + "/redact_unsort.txt",
}