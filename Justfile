build:
  dotnet build

run-eventserver: 
  wezterm cli spawn --cwd . dotnet run --project src/EventServer/EventServer.csproj

run-fxexpert: 
  wezterm cli spawn --cwd . dotnet run --project src/FxExpert.Blazor/FxExpert.Blazor/FxExpert.Blazor.csproj

run: run-eventserver run-fxexpert
  open http://localhost:8500
