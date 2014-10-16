REM Calling the custom BuildTool if exists to run ILMerge and Obfuscate

echo ProjectDir is %1
echo Configuration is %2

set BuildToolPath="%1..\..\BuildTool.exe"
echo BuildToolPath is %BuildToolPath%

if not exist %BuildToolPath% goto end

echo Processing assemblies
%BuildToolPath% "%1..\..\EQATECTracerMain\bin\%2"   /smartassembly /copytoobjfolder

:end
echo Done