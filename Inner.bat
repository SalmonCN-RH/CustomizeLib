@echo off
setlocal enabledelayedexpansion

set "props=Directory.Build.props"

:: 检查文件是否存在
if not exist "%props%" (
    echo Error: %props% not found!
    pause
    exit /b 1
)

echo Replacing "$(PluginsPath)" with "$(InnerPath)" in %props%...

:: 使用 PowerShell 直接替换子字符串，指定 UTF8 编码
powershell -Command "$content = Get-Content '%props%' -Raw -Encoding UTF8; $content = $content.Replace('$(PluginsPath)', '$(InnerPath)'); $content | Out-File '%props%' -Encoding UTF8"

echo Replacement done.

echo Checking directories for safe rename...
set "base=lib\BepInEx"

:: 检查目标是否存在冲突（不删除任何文件）
if exist "%base%\libs-re" (
    echo Error: "%base%\libs-re" already exists.
    echo Please manually remove or rename it, then rerun this script.
    pause
    exit /b 1
)

:: 执行重命名：libs -> libs-re
if exist "%base%\libs" (
    echo    Renaming libs to libs-re
    ren "%base%\libs" "libs-re"
)

:: 执行重命名：libs-in -> libs
if exist "%base%\libs-in" (
    echo    Renaming libs-in to libs
    ren "%base%\libs-in" "libs"
)

echo Operation completed.
pause