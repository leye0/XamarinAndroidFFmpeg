using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Android;
using Java.IO;
using Java.Lang;
using Java.Util;
using ApplicationInfo = Android.Content.PM.ApplicationInfo;
using Bitmap = Android.Graphics.Bitmap;
using Context = Android.Content.Context;
using Log = Android.Util.Log;
using MediaMetadataRetriever = Android.Media.MediaMetadataRetriever;
using NameNotFoundException = Android.Content.PM.PackageManager.NameNotFoundException;
using PackageInfo = Android.Content.PM.PackageInfo;
using PackageManager = Android.Content.PM.PackageManager;
using System.Windows.Input;
using Android.Media;

namespace XamarinAndroidFFmpeg
{
	public class FFMpeg
	{
		private Android.Content.Context _applicationContext;

		private string _ffmpegBin;

		private const string TAG = "FFMPEG";

		private string _folderForTempStuff;

		private string _cmdCat = "sh cat";

		public FFMpeg(Android.Content.Context applicationContext, string fileTemp)
		{
			_folderForTempStuff = fileTemp;
			_applicationContext = applicationContext;
			_ffmpegBin = InstallBinary(XamarinAndroidFFmpeg.Resource.Raw.ffmpeg, "ffmpeg", false);
		}

		public void InstallBinaries(bool overwrite)
		{
			
		}

		public string BinaryPath
		{
			get
			{
				return _ffmpegBin;
			}
		}

		private string InstallBinary(int resId, string shortFileName, bool upgrade)
		{
			try
			{
				File f = new File(_applicationContext.GetDir("bin", 0), shortFileName);
				if (f.Exists())
				{
					f.Delete();
				}
				CopyRawFile(resId, f.AbsolutePath, "0755");
				return f.CanonicalPath;
			}
			catch (System.Exception e)
			{
				Android.Util.Log.Error(TAG, "installBinary failed: " + e.Message);
				return null;
			}
		}

		/// <summary>
		/// Copies a raw resource file, given its ID to the given location </summary>
		/// <param name="ctx"> context </param>
		/// <param name="resid"> resource id </param>
		/// <param name="file"> destination file path </param>
		/// <param name="mode"> file permissions (E.g.: "755") </param>
		/// <exception cref="IOException"> on error </exception>
		/// <exception cref="InterruptedException"> when interrupted </exception>
		private void CopyRawFile(int resid, string filePath, string mode)
		{
			string abspath = filePath;
			// Write the iptables binary

			var outputStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
			System.IO.Stream inputStream = _applicationContext.Resources.OpenRawResource(resid);
			var buf = new byte[1024];
			int len;
			while ((len = inputStream.Read(buf, 0, buf.Length)) > 0)
			{
				outputStream.Write(buf, 0, len);
			}
			outputStream.Close();
			inputStream.Close();
			// Change the permissions
			var r = Runtime.GetRuntime();
			r.Exec("chmod " + mode + " " + abspath).WaitFor();
		}

		private void ExecFFMPEG(IList<string> cmd, FFMpegCallbacks sc, File fileExec)
		{

			EnablePermissions();

			ExecProcess(cmd, sc, fileExec);
		}

		private void EnablePermissions()
		{
			var r = Runtime.GetRuntime ();
			r.Exec("chmod 700 " + _ffmpegBin);
		}

		private void ExecFFMPEG(IList<string> cmd, FFMpegCallbacks sc)
		{
			ExecFFMPEG(cmd, sc, (new File(_ffmpegBin)).ParentFile);
		}

		private int ExecProcess(IList<string> cmds, FFMpegCallbacks sc, File fileExec)
		{

			var cmdlog = new Java.Lang.StringBuilder();

			foreach (string cmd in cmds)
			{
				cmdlog.Append(cmd);
				cmdlog.Append(' ');
			}



			ProcessBuilder pb = new ProcessBuilder(cmds);
			pb.Directory(fileExec);


			sc.ShellOut(cmdlog.ToString());

			pb.RedirectErrorStream (true);

			Process process = pb.Start();

			StreamGobbler errorGobbler = new StreamGobbler(this, process.ErrorStream, "ERROR", sc);
			StreamGobbler outputGobbler = new StreamGobbler(this, process.InputStream, "OUTPUT", sc);

			errorGobbler.Run();
			outputGobbler.Run();
			int exitVal = -1000;
			try {
				exitVal = process.WaitFor();
			} catch (System.Exception e) {
				sc.ShellOut (e.Message);
			}

			sc.ProcessComplete(exitVal);

			return exitVal;

		}

		private int ExecProcess(string cmd, FFMpegCallbacks sc, File fileExec)
		{
			// TODO: This is from the legacy java code (Locale.US), but this is useless since ffmpeg is called internally.
			// Remove everywhere.
			// cmd = string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"), "%s", cmd);

			ProcessBuilder pb = new ProcessBuilder(cmd);
			pb.Directory(fileExec);

			pb.RedirectErrorStream (true);
			Process process = pb.Start();

			StreamGobbler errorGobbler = new StreamGobbler(this, process.ErrorStream, "ERROR", sc);
			StreamGobbler outputGobbler = new StreamGobbler(this, process.InputStream, "OUTPUT", sc);

			errorGobbler.Run();
			outputGobbler.Run();

			int exitVal = process.WaitFor ();

			sc.ProcessComplete(exitVal);

			return exitVal;
		}

		public class Argument
		{
			private readonly FFMpeg outerInstance;

			public Argument(FFMpeg outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public const string VIDEOCODEC = "-vcodec";
			public const string AUDIOCODEC = "-acodec";

			public const string VIDEO_QSCALE = "-q:v";
			public const string AUDIO_QSCALE = "-q:a";
			public const string QSCALE = "-q";
			public const string H264_CRF = "-crf";

			public const string VIDEOBITSTREAMFILTER = "-vbsf";
			public const string AUDIOBITSTREAMFILTER = "-absf";

			public const string VERBOSITY = "-v";
			public const string FILE_INPUT = "-i";
			public const string SIZE = "-s";
			public const string FRAMERATE = "-r";
			public const string FORMAT = "-f";
			public const string BITRATE_VIDEO = "-b:v";

			public const string BITRATE_AUDIO = "-b:a";
			public const string CHANNELS_AUDIO = "-ac";
			public const string FREQ_AUDIO = "-ar";

			public const string STARTTIME = "-ss";
			public const string DURATION = "-t";


		}


