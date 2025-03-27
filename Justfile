build:
  dotnet build

run-eventserver: 
  wezterm cli spawn --cwd . dotnet watch --project src/EventServer/EventServer.csproj

run-fxexpert: 
  wezterm cli spawn --cwd . dotnet watch --project src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj

run: run-eventserver run-fxexpert
