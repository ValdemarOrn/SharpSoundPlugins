rd /S /Q Builds\Current

rd MidiScript\bin /s /q
rd MrFuzz\bin /s /q
rd Rodent.V2\bin /s /q
rd RXG100\bin /s /q
rd SmashMaster\bin /s /q

msbuild SharpSoundPlugins.sln /p:Configuration=Debug
msbuild SharpSoundPlugins.sln /p:Configuration=Release

set ts=%DATE:~6,4%-%DATE:~3,2%-%DATE:~0,2%

rd Builds\Current /s /q
mkdir Builds\Current
xcopy MidiScript\bin\Release\*.dll Builds\Current\MidiScript\ /s /e /h
xcopy MrFuzz\bin\Release\*.dll Builds\Current\MrFuzz\ /s /e /h
xcopy Rodent.V2\bin\Release\*.dll Builds\Current\Rodent.V2\ /s /e /h
xcopy RXG100\bin\Release\*.dll Builds\Current\RXG100\ /s /e /h
xcopy SmashMaster\bin\Release\*.dll Builds\Current\SmashMaster\ /s /e /h

cd Builds\Current

..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe MidiScript\MidiScript.dll MidiScript.VST.dll
..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe MrFuzz\MrFuzz.dll MrFuzz.VST.dll
..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe Rodent.V2\Rodent.V2.dll Rodent.V2.VST.dll
..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe RXG100\RXG100.dll RXG100.VST.dll
..\..\Binaries\SharpSoundDevice\x64\Release\BridgeGenerator.exe SmashMaster\SmashMaster.dll SmashMaster.VST.dll

cd Builds\Current
rm ../SharpSoundPlugins-%ts%.zip
7z a ../SharpSoundPlugins-%ts%.zip -r *.*
cd..
cd..