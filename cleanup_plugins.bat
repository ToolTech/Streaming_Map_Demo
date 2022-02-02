rem ---- Run this to clean plugin folders --------------------------------
rem ---- Then build Packages project with correct pltform and configuration  ---

rmdir /S /Q Assets\Plugins\x64
rmdir /S /Q Assets\Plugins\ARM64
rmdir /S /Q Assets\Plugins\x86
rmdir /S /Q Assets\Plugins\ARM
mkdir Assets\Plugins

