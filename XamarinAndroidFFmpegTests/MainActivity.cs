// This project is based on https://github.com/guardianproject/android-ffmpeg-java
// Thus, it is licensed under the GPL


using Android.Widget;
using Android.App;
using Android.OS;
using System.Threading.Tasks;
using XamarinAndroidFFmpeg;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;
using System;

namespace XamarinAndroidFFmpegTests
{
	[Activity (Label = "XamarinAndroidFFmpegTests", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity
	{
		static string _workingDirectory = "";

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);
			_logView = FindViewById<EditText> (Resource.Id.message);
			Task.Run (() => Start ());
		}

		EditText _logView;

		void Start() {
			
			_workingDirectory = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
			var sourceMp4 = "cat1.mp4";
			var destinationPathAndFilename = System.IO.Path.Combine (_workingDirectory, "cat1_out.mp4");
			var destinationPathAndFilename2 = System.IO.Path.Combine (_workingDirectory, "cat1_out2.mp4");
			var destinationPathAndFilename4 = System.IO.Path.Combine (_workingDirectory, "cat1_out4.wav");
			if (File.Exists (destinationPathAndFilename))
				File.Delete (destinationPathAndFilename);
			CreateSampleFile(Resource.Raw.cat1, _workingDirectory, sourceMp4);


			var ffmpeg = new FFMpeg (this, _workingDirectory);

			var sourceClip = new Clip (System.IO.Path.Combine(_workingDirectory, sourceMp4));

			var result = ffmpeg.GetInfo (sourceClip);

			var br = System.Environment.NewLine;

			// There are callbacks based on Standard Output and Standard Error when ffmpeg binary is running as a process:

			var onComplete = new MyCommand ((_) => {
				RunOnUiThread(() =>_logView.Append("DONE!" + br + br));
			});

			var onMessage = new MyCommand ((message) => {
				RunOnUiThread(() =>_logView.Append(message + br + br));
			});

			var callbacks = new FFMpegCallbacks (onComplete, onMessage);

			// 1. The idea of this first test is to show that video editing is possible via FFmpeg:
			// It results in a 150x150 movie that eventually zooms on a cat ear. This is desaturated, and there's a fade in.
			 
			var filters = new List<VideoFilter> ();
			filters.Add (new FadeVideoFilter ("in", 0, 100));
			filters.Add(new CropVideoFilter("150","150","0","0"));
			filters.Add(new ColorVideoFilter(1.0m, 1.0m, 0.0m, 0.5m, 1.0m, 1.0m, 1.0m, 1.0m));
			var outputClip = new Clip (destinationPathAndFilename) { videoFilter = VideoFilter.Build (filters)  };
			outputClip.H264_CRF = "18"; // It's the quality coefficient for H264 - Default is 28. I think 18 is pretty good.
			ffmpeg.ProcessVideo(sourceClip, outputClip, true, new FFMpegCallbacks(onComplete, onMessage));

			//2. This is a similar version version in command line only:
			string[] cmds = new string[] {
				"-y",
				"-i",
				sourceClip.path,
				"-strict",
				"-2",
				"-vf",
				"mp=eq2=1:1.68:0.3:1.25:1:0.96:1",
				destinationPathAndFilename2,
				"-acodec",
				"copy",
			};
			ffmpeg.Execute (cmds, callbacks);

			// 3. This lists codecs:
			string[] cmds3 = new string[] {
				"-codecs",
			};
			ffmpeg.Execute (cmds, callbacks);

			// 4. This convers to WAV
			// Note that the cat movie just has some silent house noise.
			ffmpeg.ConvertToWaveAudio(sourceClip, destinationPathAndFilename4, 44100, 2, callbacks, true);

			// Etc...

			// Rules of thumb:
			// a) Provide the minimum of info to ffmpeg to not mix it up
			// b) These helpers are cool to test capabilities, but useless otherwise, and crashy: Use command lines.
			// c) Try to compile a newer FFmpeg :)

		}


		private void CreateSampleFile(int resource, string destinationFolder, string filename) {
			var data = new byte[0];
			using (var file = Resources.OpenRawResource (resource))
			using (var fileInMemory = new MemoryStream ()) {
				file.CopyTo (fileInMemory);
				data = fileInMemory.ToArray ();
			}
			var fileName = System.IO.Path.Combine (destinationFolder, filename);
			System.IO.File.WriteAllBytes (fileName, data);
		}

		void RemoveSampleFile(string sourceFolder, string name) {
			System.IO.File.Delete (System.IO.Path.Combine (sourceFolder, name));
		}

		public class MyCommand : ICommand
		{
			public delegate void ICommandOnExecute(object parameter = null);
			public delegate bool ICommandOnCanExecute(object parameter);

			private ICommandOnExecute _execute;
			private ICommandOnCanExecute _canExecute;

			public MyCommand(ICommandOnExecute onExecuteMethod)
			{
				_execute = onExecuteMethod;
			}

			public MyCommand(ICommandOnExecute onExecuteMethod, ICommandOnCanExecute onCanExecuteMethod)
			{
				_execute = onExecuteMethod;
				_canExecute = onCanExecuteMethod;
			}

			#region ICommand Members

			public event EventHandler CanExecuteChanged
			{
				add { throw new NotImplementedException(); }
				remove { throw new NotImplementedException(); }
			}

			public bool CanExecute(object parameter)
			{
				if (_canExecute == null && _execute != null)
					return true;

				return _canExecute.Invoke(parameter);
			}

			public void Execute(object parameter)
			{	
				if (_execute == null)
					return;

				_execute.Invoke(parameter);
			}

			#endregion
		}

	}
}