		public void Execute(string[] cmds, FFMpegCallbacks sc)
		{
			List<string> cmd = new List<string>(cmds);
			cmd.Insert (0, _ffmpegBin);
			ExecFFMPEG (cmd, sc);
		}

		public void ProcessVideo(Clip inputClip, Clip outputClip, bool enableExperimental, FFMpegCallbacks sc)
		{

			List<string> cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");

			if (inputClip.format != null)
			{
				cmd.Add(Argument.FORMAT);
				cmd.Add(inputClip.format);
			}

			if (inputClip.videoCodec != null)
			{
				cmd.Add(Argument.VIDEOCODEC);
				cmd.Add(inputClip.videoCodec);
			}

			if (inputClip.audioCodec != null)
			{
				cmd.Add(Argument.AUDIOCODEC);
				cmd.Add(inputClip.audioCodec);
			}

			cmd.Add("-i");
			cmd.Add((new File(inputClip.path)).CanonicalPath);

			if (outputClip.videoBitrate > 0)
			{
				cmd.Add(Argument.BITRATE_VIDEO);
				cmd.Add(outputClip.videoBitrate + "k");
			}

			if (outputClip.width > 0)
			{
				cmd.Add(Argument.SIZE);
				cmd.Add(outputClip.width + "x" + outputClip.height);

			}
			if (outputClip.videoFps != null)
			{
				cmd.Add(Argument.FRAMERATE);
				cmd.Add(outputClip.videoFps);
			}

			if (outputClip.videoCodec != null)
			{
				cmd.Add(Argument.VIDEOCODEC);
				cmd.Add(outputClip.videoCodec);
			}

			if (outputClip.videoBitStreamFilter != null)
			{
				cmd.Add(Argument.VIDEOBITSTREAMFILTER);
				cmd.Add(outputClip.videoBitStreamFilter);
			}


			if (outputClip.videoFilter != null)
			{
				cmd.Add("-vf");
				cmd.Add(outputClip.videoFilter);
			}

			if (outputClip.qscaleVideo != null)
			{
				cmd.Add(Argument.VIDEO_QSCALE);
				cmd.Add(outputClip.qscaleVideo);
			}

			if (outputClip.audioCodec != null)
			{
				cmd.Add(Argument.AUDIOCODEC);
				cmd.Add(outputClip.audioCodec);
			}

			if (outputClip.audioBitStreamFilter != null)
			{
				cmd.Add(Argument.AUDIOBITSTREAMFILTER);
				cmd.Add(outputClip.audioBitStreamFilter);
			}
			if (outputClip.audioChannels > 0)
			{
				cmd.Add(Argument.CHANNELS_AUDIO);
				cmd.Add(outputClip.audioChannels + "");
			}

			if (outputClip.audioBitrate > 0)
			{
				cmd.Add(Argument.BITRATE_AUDIO);
				cmd.Add(outputClip.audioBitrate + "k");
			}

			if (outputClip.qscaleAudio != null)
			{
				cmd.Add(Argument.AUDIO_QSCALE);
				cmd.Add(outputClip.qscaleAudio);
			}

			if (outputClip.qscale != null)
			{
				cmd.Add(Argument.QSCALE);
				cmd.Add(outputClip.qscale);
			}

			if (outputClip.format != null)
			{
				cmd.Add("-f");
				cmd.Add(outputClip.format);
			}

			if (enableExperimental)
			{
				cmd.Add("-strict");
				cmd.Add("-2"); //experimental
			}

			if (outputClip.H264_CRF != null)
			{
				cmd.Add(Argument.H264_CRF);
				cmd.Add(outputClip.H264_CRF);
			}

			cmd.Add((new File(outputClip.path)).CanonicalPath);

			ExecFFMPEG(cmd, sc);

		}

		public void BuildPreview(Clip inputClip, string outPattern, FFMpegCallbacks sc)
		{
			// Use temp dir
			// Build name with %
			// Return array of frame paths
			// At 5 fps
			// Return Bitmaps
			// In VideoConverter, change for ImageSource.

			List<string> cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");

			cmd.Add("-i");
			cmd.Add((new File(inputClip.path)).CanonicalPath);

			cmd.Add("-r");
			cmd.Add("5");
				
			cmd.Add("-f");
			cmd.Add("image2");

			cmd.Add(outPattern);

			ExecFFMPEG(cmd, sc);
		}

		public void GetFrameAt(Clip inputClip, string outFrame, FFMpegCallbacks sc)
		{
			// Use temp dir
			// Build name with %
			// Return array of frame paths
			// At 5 fps
			// Return Bitmaps
			// In VideoConverter, change for ImageSource.

			List<string> cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");

			cmd.Add("-r");
			cmd.Add("5");

			cmd.Add("-ss");
			cmd.Add("4.20");

			cmd.Add("-i");
			cmd.Add((new File(inputClip.path)).CanonicalPath);

			cmd.Add("-to");
			cmd.Add("8.20");

			cmd.Add("-frames:v");
			cmd.Add("1");

			cmd.Add(outFrame);

			ExecFFMPEG(cmd, sc);
		}



