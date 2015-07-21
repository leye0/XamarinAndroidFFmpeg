using System;
using System.Collections.Generic;
using System.Text;
using Java.Lang;
using Java.IO;
using XamarinAndroidFFmpeg;
using System.Windows.Input;

namespace SoxTools
{
	using R = Android.Resource;

	using Context = Android.Content.Context;
	using Log = Android.Util.Log;

	public class SoxHelpers
	{
		private const string TAG = "SOX";
		internal string[] libraryAssets = new string[] {"sox"};
		private string soxBin;
		private File fileBinDir;
		private FFMpegCallbacks _globalCallback;
		private Context _context;

		public SoxHelpers(Context context, int soxResId, Java.IO.File fileAppRoot, FFMpegCallbacks callback)
		{
			_globalCallback = callback;
			_context = context;
			InstallBinaries(soxResId, false);
			fileBinDir = (new File(soxBin)).ParentFile;
		}


		public void InstallBinaries(int soxResId, bool overwrite)
		{
			soxBin = InstallBinary(soxResId, "sox", overwrite);
		}

		public string BinaryPath
		{
			get
			{
				return soxBin;
			}
		}

		private string InstallBinary(int resId, string filename, bool upgrade)
		{
			try
			{
				File f = new File(_context.GetDir("bin", 0), filename);
				if (f.Exists())
				{
					f.Delete();
				}
				CopyRawFile(resId, f.AbsolutePath, "0755");
				return f.CanonicalPath;
			}
			catch (System.Exception)
			{
//				Log.e(TAG, "installBinary failed: " + e.LocalizedMessage);
				return null;
			}
		}

		/// <summary>
		/// Copies a raw resource file, given its ID to the given location </summary>
		/// <param name="ctx"> context </param>
		/// <param name="resid"> resource id </param>
		/// <param name="file"> destination file </param>
		/// <param name="mode"> file permissions (E.g.: "755") </param>
		/// <exception cref="IOException"> on error </exception>
		/// <exception cref="InterruptedException"> when interrupted </exception>
		private void CopyRawFile(int resid, string fileFullPath, string mode)
		{

			// Write the iptables binary
			var outputStream = new System.IO.FileStream(fileFullPath, System.IO.FileMode.Create);

			var inputStream = _context.Resources.OpenRawResource(resid);
			var buf = new byte[1024];
			int len;
			while ((len = inputStream.Read(buf, 0, buf.Length)) > 0)
			{
				outputStream.Write(buf, 0, len);
			}
			outputStream.Close();
			inputStream.Close();
			// Change the permissions
			var r = Runtime.GetRuntime ();
			r.Exec("chmod " + mode + " " + fileFullPath).WaitFor();
		}

		private class LengthParser : FFMpegCallbacks
		{
			private readonly SoxHelpers _outerInstance;

			public LengthParser(SoxHelpers outerInstance, FFMpegCallbacks callbacks) : base(callbacks._completedAction, callbacks._messageAction)
			{
				_outerInstance = outerInstance;
				_messageAction = callbacks._messageAction;
				_completedAction= callbacks._completedAction;
			}

			public double length;
			public int retValue = -1;

			public override void ShellOut(string shellLine)
			{
				if (!shellLine.StartsWith("Length"))
				{
					return;
				}
				string[] split = shellLine.Split(':');
				if (split.Length != 2)
				{
					return;
				}

				string lengthStr = split[1].Trim();

				try
				{
					length = Convert.ToDouble(lengthStr);
				} catch (System.Exception e) {
					_messageAction.Execute (e.ToString ());
					_messageAction.Execute (e.StackTrace);
				}
			}

			public override void ProcessComplete(int exitValue)
			{
				retValue = exitValue;
				base.ProcessComplete (exitValue);
			}
		}

