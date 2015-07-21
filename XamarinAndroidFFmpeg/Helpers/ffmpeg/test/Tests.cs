//using System;
//
//namespace FFMpegLib
//{
//
//
//	public class Tests
//	{
//
//		/// <param name="args"> </param>
//		public static void Main(string[] args)
//		{
//
//
//
//			string[] testpaths = new string[] {"/home/n8fr8/Desktop/sm3"};
//
//			int idx = -1;
//
//			 double fadeLen = 1;
//
//			foreach (string testpath in testpaths)
//			{
//				idx++;
//
//				Console.WriteLine("************************************");
//				Console.WriteLine("CONCAT TEST: " + testpath);
//
//				File fileVideoOutput = new File("/tmp/test" + idx + ".mp4");
//				fileVideoOutput.delete();
//
//				ConcatTest.test(testpath, "/tmp", fileVideoOutput.CanonicalPath, fadeLen);
//
//				if (!fileVideoOutput.exists())
//				{
//					Console.WriteLine("FAIL!! > output file did not get created: " + fileVideoOutput.CanonicalPath);
//					continue;
//				}
//				else
//				{
//					Console.WriteLine("SUCCESS!! > " + fileVideoOutput.CanonicalPath);
//				}
//
//				Console.WriteLine("************************************");
//				Console.WriteLine("CROSSFADE TEST: " + testpath);
//
//				File fileAudioOutput = new File("/tmp/test" + idx + ".3gp");
//				fileAudioOutput.delete();
//				CrossfadeTest.test(testpath, "/tmp", fileAudioOutput.CanonicalPath,fadeLen);
//				if (!fileAudioOutput.exists())
//				{
//					Console.WriteLine("FAIL!! > output file did not get created: " + fileAudioOutput.CanonicalPath);
//					continue;
//				}
//				else
//				{
//					Console.WriteLine("SUCCESS!! > " + fileAudioOutput.CanonicalPath);
//				}
//
//				Console.WriteLine("************************************");
//				Console.WriteLine("MIX TEST: " + testpath);
//
//				File fileMix = new File("/tmp/test" + idx + "mix.mp4");
//				fileMix.delete();
//				Clip clipMixOut = new Clip(fileMix.CanonicalPath);
//				MixTest.test("/tmp", fileVideoOutput.CanonicalPath, fileAudioOutput.CanonicalPath, clipMixOut);
//				if (!fileMix.exists())
//				{
//					Console.WriteLine("FAIL!! > output file did not get created: " + fileMix.CanonicalPath);
//				}
//				else
//				{
//					Console.WriteLine("SUCCESS!! > " + fileMix.CanonicalPath);
//				}
//
//
//			}
//
//			Console.WriteLine("**********************");
//			Console.WriteLine("*******FIN**********");
//
//		}
//
//	}
//
//}