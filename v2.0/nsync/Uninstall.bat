@echo off

ATTRIB -R -S -H "%APPDATA%\nsync\*" /s

DEL "%APPDATA%\nsync\*" /s /q /f

RD "%APPDATA%\nsync" /s /q

msiexec /x {4F2F382E-3D60-4304-8D3C-68A808D9C1DB}