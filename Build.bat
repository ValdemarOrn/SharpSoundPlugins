msbuild SharpSoundPlugins.sln /p:Configuration=Release

set ts=%DATE:~11%-%DATE:~8,2%-%DATE:~5,2%

rd Builds\Current /s /q
mkdir Builds\Current

mkdir Builds\Current\Biquad
rem mkdir Builds\Current\Modelay
mkdir Builds\Current\MrFuzz
mkdir Builds\Current\Nearfield
mkdir Builds\Current\Rodent.V2
mkdir Builds\Current\RXG100
mkdir Builds\Current\SmashMaster

xcopy Biquad\bin\Release\*.dll Builds\Current\Biquad
rem xcopy Modelay\bin\Release\*.dll Builds\Current\Modelay
xcopy MrFuzz\bin\Release\*.dll Builds\Current\MrFuzz
xcopy Nearfield\bin\Release\*.dll Builds\Current\Nearfield
xcopy Rodent.V2\bin\Release\*.dll Builds\Current\Rodent.V2
xcopy RXG100\bin\Release\*.dll Builds\Current\RXG100
xcopy SmashMaster\bin\Release\*.dll Builds\Current\SmashMaster

Binaries\BridgeGenerator.exe Builds\Current\Biquad\Biquad.dll Builds\Current\Biquad\Biquad.VST.dll
rem Binaries\BridgeGenerator.exe Builds\Current\Modelay\Modelay.dll Builds\Current\Modelay\Modelay.VST.dll
Binaries\BridgeGenerator.exe Builds\Current\MrFuzz\MrFuzz.dll Builds\Current\MrFuzz\MrFuzz.VST.dll
Binaries\BridgeGenerator.exe Builds\Current\Nearfield\Nearfield.dll Builds\Current\Nearfield\Nearfield.VST.dll
Binaries\BridgeGenerator.exe Builds\Current\Rodent.V2\Rodent.V2.dll Builds\Current\Rodent.V2\Rodent.V2.VST.dll
Binaries\BridgeGenerator.exe Builds\Current\RXG100\RXG100.dll Builds\Current\RXG100\RXG100.VST.dll
Binaries\BridgeGenerator.exe Builds\Current\SmashMaster\SmashMaster.dll Builds\Current\SmashMaster\SmashMaster.VST.dll

cd Builds\Current
rm ../Plugins-%ts%.zip
7z a ../Plugins-%ts%.zip -r *.*
cd..
cd..
rd Builds\Current /s /q