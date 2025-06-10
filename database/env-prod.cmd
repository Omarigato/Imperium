@echo off
title Imperium Database Migration - Production
echo =============================================
echo     Imperium Database Migration - PRODUCTION
echo =============================================
echo.

REM Set error handling
setlocal enabledelayedexpansion

REM Check if Java is installed
echo [1/5] Checking Java installation...
java -version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ ERROR: Java is not installed or not in PATH
    echo Please install Java 8 or higher and add it to your PATH
    echo Download from: https://www.oracle.com/java/technologies/downloads/
    echo.
    pause
    exit /b 1
)
echo ✅ Java is installed

REM Check if liquibase.jar exists
echo [2/5] Checking Liquibase installation...
if not exist "./liquibase/liquibase.jar" (
    echo ❌ ERROR: liquibase.jar not found in ./liquibase/ directory
    echo Please download Liquibase from https://www.liquibase.org/download
    echo.
    pause
    exit /b 1
)
echo ✅ Liquibase found

REM Check if PostgreSQL driver exists
echo [3/5] Checking PostgreSQL driver...
if not exist "./driver/postgresql-42.7.6.jar" (
    echo ❌ ERROR: PostgreSQL driver not found
    echo Please download postgresql-42.7.6.jar and place it in ./driver/ directory
    echo Download from: https://jdbc.postgresql.org/download.html
    echo.
    pause
    exit /b 1
)
echo ✅ PostgreSQL driver found

REM Check if properties file exists
echo [4/5] Checking configuration...
if not exist "./env-prod.properties" (
    echo ❌ ERROR: env-prod.properties file not found
    echo.
    pause
    exit /b 1
)
echo ✅ Configuration file found

REM Warning about production environment
echo.
echo ⚠️  WARNING: You are about to run migration on PRODUCTION database!
echo.
echo Target: Supabase Production Database
echo.
set /p confirm="Are you sure you want to continue? (type 'YES' to proceed): "
if not "%confirm%"=="YES" (
    echo Operation cancelled.
    pause
    exit /b 0
)

echo.
echo [5/5] Running Liquibase migration...
echo.

java -jar ./liquibase/liquibase.jar --defaultsFile=./env-prod.properties update

if %errorlevel% equ 0 (
    echo.
    echo ✅ SUCCESS: Production database migration completed successfully!
    echo.
    echo Database Tables Created:
    echo - Users (with Admin user)
    echo - Dictionaries (with initial data)
    echo - Products, Files, ProductFiles
    echo - ProductColors, ProductSizes
    echo - Carts, Favorites, Addresses
    echo - Orders, OrderItems, Reviews
    echo - Verifications, Logs
    echo.
) else (
    echo.
    echo ❌ ERROR: Database migration failed with error code %errorlevel%
    echo.
    echo Common issues:
    echo 1. Check your password in
    echo 1. Check your password in env-prod.properties
   echo 2. Verify network connection to Supabase
   echo 3. Ensure database exists and permissions are correct
   echo.
)

echo.
echo Migration log saved to: liquibase-output.log
echo.
pause