taskkill /f /im valuator.exe
taskkill /f /im rankcalculator.exe

cd %~dp0..\nginx\
nginx -s stop 