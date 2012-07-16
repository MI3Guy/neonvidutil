#ifndef __MAIN_H__
#define __MAIN_H__

#ifdef __cplusplus
extern "C" {
#endif

// Add function prototypes here
bool ConvertFFmpegFile(char* inFile, char* inFormat, char* outFile, char* outFormat, char* codec);
#ifdef __cplusplus
}
#endif

#endif

