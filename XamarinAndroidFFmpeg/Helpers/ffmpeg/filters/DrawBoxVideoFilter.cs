using System;
using Java.IO;
using Microsoft.Win32.SafeHandles;

namespace XamarinAndroidFFmpeg
{


	using FileStream = System.IO.FileStream;
	using Context = Android.Content.Context;
	using Bitmap = Android.Graphics.Bitmap;
	using Config = Android.Graphics.Bitmap.Config;
	using Canvas = Android.Graphics.Canvas;
	using Color = Android.Graphics.Color;
	using Paint = Android.Graphics.Paint;

	public class DrawBoxVideoFilter : OverlayVideoFilter
	{

		public int x;
		public int y;
		public int width;
		public int height;
		public string color;

		public DrawBoxVideoFilter(int x, int y, int width, int height, int alpha, string color, string tmpDir)
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
			this.color = color;

			if (alpha < 0 || alpha > 255)
			{
				throw new System.ArgumentException("Alpha must be an integer betweeen 0 and 255");
			}
			Paint paint = new Paint();
			paint.Alpha = alpha;


			Bitmap bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
			bitmap.EraseColor(Color.ParseColor(color));
			Bitmap temp_box = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
			Canvas canvas = new Canvas(temp_box);
			canvas.DrawBitmap(bitmap, 0, 0, paint);

			File outputFile;
			outputFile = File.CreateTempFile("box_" + width + height + color, ".png", new File(tmpDir));

			// TODO: Warning - risky and untested, trying to jump between java.lang.io and system.io using a handle
			var os = new FileStream(outputFile.Handle, System.IO.FileAccess.ReadWrite);
			temp_box.Compress(Bitmap.CompressFormat.Png, 100, os);
			overlayFile = outputFile;
			xParam = Convert.ToString(x);
			yParam = Convert.ToString(y);
		}
	}
}