		public Clip CreateSlideshowFromImagesAndAudio(List<Clip> images, Clip audio, Clip outputClip, int durationPerSlide, FFMpegCallbacks sc)
		{
			string imageBasePath = (new File(_folderForTempStuff)).CanonicalPath;

			string imageBaseVariablePath = imageBasePath + "%05d.jpg";

			List<string> cmd = new List<string>();


			string newImagePath = null;
			int imageCounter = 0;

			Clip imageCover = images[0]; //add the first image twice

			cmd = new List<string>();
			cmd.Add(_ffmpegBin);
			cmd.Add("-y");

			cmd.Add("-i");
			cmd.Add((new File(imageCover.path)).CanonicalPath);

			if (outputClip.width != -1 && outputClip.height != -1)
			{
				cmd.Add("-s");
				cmd.Add(outputClip.width + "x" + outputClip.height);
			}

			newImagePath = imageBasePath + string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"), "%05", imageCounter++) + ".png";
			cmd.Add(newImagePath);

			ExecFFMPEG(cmd, sc);

			foreach (Clip image in images)
			{
				cmd = new List<string>();
				cmd.Add(_ffmpegBin);
				cmd.Add("-y");

				cmd.Add("-i");
				cmd.Add((new File(image.path)).CanonicalPath);

				if (outputClip.width != -1 && outputClip.height != -1)
				{
					cmd.Add("-s");
					cmd.Add(outputClip.width + "x" + outputClip.height);
				}

				newImagePath = imageBasePath + string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"), "%05d", imageCounter++) + ".png";
				cmd.Add(newImagePath);

				ExecFFMPEG(cmd, sc);


			}

			//then combine them
			cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");

			cmd.Add("-loop");
			cmd.Add("0");

			cmd.Add("-f");
			cmd.Add("image2");

			cmd.Add("-r");
			cmd.Add("1/" + durationPerSlide);

			cmd.Add("-i");
			cmd.Add(imageBaseVariablePath);

			cmd.Add("-strict");
			cmd.Add("-2"); //experimental

			string fileTempMpg = (new File(_folderForTempStuff,"tmp.mpg")).CanonicalPath;

			cmd.Add(fileTempMpg);

			ExecFFMPEG(cmd, sc);

			//now combine and encode
			cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");

			cmd.Add("-i");
			cmd.Add(fileTempMpg);

			if (audio != null && audio.path != null)
			{
				cmd.Add("-i");
				cmd.Add((new File(audio.path)).CanonicalPath);

				cmd.Add("-map");
				cmd.Add("0:0");

				cmd.Add("-map");
				cmd.Add("1:0");

				cmd.Add(Argument.AUDIOCODEC);
				cmd.Add("aac");

				cmd.Add(Argument.BITRATE_AUDIO);
				cmd.Add("128k");

			}

			cmd.Add("-strict");
			cmd.Add("-2"); //experimental

			cmd.Add(Argument.VIDEOCODEC);


			if (outputClip.videoCodec != null)
			{
				cmd.Add(outputClip.videoCodec);
			}
			else
			{
				cmd.Add("mpeg4");
			}

			if (outputClip.videoBitrate != -1)
			{
				cmd.Add(Argument.BITRATE_VIDEO);
				cmd.Add(outputClip.videoBitrate + "k");
			}

			cmd.Add((new File(outputClip.path)).CanonicalPath);


			ExecFFMPEG(cmd, sc);

			return outputClip;
		}

		/*
		 * ffmpeg -y -loop 0 -f image2 -r 0.5 -i image-%03d.jpg -s:v 1280x720 -b:v 1M \
	   -i soundtrack.mp3 -t 01:05:00 -map 0:0 -map 1:0 out.avi
	   
	   -loop_input â€" loops the images. Disable this if you want to stop the encoding when all images are used or the soundtrack is finished.
	
	-r 0.5 â€" sets the framerate to 0.5, which means that each image will be shown for 2 seconds. Just take the inverse, for example if you want each image to last for 3 seconds, set it to 0.33.
	
	-i image-%03d.jpg â€" use these input files. %03d means that there will be three digit numbers for the images.
	
	-s 1280x720 â€" sets the output frame size.
	
	-b 1M â€" sets the bitrate. You want 500MB for one hour, which equals to 4000MBit in 3600 seconds, thus a bitrate of approximately 1MBit/s should be sufficient.
	
	-i soundtrack.mp3 â€" use this soundtrack file. Can be any format.
	
	-t 01:05:00 â€" set the output length in hh:mm:ss format.
	
	out.avi â€" create this output file. Change it as you like, for example using another container like MP4.
		 */
		public Clip CombineAudioAndVideo(Clip audioIn, Clip videoIn, Clip movieOut, FFMpegCallbacks sc)
		{
			List<string> cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");

			cmd.Add("-i");
			cmd.Add((new File(audioIn.path)).CanonicalPath);

			cmd.Add("-i");
			cmd.Add((new File(videoIn.path)).CanonicalPath);

			cmd.Add("-strict");
			cmd.Add("-2"); //experimental

			cmd.Add(Argument.AUDIOCODEC);
			if (movieOut.audioCodec != null)
			{
				cmd.Add(movieOut.audioCodec);
			}
			else
			{
				cmd.Add("copy");
			}

			cmd.Add(Argument.VIDEOCODEC);
			if (movieOut.videoCodec != null)
			{
				cmd.Add(movieOut.videoCodec);
			}
			else
			{
				cmd.Add("copy");
			}

			if (movieOut.videoBitrate != -1)
			{
				cmd.Add(Argument.BITRATE_VIDEO);
				cmd.Add(movieOut.videoBitrate + "k");
			}

			if (movieOut.videoFps != null)
			{
				cmd.Add(Argument.FRAMERATE);
				cmd.Add(movieOut.videoFps);
			}

			if (movieOut.audioBitrate != -1)
			{
				cmd.Add(Argument.BITRATE_AUDIO);
				cmd.Add(movieOut.audioBitrate + "k");
			}
			cmd.Add("-y");

			cmd.Add("-cutoff");
			cmd.Add("15000");

			if (movieOut.width > 0)
			{
				cmd.Add(Argument.SIZE);
				cmd.Add(movieOut.width + "x" + movieOut.height);

			}

			if (movieOut.format != null)
			{
				cmd.Add("-f");
				cmd.Add(movieOut.format);
			}

			File fileOut = new File(movieOut.path);
			cmd.Add(fileOut.CanonicalPath);

			ExecFFMPEG(cmd, sc);


			return movieOut;

		}

		// Only keep each stream
		public Clip CombineAudioAndVideo2(Clip audioIn, Clip videoIn, Clip movieOut, FFMpegCallbacks sc)
		{
			List<string> cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");

			cmd.Add("-i");
			cmd.Add((new File(audioIn.path)).CanonicalPath);

			cmd.Add("-i");
			cmd.Add((new File(videoIn.path)).CanonicalPath);

			cmd.Add("-map");
			cmd.Add ("0:0");

			cmd.Add("-map");
			cmd.Add ("1:0");

			cmd.Add("-strict");
			cmd.Add("experimental"); //experimental

			cmd.Add(Argument.AUDIOCODEC);
			if (movieOut.audioCodec != null)
			{
				cmd.Add(movieOut.audioCodec);
			}
			else
			{
				cmd.Add("copy");
			}

			cmd.Add(Argument.VIDEOCODEC);
			if (movieOut.videoCodec != null)
			{
				cmd.Add(movieOut.videoCodec);
			}
			else
			{
				cmd.Add("copy");
			}

			if (movieOut.videoBitrate != -1)
			{
				cmd.Add(Argument.BITRATE_VIDEO);
				cmd.Add(movieOut.videoBitrate + "k");
			}

			if (movieOut.videoFps != null)
			{
				cmd.Add(Argument.FRAMERATE);
				cmd.Add(movieOut.videoFps);
			}

			if (movieOut.audioBitrate != -1)
			{
				cmd.Add(Argument.BITRATE_AUDIO);
				cmd.Add(movieOut.audioBitrate + "k");
			}
			cmd.Add("-y");

			cmd.Add("-cutoff");
			cmd.Add("15000");

			if (movieOut.width > 0)
			{
				cmd.Add(Argument.SIZE);
				cmd.Add(movieOut.width + "x" + movieOut.height);

			}

			if (movieOut.format != null)
			{
				cmd.Add("-f");
				cmd.Add(movieOut.format);
			}


			File fileOut = new File(movieOut.path);

			cmd.Add("-shortest");

			cmd.Add(fileOut.CanonicalPath);

			ExecFFMPEG(cmd, sc);


			return movieOut;

		}

