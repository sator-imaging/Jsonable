@echo off

:: force overwrite
dotnet run --project ./embedded  -f net8.0 -- --force --quiet  || (echo === ERROR === & exit 1)

echo.

:: test both sdk (net6.0 for Unity compatibility test)
dotnet run --project ./tests  -f net9.0  %*  || (echo === ERROR === & exit 1)
dotnet run --project ./tests  -f net6.0  %*  || (echo === ERROR === & exit 1)

:: should also run with --pretty-print option in CI
