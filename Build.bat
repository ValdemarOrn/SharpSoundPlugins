rd /S /Q bin
msbuild SharpSoundDevice.sln /p:Configuration=Debug
msbuild SharpSoundDevice.sln /p:Configuration=Release

set ts=%DATE:~11%-%DATE:~8,2%-%DATE:~5,2%

rd Builds\Current /s /q
mkdir Builds\Current
xcopy bin Builds\Current\ /s /e /h

cd Builds\Current
rm ../SharpSoundDevice-%ts%.zip
7z a ../SharpSoundDevice-%ts%.zip -r *.*
cd..
cd..
rd /S /Q Builds\Current