//
//using System;
//using System.Collections.Generic;
//
//namespace FFMpegLib
//{
//
//
//	using CrossfadeCat = net.sourceforge.sox.CrossfadeCat;
//	using SoxController = net.sourceforge.sox.SoxController;
//
//
//	public class CrossfadeTest
//	{
//
//
//		public static void test(string videoRoot, string fileTmpPath, string clipOutPath, double fadeLen)
//		{
//			 File fileTmp = new File(fileTmpPath);
//			 File fileAppRoot = new File("");
//			 File fileVideoRoot = new File(videoRoot);
//
//			 string fadeType = "l";
//			 int sampleRate = 22050;
//			 int channels = 1;
//
//			 FFMpegHelpers ffmpegc = new FFMpegHelpers(null, fileTmp);
//
//			 Clip clipOut = new Clip();
//			 clipOut.path = clipOutPath;
//			 clipOut.audioCodec = "aac";
//			 clipOut.audioBitrate = 56;
//
//
//			 List<Clip> listVideos = new List<Clip>();
//
//				string[] fileList = fileVideoRoot.list();
//				foreach (string fileVideo in fileList)
//				{
//					if (fileVideo.EndsWith("mp4"))
//					{
//						Clip clip = new Clip();
//						clip.path = (new File(fileVideoRoot,fileVideo)).CanonicalPath;
//						//clip.startTime = "00:00:03";
//						//clip.duration = "00:00:02";
//
//						ffmpegc.getInfo(clip);
//
//						//System.out.println("clip " + fileVideo + " duration=" + clip.duration);
//
//						listVideos.Add(clip);
//
//					}
//				}
//
//			 //now add 1 second cross fade to each audio file and cat them together
//			 SoxController sxCon = new SoxController(null, fileAppRoot, new ShellCallbackAnonymousInnerClassHelper());
//
//			 List<Clip> alAudio = new List<Clip>();
//
//			 //convert each input file to a WAV so we can use Sox to process
//			 int wavIdx = 0;
//
//			 foreach (Clip mediaIn in listVideos)
//			 {
//				if (System.IO.Directory.Exists(mediaIn.path) || System.IO.File.Exists(mediaIn.path))
//				{
//
//					if (mediaIn.audioCodec == null)
//					{
//						//there is no audio track so let's generate silence
//
//
//					}
//					else
//					{
//						Clip audioOut = ffmpegc.convertToWaveAudio(mediaIn, (new File(fileTmp, wavIdx + ".wav")).CanonicalPath,sampleRate,channels, new ShellCallbackAnonymousInnerClassHelper2());
//
//						alAudio.Add(audioOut);
//
//						/*
//						float duration = (float) sxCon.getLength(new File(audioOut.path).getCanonicalPath());
//						
//						if (mediaIn.duration == null)
//						{	
//							mediaIn.duration = string.format(Locale.US, "%f", duration);
//						}*/
//						ffmpegc.getInfo(mediaIn);
//
//
//						wavIdx++;
//					}
//				}
//				else
//				{
//					throw new FileNotFoundException(mediaIn.path);
//				}
//			 }
//
//			 if (alAudio.Count > 0)
//			 {
//				 string fileOut = alAudio[0].path;
//
//				 Console.WriteLine("mix length=" + sxCon.getLength(fileOut));
//
//				 for (int i = 1; i < alAudio.Count; i++)
//				 {
//
//					 File fileAdd = new File(alAudio[i].path);
//
//					 CrossfadeCat xCat = new CrossfadeCat(sxCon, fileOut, fileAdd.CanonicalPath, fadeLen, fileOut);
//					 xCat.start();
//
//					 fileAdd.deleteOnExit();
//
//					 Console.WriteLine("mix length=" + sxCon.getLength(fileOut));
//
//				 }
//
//
//				 //1 second fade in and fade out, t = triangle or linear
//				   //String fadeLenStr = sxCon.formatTimePeriod(fadeLen);
//
//
//
//				 string fadeFileOut = sxCon.fadeAudio(fileOut, fadeType, fadeLen, sxCon.getLength(fileOut) - fadeLen, fadeLen);
//
//				 //now export the final file to our requested output format		    mOut.mimeType = AppConstants.MimeTypes.MP4_AUDIO;
//
//				 Clip mdFinalIn = new Clip();
//				 mdFinalIn.path = fadeFileOut;
//
//
//				 Console.WriteLine("final duration: " + sxCon.getLength(fadeFileOut));
//
//				 Clip exportOut = ffmpegc.convertTo3GPAudio(mdFinalIn, clipOut, new ShellCallbackAnonymousInnerClassHelper3());
//			 }
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
//			//	System.out.println("sxCon> " + shellLine);
//
//			}
//
//			public virtual void ProcessComplete(int exitValue)
//			{
//
//
//				if (exitValue != 0)
//				{
//					Console.Error.WriteLine("sxCon> EXIT=" + exitValue);
//
//					Exception re = new Exception("non-zero exit: " + exitValue);
//					Console.WriteLine(re.ToString());
//					Console.Write(re.StackTrace);
//					throw re;
//				}
//
//			}
//		}
//
//		private class ShellCallbackAnonymousInnerClassHelper2 : ShellUtils.IShellCallback
//		{
//			public ShellCallbackAnonymousInnerClassHelper2()
//			{
//			}
//
//
//			public virtual void ShellOut(string shellLine)
//			{
//
//			//	System.out.println("convertToWav> " + shellLine);
//
//			}
//
//			public virtual void ProcessComplete(int exitValue)
//			{
//
//				if (exitValue != 0)
//				{
//
//					Console.Error.WriteLine("convertToWav> EXIT=" + exitValue);
//
//					Exception re = new Exception("non-zero exit: " + exitValue);
//					Console.WriteLine(re.ToString());
//					Console.Write(re.StackTrace);
//					throw re;
//				}
//			}
//		}
//
//		private class ShellCallbackAnonymousInnerClassHelper3 : ShellUtils.IShellCallback
//		{
//			public ShellCallbackAnonymousInnerClassHelper3()
//			{
//			}
//
//
//			public virtual void ShellOut(string shellLine)
//			{
//
//				//System.out.println("convertTo3gp> " + shellLine);
//			}
//
//			public virtual void ProcessComplete(int exitValue)
//			{
//
//				if (exitValue < 0)
//				{
//					Exception re = new Exception("non-zero exit: " + exitValue);
//					Console.WriteLine(re.ToString());
//					Console.Write(re.StackTrace);
//					throw re;
//				}
//			}
//		}
//
//	}
//
//}
