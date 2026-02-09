@echo off
setlocal

echo Publishing for Windows (win-x64)...
dotnet publish FantaniaApp\Fantania.csproj -c Release -r win-x64 --self-contained true -o .\Publish\win-x64
echo Done.

endlocal
