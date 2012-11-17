
all: archive/NeonVidUtil-dotNET.tar.gz
# archive/NeonVidUtil-win32.7z

archive/NeonVidUtil-dotNET.tar.gz: archive/NeonVidUtil-dotNET
	cd archive/ && tar -czf NeonVidUtil-dotNET.tar.gz NeonVidUtil-dotNET/

archive/:
	mkdir archive/
	
clean:
	rm -rf archive/
	rm -f NeonVidUtil/bin/Release/NeonVidUtil.exe
	rm -f NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll

# Rules for .NET code

NeonVidUtil/bin/Release/NeonVidUtil.exe: libraries/NDesk.Options.dll NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll NeonVidUtil/*.cs
	xbuild NeonVidUtil/NeonVidUtil.csproj /p:Configuration=Release /verbosity:quiet

NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll: NeonVidUtilCore/*.cs
	xbuild NeonVidUtilCore/NeonVidUtilCore.csproj /p:Configuration=Release /verbosity:quiet


archive/NeonVidUtil-dotNET: archive/ NeonVidUtil/bin/Release/NeonVidUtil.exe NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll
	mkdir $@
	cp NeonVidUtil/bin/Release/NeonVidUtil.exe $@/
	cp NeonVidUtilCore/bin/Release/NeonVidUtilCore.dll $@/
	cp libraries/NDesk.Options.dll $@/



# Rules for Native build



# Rules for Windows build