		/// <summary>
		/// Retrieve the length of the audio file
		/// sox file.wav 2>&1 -n stat | grep Length | cut -d : -f 2 | cut -f 1 </summary>
		/// <returns> the length in seconds or null </returns>
		public double GetLength(string path)
		{
			List<string> cmd = new List<string>();

			cmd.Add(soxBin);
			cmd.Add(path);
			cmd.Add("-n");
			cmd.Add("stat");

			LengthParser sc = new LengthParser (this, _globalCallback);

			try
			{
				ExecSox(cmd, sc);
			}
			catch (System.Exception)
			{
				return -1;
			}

			return sc.length;
		}

		/// <summary>
		/// Discard all audio not between start and length (length = end by default)
		/// sox <path> -e signed-integer -b 16 outFile trim <start> <length> </summary>
		/// <param name="start"> </param>
		/// <param name="length"> (optional) </param>
		/// <returns> path to trimmed audio </returns>
		public string TrimAudio(string path, double start, double length)
		{
			List<string> cmd = new List<string>();

			File file = new File(path);
			string outFile = file.CanonicalPath + "_trimmed.wav";
			cmd.Add(soxBin);
			cmd.Add(path);
			cmd.Add("-e");
			cmd.Add("signed-integer");
			cmd.Add("-b");
			cmd.Add("16");
			cmd.Add(outFile);
			cmd.Add("trim");
			cmd.Add(start + "");
			if (length != -1)
			{
				cmd.Add(length.ToString() + "");
			}

			int rc = ExecSox (cmd, _globalCallback);;
			if (rc != 0)
			{
				outFile = null;
			}

			if (file.Exists())
			{
				return outFile;
			}
			else
			{
				return null;
			}

		}

		/// <summary>
		/// Fade audio file
		/// sox <path> outFile fade <type> <fadeInLength> <stopTime> <fadeOutLength> </summary>
		/// <param name="path"> </param>
		/// <param name="type"> </param>
		/// <param name="fadeInLength"> specify 0 if no fade in is desired </param>
		/// <param name="stopTime"> (optional) </param>
		/// <param name="fadeOutLength"> (optional)
		/// @return </param>
		public string FadeAudio(string path, string type, double fadeInLength, double stopTime, double fadeOutLength)
		{
			var curves = new List<string> {"q", "h", "t", "l", "p"};

			if (!curves.Contains(type))
			{
				throw new System.Exception("fadeAudio: passed invalid type: " + type);

			}

			File file = new File(path);
			string outFile = file.CanonicalPath + "_faded.wav";

			List<string> cmd = new List<string>();
			cmd.Add(soxBin);
			cmd.Add(path);
			cmd.Add(outFile);
			cmd.Add("fade");
			cmd.Add(type);
			cmd.Add(fadeInLength + "");
			if (stopTime != -1)
			{
				cmd.Add(stopTime + "");
			}
			if (fadeOutLength != -1)
			{
				cmd.Add(fadeOutLength + "");
			}

			try
			{
				int rc = ExecSox(cmd, _globalCallback);
				if (rc != 0)
				{
					//Log.e(TAG, "fadeAudio receieved non-zero return code!");

					outFile = null;
				}
			}
			catch (IOException e)
			{
				// TODO Auto-generated catch block
				_globalCallback._messageAction.Execute(e.ToString());
				_globalCallback._messageAction.Execute(e.StackTrace);
			}
			catch (InterruptedException e)
			{
				// TODO Auto-generated catch block
				_globalCallback._messageAction.Execute(e.ToString());
				_globalCallback._messageAction.Execute(e.StackTrace);
			}
			return outFile;
		}

