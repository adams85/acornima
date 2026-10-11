@ECHO OFF
REM Run this script to format code manually.

dotnet format Acornima.slnx --severity warn --report .
