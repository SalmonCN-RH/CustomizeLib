@echo off
chcp 65001 >nul
powershell -NoProfile -ExecutionPolicy Bypass -Command "& {$str = Read-Host '请输入要转换的字符串'; if ([string]::IsNullOrEmpty($str)) { Write-Host '输入为空'; exit }; $bytes = [System.Text.Encoding]::Unicode.GetBytes($str); $result = ''; for ($i=0; $i -lt $bytes.Length; $i+=2) { $code = [bitconverter]::ToUInt16($bytes, $i); $result += '\u{0:X4}' -f $code }; Write-Host ('转换结果：' + $result); try { Set-Clipboard $result; Write-Host '已复制到剪贴板 (Set-Clipboard)' } catch { $result | clip; Write-Host '已复制到剪贴板 (clip)' } }"
pause