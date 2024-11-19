@echo off

SETLOCAL

IF NOT [%1] == [] (
  SET RUNTIMES=%1
)

IF [%1] == [] (
  SET RUNTIMES=net8.0 net48
)

dotnet run -c Release -f net8.0 -- --job medium --runtimes %RUNTIMES%