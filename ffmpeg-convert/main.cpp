extern "C" {
#include <libavformat/avformat.h>
#include <libavcodec/avcodec.h>
}
#include <cstdio>

#include "main.h"


extern "C" {
	bool ConvertFFmpegFile(char* inFile, char* inFormat, char* outFile, char* outFormat, char* codec) {
		printf("inFile: %s\ninFormat: %s\noutFile: %s\noutFormat: %s\ncodec: %s\n", inFile, inFormat, outFile, outFormat, codec);
		
		AVFormatContext* pFormatCtx = NULL;
		AVCodecContext* pCodecCtx = NULL;
		AVCodec* pCodec = NULL;
		
		av_register_all();
		
		AVInputFormat* inFormatStruct = NULL;
		if(!av_find_input_format(inFormat)) {
			return false;
		}
		
		if(avformat_open_input(&pFormatCtx, inFile, inFormatStruct, NULL) != 0) {
			return false;
		}
		
		if(av_find_stream_info(pFormatCtx) < 0) {
			return -1;
		}
		
		//dump_format(pFormatCtx, 0, inFile, 0);
		
		int audioStream = -1;
		for(int i = 0; i < pFormatCtx->nb_streams; ++i) {
			if(pFormatCtx->streams[i]->codec->codec_type == CODEC_TYPE_AUDIO) {
				audioStream = i;
				break;
			}
		}
		
		if(audioStream == -1) {
			return false;
		}
		
		
		
		av_close_input_file(pFormatCtx);
		
		return true;
	}
}