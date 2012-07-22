extern "C" {
#include <libavformat/avformat.h>
#include <libavcodec/avcodec.h>
#include <libavutil/avutil.h>
}
#include <iostream>

#include "main.h"


extern "C" {
	bool ConvertFFmpegFile(const char* inFile, const char* inFormat, const char* outFile, const char* outFormat, const char* codec) {
		printf("inFile: %s\ninFormat: %s\noutFile: %s\noutFormat: %s\ncodec: %s\n", inFile, inFormat, outFile, outFormat, codec);
		
		AVFormatContext* pFormatCtx = NULL;
		AVCodec* pCodec = NULL;
		
		av_register_all();
		
		AVInputFormat* inFormatStruct = av_find_input_format(inFormat);
		if(!inFormatStruct) {
			std::cerr << "ffmpeg-convert: Could not find input format.\n";
			return false;
		}
		
		if(avformat_open_input(&pFormatCtx, inFile, inFormatStruct, NULL) != 0) {
			std::cerr << "ffmpeg-convert: Could not open input.\n";
			return false;
		}
		
		if(avformat_find_stream_info(pFormatCtx, NULL) < 0) {
			std::cerr << "ffmpeg-convert: Could not find stream info.\n";
			return false;
		}
		
		av_dump_format(pFormatCtx, 0, inFile, 0);
		
		AVCodec* inputCodec = NULL;
		int audioStreamIdx = av_find_best_stream(pFormatCtx, AVMEDIA_TYPE_AUDIO, -1, -1, &inputCodec, 0);
		/*for(int i = 0; i < pFormatCtx->nb_streams; ++i) {
			if(pFormatCtx->streams[i]->codec->codec_type == AVMEDIA_TYPE_AUDIO) {
				audioStreamIdx = i;
				break;
			}
		}*/
		
		
		if(audioStreamIdx < 0) {
			std::cerr << "ffmpeg-convert: Could not find input stream.\n";
			return false;
		}
		
		/*if(audioStreamIdx == -1) {
			std::cerr << "ffmpeg-convert: Could not find audio stream.\n";
			avformat_close_input(&pFormatCtx);
			return false;
		}*/
		
		pCodec = avcodec_find_encoder_by_name(codec);
		if(pCodec == NULL) {
			std::cerr << "ffmpeg-convert: Could not find codec.\n";
			avformat_close_input(&pFormatCtx);
			return false;
		}
		
		AVFormatContext* outputFormatContext;
		int err = avformat_alloc_output_context2(&outputFormatContext, NULL, outFormat, outFile);
		if(outputFormatContext == NULL || err < 0) {
			std::cerr << "ffmpeg-convert: Could not open output. Error: " << err << "\n";
			avformat_close_input(&pFormatCtx);
			return false;
		}
		
		AVCodecContext* pCodecCtx = pFormatCtx->streams[audioStreamIdx]->codec;
		
		AVStream* audioStream = avformat_new_stream(outputFormatContext, pCodec);
		if(!audioStream) {
			std::cerr << "ffmpeg-convert: Could not create audio stream.\n";
			return false;
		}
		audioStream->id = 1;
		audioStream->codec->sample_fmt = pCodecCtx->sample_fmt;
		audioStream->codec->sample_rate = pCodecCtx->sample_rate;
		audioStream->codec->channels = pCodecCtx->channels;
		audioStream->codec->channel_layout = pCodecCtx->channel_layout;
		
		if(outputFormatContext->oformat->flags & AVFMT_GLOBALHEADER) {
			audioStream->codec->flags |= CODEC_FLAG_GLOBAL_HEADER;
		}
		
		if(avcodec_open2(pCodecCtx, inputCodec, NULL) < 0) {
			std::cerr << "ffmpeg-convert: Could not open input audio codec.\n";
			return false;
		}
		
		if(avcodec_open2(audioStream->codec, pCodec, NULL) < 0) {
			std::cerr << "ffmpeg-convert: Could not open output audio codec.\n";
			return false;
		}
		
		/* open the output file, if needed */
		if (!(outputFormatContext->oformat->flags & AVFMT_NOFILE)) {
			if (avio_open(&outputFormatContext->pb, outFile, AVIO_FLAG_WRITE) < 0) {
				std::cerr << "ffmpeg-convert: Could not open output file\n";
				return false;
			}
		}
		
		if(avformat_write_header(outputFormatContext, NULL) != 0) {
			std::cerr << "ffmpeg-convert: Could not write header.\n";
			return false;
		}
		
		AVPacket packet;
		av_init_packet(&packet);
		
		AVFrame* frame = NULL;
		
		while(av_read_frame(pFormatCtx, &packet) == 0) {
			std::cerr << "index = " << packet.stream_index << ", " << audioStreamIdx << "\n";
			if(packet.stream_index == audioStreamIdx) {
				int len = 0;
				int got_frame_ptr = 0;
				while(!got_frame_ptr) {
					
					if(frame == NULL) {
						if((frame = avcodec_alloc_frame()) == NULL) {
							std::cerr << "ffmpeg-convert: Error allocating frame.\n";
						}
					}
					else {
						avcodec_get_frame_defaults(frame);
					}
					
					len = avcodec_decode_audio4(pCodecCtx, frame, &got_frame_ptr, &packet);
					
					if(len < 0) {
						std::cerr << "ffmpeg-convert: Error while decoding.\n";
						avformat_close_input(&pFormatCtx);
						return false;
					}
				}
				
				
				AVPacket outPacket = { 0 };
				av_init_packet(&outPacket);
				int got_packet_ptr;
				if(avcodec_encode_audio2(audioStream->codec, &outPacket, frame, &got_packet_ptr) != 0) {
					std::cerr << "ffmpeg-convert: Error while encoding.\n";
					avformat_close_input(&pFormatCtx);
					return false;
				}
				
				if(av_interleaved_write_frame(outputFormatContext, &outPacket) != 0) {
					std::cerr << "ffmpeg-convert: Error while writing frame.\n";
					return false;
				}
				
				
			}
		}
		
		av_write_trailer(outputFormatContext);
		
		avcodec_close(audioStream->codec);
		
		for(int i = 0; i < outputFormatContext->nb_streams; ++i) {
			av_freep(&outputFormatContext->streams[i]->codec);
			av_freep(&outputFormatContext->streams[i]);
		}
		
		av_free(outputFormatContext);
		av_free(frame);
		
		avformat_close_input(&pFormatCtx);
		
		return true;
	}
}

/*
int main() {
	ConvertFFmpegFile("/home/john/Projects/audio.thd", "truehd", "test.wav", "wav", "pcm_s24le");
}
*/