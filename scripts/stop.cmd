taskkill /f /im valuator.exe
taskkill /f /im rankcalculator.exe
taskkill /f /im eventslogger.exe

cd %~dp0..\nginx\
nginx -s stop 