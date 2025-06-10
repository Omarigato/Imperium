@echo off
title Validate Supabase Connection
echo =============================================
echo     Supabase Connection Test
echo =============================================
echo.

REM Check if properties file exists
if not exist "./env-prod.properties" (
    echo ❌ ERROR: env-prod.properties file not found
    pause
    exit /b 1
)

echo Testing connection to Supabase...
echo.

REM Run a simple Liquibase status command to test connection
java -jar ./liquibase/liquibase.jar --defaultsFile=./env-prod.properties status

if %errorlevel% equ 0 (
    echo.
    echo ✅ SUCCESS: Connection to Supabase database is working!
    echo You can now run the migration with: env-prod.cmd
) else (
    echo.
    echo ❌ ERROR: Cannot connect to Supabase database
    echo.
    echo Possible issues:
    echo 1. Wrong password in env-prod.properties
    echo 2. Network connectivity issues
    echo 3. Supabase database is down
    echo 4. SSL certificate issues
    echo.
    echo Check your configuration and try again.
)

echo.
pause