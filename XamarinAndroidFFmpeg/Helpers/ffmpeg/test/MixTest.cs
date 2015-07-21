//using System;
//
//namespace FFMpegLib
//{
//
//
//	public class MixTest
//	{
//		public static void test(string fileTmpPath, string videoClipPath, string audioClipPath, Clip clipOut)
//		{
//			File fileTmp = new File(fileTmpPath);
//			File fileAppRoot = new File("");
//
//			FFMpegHelpers fc = new FFMpegHelpers(null, fileTmp);
//
//			Clip clipVideo = new Clip(videoClipPath);
//			//fc.getInfo(clipVideo);
//
//			Clip clipAudio = new Clip(audioClipPath);
//			//fc.getInfo(clipAudio);
//
//			fc.combineAudioAndVideo(clipVideo, clipAudio, clipOut, new ShellCallbackAnonymousInnerClassHelper());
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
//			//	System.out.println("MIX> " + shellLine);
//			}
//
//			public virtual void ProcessComplete(int exitValue)
//			{
//
//				if (exitValue != 0)
//				{
//					Console.Error.WriteLine("concat non-zero exit: " + exitValue);
//				}
//			}
//		}
//
//	}
//
//}