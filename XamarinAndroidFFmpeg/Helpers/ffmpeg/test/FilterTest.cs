//using System.Collections.Generic;
//using Java.IO;
//
//namespace FFMpegLib
//{
//
//
//	using CropVideoFilter = FFMpegLib.CropVideoFilter;
//	using DrawBoxVideoFilter = FFMpegLib.DrawBoxVideoFilter;
//	using DrawTextVideoFilter = FFMpegLib.DrawTextVideoFilter;
//	using FadeVideoFilter = FFMpegLib.FadeVideoFilter;
//	using TransposeVideoFilter = FFMpegLib.TransposeVideoFilter;
//	using VideoFilter = FFMpegLib.VideoFilter;
//
//	using Activity = Android.App.Activity;
//	using Context = Android.Content.Context;
//
//	public class FilterTest
//	{
//
//
//		public static void test(string title, string textColor, File fileFont, string boxColor, string opacity)
//		{
//			List<VideoFilter> listFilters = new List<VideoFilter>();
//
//			File fileDir = new File("tmp");
//			fileDir.mkdir();
//
//			int height = 480;
//			int width = 720;
//			int lowerThird = height / 3;
//			DrawBoxVideoFilter vf = new DrawBoxVideoFilter(0,height - lowerThird,width,lowerThird,100,"blue",fileDir);
//
//			DrawTextVideoFilter vfTitle = new DrawTextVideoFilter(title, DrawTextVideoFilter.X_CENTERED,DrawTextVideoFilter.Y_CENTERED, textColor, 38, fileFont, true, boxColor, opacity);
//
//			float fps = 29.97f;
//			int fadeTime = (int)(fps * 3);
//			//fades in first 3 seconds
//			FadeVideoFilter vfFadeIn = new FadeVideoFilter("in",0,fadeTime);
//
//			//fades out last 50 frames
//			int totalFrames = (int)(14.37 * 29.97);
//			FadeVideoFilter vfFadeOut = new FadeVideoFilter("out",totalFrames - fadeTime,fadeTime);
//
//			//crops video in 100 pixels on each side
//			CropVideoFilter vfCrop = new CropVideoFilter("in_w-100","in_h-100","100","100");
//
//			//rotates video 90 degress clockwise
//			TransposeVideoFilter vfTranspose = new TransposeVideoFilter(TransposeVideoFilter.NINETY_CLOCKWISE);
//
//			listFilters.Add(vfTranspose);
//			listFilters.Add(vfCrop);
//			listFilters.Add(vfTitle);
//			listFilters.Add(vfFadeIn);
//			listFilters.Add(vfFadeOut);
//
//
//
//			fileDir.deleteOnExit();
//		}
//	}
//
//}