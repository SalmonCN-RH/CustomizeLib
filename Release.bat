@echo off
setlocal enabledelayedexpansion

set "props=Directory.Build.props"

:: 检查文件是否存在
if not exist "%props%" (
    echo Error: %props% not found!
    pause
    exit /b 1
)

echo Replacing "$(InnerPath)" with "$(PluginsPath)" in %props%...

:: 使用 PowerShell 直接替换，指定 UTF8 编码，避免 cmd 转义问题
powershell -Command "$content = Get-Content '%props%' -Raw -Encoding UTF8; $content = $content.Replace('$(InnerPath)', '$(PluginsPath)'); $content | Out-File '%props%' -Encoding UTF8"

echo Replacement done.

echo Checking directories for safe rename...
set "base=lib\BepInEx"

:: 如果 libs-in 已存在，报错退出，不删除任何文件
if exist "%base%\libs-in" (
    echo Error: "%base%\libs-in" already exists.
    echo Please manually remove or rename it, then rerun this script.
    pause
    exit /b 1
)

:: 安全重命名（无删除）
if exist "%base%\libs" (
    echo    Renaming libs to libs-in
    ren "%base%\libs" "libs-in"
)
if exist "%base%\libs-re" (
    echo    Renaming libs-re to libs
    ren "%base%\libs-re" "libs"
)

echo Operation completed.
pause