		/// <summary>
		/// Combine and mix audio files
		/// sox -m -v 1.0 file[0] -v 1.0 file[1] ... -v 1.0 file[n] outFile
		/// Should support passing of volume </summary>
		/// <param name="files"> </param>
		/// <returns> combined and mixed file (null on failure) </returns>
		public string CombineMix(IList<string> files, string outFile)
		{
			List<string> cmd = new List<string>();
			cmd.Add(soxBin);
			cmd.Add("-m");

			foreach (string file in files)
			{
				cmd.Add("-v");
				cmd.Add("1.0");
				cmd.Add(file);
			}
			cmd.Add(outFile);

			try
			{
				int rc = ExecSox(cmd, _globalCallback);
				if (rc != 0)
				{
					//	Log.e(TAG, "combineMix receieved non-zero return code!");
					outFile = null;
				}
			}
			catch (IOException e)
			{
				_globalCallback._messageAction.Execute(e.ToString());
				_globalCallback._messageAction.Execute(e.StackTrace);
			}
			catch (InterruptedException e)
			{
				_globalCallback._messageAction.Execute(e.ToString());
				_globalCallback._messageAction.Execute(e.StackTrace);
			}
			return outFile;
		}

		/// <summary>
		/// Simple combiner
		/// sox file[0] file[1] ... file[n] <outFile> </summary>
		/// <param name="files"> </param>
		/// <param name="outFile"> </param>
		/// <returns> outFile or null on failure </returns>
		public string Combine(IList<string> files, string outFile)
		{
			List<string> cmd = new List<string>();
			cmd.Add(soxBin);

			foreach (string file in files)
			{
				cmd.Add(file);
			}
			cmd.Add(outFile);

			int rc = ExecSox(cmd, _globalCallback);
			if (rc != 0)
			{
				throw new System.Exception("exit code: " + rc);

			}

			return outFile;
		}

		public int ExecSox(IList<string> cmd, FFMpegCallbacks sc)
		{

			string soxBin = (new File(fileBinDir, "sox")).CanonicalPath;
			var r = Runtime.GetRuntime ();
			r.Exec("chmod 700 " + soxBin);
			return ExecProcess(cmd, sc);
		}

		private int ExecProcess(IList<string> cmds, FFMpegCallbacks sc)
		{
			//ensure that the arguments are in the correct Locale format
			// TODO: Useless since SOX is called internally. Remove.
			for ( int i = 0; i < cmds.Count; i++)
			{
				cmds[i] = string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"), "%s", cmds[i]);
			}

			ProcessBuilder pb = new ProcessBuilder(cmds);
			pb.Directory(fileBinDir);

			var cmdlog = new Java.Lang.StringBuilder();

			foreach (string cmd in cmds)
			{
				cmdlog.Append(cmd);
				cmdlog.Append(' ');
			}

			sc.ShellOut(cmdlog.ToString());

			pb.RedirectErrorStream(true);
			var process = pb.Start();

			StreamGobbler errorGobbler = new StreamGobbler(this, process.ErrorStream, "ERROR", sc);
			StreamGobbler outputGobbler = new StreamGobbler(this, process.InputStream, "OUTPUT", sc);

			errorGobbler.Run();
			outputGobbler.Run();

			int exitVal = process.WaitFor();

			while (outputGobbler.IsAlive || errorGobbler.IsAlive);
            
			sc.ProcessComplete(exitVal);

			return exitVal;
		}

		internal class StreamGobbler : Thread
		{
			private readonly SoxHelpers outerInstance;

			internal System.IO.Stream inputStream;
			internal string type;
			internal FFMpegCallbacks sc;

			internal StreamGobbler(SoxHelpers outerInstance, System.IO.Stream inputStream, string type, FFMpegCallbacks sc)
			{
				this.outerInstance = outerInstance;
				this.inputStream = inputStream;
				this.type = type;
				this.sc = sc;
			}

			public override void Run()
			{
				try
				{
					var isr = new Java.IO.InputStreamReader(inputStream);
					BufferedReader br = new BufferedReader(isr);
					string line = null;
					while ((line = br.ReadLine()) != null)
					{
						if (sc != null)
						{
							sc.ShellOut(line);
						}
					}
				}
				catch (IOException ioe)
				{
//					_messageAction.Invoke(ioe.ToString());
//					_messageAction.Invoke(ioe.StackTrace);
				}
			}
		}
	}
}