		public Clip ConvertImageToMP4(Clip mediaIn, int duration, string outPath, FFMpegCallbacks sc)
		{
			Clip result = new Clip();
			List<string> cmd = new List<string>();

			// ffmpeg -loop 1 -i IMG_1338.jpg -t 10 -r 29.97 -s 640x480 -qscale 5 test.mp4

			cmd = new List<string>();

			//convert images to MP4
			cmd.Add(_ffmpegBin);
			cmd.Add("-y");

			cmd.Add("-loop");
			cmd.Add("1");

			cmd.Add("-i");
			cmd.Add((new File(mediaIn.path)).CanonicalPath);

			cmd.Add(Argument.FRAMERATE);
			cmd.Add(mediaIn.videoFps);

			cmd.Add("-t");
			cmd.Add(duration + "");

			cmd.Add("-qscale");
			cmd.Add("5"); //a good value 1 is best 30 is worst

			if (mediaIn.width != -1)
			{
				cmd.Add(Argument.SIZE);
				cmd.Add(mediaIn.width + "x" + mediaIn.height);
			//	cmd.add("-vf");
			//	cmd.add("\"scale=-1:" + mediaIn.width + "\"");
			}

			if (mediaIn.videoBitrate != -1)
			{
				cmd.Add(Argument.BITRATE_VIDEO);
				cmd.Add(mediaIn.videoBitrate + "");
			}


		//	-ar 44100 -acodec pcm_s16le -f s16le -ac 2 -i /dev/zero -acodec aac -ab 128k \ 
		//	-map 0:0 -map 1:0

			result.path = outPath;
			result.videoBitrate = mediaIn.videoBitrate;
			result.videoFps = mediaIn.videoFps;
			result.mimeType = "video/mp4";

			cmd.Add((new File(result.path)).CanonicalPath);

			ExecFFMPEG(cmd, sc);

			return result;
		}

		//based on this gist: https://gist.github.com/3757344
		//ffmpeg -i input1.mp4 -vcodec copy -vbsf h264_mp4toannexb -acodec copy part1.ts
		//ffmpeg -i input2.mp4 -c copy -bsf:v h264_mp4toannexb -f mpegts intermediate2.ts

