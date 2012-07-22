#ifndef __MAIN_H__
#define __MAIN_H__

#ifdef __cplusplus
extern "C" {
#endif

// Add function prototypes here
bool ConvertFFmpegFile(const char* inFile, const char* inFormat, const char* outFile, const char* outFormat, const char* codec);
#ifdef __cplusplus
}
#endif

#endif

