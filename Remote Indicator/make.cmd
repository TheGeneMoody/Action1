@ECHO OFF
if "%~1"=="" (
    ECHO First argument of source code file is missing.
    EXIT /b 1
)

if "%~2"=="" (
    ECHO Second argument of output filename is missing.
    EXIT /b 1
)
ECHO Building %2.exe, this may take a few seconds, a little more if your AV needs to scan it...
	::Locates highest framework version C# source compiler installed on system.
	FOR /f %%. IN ('DIR /s /b %windir%\Microsoft.NET ^| FINDSTR "Framework64\\v.*csc.exe$"') DO SET MAKE=%%. /t:winexe /nologo /out:%2.exe %1 
	%MAKE% && EXIT /b %errorlevel%