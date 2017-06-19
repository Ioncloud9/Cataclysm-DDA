@echo off
pushd "%~dp0"
for %%a in (x86 x64) do (
    mkdir .\bin\%%a
	xcopy /D /y zeromq-4.2.1\lib\%%a\smpeg2.dll          .\bin\%%a
)
popd