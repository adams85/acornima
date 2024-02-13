@echo off

SETLOCAL

IF NOT [%2] == [] (
  SET RUNTIMES=%2
)

IF [%2] == [] (
  SET RUNTIMES=net8.0 net48
)

dotnet run -c Release -f net8.0 -- --job medium --runtimes %RUNTIMES%