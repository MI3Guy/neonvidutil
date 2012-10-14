extern "C" {
#include <libavformat/avformat.h>
#include <libavcodec/avcodec.h>
#include <libavutil/avutil.h>
}
#include <iostream>
#include <string>

#include "main.h"

const int BufferSize = 4*1024;

// Helper Functions
bool ConvertFFmpeg(AVFormatContext* inFmt, AVFormatContext* outFmt, int streamIndex, std::string codecName, Action callback);
bool ConvertFFmpegAudio(AVFormatContext* inFmt, AVCodecContext* inCodecCtx, AVCodec* inCodec, AVFormatContext* outFmt, AVStream* stream, AVCodec* outCodec, int streamIndex, Action callback);
bool ConvertFFmpegCopy(AVFormatContext* inFmt, AVCodecContext* inCodecCtx, AVFormatContext* outFmt, AVStream* outStream, int streamIndex, AVMediaType mediaType, Action callback);

extern "C" {
	void InitFFmpeg() {
		av_register_all();
	}
	
	bool ConvertFFmpegFileFile(const char* inFile, const char* inFormatName, const char* outFile, const char* outFormatName, const char* codecName, int streamIndex, Action callback) {
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
		
		if(!ConvertFFmpeg(inFmt, outFmt, streamIndex, codecName, callback)) {
			return false;
		}
		
		avformat_close_input(&inFmt);
		av_free(outFmt);
		
		return true;
	}
	
	bool ConvertFFmpegFileStream(const char* inFile, const char* inFormatName, FFmpegURLWrite outStreamWrite, const char* outFormatName, const char* codecName, int streamIndex, Action callback) {
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
			
			outFmt->pb = avio_alloc_context(outBuff, BufferSize, 1, NULL, NULL, outStreamWrite, NULL);
			if(outFmt->pb == NULL) {
				std::cerr << "ffmpeg-convert: Could not allocate output IO context.\n";
				return false;
			}
		}
		
		if(!ConvertFFmpeg(inFmt, outFmt, streamIndex, codecName, callback)) {
			return false;
		}
		
		avformat_close_input(&inFmt);
		
		if(outBuff) av_free(outBuff);
		if(outFmt->pb) av_free(outFmt->pb);
		av_free(outFmt);
		
		
		return true;
	}
	
	bool ConvertFFmpegStreamFile(FFmpegURLRead inStreamRead, const char* inFormatName, const char* outFile, const char* outFormatName, const char* codecName, int streamIndex, Action callback) {
		AVInputFormat* inFmtStruct = av_find_input_format(inFormatName);
		if(!inFmtStruct) {
			std::cerr << "ffmpeg-convert: Could not find input format " << inFormatName << ".\n";
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
		
		inFmt->pb = avio_alloc_context(inBuff, BufferSize, 0, NULL, inStreamRead, NULL, NULL);
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
		
		if(!ConvertFFmpeg(inFmt, outFmt, streamIndex, codecName, callback)) {
			return false;
		}
		
		av_free(inBuff);
		av_free(inFmt->pb);
		avformat_close_input(&inFmt);
		
		av_free(outFmt);
		
		
		return true;
	}
	
	bool ConvertFFmpegStreamStream(FFmpegURLRead inStreamRead, const char* inFormatName, FFmpegURLWrite outStreamWrite, const char* outFormatName, const char* codecName, int streamIndex, Action callback) {
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
		
		inFmt->pb = avio_alloc_context(inBuff, BufferSize, 0, NULL, inStreamRead, NULL, NULL);
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
			
			outFmt->pb = avio_alloc_context(outBuff, BufferSize, 1, NULL, NULL, outStreamWrite, NULL);
			if(outFmt->pb == NULL) {
				std::cerr << "ffmpeg-convert: Could not allocate output IO context.\n";
				return false;
			}
		}
		
		if(!ConvertFFmpeg(inFmt, outFmt, streamIndex, codecName, callback)) {
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
	
	bool FFmpegDemuxFileFile(const char* inFile, const char* inFormatName, const char* outFile, int streamIndex, Action callback) {
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
			realStreamIndex = av_find_best_stream(inFmt, AVMEDIA_TYPE_AUDIO, streamIndex, -1, NULL, 0);
			if(realStreamIndex < 0) {
				std::cerr << "ffmpeg-convert: Could not find input stream.\n";
				return false;
			}
		}
		
		
		
		FILE* fp = fopen(outFile, "wb");
		if(fp == NULL) {
			std::cerr << "ffmpeg-convert: Could not open output file.\n";
			return false;
		}
		
		AVPacket inPacket;
		av_init_packet(&inPacket);
		
		while(av_read_frame(inFmt, &inPacket) == 0) {
			std::cerr << "index = " << inPacket.stream_index << ", " << realStreamIndex << "\n";
			if(inPacket.stream_index == realStreamIndex) {
				fwrite(inPacket.data, 1, inPacket.size, fp);
			}
			
			av_free_packet(&inPacket);
		}
		
		fclose(fp);
		
		
		avformat_close_input(&inFmt);
		
		return true;
	}
	
	bool FFmpegDemuxStreamStream(FFmpegURLRead inStreamRead, const char* inFormatName, FFmpegURLWrite outStreamWrite, int streamIndex, Action callback) {
		AVInputFormat* inFmtStruct = av_find_input_format(inFormatName);
		if(!inFmtStruct) {
			std::cerr << "ffmpeg-convert: Could not find input format " << inFormatName << ".\n";
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
		
		inFmt->pb = avio_alloc_context(inBuff, BufferSize, 0, NULL, inStreamRead, NULL, NULL);
		if(inFmt->pb == NULL) {
			std::cerr << "ffmpeg-convert: Could not allocate input IO context.\n";
			return false;
		}
		
		if(avformat_open_input(&inFmt, "", inFmtStruct, NULL) != 0) {
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
			realStreamIndex = av_find_best_stream(inFmt, AVMEDIA_TYPE_AUDIO, streamIndex, -1, NULL, 0);
			if(realStreamIndex < 0) {
				std::cerr << "ffmpeg-convert: Could not find input stream.\n";
				return false;
			}
		}
		
		AVPacket inPacket;
		av_init_packet(&inPacket);
		
		while(av_read_frame(inFmt, &inPacket) == 0) {
			if(inPacket.stream_index == realStreamIndex) {
				outStreamWrite(NULL, inPacket.data, inPacket.size);
			}
			
			av_free_packet(&inPacket);
		}
		
		av_free(inBuff);
		av_free(inFmt->pb);
		avformat_close_input(&inFmt);
		
		return true;
	}
	
	int FFmpegGetEOF() {
		return AVERROR_EOF;
	}
}

// Helper functions
bool ConvertFFmpeg(AVFormatContext* inFmt, AVFormatContext* outFmt, int streamIndex, std::string codecName, Action callback) {
	CodecID codecId = CODEC_ID_NONE;
	
	// Prepare inFmt
	if(avformat_find_stream_info(inFmt, NULL) < 0) {
		std::cerr << "ffmpeg-convert: Could not find stream info.\n";
		return false;
	}
	
	// Prepare inCodec
	AVMediaType mediaType = AVMEDIA_TYPE_AUDIO;
	AVCodec* inCodec = NULL;
	int realStreamIndex = av_find_best_stream(inFmt, AVMEDIA_TYPE_AUDIO, streamIndex, -1, &inCodec, 0);
	if(realStreamIndex < 0) {
		realStreamIndex = av_find_best_stream(inFmt, AVMEDIA_TYPE_VIDEO, streamIndex, -1, &inCodec, 0);
		mediaType = AVMEDIA_TYPE_VIDEO;
		if(realStreamIndex < 0) {
			std::cerr << "ffmpeg-convert: Could not find input stream.\n";
			return false;
		}
	}
	
	AVCodecContext* inCodecCtx = inFmt->streams[realStreamIndex]->codec;
	AVCodec* outCodec = NULL;
	if(codecName == "copy" && (inCodecCtx->codec_id == CODEC_ID_PCM_S16LE || inCodecCtx->codec_id == CODEC_ID_PCM_S24LE)) {
		codecName = "";
		codecId = inCodecCtx->codec_id;
	}
	
	if(codecName != "copy") {
		outCodec = codecName.empty() ? avcodec_find_encoder(codecId) : avcodec_find_encoder_by_name(codecName.c_str());
		if(outCodec == NULL) {
			std::cerr << "ffmpeg-convert: Could not find codec.\n";
			avformat_close_input(&inFmt);
			return false;
		}
	}
	else {
		outCodec = inCodecCtx->codec;
	}
	
	
	AVStream* outStream = avformat_new_stream(outFmt, outCodec);
	if(!outStream) {
		std::cerr << "ffmpeg-convert: Could not create stream.\n";
		return false;
	}
	
	if(outFmt->oformat->flags & AVFMT_GLOBALHEADER) {
		outStream->codec->flags |= CODEC_FLAG_GLOBAL_HEADER;
	}
	
	if(codecName != "copy") {
		if(mediaType == AVMEDIA_TYPE_AUDIO) {
			if(!ConvertFFmpegAudio(inFmt, inCodecCtx, inCodec, outFmt, outStream, outCodec, realStreamIndex, callback)) {
				return false;
			}
		}
		else {
			std::cerr << "ffmpeg-convert: Could not convert unknown media type.";
			return false;
		}
	}
	else {
		ConvertFFmpegCopy(inFmt, inCodecCtx, outFmt, outStream, realStreamIndex, mediaType, callback);
	}
	
	avcodec_close(inCodecCtx);
	
	for(int i = 0; i < outFmt->nb_streams; ++i) {
		av_freep(&outFmt->streams[i]->codec);
		av_freep(&outFmt->streams[i]);
	}
	
	return true;
}

bool ConvertFFmpegAudio(AVFormatContext* inFmt, AVCodecContext* inCodecCtx, AVCodec* inCodec, AVFormatContext* outFmt, AVStream* audioStream, AVCodec* outCodec, int streamIndex, Action callback) {

	if(avcodec_open2(inCodecCtx, inCodec, NULL) < 0) {
		std::cerr << "ffmpeg-convert: Could not open input codec.\n";
		return false;
	}
	
	audioStream->id = 1;
	audioStream->codec->sample_fmt = inCodecCtx->sample_fmt;
	audioStream->codec->sample_rate = inCodecCtx->sample_rate;
	audioStream->codec->channels = inCodecCtx->channels;
	audioStream->codec->channel_layout = inCodecCtx->channel_layout;
	
	if(avcodec_open2(audioStream->codec, outCodec, NULL) < 0) {
		std::cerr << "ffmpeg-convert: Could not open output codec.\n";
		return false;
	}
	
	if(avformat_write_header(outFmt, NULL) != 0) {
		std::cerr << "ffmpeg-convert: Could not write header.\n";
		return false;
	}
	
	AVPacket inPacket;
	AVPacket outPacket = { 0 };
	av_init_packet(&inPacket);
	av_init_packet(&outPacket);
	
	AVFrame* frame = avcodec_alloc_frame();
	if(frame == NULL) {
		std::cerr << "ffmpeg-convert: Error allocating frame.\n";
		return false;
	}
	
	while(av_read_frame(inFmt, &inPacket) == 0) {
		if(inPacket.stream_index == streamIndex) {
			int got_frame_ptr = 0;
			while(got_frame_ptr == 0) {
				avcodec_get_frame_defaults(frame);
				
				if(avcodec_decode_audio4(inCodecCtx, frame, &got_frame_ptr, &inPacket) < 0) {
					std::cerr << "ffmpeg-convert: Error while decoding.\n";
					avformat_close_input(&inFmt);
					return false;
				}
			}
			
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
			
			//std::cerr << "outPacket->destruct: " << outPacket.destruct << "\n"; 
			av_free_packet(&outPacket);
		}
		av_free_packet(&inPacket);
		
		callback();
	}
	
	av_free(frame);
	av_write_trailer(outFmt);
	return true;
}

bool ConvertFFmpegCopy(AVFormatContext* inFmt, AVCodecContext* inCodecCtx, AVFormatContext* outFmt, AVStream* outStream, int streamIndex, AVMediaType mediaType, Action callback) {
	if(mediaType == AVMEDIA_TYPE_AUDIO) {
		outStream->id = 1;
		outStream->codec->codec = NULL;
		outStream->codec->codec_id = inCodecCtx->codec_id;
		outStream->codec->sample_fmt = inCodecCtx->sample_fmt;
		outStream->codec->sample_rate = inCodecCtx->sample_rate;
		outStream->codec->channels = inCodecCtx->channels;
		outStream->codec->channel_layout = inCodecCtx->channel_layout;
	}
	
	if(avformat_write_header(outFmt, NULL) != 0) {
		std::cerr << "ffmpeg-convert: Could not write header.\n";
		return false;
	}
	
	AVPacket inPacket;
	av_init_packet(&inPacket);
	
	while(av_read_frame(inFmt, &inPacket) == 0) {
		std::cerr << "index = " << inPacket.stream_index << ", " << streamIndex << "\n";
		if(inPacket.stream_index == streamIndex) {
			if(av_interleaved_write_frame(outFmt, &inPacket) != 0) {
				std::cerr << "ffmpeg-convert: Error while writing frame.\n";
				return false;
			}
		}
		av_free_packet(&inPacket);
		
		callback();
	}
	
	av_write_trailer(outFmt);
	return true;
}

/*
int main() {
	InitFFmpeg();
	//ConvertFFmpegFileFile("/home/john/Videos/vid2.mkv", "matroska", "test.wav", "wav", "pcm_s32le", 1);
	ConvertFFmpegFileFile("/media/EXTRADATA4/Videos/HELLBOY/title01.mkv", "matroska", "test.wav", "wav", "copy", 1);
}
*/