@echo off

:: force overwrite
dotnet run --project ./embedded  -f net8.0 -- --force --quiet  || (echo === ERROR === & exit 1)

echo.

:: test both sdk (net5.0 for Unity compatibility test)
dotnet run --project ./sample  -f net9.0  %*  || (echo === ERROR === & exit 1)
dotnet run --project ./sample  -f net5.0  %*  || (echo === ERROR === & exit 1)

:: should also run with --pretty-print option in CI
