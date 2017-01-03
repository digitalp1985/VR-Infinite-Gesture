@ECHO OFF

SETLOCAL ENABLEEXTENSIONS

SET UnityPath="%ProgramFiles%/Unity 5.4.3f1/Editor/Unity.exe"
SET ProjectPath="%USERPROFILE%/Projects/VR-Infinite-Gesture/Unity"
SET Method1="CommandLineMethods.OpenScriptEditor"

ECHO VR-Infinite-Gesture
ECHO Unity 5.4.3f1

REM this command opens unity with the given project and runs the method given
START "Unity" %UnityPath% -projectPath %ProjectPath% -executeMethod %Method1%

EXIT

ENDLOCAL

