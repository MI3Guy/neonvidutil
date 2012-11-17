
all: archive/NeonVidUtil-dotNET.tar.gz build/ archive/NeonVidUtil-win32.7z
# archive/NeonVidUtil-win32.7z

archive/NeonVidUtil-dotNET.tar.gz: archive/NeonVidUtil-dotNET/
	cd archive/ && tar -czf NeonVidUtil-dotNET.tar.gz NeonVidUtil-dotNET/

archive/NeonVidUtil-win32.7z: archive/NeonVidUtil-win32/
	cd archive/ && 7zr a NeonVidUtil-win32.7z NeonVidUtil-win32/

archive/:
	mkdir $@
	
clean:
	rm -rf $(dotNetArchiveFiles) build/ $(unixFiles) $(windowsFiles)

# Rules for .NET code

NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll: NeonVidUtilCore/*.cs
	xbuild NeonVidUtilCore/NeonVidUtilCore.csproj /p:Configuration=Release /verbosity:quiet

NeonVidUtil/bin/Release/NeonVidUtil.exe: libraries/NDesk.Options.dll NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll NeonVidUtil/*.cs
	xbuild NeonVidUtil/NeonVidUtil.csproj /p:Configuration=Release /verbosity:quiet

DGPulldownFormatHandler/bin/Release/DGPulldownFormatHandler.dll: NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll DGPulldownFormatHandler/*.cs
	xbuild DGPulldownFormatHandler/DGPulldownFormatHandler.csproj /p:Configuration=Release /verbosity:quiet

FFmpegFormatHandler/bin/Release/FFmpegFormatHandler.dll: NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll FFmpegFormatHandler/*.cs
	xbuild FFmpegFormatHandler/FFmpegFormatHandler.csproj /p:Configuration=Release /verbosity:quiet

FLACFormatHandler/bin/Release/FLACFormatHandler.dll: FLACSharp/bin/Release/FLACSharp.dll NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll FLACFormatHandler/*.cs
	xbuild FLACFormatHandler/FLACFormatHandler.csproj /p:Configuration=Release /verbosity:quiet
FLACSharp/bin/Release/FLACSharp.dll: NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll FLACSharp/*.cs
	xbuild FLACSharp/FLACSharp.csproj /p:Configuration=Release /verbosity:quiet


MediaInfoFormatHandler/bin/Release/MediaInfoFormatHandler.dll: NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll MediaInfoFormatHandler/*.cs
	xbuild MediaInfoFormatHandler/MediaInfoFormatHandler.csproj /p:Configuration=Release /verbosity:quiet

VC1FormatHandler/bin/Release/VC1FormatHandler.dll: NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll VC1FormatHandler/*.cs
	xbuild VC1FormatHandler/VC1FormatHandler.csproj /p:Configuration=Release /verbosity:quiet

WAVFormatHandler/bin/Release/WAVFormatHandler.dll: WAVSharp/bin/Release/WAVSharp.dll NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll WAVFormatHandler/*.cs
	xbuild WAVFormatHandler/WAVFormatHandler.csproj /p:Configuration=Release /verbosity:quiet
WAVSharp/bin/Release/WAVSharp.dll: WAVSharp/*.cs
	xbuild WAVSharp/WAVSharp.csproj /p:Configuration=Release /verbosity:quiet

WavPackFormatHandler/bin/Release/WavPackFormatHandler.dll: NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll WavPackFormatHandler/*.cs
	xbuild WavPackFormatHandler/WavPackFormatHandler.csproj /p:Configuration=Release /verbosity:quiet
WavPackSharp/bin/Release/WavPackSharp.dll: WavPackSharp/*.cs
	xbuild WavPackSharp/WavPackSharp.csproj /p:Configuration=Release /verbosity:quiet

dotNetArchiveFiles = archive/ NeonVidUtil/bin/Release/NeonVidUtil.exe NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll \
	DGPulldownFormatHandler/bin/Release/DGPulldownFormatHandler.dll FFmpegFormatHandler/bin/Release/FFmpegFormatHandler.dll \
	FLACFormatHandler/bin/Release/FLACFormatHandler.dll FLACSharp/bin/Release/FLACSharp.dll \
	MediaInfoFormatHandler/bin/Release/MediaInfoFormatHandler.dll VC1FormatHandler/bin/Release/VC1FormatHandler.dll \
	WAVFormatHandler/bin/Release/WAVFormatHandler.dll WAVSharp/bin/Release/WAVSharp.dll \
	WavPackFormatHandler/bin/Release/WavPackFormatHandler.dll WavPackSharp/bin/Release/WavPackSharp.dll

archive/NeonVidUtil-dotNET/: $(dotNetArchiveFiles)
	mkdir -p $@
	cp NeonVidUtil/bin/Release/NeonVidUtil.exe $@/
	cp NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll $@/
	cp libraries/NDesk.Options.dll $@/
	mkdir -p $@/Plugins
	cp DGPulldownFormatHandler/bin/Release/DGPulldownFormatHandler.dll $@/Plugins/
	cp FFmpegFormatHandler/bin/Release/FFmpegFormatHandler.dll $@/Plugins/
	cp FLACFormatHandler/bin/Release/FLACFormatHandler.dll $@/Plugins/
	cp FLACSharp/bin/Release/FLACSharp.dll $@/Plugins/
	cp MediaInfoFormatHandler/bin/Release/MediaInfoFormatHandler.dll $@/Plugins/
	cp MediaInfoFormatHandler/MediaInfoFormatHandler.dll.config $@/Plugins/
	cp VC1FormatHandler/bin/Release/VC1FormatHandler.dll $@/Plugins/
	cp WAVFormatHandler/bin/Release/WAVFormatHandler.dll $@/Plugins/
	cp WAVSharp/bin/Release/WAVSharp.dll $@/Plugins/
	cp WavPackFormatHandler/bin/Release/WavPackFormatHandler.dll $@/Plugins/
	cp WavPackSharp/bin/Release/WavPackSharp.dll $@/Plugins/
	mkdir -p $@/src
	mkdir -p $@/src/DGPulldown
	cp DGPulldown/*.cpp DGPulldown/*.h $@/src/DGPulldown/
	mkdir -p $@/src
	mkdir -p $@/src/ffmpeg-convert
	cp ffmpeg-convert/*.cpp ffmpeg-convert/*.h $@/src/ffmpeg-convert/
	mkdir -p $@/src
	mkdir -p $@/src/vc1conv
	cp vc1conv/*.c $@/src/vc1conv/
	cp unix.mk $@/src/makefile
	cp unixcommon.mk $@/src/
	
# Rules for Unix build

include unixcommon.mk

build/: $(unixFiles) $(dotNetArchiveFiles)
	mkdir -p $@
	cp NeonVidUtil/bin/Release/NeonVidUtil.exe $@/
	cp NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll $@/
	cp libraries/NDesk.Options.dll $@/
	mkdir -p $@/Plugins
	cp DGPulldownFormatHandler/bin/Release/DGPulldownFormatHandler.dll $@/Plugins/
	cp FFmpegFormatHandler/bin/Release/FFmpegFormatHandler.dll $@/Plugins/
	cp FLACFormatHandler/bin/Release/FLACFormatHandler.dll $@/Plugins/
	cp FLACSharp/bin/Release/FLACSharp.dll $@/Plugins/
	cp MediaInfoFormatHandler/bin/Release/MediaInfoFormatHandler.dll $@/Plugins/
	cp MediaInfoFormatHandler/MediaInfoFormatHandler.dll.config $@/Plugins/
	cp VC1FormatHandler/bin/Release/VC1FormatHandler.dll $@/Plugins/
	cp WAVFormatHandler/bin/Release/WAVFormatHandler.dll $@/Plugins/
	cp WAVSharp/bin/Release/WAVSharp.dll $@/Plugins/
	cp WavPackFormatHandler/bin/Release/WavPackFormatHandler.dll $@/Plugins/
	cp WavPackSharp/bin/Release/WavPackSharp.dll $@/Plugins/
	cp DGPulldown/bin/Release/libDGPulldown.so $@/Plugins/
	cp ffmpeg-convert/bin/Release/libffmpeg-convert.so $@/Plugins/
	cp vc1conv/bin/Release/libvc1conv.so $@/Plugins/

# Rules for Windows build

WINCXX = i586-mingw32msvc-g++
WINC = i586-mingw32msvc-gcc

windowsFiles = DGPulldown/bin/Release/DGPulldown.dll ffmpeg-convert/bin/Release/ffmpeg-convert.dll vc1conv/bin/Release/vc1conv.dll

DGPulldown/bin/Release/DGPulldown.dll: DGPulldown/DGPulldown.cpp
	mkdir -p DGPulldown/bin/Release/
	$(WINCXX) -fpic -shared $^ -o $@

ffmpeg-convert/bin/Release/ffmpeg-convert.dll: ffmpeg-convert/main.cpp
	mkdir -p ffmpeg-convert/bin/Release/
	$(WINCXX) -fpic -shared -D__STDC_CONSTANT_MACROS -Ilibraries/windows/ffmpeg/dev/include -Llibraries/windows/ffmpeg/dev/lib -o $@ $^ -lavcodec -lavformat -lavutil

vc1conv/bin/Release/vc1conv.dll: vc1conv/vc1conv.c
	mkdir -p vc1conv/bin/Release/
	$(WINC) -fpic -shared $^ -o $@


archive/NeonVidUtil-win32/: $(windowsFiles) $(dotNetArchiveFiles)
	mkdir -p $@
	cp NeonVidUtil/bin/Release/NeonVidUtil.exe $@/
	cp NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll $@/
	cp libraries/NDesk.Options.dll $@/
	mkdir -p $@/Plugins
	cp DGPulldownFormatHandler/bin/Release/DGPulldownFormatHandler.dll $@/Plugins/
	cp FFmpegFormatHandler/bin/Release/FFmpegFormatHandler.dll $@/Plugins/
	cp FLACFormatHandler/bin/Release/FLACFormatHandler.dll $@/Plugins/
	cp FLACSharp/bin/Release/FLACSharp.dll $@/Plugins/
	cp MediaInfoFormatHandler/bin/Release/MediaInfoFormatHandler.dll $@/Plugins/
	cp MediaInfoFormatHandler/MediaInfoFormatHandler.dll.config $@/Plugins/
	cp VC1FormatHandler/bin/Release/VC1FormatHandler.dll $@/Plugins/
	cp WAVFormatHandler/bin/Release/WAVFormatHandler.dll $@/Plugins/
	cp WAVSharp/bin/Release/WAVSharp.dll $@/Plugins/
	cp WavPackFormatHandler/bin/Release/WavPackFormatHandler.dll $@/Plugins/
	cp WavPackSharp/bin/Release/WavPackSharp.dll $@/Plugins/
	cp DGPulldown/bin/Release/DGPulldown.dll $@/Plugins/
	cp ffmpeg-convert/bin/Release/ffmpeg-convert.dll $@/Plugins/
	cp vc1conv/bin/Release/vc1conv.dll $@/Plugins/
	cp libraries/windows/mediainfo/MediaInfo.dll $@/Plugins/
	cp libraries/windows/mediainfo/wavpack.dll $@/Plugins/


