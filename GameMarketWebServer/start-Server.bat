@echo on

call start http://localhost:5173
call npm install
if %ERRORLEVEL% neq 0 pause
call npm run dev
if %ERRORLEVEL% neq 0 pause
echo Server is running...
pause
