extern "C" {
#include <libavformat/avformat.h>
#include <libavcodec/avcodec.h>
#include <libavutil/avutil.h>
}
#include <iostream>

#include "main.h"

bool ConvertFFmpeg(AVFormatContext* inFmt, AVFormatContext* outFmt, int streamIndex, const char* codecName);
bool ConvertFFmpegAudio(AVFormatContext* inFmt, AVCodecContext* inCodecCtx, AVCodec* inCodec, AVFormatContext* outFmt, AVStream* stream, AVCodec* outCodec, int streamIndex);

extern "C" {
	bool ConvertFFmpegFile(const char* inFile, const char* inFormatName, const char* outFile, const char* outFormatName, const char* codecName) {
		av_register_all();
		
		AVInputFormat* inFmtStruct = av_find_input_format(inFormatName);
		if(!inFmtStruct) {
			std::cerr << "ffmpeg-convert: Could not find input format.\n";
			return false;
		}
		
		AVFormatContext* inFmt = NULL;
		if(avformat_open_input(&inFmt, inFile, inFmtStruct, NULL) != 0) {
			std::cerr << "ffmpeg-convert: Could not open input.\n";
			return false;
		}
		
		AVFormatContext* outFmt;
		int err = avformat_alloc_output_context2(&outFmt, NULL, outFormatName, outFile);
		if(outFmt == NULL || err < 0) {
			std::cerr << "ffmpeg-convert: Could not open output. Error: " << err << "\n";
			avformat_close_input(&inFmt);
			return false;
		}
		
		// open the output file, if needed
		if(!(outFmt->oformat->flags & AVFMT_NOFILE)) {
			if(avio_open(&outFmt->pb, outFile, AVIO_FLAG_WRITE) < 0) {
				std::cerr << "ffmpeg-convert: Could not open output file\n";
				return false;
			}
		}
		
		if(!ConvertFFmpeg(inFmt, outFmt, -1, codecName)) {
			return false;
		}
		
		avformat_close_input(&inFmt);
		av_free(outFmt);
		
		return true;
	}
	
	bool ConvertFFmpegStream(FFmpegURLRead inStreamRead, const char* inFormat, FFmpegURLWrite outStreamWrite, const char* outFormat, const char* codec) {
		
	}
}

// Helper functions
bool ConvertFFmpeg(AVFormatContext* inFmt, AVFormatContext* outFmt, int streamIndex, const char* codecName) {
	// Prepare inFmt
	if(avformat_find_stream_info(inFmt, NULL) < 0) {
		std::cerr << "ffmpeg-convert: Could not find stream info.\n";
		return false;
	}
	
	// Prepare inCodec
	AVCodec* inCodec = NULL;
	int realStreamIndex = av_find_best_stream(inFmt, AVMEDIA_TYPE_AUDIO, streamIndex, -1, &inCodec, 0);
	
	if(realStreamIndex < 0) {
		std::cerr << "ffmpeg-convert: Could not find input stream.\n";
		return false;
	}
	
	AVCodec* outCodec = avcodec_find_encoder_by_name(codecName);
	if(outCodec == NULL) {
		std::cerr << "ffmpeg-convert: Could not find codec.\n";
		avformat_close_input(&inFmt);
		return false;
	}
	
	AVCodecContext* inCodecCtx = inFmt->streams[realStreamIndex]->codec;
	
	
	AVStream* outStream = avformat_new_stream(outFmt, outCodec);
	if(!outStream) {
		std::cerr << "ffmpeg-convert: Could not create stream.\n";
		return false;
	}
	
	if(outFmt->oformat->flags & AVFMT_GLOBALHEADER) {
		outStream->codec->flags |= CODEC_FLAG_GLOBAL_HEADER;
	}
	
	if(avformat_write_header(outFmt, NULL) != 0) {
		std::cerr << "ffmpeg-convert: Could not write header.\n";
		return false;
	}
	
	if(!ConvertFFmpegAudio(inFmt, inCodecCtx, inCodec, outFmt, outStream, outCodec, realStreamIndex)) {
		return false;
	}
	
	av_write_trailer(outFmt);
	
	avcodec_close(inCodecCtx);
	
	for(int i = 0; i < outFmt->nb_streams; ++i) {
		av_freep(&outFmt->streams[i]->codec);
		av_freep(&outFmt->streams[i]);
	}
	
	return true;
}

bool ConvertFFmpegAudio(AVFormatContext* inFmt, AVCodecContext* inCodecCtx, AVCodec* inCodec, AVFormatContext* outFmt, AVStream* audioStream, AVCodec* outCodec, int streamIndex) {
	audioStream->id = 1;
	audioStream->codec->sample_fmt = inCodecCtx->sample_fmt;
	audioStream->codec->sample_rate = inCodecCtx->sample_rate;
	audioStream->codec->channels = inCodecCtx->channels;
	audioStream->codec->channel_layout = inCodecCtx->channel_layout;
	
	if(avcodec_open2(inCodecCtx, inCodec, NULL) < 0) {
		std::cerr << "ffmpeg-convert: Could not open input codec.\n";
		return false;
	}
	
	if(avcodec_open2(audioStream->codec, outCodec, NULL) < 0) {
		std::cerr << "ffmpeg-convert: Could not open output codec.\n";
		return false;
	}
	
	AVPacket inPacket;
	av_init_packet(&inPacket);
	
	AVFrame* frame = NULL;
	
	while(av_read_frame(inFmt, &inPacket) == 0) {
		std::cerr << "index = " << inPacket.stream_index << ", " << streamIndex << "\n";
		if(inPacket.stream_index == streamIndex) {
			int got_frame_ptr = 0;
			while(got_frame_ptr == 0) {
				if(frame == NULL) {
					if((frame = avcodec_alloc_frame()) == NULL) {
						std::cerr << "ffmpeg-convert: Error allocating frame.\n";
						return false;
					}
				}
				else {
					avcodec_get_frame_defaults(frame);
				}
				
				if(avcodec_decode_audio4(inCodecCtx, frame, &got_frame_ptr, &inPacket) < 0) {
					std::cerr << "ffmpeg-convert: Error while decoding.\n";
					avformat_close_input(&inFmt);
					return false;
				}
			}
			
			
			AVPacket outPacket = { 0 };
			av_init_packet(&outPacket);
			int got_packet_ptr = 0;
			while(got_packet_ptr == 0) {
				if(avcodec_encode_audio2(audioStream->codec, &outPacket, frame, &got_packet_ptr) != 0) {
					std::cerr << "ffmpeg-convert: Error while encoding.\n";
					avformat_close_input(&inFmt);
					return false;
				}
			}
			
			if(av_interleaved_write_frame(outFmt, &outPacket) != 0) {
				std::cerr << "ffmpeg-convert: Error while writing frame.\n";
				return false;
			}
		}
	}
	
	av_free(frame);
	return true;
}

/*
int main() {
	ConvertFFmpegFile("/home/john/Projects/audio.thd", "truehd", "test.wav", "wav", "pcm_s24le");
}
*/