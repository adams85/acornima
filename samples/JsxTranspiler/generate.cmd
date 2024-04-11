@echo off

REM Transpile template
dotnet run -f net6.0 -- --type module < template.jsx > template.mjs

REM Generate output from template
node generate.mjs