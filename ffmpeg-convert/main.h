#ifndef __MAIN_H__
#define __MAIN_H__

extern "C" {
#include <libavformat/avformat.h>
#include <libavcodec/avcodec.h>
#include <libavutil/avutil.h>
}

#ifdef WINDOWS
#ifdef FFMPEG_CONVERT
#define LIBFUNC __declspec(dllexport)
#else
#define LIBFUNC __declspec(dllimport)
#endif
#else
#define LIBFUNC
#endif

typedef int (*FFmpegURLRead)(URLContext* h, unsigned char* buf, int size);
typedef int (*FFmpegURLWrite)(URLContext* h, const unsigned char* buf, int size);

#ifdef __cplusplus
extern "C" {
#endif
	bool ConvertFFmpegFile(const char* inFile, const char* inFormatName,
							const char* outFile, const char* outFormatName,
							const char* codecName);
	bool ConvertFFmpegStream(FFmpegURLRead inStreamRead, const char* inFormatName,
							FFmpegURLWrite outStreamWrite, const char* outFormatName,
							const char* codecName);
#ifdef __cplusplus
}
#endif

#endif

