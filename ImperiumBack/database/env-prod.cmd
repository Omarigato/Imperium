@echo off
title Imperium Database Migration - Production
echo =============================================
echo     Imperium Database Migration - PRODUCTION
echo =============================================
echo.

setlocal enabledelayedexpansion

:: 1. Check Java
echo [1/5] Checking Java installation...
java -version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ ERROR: Java not installed or not in PATH
    pause
    exit /b 1
)
echo ✅ Java is installed

:: 2. Check Liquibase
echo [2/5] Checking Liquibase installation...
if not exist "./liquibase/liquibase.jar" (
    echo ❌ ERROR: liquibase.jar not found in ./liquibase/
    pause
    exit /b 1
)
echo ✅ Liquibase found

:: 3. Check MySQL driver
echo [3/5] Checking MySQL driver...
if not exist "./driver/mysql-connector-java-8.0.33.jar" (
    echo ❌ ERROR: MySQL driver not found in ./driver/
    echo Download from: https://dev.mysql.com/downloads/connector/j/
    pause
    exit /b 1
)
echo ✅ MySQL driver found

:: 4. Check .properties
echo [4/5] Checking configuration...
if not exist "./env-prod.properties" (
    echo ❌ ERROR: env-prod.properties file not found
    pause
    exit /b 1
)
echo ✅ Configuration file found

echo.
echo ⚠️  WARNING: You are about to run migration on PRODUCTION MySQL database!
echo.
set /p confirm="Type YES to continue: "
if /I not "%confirm%"=="YES" (
    echo ❌ Cancelled.
    pause
    exit /b 0
)

:: 5. Run migration
echo.
echo [5/5] Running Liquibase migration...
java -jar ./liquibase/liquibase.jar --defaultsFile=./env-prod.properties update

if %errorlevel% equ 0 (
    echo ✅ SUCCESS: MySQL database migration completed!
) else (
    echo ❌ ERROR: Migration failed. Code %errorlevel%
)
echo.
pause
