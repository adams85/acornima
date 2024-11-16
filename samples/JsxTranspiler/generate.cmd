@echo off

SET TARGET_FRAMEWORK=%1
IF [%TARGET_FRAMEWORK%] == [] (
  SET TARGET_FRAMEWORK=net6.0
)

REM Transpile template
dotnet build -f %TARGET_FRAMEWORK%
bin\debug\%TARGET_FRAMEWORK%\jsxt --type module < template.jsx > template.mjs

REM Generate output from template
node generate.mjs