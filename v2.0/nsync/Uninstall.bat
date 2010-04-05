@echo off

DEL "%PROGRAMFILES%\TEAM14\NSYNC\SETTINGS.XML" /s /q

DEL "%localappdata%\VirtualStore\Program Files\team14\nsync\settings.xml" /s /q

msiexec /x {4F2F382E-3D60-4304-8D3C-68A808D9C1DB}