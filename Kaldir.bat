@echo off
chcp 65001 >nul
title NetSpeed Widget Kaldirma
color 0C

echo =======================================================
echo          NetSpeed Widget Kaldiriliyor...
echo =======================================================
echo.

set "INSTALL_DIR=%LOCALAPPDATA%\NetSpeedWidget"

:: Calisan sureci durdur
echo [1/4] Calisan surec durduruluyor...
taskkill /F /IM NetSpeedWidget.exe >nul 2>&1

:: Windows Baslangic kaydini kaldir
echo [2/4] Windows otomatik baslatma kaydi siliniyor...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "Remove-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'NetSpeedWidget' -ErrorAction SilentlyContinue;"

:: Masaustu ve Baslat Menusu kisayollarini kaldir
echo [3/4] Kisayollar temizleniyor...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$desktop = [Environment]::GetFolderPath('Desktop');" ^
    "$startMenu = [Environment]::GetFolderPath('Programs');" ^
    "Remove-Item -Path (Join-Path $desktop 'NetSpeed Widget.lnk') -Force -ErrorAction SilentlyContinue;" ^
    "Remove-Item -Path (Join-Path $startMenu 'NetSpeed Widget.lnk') -Force -ErrorAction SilentlyContinue;"

:: Dosyalari temizle
echo [4/4] Program dosyalari siliniyor...
if exist "%INSTALL_DIR%" (
    rmdir /S /Q "%INSTALL_DIR%"
)

echo.
echo =======================================================
echo          KALDIRMA ISLEMI TAMAMLANDI!
echo =======================================================
echo.
echo NetSpeed Widget sisteminizden tamamen temizlendi.
echo.
pause
