using System.Collections.Generic;
using System.IO;

namespace SoxTools
{


	/// <summary>
	/// Concatenates two files together with a crossfade of user
	/// defined mClipLength.
	/// 
	/// It is a Java port of the scripts/crossfade_cat.sh script
	/// in the sox source tree.
	/// 
	/// Original script by Kester Clegg, with modifications by Chris
	/// Bagwell.
	/// 
	/// @author Abel Luck
	/// 
	/// </summary>
	// TODO make runnable?
	public class CrossfadeCat
	{
		private const string TAG = "SOX-XFADE";
		private SoxHelpers _soxHelper;
		private string mFirstFile;
		private string mSecondFile;
		private double mFadeLength;
		private string mFinalMix;

		public CrossfadeCat(SoxHelpers soxHelper, string firstFile, string secondFile, double fadeLength, string outFile)
		{
			_soxHelper = soxHelper;
			mFirstFile = firstFile;
			mSecondFile = secondFile;
			mFadeLength = fadeLength;
			mFinalMix = outFile;

			//double mClipLength = mController.getLength(mFirstFile);
		}

		public virtual bool start()
		{
			// find mClipLength of first file


			// Obtain trimLength seconds of fade out position from the first File
			double firstFileLength = _soxHelper.GetLength(mFirstFile);
			double trimLength = firstFileLength - mFadeLength;

			string trimmedOne = _soxHelper.TrimAudio(mFirstFile, trimLength, mFadeLength);

			if (trimmedOne == null)
			{
				throw new IOException("audio trim did not complete: " + mFirstFile);
			}

				// We assume a fade out is needed (i.e., firstFile doesn't already fade out)

			string fadedOne = _soxHelper.FadeAudio(trimmedOne, "t", 0, mFadeLength, mFadeLength);
			if (fadedOne == null)
			{
				throw new IOException("audio fade did not complete: " + trimmedOne);
			}

			// Get crossfade section from the second file
			string trimmedTwo = _soxHelper.TrimAudio(mSecondFile, 0, mFadeLength);
			if (trimmedTwo == null)
			{
				throw new IOException("audio trim did not complete: " + mSecondFile);
			}

			string fadedTwo = _soxHelper.FadeAudio(trimmedTwo, "t", mFadeLength, -1, -1);
			if (fadedTwo == null)
			{
				throw new IOException("audio fade did not complete: " + trimmedTwo);
			}

			// Mix crossfaded files together at full volume
			List<string> files = new List<string>();
			files.Add(fadedOne);
			files.Add(fadedTwo);

			string crossfaded = (new Java.IO.File(mFirstFile)).CanonicalPath + "-x-" + (new System.IO.FileInfo(mSecondFile)).Name + ".wav";
			crossfaded = _soxHelper.CombineMix(files, crossfaded);
			if (crossfaded == null)
			{
				throw new IOException("crossfade did not complete");
			}

			// Trim off crossfade sections from originals
			string trimmedThree = _soxHelper.TrimAudio(mFirstFile, 0, trimLength);
			if (trimmedThree == null)
			{
				throw new IOException("crossfade trim beginning did not complete");
			}

			string trimmedFour = _soxHelper.TrimAudio(mSecondFile, mFadeLength, -1);
			if (trimmedFour == null)
			{
				throw new IOException("crossfade trim end did not complete");
			}

			// Combine into final mix
			files.Clear();
			files.Add(trimmedThree);
			files.Add(crossfaded);
			files.Add(trimmedFour);
			mFinalMix = _soxHelper.Combine(files, mFinalMix);

			if (mFinalMix == null)
			{
				throw new IOException("final mix did not complete");
			}

			return true;
		}


	}

}