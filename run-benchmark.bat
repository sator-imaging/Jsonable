@echo off

:: force overwrite
dotnet run --project ./embedded  -f net8.0 -- --force --quiet  || (echo === ERROR === & exit 1)

echo.

dotnet run --project ./sample  -c Release  %*  -- --benchmark  || (echo === ERROR === & exit 1)
