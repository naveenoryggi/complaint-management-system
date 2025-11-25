@echo off
echo ============================================
echo  Setup Error Log
echo ============================================
echo.

if exist setup-errors.txt (
    type setup-errors.txt
    echo.
    echo ============================================
    echo Copy the errors above and share with me
    echo ============================================
) else (
    echo No errors found! Setup was successful.
    echo.
    echo Checking if solution exists...
    if exist ComplaintManagementSystem.sln (
        echo ✓ Solution file exists
    ) else (
        echo ✗ Solution file NOT found
    )
    echo.
    echo Checking projects...
    if exist src\ComplaintManagement.API (
        echo ✓ API project exists
    )
    if exist src\ComplaintManagement.Domain (
        echo ✓ Domain project exists
    )
    if exist src\ComplaintManagement.Infrastructure (
        echo ✓ Infrastructure project exists
    )
)

echo.
pause