		public Clip ConvertToMP4Stream(Clip mediaIn, string startTime, double duration, string outPath, FFMpegCallbacks sc)
		{
			List<string> cmd = new List<string>();

			Clip mediaOut = new Clip();
			mediaOut.path = outPath;

			string mediaPath = (new File(mediaIn.path)).CanonicalPath;

			cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");

			if (startTime != null)
			{
				cmd.Add(Argument.STARTTIME);
				cmd.Add(startTime);
			}

			if (duration != -1)
			{
				cmd.Add(Argument.DURATION);

				double dValue = mediaIn.duration;
				int hours = (int)(dValue / 3600f);
				dValue -= (hours * 3600);

				cmd.Add("0");
				cmd.Add(string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"),"%s",hours));
				cmd.Add(":");

				int min = (int)(dValue / 60f);
				dValue -= (min * 60);

				cmd.Add("0");
				cmd.Add(string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"),"%s",min));
				cmd.Add(":");

				cmd.Add(string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"),"%f",dValue));

				//cmd.add("00:00:" + string.format(Locale.US,"%f",mediaIn.duration));


			}

			cmd.Add("-i");
			cmd.Add(mediaPath);

			cmd.Add("-f");
			cmd.Add("mpegts");

			cmd.Add("-c");
			cmd.Add("copy");

			cmd.Add("-an");

			//cmd.add(Argument.VIDEOBITSTREAMFILTER);
			cmd.Add("-bsf:v");
			cmd.Add("h264_mp4toannexb");

			File fileOut = new File(mediaOut.path);
			mediaOut.path = fileOut.CanonicalPath;

			cmd.Add(mediaOut.path);

			ExecFFMPEG(cmd, sc);

			return mediaOut;
		}

		public Clip ConvertToWaveAudio(Clip mediaIn, string outPath, int sampleRate, int channels, FFMpegCallbacks sc, bool strict = false)
		{
			List<string> cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");

			if (mediaIn.startTime != null)
			{
				cmd.Add("-ss");
				cmd.Add(mediaIn.startTime);
			}

			if (mediaIn.duration != -1)
			{
				cmd.Add("-t");
				cmd.Add(mediaIn.duration.ToString());
			}

			cmd.Add("-i");
			cmd.Add((new File(mediaIn.path)).CanonicalPath);

			if (strict) {
				cmd.Add ("-strict");
				cmd.Add ("-2");
			}

			cmd.Add("-ar");
			cmd.Add(sampleRate + "");

			cmd.Add("-ac");
			cmd.Add(channels + "");

			cmd.Add("-vn");

			Clip mediaOut = new Clip();

			File fileOut = new File(outPath);
			mediaOut.path = fileOut.CanonicalPath;

			cmd.Add(mediaOut.path);

			ExecFFMPEG(cmd, sc);

			return mediaOut;
		}

		public Clip ConvertTo3GPAudio(Clip mediaIn, Clip mediaOut, FFMpegCallbacks sc)
		{
			List<string> cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");
			cmd.Add("-i");
			cmd.Add((new File(mediaIn.path)).CanonicalPath);

			if (mediaIn.startTime != null)
			{
				cmd.Add("-ss");
				cmd.Add(mediaIn.startTime);
			}

			if (mediaIn.duration != -1)
			{
				cmd.Add("-t");
				cmd.Add(mediaIn.duration.ToString());

			}

			cmd.Add("-vn");

			if (mediaOut.audioCodec != null)
			{
				cmd.Add("-acodec");
				cmd.Add(mediaOut.audioCodec);
			}

			if (mediaOut.audioBitrate != -1)
			{
				cmd.Add("-ab");
				cmd.Add(mediaOut.audioBitrate + "k");
			}

			cmd.Add("-strict");
			cmd.Add("-2");

			File fileOut = new File(mediaOut.path);

			cmd.Add(fileOut.CanonicalPath);

			ExecFFMPEG(cmd, sc);

			return mediaOut;
		}

		public Clip Convert(Clip mediaIn, string outPath, FFMpegCallbacks sc)
		{
			List<string> cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");
			cmd.Add("-i");
			cmd.Add((new File(mediaIn.path)).CanonicalPath);

			if (mediaIn.startTime != null)
			{
				cmd.Add("-ss");
				cmd.Add(mediaIn.startTime);
			}

			if (mediaIn.duration != -1)
			{
				cmd.Add("-t");
				cmd.Add(mediaIn.duration.ToString());

			}

			Clip mediaOut = new Clip();

			File fileOut = new File(outPath);

			mediaOut.path = fileOut.CanonicalPath;

			cmd.Add(mediaOut.path);

			ExecFFMPEG(cmd, sc);

			return mediaOut;
		}

		public Clip ConvertToMPEG(Clip mediaIn, string outPath, FFMpegCallbacks sc)
		{
			List<string> cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");
			cmd.Add("-i");
			cmd.Add((new File(mediaIn.path)).CanonicalPath);

			if (mediaIn.startTime != null)
			{
				cmd.Add("-ss");
				cmd.Add(mediaIn.startTime);
			}

			if (mediaIn.duration != -1)
			{
				cmd.Add("-t");
//				cmd.Add(string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"),"%f",mediaIn.duration));
				cmd.Add(mediaIn.duration.ToString());
			}


			//cmd.add("-strict");
			//cmd.add("experimental");

			//everything to mpeg
			cmd.Add("-f");
			cmd.Add("mpeg");

			Clip mediaOut = mediaIn.Clone();

			File fileOut = new File(outPath);

			mediaOut.path = fileOut.CanonicalPath;

			cmd.Add(mediaOut.path);

			ExecFFMPEG(cmd, sc);

			return mediaOut;
		}

		public void ConcatAndTrimFilesMPEG(List<Clip> videos, Clip outputClip, bool preConvert, FFMpegCallbacks sc)
		{

			int idx = 0;

			if (preConvert)
			{
				foreach (Clip mdesc in videos)
				{
					if (mdesc.path == null)
					{
						continue;
					}

					//extract MPG video
					List<string> cmd = new List<string>();

					cmd.Add(_ffmpegBin);
					cmd.Add("-y");
					cmd.Add("-i");
					cmd.Add(mdesc.path);

					if (mdesc.startTime != null)
					{
						cmd.Add("-ss");
						cmd.Add(mdesc.startTime);
					}

					if (mdesc.duration != -1)
					{
						cmd.Add("-t");
						cmd.Add(mdesc.duration.ToString());
						//				cmd.Add(string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"),"%f",mediaIn.duration));

					}

					/*
					cmd.add ("-acodec");
					cmd.add("pcm_s16le");
					
					cmd.add ("-vcodec");
					cmd.add("mpeg2video");
					*/

					if (outputClip.audioCodec == null)
					{
						cmd.Add("-an"); //no audio
					}

					//cmd.add("-strict");
					//cmd.add("experimental");

					//everything to mpeg
					cmd.Add("-f");
					cmd.Add("mpeg");
					cmd.Add(outputClip.path + '.' + idx + ".mpg");

					ExecFFMPEG(cmd, sc);

					idx++;
				}
			}

			var cmdRun = new Java.Lang.StringBuilder();

			cmdRun.Append(_cmdCat);

			idx = 0;

			foreach (Clip vdesc in videos)
			{
				if (vdesc.path == null)
				{
					continue;
				}

				if (preConvert)
				{
					cmdRun.Append(outputClip.path).Append('.').Append((char) idx++).Append(".mpg").Append(' '); //leave a space at the end!
				}
				else
				{
					cmdRun.Append(vdesc.path).Append(' ');
				}
			}

			string mCatPath = outputClip.path + ".full.mpg";

			cmdRun.Append("> ");
			cmdRun.Append(mCatPath);

			string[] cmds = new string[] {"sh","-c",cmdRun.ToString()};

			var r = Runtime.GetRuntime ();

			r.Exec(cmds).WaitFor();


			Clip mInCat = new Clip();
			mInCat.path = mCatPath;

			ProcessVideo(mInCat, outputClip, false, sc);

			outputClip.path = mCatPath;
		}

		public Clip ConvertVideoToImages(Clip mediaIn, string outPath, FFMpegCallbacks sc)
		{
			List<string> cmd = new List<string>();

			cmd.Add(_ffmpegBin);

			//ffmpeg -i video.mpg -r 1 -f image2 ffmpeg_temp/%05d.png

			cmd.Add("-y");
			cmd.Add("-i");
			cmd.Add((new File(mediaIn.path)).CanonicalPath);

			if (mediaIn.startTime != null)
			{
				cmd.Add("-ss");
				cmd.Add(mediaIn.startTime);
			}

			if (mediaIn.duration != -1)
			{
				cmd.Add("-t");
				//				cmd.Add(string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"),"%f",mediaIn.duration));
				cmd.Add(mediaIn.duration.ToString());
			}


			//cmd.add("-strict");
			//cmd.add("experimental");

			//everything to images
			cmd.Add("-f");
			cmd.Add("image2");

			Clip mediaOut = mediaIn.Clone();

			File fileOut = new File(outPath);

			mediaOut.path = fileOut.CanonicalPath;

			cmd.Add(mediaOut.path);

			ExecFFMPEG(cmd, sc);

			return mediaOut;
		}

		public Clip ConvertImagesToVideo(string imagePathAndPattern, string outPath, FFMpegCallbacks sc)
		{
			List<string> cmd = new List<string>();

			cmd.Add(_ffmpegBin);

			// ffmpeg -i ffmpeg_temp/%05d.png -b 512 video2.mpg

//			cmd.Add("-y");
			cmd.Add("-f");
			cmd.Add("image2");
//			cmd.Add("-i");
//			cmd.Add(imagePattern);

			cmd.Add("-i");
			cmd.Add(imagePathAndPattern);

			cmd.Add("-vcodec");
			cmd.Add("mpeg4");

			Clip mediaOut = new Clip ();

			File fileOut = new File(outPath);

			mediaOut.path = fileOut.CanonicalPath;

			cmd.Add(mediaOut.path);

			ExecFFMPEG(cmd, sc);

			return mediaOut;
		}
	
		public void ExtractAudio(Clip mdesc, string audioFormat, File audioOutPath, FFMpegCallbacks sc)
		{

			//no just extract the audio
			List<string> cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");
			cmd.Add("-i");
			cmd.Add((new File(mdesc.path)).CanonicalPath);

			cmd.Add("-vn");

			if (mdesc.startTime != null)
			{
				cmd.Add("-ss");
				cmd.Add(mdesc.startTime);
			}

			if (mdesc.duration != -1)
			{
				cmd.Add("-t");
				cmd.Add(mdesc.duration.ToString());

			}

			cmd.Add("-f");
			cmd.Add(audioFormat); //wav

			//everything to WAV!
			cmd.Add(audioOutPath.CanonicalPath);

			ExecFFMPEG(cmd, sc);

		}

		public int KillVideoProcessor(bool asRoot, bool waitFor)
		{
			int killDelayMs = 300;

			int result = -1;

			int procId = -1;

			while ((procId = ShellUtils.FindProcessId (_ffmpegBin)) != -1) {

				//	Log.d(TAG, "Found PID=" + procId + " - killing now...");

				string[] cmd = new string[] { ShellUtils.SHELL_CMD_KILL + ' ' + procId + "" };

				try {
					result = ShellUtils.doShellCommand (cmd, new ShellCallbackAnonymousInnerClassHelper (this, null, null), asRoot, waitFor);
					System.Threading.Thread.Sleep (killDelayMs);
				} catch (System.Exception) {
				}
			}

			return result;
		}

		private class ShellCallbackAnonymousInnerClassHelper : FFMpegCallbacks
		{
			private readonly FFMpeg outerInstance;

			private ICommand _messageAction;

			private ICommand _completedAction;

			public ShellCallbackAnonymousInnerClassHelper(FFMpeg outerInstance, ICommand completedAction, ICommand messageAction) : base(completedAction, messageAction)
			{
				this.outerInstance = outerInstance;
				_messageAction = messageAction;
				_completedAction = completedAction;
			}


			public override void ShellOut(string msg)
			{
				_messageAction.Execute (msg);
			}

			public override void ProcessComplete(int exitValue)
			{
				_completedAction.Execute (null);
				base.ProcessComplete (exitValue);
			}

		}

		public Clip Trim(Clip mediaIn, bool withSound, string outPath, FFMpegCallbacks sc)
		{
			List<string> cmd = new List<string>();

			Clip mediaOut = new Clip();

			string mediaPath = mediaIn.path;

			cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");

			if (mediaIn.startTime != null)
			{
				cmd.Add(Argument.STARTTIME);
				cmd.Add(mediaIn.startTime);
			}

			if (mediaIn.duration != -1)
			{
				cmd.Add("-t");
				cmd.Add(mediaIn.duration.ToString());

			}

			cmd.Add("-i");
			cmd.Add(mediaPath);

			if (!withSound)
			{
				cmd.Add("-an");
			}

			cmd.Add("-strict");
			cmd.Add("-2"); //experimental

			mediaOut.path = outPath;

			cmd.Add(mediaOut.path);

			ExecFFMPEG(cmd, sc);

			return mediaOut;
		}

		public void ConcatAndTrimFilesMP4Stream(List<Clip> videos, Clip outputClip, bool preconvertClipsToMP4, bool useCatCmd, FFMpegCallbacks sc)
		{
			File fileExportOut = new File(outputClip.path);

			var sbCat = new Java.Lang.StringBuilder();

			int tmpIdx = 0;


			foreach (Clip vdesc in videos)
			{

				Clip mdOut = null;

				if (preconvertClipsToMP4)
				{
					File fileOut = new File(_folderForTempStuff,tmpIdx + "-trim.mp4");
					if (fileOut.Exists())
					{
						fileOut.Delete();
					}

					bool withSound = false;

					mdOut = Trim(vdesc,withSound,fileOut.CanonicalPath, sc);

					fileOut = new File(_folderForTempStuff,tmpIdx + ".ts");
					if (fileOut.Exists())
					{
						fileOut.Delete();
					}

					mdOut = ConvertToMP4Stream(mdOut,null,-1,fileOut.CanonicalPath, sc);
				}
				else
				{
					File fileOut = new File(_folderForTempStuff,tmpIdx + ".ts");
					if (fileOut.Exists())
					{
						fileOut.Delete();
					}
					mdOut = ConvertToMP4Stream(vdesc,vdesc.startTime,vdesc.duration,fileOut.CanonicalPath, sc);
				}

				if (mdOut != null)
				{
					if (sbCat.Length() > 0)
					{
						sbCat.Append("|");
					}

					sbCat.Append((new File(mdOut.path)).CanonicalPath);
					tmpIdx++;
				}
			}

			File fileExportOutTs = new File(fileExportOut.CanonicalPath + ".ts");

			if (useCatCmd)
			{

				//cat 0.ts 1.ts > foo.ts
				Java.Lang.StringBuilder cmdBuff = new Java.Lang.StringBuilder();

				cmdBuff.Append(_cmdCat);
				cmdBuff.Append(" ");

				StringTokenizer st = new StringTokenizer(sbCat.ToString(),"|");

				while (st.HasMoreTokens)
				{
					cmdBuff.Append(st.NextToken()).Append(" ");
				}

				cmdBuff.Append("> ");

				cmdBuff.Append(fileExportOut.CanonicalPath + ".ts");
				var r = Runtime.GetRuntime ();
				r.Exec(cmdBuff.ToString());

				List<string> cmd = new List<string>();

				cmd = new List<string>();

				cmd.Add(_ffmpegBin);
				cmd.Add("-y");
				cmd.Add("-i");

				cmd.Add(fileExportOut.CanonicalPath + ".ts");

				cmd.Add("-c");
				cmd.Add("copy");

				cmd.Add("-an");

				cmd.Add(fileExportOut.CanonicalPath);

				ExecFFMPEG(cmd, sc, null);


			}
			else
			{

				//ffmpeg -i "concat:intermediate1.ts|intermediate2.ts" -c copy -bsf:a aac_adtstoasc output.mp4
				List<string> cmd = new List<string>();

				cmd.Add(_ffmpegBin);
				cmd.Add("-y");
				cmd.Add("-i");
				cmd.Add("concat:" + sbCat.ToString());

				cmd.Add("-c");
				cmd.Add("copy");

				cmd.Add("-an");

				cmd.Add(fileExportOut.CanonicalPath);

				ExecFFMPEG(cmd, sc);

			}

			if ((!fileExportOut.Exists()) || fileExportOut.Length() == 0)
			{
				throw new System.Exception("There was a problem rendering the video: " + fileExportOut.CanonicalPath);
			}


		}

		public Clip GetInfo(Clip inputClip)
		{
			List<string> cmd = new List<string>();

			cmd = new List<string>();

			cmd.Add(_ffmpegBin);
			cmd.Add("-y");
			cmd.Add("-i");

			cmd.Add((new File(inputClip.path)).CanonicalPath);
			var exi = System.IO.File.Exists("/data/data/com.pure.ffmpeg/app_bin/ffmpeg");
			InfoParser ip = new InfoParser(this, inputClip, null, null);
			ExecFFMPEG(cmd,ip, null);

			try
			{
				System.Threading.Thread.Sleep(200);
			}
			catch (System.Exception)
			{
			}
			

			return inputClip;

		}

		private class InfoParser : FFMpegCallbacks
		{
			private readonly FFMpeg outerInstance;


			internal Clip mMedia;
			internal int retValue;

			public InfoParser(FFMpeg outerInstance, Clip media, ICommand completedAction, ICommand messageAction) : base(completedAction, messageAction)
			{
				this.outerInstance = outerInstance;
				mMedia = media;
			}

			public override void ShellOut(string shellLine)
			{
				if (shellLine.Contains ("Duration:")) {

					//		  Duration: 00:01:01.75, start: 0.000000, bitrate: 8184 kb/s

					string[] timecode = shellLine.Split (',') [0].Split (':');


					double duration = 0;

					duration = System.Convert.ToDouble (timecode [1].Trim ()) * 60 * 60; //hours
					duration += System.Convert.ToDouble (timecode [2].Trim ()) * 60; //minutes
					duration += System.Convert.ToDouble (timecode [3].Trim ()); //seconds

					mMedia.duration = duration;


				}

				//   Stream #0:0(eng): Video: h264 (High) (avc1 / 0x31637661), yuv420p, 1920x1080, 16939 kb/s, 30.02 fps, 30 tbr, 90k tbn, 180k tbc
				else if (shellLine.Contains (": Video:")) {
					string[] line = shellLine.Split (':');

					// Leon Pelletier - Added the space delimiter too. Actually it seems like the only useful delimiter here for the purpose of this helper. :O
					string[] videoInfo = line [3].Trim().Split (',') [0].Trim().Split (' ');
					mMedia.videoCodec = videoInfo [0];
				}

				//Stream #0:1(eng): Audio: aac (mp4a / 0x6134706D), 48000 Hz, stereo, s16, 121 kb/s
				else if (shellLine.Contains (": Audio:")) {
					string[] line = shellLine.Split (':');

					// Leon Pelletier - Added the space delimiter too. Actually it seems like the only useful delimiter here for the purpose of this helper. :O
					string[] audioInfo = line [3].Trim().Split (',') [0].Trim().Split (' ');
					mMedia.audioCodec = audioInfo [0];

				} else {
//					_messageAction.Invoke ("...");
				}


		//
		//Stream #0.0(und): Video: h264 (Baseline), yuv420p, 1280x720, 8052 kb/s, 29.97 fps, 90k tbr, 90k tbn, 180k tbc
		//Stream #0.1(und): Audio: mp2, 22050 Hz, 2 channels, s16, 127 kb/s

			}

			public override void ProcessComplete(int exitValue)
			{
				retValue = exitValue;

			}
		}

		private class StreamGobbler : Java.Lang.Thread
		{
			private readonly FFMpeg outerInstance;

			internal System.IO.Stream inputStream;
			internal string type;
			internal FFMpegCallbacks sc;

			internal StreamGobbler(FFMpeg outerInstance, System.IO.Stream inputStream, string type, FFMpegCallbacks sc)
			{
				this.outerInstance = outerInstance;
				this.inputStream = inputStream;
				this.type = type;
				this.sc = sc;
			}

			public override void Run()
			{
				try {
					InputStreamReader isr = new InputStreamReader (inputStream);
					BufferedReader br = new BufferedReader (isr);
					string line = null;
					while ((line = br.ReadLine ()) != null) {
						if (sc != null) {
							sc.ShellOut (line);
						}
					}
//					if (br.Ready()) {
//						var line = br.ReadLine();
//						if (sc != null) {
//							sc.ShellOut (line);
//						}
//					}

				} catch (IOException ioe) {
					//   Log.e(TAG,"error reading shell slog",ioe);
					sc._messageAction.Execute (ioe.ToString ());
					sc._messageAction.Execute (ioe.StackTrace);
				}
			}
		}

		public Bitmap GetVideoFrame(string videoPath, long frameTime)
		{
			MediaMetadataRetriever retriever = new MediaMetadataRetriever();

			try
			{
				retriever.SetDataSource(videoPath);
				return retriever.GetFrameAtTime(frameTime, Option.Closest);
			}
			finally
			{
				try
				{
					retriever.Release();
				}
				catch (System.Exception)
				{
				}
			}
		}
	}

	/*
	 * Main options:
	-L                  show license
	-h                  show help
	-?                  show help
	-help               show help
	--help              show help
	-version            show version
	-formats            show available formats
	-codecs             show available codecs
	-bsfs               show available bit stream filters
	-protocols          show available protocols
	-filters            show available filters
	-pix_fmts           show available pixel formats
	-sample_fmts        show available audio sample formats
	-loglevel loglevel  set libav* logging level
	-v loglevel         set libav* logging level
	-debug flags        set debug flags
	-report             generate a report
	-f fmt              force format
	-i filename         input file name
	-y                  overwrite output files
	-n                  do not overwrite output files
	-c codec            codec name
	-codec codec        codec name
	-pre preset         preset name
	-t duration         record or transcode "duration" seconds of audio/video
	-fs limit_size      set the limit file size in bytes
	-ss time_off        set the start time offset
	-itsoffset time_off  set the input ts offset
	-itsscale scale     set the input ts scale
	-timestamp time     set the recording timestamp ('now' to set the current time)
	-metadata string=string  add metadata
	-dframes number     set the number of data frames to record
	-timelimit limit    set max runtime in seconds
	-target type        specify target file type ("vcd", "svcd", "dvd", "dv", "dv50", "pal-vcd", "ntsc-svcd", ...)
	-xerror             exit on error
	-frames number      set the number of frames to record
	-tag fourcc/tag     force codec tag/fourcc
	-filter filter_list  set stream filterchain
	-stats              print progress report during encoding
	-attach filename    add an attachment to the output file
	-dump_attachment filename  extract an attachment into a file
	-bsf bitstream_filters  A comma-separated list of bitstream filters
	-dcodec codec       force data codec ('copy' to copy stream)
	
	Advanced options:
	-map file.stream[:syncfile.syncstream]  set input stream mapping
	-map_channel file.stream.channel[:syncfile.syncstream]  map an audio channel from one stream to another
	-map_meta_data outfile[,metadata]:infile[,metadata]  DEPRECATED set meta data information of outfile from infile
	-map_metadata outfile[,metadata]:infile[,metadata]  set metadata information of outfile from infile
	-map_chapters input_file_index  set chapters mapping
	-benchmark          add timings for benchmarking
	-dump               dump each input packet
	-hex                when dumping packets, also dump the payload
	-re                 read input at native frame rate
	-loop_input         deprecated, use -loop
	-loop_output        deprecated, use -loop
	-vsync              video sync method
	-async              audio sync method
	-adrift_threshold threshold  audio drift threshold
	-copyts             copy timestamps
	-copytb source      copy input stream time base when stream copying
	-shortest           finish encoding within shortest input
	-dts_delta_threshold threshold  timestamp discontinuity delta threshold
	-copyinkf           copy initial non-keyframes
	-q q                use fixed quality scale (VBR)
	-qscale q           use fixed quality scale (VBR)
	-streamid streamIndex:value  set the value of an outfile streamid
	-muxdelay seconds   set the maximum demux-decode delay
	-muxpreload seconds  set the initial demux-decode delay
	-fpre filename      set options from indicated preset file
	
	Video options:
	-vframes number     set the number of video frames to record
	-r rate             set frame rate (Hz value, fraction or abbreviation)
	-s size             set frame size (WxH or abbreviation)
	-aspect aspect      set aspect ratio (4:3, 16:9 or 1.3333, 1.7777)
	-bits_per_raw_sample number  set the number of bits per raw sample
	-croptop size       Removed, use the crop filter instead
	-cropbottom size    Removed, use the crop filter instead
	-cropleft size      Removed, use the crop filter instead
	-cropright size     Removed, use the crop filter instead
	-padtop size        Removed, use the pad filter instead
	-padbottom size     Removed, use the pad filter instead
	-padleft size       Removed, use the pad filter instead
	-padright size      Removed, use the pad filter instead
	-padcolor color     Removed, use the pad filter instead
	-vn                 disable video
	-vcodec codec       force video codec ('copy' to copy stream)
	-sameq              use same quantizer as source (implies VBR)
	-same_quant         use same quantizer as source (implies VBR)
	-pass n             select the pass number (1 or 2)
	-passlogfile prefix  select two pass log file name prefix
	-vf filter list     video filters
	-b bitrate          video bitrate (please use -b:v)
	-dn                 disable data
	
	Advanced Video options:
	-pix_fmt format     set pixel format
	-intra              use only intra frames
	-vdt n              discard threshold
	-rc_override override  rate control override for specific intervals
	-deinterlace        deinterlace pictures
	-psnr               calculate PSNR of compressed frames
	-vstats             dump video coding statistics to file
	-vstats_file file   dump video coding statistics to file
	-intra_matrix matrix  specify intra matrix coeffs
	-inter_matrix matrix  specify inter matrix coeffs
	-top                top=1/bottom=0/auto=-1 field first
	-dc precision       intra_dc_precision
	-vtag fourcc/tag    force video tag/fourcc
	-qphist             show QP histogram
	-force_fps          force the selected framerate, disable the best supported framerate selection
	-force_key_frames timestamps  force key frames at specified timestamps
	-vbsf video bitstream_filters  deprecated
	-vpre preset        set the video options to the indicated preset
	
	Audio options:
	-aframes number     set the number of audio frames to record
	-aq quality         set audio quality (codec-specific)
	-ar rate            set audio sampling rate (in Hz)
	-ac channels        set number of audio channels
	-an                 disable audio
	-acodec codec       force audio codec ('copy' to copy stream)
	-vol volume         change audio volume (256=normal)
	-rmvol volume       rematrix volume (as factor)
	
	 */

	/*
	 * //./ffmpeg -y -i test.mp4 -vframes 999999  -vf 'redact=blurbox.txt [out] [d], [d]nullsink' -acodec copy outputa.mp4
	    	
	    	//ffmpeg -v 10 -y -i /sdcard/org.witness.sscvideoproto/videocapture1042744151.mp4 -vcodec libx264
	    	//-b 3000k -s 720x480 -r 30 -acodec copy -f mp4 -vf 'redact=/data/data/org.witness.sscvideoproto/redact_unsort.txt'
	    	///sdcard/org.witness.sscvideoproto/new.mp4
	    	
	    	//"-vf" , "redact=" + Environment.getExternalStorageDirectory().getPath() + "/" + PACKAGENAME + "/redact_unsort.txt",
	
	    	
	    	// Need to make sure this will create a legitimate mp4 file
	    	//"-acodec", "ac3", "-ac", "1", "-ar", "16000", "-ab", "32k",
	
	    	
	    	String[] ffmpegCommand = {"/data/data/"+PACKAGENAME+"/ffmpeg", "-v", "10", "-y", "-i", recordingFile.getPath(), 
	    					"-vcodec", "libx264", "-b", "3000k", "-vpre", "baseline", "-s", "720x480", "-r", "30",
	    					//"-vf", "drawbox=10:20:200:60:red@0.5",
	    					"-vf" , "\"movie="+ overlayImage.getPath() +" [logo];[in][logo] overlay=0:0 [out]\"",
	    					"-acodec", "copy",
	    					"-f", "mp4", savePath.getPath()+"/output.mp4"};
	    	
	    	
	    	
	
	//ffmpeg -i source-video.avi -s 480x320 -vcodec mpeg4 -acodec aac -ac 1 -ar 16000 -r 13 -ab 32000 -aspect 3:2 output-video.mp4/
	
	
	 */


	/* concat doesn't seem to work
	cmd.add("-i");
	
	StringBuffer concat = new stringBuffer();
	
	for (int i = 0; i < videos.size(); i++)
	{
		if (i > 0)
			concat.append("|");
		
		concat.append(out.path + '.' + i + ".wav");
		
	}
	
	cmd.add("concat:\"" + concat.ToString() + "\"");
	*/


}