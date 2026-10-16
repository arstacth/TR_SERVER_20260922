@echo off
rem Move dependency DLL/XML from bin\ into bin\bin\ after server build.
rem Keeps main EXE + *.exe.config in bin\ only. PDBs are not required to run.
setlocal EnableExtensions
set "OUT=%~1"
if "%OUT%"=="" exit /b 0
if not "%OUT:~-1%"=="\" set "OUT=%OUT%\"
if not exist "%OUT%bin\" mkdir "%OUT%bin\"

for %%F in ("%OUT%*.dll") do move /Y "%%~F" "%OUT%bin\" >nul 2>&1
for %%F in ("%OUT%*.xml") do move /Y "%%~F" "%OUT%bin\" >nul 2>&1
rem Sweep stray dependency PDBs out of bin\ root (leave none required for runtime).
for %%F in ("%OUT%*.pdb") do del /f /q "%%~F" >nul 2>&1
exit /b 0
