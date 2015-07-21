//using System;
//using System.Collections.Generic;
//using Java.IO;
//
//namespace FFMpegLib
//{
//
//
//
//	using SoxController = net.sourceforge.sox.SoxController;
//
//
//	public class ConcatTest
//	{
//		public static void test(string videoRoot, string fileTmpPath, string fileOut, double fadeLen)
//		{
//			File fileTmp = new File(fileTmpPath);
//			File fileAppRoot = new File("");
//			File fileVideoRoot = new File(videoRoot);
//
//			FFMpegHelpers fc = new FFMpegHelpers(null, fileTmp);
//			 SoxController sxCon = new SoxController(null, fileAppRoot, null);
//
//			List<Clip> listVideos = new List<Clip>();
//
//			string[] fileList = fileVideoRoot.List();
//			foreach (string fileVideo in fileList)
//			{
//				if (fileVideo.EndsWith("mp4"))
//				{
//					Clip clip = new Clip();
//					clip.path = (new File(fileVideoRoot,fileVideo)).CanonicalPath;
//
//					fc.getInfo(clip);
//
//					clip.duration = clip.duration - fadeLen;
//					listVideos.Add(clip);
//
//
//				}
//			}
//
//			Clip clipOut = new Clip();
//			clipOut.path = (new File(fileOut)).CanonicalPath;
//
//			fc.concatAndTrimFilesMP4Stream(listVideos, clipOut, false, false, new ShellCallbackAnonymousInnerClassHelper());
//
//
//
//		}
//
//		private class ShellCallbackAnonymousInnerClassHelper : ShellUtils.IShellCallback
//		{
//			public ShellCallbackAnonymousInnerClassHelper()
//			{
//			}
//
//
//			public virtual void ShellOut(string shellLine)
//			{
//
//				Console.WriteLine("fc>" + shellLine);
//			}
//
//			public virtual void ProcessComplete(int exitValue)
//			{
//
//				if (exitValue < 0)
//				{
//					Console.Error.WriteLine("concat non-zero exit: " + exitValue);
//				}
//			}
//		}
//	}
//
//}