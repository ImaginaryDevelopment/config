"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Team Tools\Performance Tools\vsinstr.exe" -coverage "%1"
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Team Tools\Performance Tools\VSPerfCmd.exe" /start:coverage /output:run.coverage
start "%1" /wait "%1"
"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Team Tools\Performance Tools\VSPerfCmd.exe" /shutdown
notepad "run.coverage"
echo "%1"
echo "%2"

pause