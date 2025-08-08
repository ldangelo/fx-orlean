build:
  dotnet build

run-eventserver: 
  wezterm cli spawn --cwd . dotnet watch --project src/EventServer/EventServer.csproj

run-fxexpert: 
  wezterm cli spawn --cwd . dotnet watch --project src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj --launch-profile https

run: run-eventserver run-fxexpert

test: run
  dotnet test
  cd tests/FxExpert.E2E.Tests; dotnet test
