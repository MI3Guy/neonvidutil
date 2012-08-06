extern "C" {
#include <libavformat/avformat.h>
#include <libavcodec/avcodec.h>
#include <libavutil/avutil.h>
}
#include <iostream>

#include "main.h"

const int BufferSize = 4*1024;

// Helper Functions
bool ConvertFFmpeg(AVFormatContext* inFmt, AVFormatContext* outFmt, int streamIndex, const char* codecName);
bool ConvertFFmpegAudio(AVFormatContext* inFmt, AVCodecContext* inCodecCtx, AVCodec* inCodec, AVFormatContext* outFmt, AVStream* stream, AVCodec* outCodec, int streamIndex);


extern "C" {
	void InitFFmpeg() {
		av_register_all();
	}
	
	bool ConvertFFmpegFileFile(const char* inFile, const char* inFormatName, const char* outFile, const char* outFormatName, const char* codecName) {
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
		
		AVFormatContext* outFmt = NULL;
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
	
	bool ConvertFFmpegFileStream(const char* inFile, const char* inFormatName, FFmpegURLWrite outStreamWrite, int outFid, const char* outFormatName, const char* codecName) {
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
		
		AVFormatContext* outFmt = NULL;
		int err = avformat_alloc_output_context2(&outFmt, NULL, outFormatName, NULL);
		if(outFmt == NULL || err < 0) {
			std::cerr << "ffmpeg-convert: Could not open output. Error: " << err << "\n";
			avformat_close_input(&inFmt);
			return false;
		}
		
		unsigned char* outBuff = NULL;
		outFmt->pb = NULL;
		if(!(outFmt->oformat->flags & AVFMT_NOFILE)) {
			outBuff = (unsigned char*)av_malloc(BufferSize);
			if(outBuff == NULL) {
				std::cerr << "ffmpeg-convert: Could not allocate output buffer.\n";
				return false;
			}
			
			outFmt->pb = avio_alloc_context(outBuff, BufferSize, 1, (void*)outFid, NULL, outStreamWrite, NULL);
			if(outFmt->pb == NULL) {
				std::cerr << "ffmpeg-convert: Could not allocate output IO context.\n";
				return false;
			}
		}
		
		if(!ConvertFFmpeg(inFmt, outFmt, -1, codecName)) {
			return false;
		}
		
		avformat_close_input(&inFmt);
		
		if(outBuff) av_free(outBuff);
		if(outFmt->pb) av_free(outFmt->pb);
		av_free(outFmt);
		
		
		return true;
	}
	
	bool ConvertFFmpegStreamFile(FFmpegURLRead inStreamRead, int inFid, const char* inFormatName, const char* outFile, const char* outFormatName, const char* codecName) {
		AVInputFormat* inFmtStruct = av_find_input_format(inFormatName);
		if(!inFmtStruct) {
			std::cerr << "ffmpeg-convert: Could not find input format.\n";
			return false;
		}
		
		AVFormatContext* inFmt = avformat_alloc_context();
		if(inFmt == NULL) {
			std::cerr << "ffmpeg-convert: Could not allocate input context.\n";
			return false;
		}
		
		unsigned char* inBuff = (unsigned char*)av_malloc(BufferSize);
		if(inBuff == NULL) {
			std::cerr << "ffmpeg-convert: Could not allocate input buffer.\n";
			return false;
		}
		
		inFmt->pb = avio_alloc_context(inBuff, BufferSize, 0, (void*)inFid, inStreamRead, NULL, NULL);
		if(inFmt->pb == NULL) {
			std::cerr << "ffmpeg-convert: Could not allocate input IO context.\n";
			return false;
		}
		
		if(avformat_open_input(&inFmt, "", inFmtStruct, NULL) != 0) {
			std::cerr << "ffmpeg-convert: Could not open input.\n";
			return false;
		}
		
		AVFormatContext* outFmt = NULL;
		int err = avformat_alloc_output_context2(&outFmt, NULL, outFormatName, NULL);
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
		
		av_free(inBuff);
		av_free(inFmt->pb);
		avformat_close_input(&inFmt);
		
		av_free(outFmt);
		
		
		return true;
	}
	
	bool ConvertFFmpegStreamStream(FFmpegURLRead inStreamRead, int inFid, const char* inFormatName, FFmpegURLWrite outStreamWrite, int outFid, const char* outFormatName, const char* codecName) {
		AVInputFormat* inFmtStruct = av_find_input_format(inFormatName);
		if(!inFmtStruct) {
			std::cerr << "ffmpeg-convert: Could not find input format.\n";
			return false;
		}
		
		AVFormatContext* inFmt = avformat_alloc_context();
		if(inFmt == NULL) {
			std::cerr << "ffmpeg-convert: Could not allocate input context.\n";
			return false;
		}
		
		unsigned char* inBuff = (unsigned char*)av_malloc(BufferSize);
		if(inBuff == NULL) {
			std::cerr << "ffmpeg-convert: Could not allocate input buffer.\n";
			return false;
		}
		
		inFmt->pb = avio_alloc_context(inBuff, BufferSize, 0, (void*)inFid, inStreamRead, NULL, NULL);
		if(inFmt->pb == NULL) {
			std::cerr << "ffmpeg-convert: Could not allocate input IO context.\n";
			return false;
		}
		
		if(avformat_open_input(&inFmt, "", inFmtStruct, NULL) != 0) {
			std::cerr << "ffmpeg-convert: Could not open input.\n";
			return false;
		}
		
		AVFormatContext* outFmt = NULL;
		int err = avformat_alloc_output_context2(&outFmt, NULL, outFormatName, NULL);
		if(outFmt == NULL || err < 0) {
			std::cerr << "ffmpeg-convert: Could not open output. Error: " << err << "\n";
			avformat_close_input(&inFmt);
			return false;
		}
		
		unsigned char* outBuff = NULL;
		outFmt->pb = NULL;
		if(!(outFmt->oformat->flags & AVFMT_NOFILE)) {
			outBuff = (unsigned char*)av_malloc(BufferSize);
			if(outBuff == NULL) {
				std::cerr << "ffmpeg-convert: Could not allocate output buffer.\n";
				return false;
			}		
			
			outFmt->pb = avio_alloc_context(outBuff, BufferSize, 1, (void*)outFid, NULL, outStreamWrite, NULL);
			if(outFmt->pb == NULL) {
				std::cerr << "ffmpeg-convert: Could not allocate output IO context.\n";
				return false;
			}
		}
		
		if(!ConvertFFmpeg(inFmt, outFmt, -1, codecName)) {
			return false;
		}
		
		av_free(inBuff);
		av_free(inFmt->pb);
		avformat_close_input(&inFmt);
		
		if(outBuff) av_free(outBuff);
		if(outFmt->pb) av_free(outFmt->pb);
		av_free(outFmt);
		
		
		return true;
	}
	
	bool FFmpegDemuxFileFile(const char* inFormatName, const char* inFile, const char* outFile, int streamIndex) {
		AVInputFormat* inFmtStruct = av_find_input_format(inFormatName);
		if(!inFmtStruct) {
			std::cerr << "ffmpeg-convert: Could not find input format " << inFormatName << ".\n";
			return false;
		}
		
		AVFormatContext* inFmt = NULL;
		if(avformat_open_input(&inFmt, inFile, inFmtStruct, NULL) != 0) {
			std::cerr << "ffmpeg-convert: Could not open input.\n";
			return false;
		}
		
		
		// Prepare inFmt
		if(avformat_find_stream_info(inFmt, NULL) < 0) {
			std::cerr << "ffmpeg-convert: Could not find stream info.\n";
			return false;
		}
		
		av_dump_format(inFmt, -1, "", 0);
		
		// Prepare inCodec
		int realStreamIndex = av_find_best_stream(inFmt, AVMEDIA_TYPE_VIDEO, streamIndex, -1, NULL, 0);
		
		if(realStreamIndex < 0) {
			std::cerr << "ffmpeg-convert: Could not find input stream.\n";
			return false;
		}
		
		FILE* fp = fopen(outFile, "wb");
		if(fp == NULL) {
			std::cerr << "ffmpeg-convert: Could not open output file.\n";
			return false;
		}
		
		AVPacket inPacket;
		av_init_packet(&inPacket);
		
		while(av_read_frame(inFmt, &inPacket) == 0) {
			std::cerr << "index = " << inPacket.stream_index << ", " << streamIndex << "\n";
			if(inPacket.stream_index == streamIndex) {
				fwrite(inPacket.data, 1, inPacket.size, fp);
			}
			
			av_free_packet(&inPacket);
		}
		
		fclose(fp);
		
		
		avformat_close_input(&inFmt);
		
		return true;
	}
	
	int FFmpegGetEOF() {
		return AVERROR_EOF;
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
	
	if(!ConvertFFmpegAudio(inFmt, inCodecCtx, inCodec, outFmt, outStream, outCodec, realStreamIndex)) {
		return false;
	}
	
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
	
	if(avformat_write_header(outFmt, NULL) != 0) {
		std::cerr << "ffmpeg-convert: Could not write header.\n";
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
			
			av_free_packet(&inPacket);
			
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
	
	av_write_trailer(outFmt);
	return true;
}

int main() {
	InitFFmpeg();
	FFmpegDemuxFileFile("matroska,webm", "/home/john/Videos/Main_Movie_t01.mkv", "test.vc1", 0);
	
	return 0;
}


