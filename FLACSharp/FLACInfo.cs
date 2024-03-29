using System;
using WAVSharp;

namespace FLACSharp {
	public class FLACInfo {
		public FLACInfo() {
			
		}
		public FLACInfo(WAVFormatChunk formatChunk) {
			sample_rate = formatChunk.nSamplesPerSec;
			channels = formatChunk.nChannels;
			bits_per_sample = formatChunk.wBitsPerSample;
			channel_mapping = formatChunk.dwChannelMask;
		}
		public uint sample_rate;
		public uint channels;
		public uint bits_per_sample;
		public uint channel_mapping;
	}
}

