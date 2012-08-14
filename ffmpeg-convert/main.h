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

typedef int (*FFmpegURLRead)(void* h, unsigned char* buf, int size);
typedef int (*FFmpegURLWrite)(void* h, unsigned char* buf, int size);


#ifdef __cplusplus
extern "C" {
#endif
	void InitFFmpeg();
	bool ConvertFFmpegFileFile(const char* inFile, const char* inFormatName,
	                           const char* outFile, const char* outFormatName,
	                           const char* codecName, int streamIndex);
	bool ConvertFFmpegFileStream(const char* inFile, const char* inFormatName,
	                             FFmpegURLWrite outStreamWrite, const char* outFormatName,
	                             const char* codecName, int streamIndex);
	bool ConvertFFmpegStreamFile(FFmpegURLRead inStreamRead, const char* inFormatName,
	                             const char* outFile, const char* outFormatName,
	                             const char* codecName, int streamIndex);
	bool ConvertFFmpegStreamStream(FFmpegURLRead inStream, const char* inFormatName,
	                               FFmpegURLWrite outStream, const char* outFormatName,
	                               const char* codecName, int streamIndex);
	bool FFmpegDemuxFileFile(const char* inFile, const char* inFormatName,
	                         const char* outFile,
	                         int streamIndex);
	bool FFmpegDemuxStreamStream(FFmpegURLRead inStreamRead, const char* inFormatName,
	                             FFmpegURLWrite outStreamWrite,
	                             int streamIndex);
	int FFmpegGetEOF();
#ifdef __cplusplus
}
#endif

#endif

