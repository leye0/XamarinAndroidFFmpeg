using System;
using System.Text;

/* Copyright (c) 2009, Nathan Freitas, Orbot / The Guardian Project - http://openideals.com/guardian */
/* See LICENSE for licensing information */
using Java.IO;
using Java.Lang;
using System.Windows.Input;


namespace XamarinAndroidFFmpeg
{
	public class ShellUtils
	{

		//various console cmds
			public const string SHELL_CMD_CHMOD = "chmod";
			public const string SHELL_CMD_KILL = "kill -9";
			public const string SHELL_CMD_RM = "rm";
			public const string SHELL_CMD_PS = "ps";
			public const string SHELL_CMD_PIDOF = "pidof";

			public const string CHMOD_EXE_VALUE = "700";

		public static bool RootPossible
		{
			get
			{
    
				var log = new Java.Lang.StringBuilder();
    
				try
				{
    
					// Check if Superuser.apk exists
					File fileSU = new File("/system/app/Superuser.apk");
					if (fileSU.Exists())
					{
						return true;
					}
    
					fileSU = new File("/system/bin/su");
					if (fileSU.Exists())
					{
						return true;
					}
    
					//Check for 'su' binary 
					string[] cmd = new string[] {"which su"};
					int exitCode = ShellUtils.doShellCommand(null,cmd, new ShellCallbackAnonymousInnerClassHelper(null, null), false, true).ExitValue();
    
					if (exitCode == 0)
					{
						logMessage("Can acquire root permissions");
						 return true;
    
					}
    
				}
				catch (IOException e)
				{
					//this means that there is no root to be had (normally) so we won't log anything
					logException("Error checking for root access",e);
    
				}
				catch (System.Exception)
				{
//					logException("Error checking for root access",e);
					//this means that there is no root to be had (normally)
				}
    
				logMessage("Could not acquire root permissions");
    
    
				return false;
			}
		}

		public static int FindProcessId(string command)
		{
			int procId = -1;

			try
			{
				procId = FindProcessIdWithPidOf(command);

				if (procId == -1)
				{
					procId = findProcessIdWithPS(command);
				}
			}
			catch (System.Exception)
			{
				try
				{
					procId = findProcessIdWithPS(command);
				}
				catch (System.Exception)
				{
					//logException("Unable to get proc id for: " + command,e2);
				}
			}

			return procId;
		}

		//use 'pidof' command
		public static int FindProcessIdWithPidOf(string command)
		{

			int procId = -1;

			Process procPs = null;

			string baseName = (new System.IO.FileInfo(command)).Name;
			//fix contributed my mikos on 2010.12.10
			var r = Runtime.GetRuntime();
			procPs = r.Exec(new string[] {SHELL_CMD_PIDOF, baseName});
			//procPs = r.exec(SHELL_CMD_PIDOF);

			BufferedReader reader = new BufferedReader(new InputStreamReader(procPs.InputStream));
			string line = null;

			while ((line = reader.ReadLine()) != null)
			{

				try
				{
					//this line should just be the process id
					procId = Convert.ToInt32(line.Trim());
					break;
				}
				catch (NumberFormatException e)
				{
					logException("unable to parse process pid: " + line,e);
				}
			}


			return procId;

		}

		//use 'ps' command
		public static int findProcessIdWithPS(string command)
		{
			int procId = -1;

			Process procPs = null;
			var r = Runtime.GetRuntime ();
			procPs = r.Exec(SHELL_CMD_PS);

			BufferedReader reader = new BufferedReader(new InputStreamReader(procPs.InputStream));
			string line = null;

			while ((line = reader.ReadLine()) != null)
			{
				if (line.IndexOf(' ' + command) != -1)
				{

					var st = new Java.Util.StringTokenizer(line," ");
					st.NextToken(); //proc owner

					procId = Convert.ToInt32(st.NextToken().Trim());

					break;
				}
			}



			return procId;

		}

		public static int doShellCommand(string[] cmds, FFMpegCallbacks sc, bool runAsRoot, bool waitFor)
		{
			return doShellCommand(null, cmds, sc, runAsRoot, waitFor).ExitValue();
		}

		public static Process doShellCommand(Java.Lang.Process proc, string[] cmds, FFMpegCallbacks sc, bool runAsRoot, bool waitFor)
		{
			var r = Runtime.GetRuntime ();

			if (proc == null)
			{
				if (runAsRoot)
				{
					proc = r.Exec("su");
				}
				else
				{
					proc = r.Exec("sh");
				}
			}

			OutputStreamWriter outputStream = new OutputStreamWriter(proc.OutputStream);

			for (int i = 0; i < cmds.Length; i++)
			{
				logMessage("executing shell cmd: " + cmds[i] + "; runAsRoot=" + runAsRoot + ";waitFor=" + waitFor);

				outputStream.Write(cmds[i]);
				outputStream.Write("\n");
			}

			outputStream.Flush();
			outputStream.Write("exit\n");
			outputStream.Flush();

			if (waitFor)
			{
				char[] buf = new char[20];

				// Consume the "stdout"
				InputStreamReader reader = new InputStreamReader(proc.InputStream);
				int read = 0;
				while ((read = reader.Read(buf)) != -1)
				{
					if (sc != null)
					{
						sc.ShellOut(new string(buf));
					}
				}

				// Consume the "stderr"
				reader = new InputStreamReader(proc.ErrorStream);
				read = 0;
				while ((read = reader.Read(buf)) != -1)
				{
					if (sc != null)
					{
						sc.ShellOut(new string(buf));
					}
				}

				proc.WaitFor();

			}

			sc.ProcessComplete(proc.ExitValue());

			return proc;

		}

		public static void logMessage(string msg)
		{
		}

		public static void logException(string msg, Java.Lang.Exception e)
		{
		}
	}

	public class ShellCallbackAnonymousInnerClassHelper : FFMpegCallbacks
	{
		public ShellCallbackAnonymousInnerClassHelper(ICommand completedAction, ICommand messageAction) : base(completedAction, messageAction)
		{
		}

		public override void ShellOut(string msg)
		{
		}

		public override void ProcessComplete(int exitValue)
		{
			base.ProcessComplete(exitValue);
		}

	}

	public class FFMpegCallbacks
	{
		public ICommand _completedAction {get;set;}

		public ICommand _messageAction {get;set;}

		public FFMpegCallbacks(ICommand completedAction, ICommand messageAction) {
			_completedAction = completedAction;
			_messageAction = messageAction;
		}

		public virtual void ShellOut(string shellLine) {
			_messageAction.Execute (shellLine);
		}

		public virtual void ProcessComplete(int exitValue) {
			_completedAction.Execute (null);
		}
	}
}