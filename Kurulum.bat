@echo off
chcp 65001 >nul
title NetSpeed Widget Kurulumu
color 0A

echo =======================================================
echo          NetSpeed Widget Kurulumu Baslatiliyor
echo =======================================================
echo.

set "INSTALL_DIR=%LOCALAPPDATA%\NetSpeedWidget"
set "SOURCE_DIR=%~dp0publish"

:: Onceki calisan surecleri sonlandir
echo [1/5] Calisan eski surecler kontrol ediliyor...
taskkill /F /IM NetSpeedWidget.exe >nul 2>&1

:: Hedef dizini olustur
echo [2/5] Kurulum dizini hazirlaniyor: "%INSTALL_DIR%"
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

:: Dosyalari kopyala
echo [3/5] Program dosyalari kopyalaniyor...
copy /Y "%SOURCE_DIR%\*.*" "%INSTALL_DIR%\" >nul

if %errorlevel% neq 0 (
    echo [HATA] Dosyalar kopyalanamadi!
    pause
    exit /b %errorlevel%
)

:: Windows Baslangic Kaydi ve Kisayollari Olustur
echo [4/5] Baslangic kaydi ve masaustu kisayollari olusturuluyor...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$ws = New-Object -ComObject WScript.Shell;" ^
    "$desktop = [Environment]::GetFolderPath('Desktop');" ^
    "$startMenu = [Environment]::GetFolderPath('Programs');" ^
    "$target = Join-Path $env:LOCALAPPDATA 'NetSpeedWidget\NetSpeedWidget.exe';" ^
    "$icon = Join-Path $env:LOCALAPPDATA 'NetSpeedWidget\app.ico';" ^
    "$sc1 = $ws.CreateShortcut((Join-Path $desktop 'NetSpeed Widget.lnk'));" ^
    "$sc1.TargetPath = $target;" ^
    "$sc1.WorkingDirectory = (Join-Path $env:LOCALAPPDATA 'NetSpeedWidget');" ^
    "$sc1.IconLocation = $icon;" ^
    "$sc1.Description = 'Windows Gorev Cubugu Ag Hiz Gostergesi';" ^
    "$sc1.Save();" ^
    "$sc2 = $ws.CreateShortcut((Join-Path $startMenu 'NetSpeed Widget.lnk'));" ^
    "$sc2.TargetPath = $target;" ^
    "$sc2.WorkingDirectory = (Join-Path $env:LOCALAPPDATA 'NetSpeedWidget');" ^
    "$sc2.IconLocation = $icon;" ^
    "$sc2.Description = 'Windows Gorev Cubugu Ag Hiz Gostergesi';" ^
    "$sc2.Save();" ^
    "Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'NetSpeedWidget' -Value ('\"' + $target + '\"');"

:: Programi Calistir
echo [5/5] NetSpeed Widget baslatiliyor...
start "" /D "%INSTALL_DIR%" "%INSTALL_DIR%\NetSpeedWidget.exe"

echo.
echo =======================================================
echo          KURULUM BASARIYLA TAMAMLANDI!
echo =======================================================
echo.
echo  * NetSpeed Widget gorev cubugunda (saatin yaninda) acildi.
echo  * Bilgisayariniz her acildiginda otomatik olarak baslayacaktir.
echo  * Hiz gostergesini farenin SOL tusuyla istediginiz yere tasiyabilirsiniz.
echo  * Sag tiklayarak:
echo      - Konumu kilitleyebilir,
echo      - Temayi (Koyu, Saydam, Acik) degistirebilir,
echo      - Ag istatistiklerini gorebilirsiniz.
echo.
pause
