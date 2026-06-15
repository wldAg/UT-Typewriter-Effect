@echo off
dotnet publish "../UT×ÖÄ».csproj" -c Release -r win-x64 -o "./"
upx *.exe