@echo off
title Supabase Setup for Imperium
echo =============================================
echo     Supabase Database Setup Guide
echo =============================================
echo.

echo This script will help you configure Supabase connection.
echo.
echo Step 1: Update your password in env-prod.properties
echo ------------------------------------------------
echo.
echo 1. Open: database\env-prod.properties
echo 2. Replace [YOUR-PASSWORD] with your actual Supabase password
echo 3. Save the file
echo.
echo Current connection string:
echo url=jdbc:postgresql://db.cedzobkdpamlpdswcjsy.supabase.co:5432/postgres?sslmode=require
echo username=postgres
echo password=[YOUR-PASSWORD]  ^<-- UPDATE THIS
echo.
echo.

echo Step 2: Test Java installation
echo --------------------------------
java -version 2>nul
if %errorlevel% equ 0 (
    echo ✅ Java is installed
) else (
    echo ❌ Java NOT found - please install Java 8 or higher
    echo Download from: https://www.oracle.com/java/technologies/downloads/
)
echo.

echo Step 3: Verify required files
echo ------------------------------
if exist "./liquibase/liquibase.jar" (
    echo ✅ Liquibase found
) else (
    echo ❌ liquibase.jar missing
)

if exist "./driver/postgresql-42.7.6.jar" (
    echo ✅ PostgreSQL driver found
) else (
    echo ❌ PostgreSQL driver missing
)

if exist "./changelog/0-db.changelog-master.xml" (
    echo ✅ Changelog files found
) else (
    echo ❌ Changelog files missing
)
echo.

echo Step 4: Ready to run migration
echo --------------------------------
echo After updating your password, run:
echo.
echo     env-prod.cmd
echo.
echo This will create all tables and initial data in your Supabase database.
echo.

pause