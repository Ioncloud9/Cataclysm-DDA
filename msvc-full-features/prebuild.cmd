@echo off
SETLOCAL

cd ..\src\lua
echo Generating lua bindings
lua generate_bindings.lua
cd ..\..\msvc-full-features
echo Copy SDL2_mixer.dll
copy .\packages\sdl2_mixer.2.0.0\lib\SDL2_mixer.dll ..\CataVOX\Assets\plugins
echo Done

echo Generating "version.h"...
for /F "tokens=*" %%i in ('git describe --tags --always --dirty --match "[0-9]*.*"') do set VERSION=%%i
echo VERSION defined as %VERSION%
>..\src\version.h echo #define VERSION "%VERSION%"
