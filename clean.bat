@echo off
echo Deleting all bin and obj folders...
rmdir /s /q bin\lib
pushd src
for /f "tokens=*" %%i in ('DIR /B /AD /S obj') do rmdir /s /q %%i 
for /f "tokens=*" %%i in ('DIR /B /AD /S bin') do rmdir /s /q %%i 
popd
