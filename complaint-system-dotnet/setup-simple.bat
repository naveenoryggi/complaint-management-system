@echo off
echo ============================================
echo  .NET Solution Setup
echo ============================================
echo.
echo This will create the .NET solution and projects...
echo Please wait, this may take 2-5 minutes...
echo.
echo All output will be saved to: setup-result.txt
echo.

REM Clear old result file
if exist setup-result.txt del setup-result.txt

REM Run the PowerShell script and save output
powershell -ExecutionPolicy Bypass -File setup-solution-with-logging.ps1 > setup-result.txt 2>&1

echo.
echo ============================================
echo  Setup Completed!
echo ============================================
echo.
echo Result saved to: setup-result.txt
echo.
echo Opening the result file in Notepad...
echo.

REM Open in Notepad
notepad setup-result.txt

echo.
echo Instructions:
echo 1. Read the setup-result.txt file that just opened
echo 2. Look for errors (lines starting with ERROR or red text)
echo 3. Copy ALL the text from Notepad
echo 4. Paste it in chat to share with me
echo.
pause
