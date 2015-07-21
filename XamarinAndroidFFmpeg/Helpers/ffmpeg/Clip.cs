using System;

namespace XamarinAndroidFFmpeg
{

	public class Clip : ICloneable
	{

		public int width = -1;
		public int height = -1;

		public string videoCodec;
		public string videoFps;
		public int videoBitrate = -1;
		public string videoBitStreamFilter;

		public string audioCodec;
		public int audioChannels = -1;
		public int audioBitrate = -1;
		public string audioQuality;
		public int audioVolume = -1;
		public string audioBitStreamFilter;

		public string path;
		public string format;
		public string mimeType;

		public string startTime; //00:00:00 or seconds format
		public double duration = -1; //00:00:00 or seconds format

		public string videoFilter;
		public string audioFilter;

		public string qscaleVideo;
		public string qscaleAudio;
		public string qscale;

		public string H264_CRF;

		public string aspect;
		public int passCount = 1; //default

		public Clip()
		{

		}

		public Clip(string path)
		{
			this.path = path;
		}

		object ICloneable.Clone() {
			return Clone ();
		}

		public virtual Clip Clone()
		{
			return new Clip (this.path) {  
				aspect = this.aspect,
				audioBitrate = this.audioBitrate,
				audioBitStreamFilter = this.audioBitStreamFilter,
				audioChannels = audioChannels,
				audioCodec = this.audioCodec,
				audioFilter = this.audioFilter,
				audioQuality = this.audioQuality,
				audioVolume = this.audioVolume,
				duration = this.duration,
				format = this.format,
				height = this.height,
				mimeType = this.mimeType,
				passCount = this.passCount,
				path = this.path,
				qscale = this.qscale,
				startTime = this.startTime,
				videoBitrate = this.videoBitrate,
				videoBitStreamFilter = this.videoBitStreamFilter,
				videoCodec = this.videoCodec,
				videoFilter = this.videoFilter,
				videoFps = this.videoFps,
				width = this.width
			};
		}

		public virtual bool Image
		{
			get
			{
				if (mimeType != null)
				{
					return mimeType.StartsWith("image");
				}
				else
				{
					return false;
				}
			}
		}

		public virtual bool Video
		{
			get
			{
				if (mimeType != null)
				{
					return mimeType.StartsWith("video");
				}
				else
				{
					return false;
				}
			}
		}

		public virtual bool Audio
		{
			get
			{
				if (mimeType != null)
				{
					return mimeType.StartsWith("audio");
				}
				else
				{
					return false;
				}
			}
		}
	}

}