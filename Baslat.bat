@echo off
chcp 65001 >nul
title NetSpeed Widget Baslat
taskkill /F /IM NetSpeedWidget.exe >nul 2>&1
start "" "%LOCALAPPDATA%\NetSpeedWidget\NetSpeedWidget.exe"
if %errorlevel% neq 0 (
    if exist "%~dp0publish\NetSpeedWidget.exe" (
        start "" "%~dp0publish\NetSpeedWidget.exe"
    )
)
echo NetSpeed Widget baslatildi.
