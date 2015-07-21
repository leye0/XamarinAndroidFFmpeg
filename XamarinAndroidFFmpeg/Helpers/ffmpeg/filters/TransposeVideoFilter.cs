namespace XamarinAndroidFFmpeg
{
	/*
	 * works for video and images
	 * 0 = 90CounterCLockwise and Vertical Flip (default)
	1 = 90Clockwise
	2 = 90CounterClockwise
	3 = 90Clockwise and Vertical Flip
	 */
	public class TransposeVideoFilter : VideoFilter
	{
		private int mTranspose = -1;

		public const int NINETY_COUNTER_CLOCKWISE_AND_VERTICAL_FLIP = 0;
		public const int NINETY_CLOCKWISE = 1;
		public const int NINETY_COUNTER_CLOCKWISE = 2;
		public const int NINETY_CLOCKWISE_AND_VERTICAL_FLIP = 3;

		public TransposeVideoFilter(int transpose)
		{
			mTranspose = transpose;
		}

		public override string FilterString
		{
			get
			{
				return "transpose=" + mTranspose;
			}
		}
	}
}