
unixFiles = DGPulldown/bin/Release/libDGPulldown.so ffmpeg-convert/bin/Release/libffmpeg-convert.so vc1conv/bin/Release/libvc1conv.so
	

DGPulldown/bin/Release/libDGPulldown.so: DGPulldown/DGPulldown.cpp
	mkdir -p DGPulldown/bin/Release/
	g++ -fpic -shared $^ -o $@

DGPulldown/DGPulldown.cpp: DGPulldown/DGPulldown.h



ffmpeg-convert/bin/Release/libffmpeg-convert.so: ffmpeg-convert/main.cpp
	mkdir -p ffmpeg-convert/bin/Release/
	g++ -fpic -shared -D__STDC_CONSTANT_MACROS -o $@ $^ -lavcodec -lavformat -lavutil

ffmpeg-convert/main.cpp: ffmpeg-convert/main.h



vc1conv/bin/Release/libvc1conv.so: vc1conv/vc1conv.c
	mkdir -p vc1conv/bin/Release/
	gcc -fpic -shared $^ -o $@

