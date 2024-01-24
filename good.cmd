REM Starting the .NET project in the Example folder in a new window
start cmd /k pushd Example ^& dotnet run ^& popd

REM Executing commands in the Demo folder
pushd Demo
dotnet tool restore
dotnet restore
dotnet graphql init http://localhost:50505/api/graphql -n StarWars -p ./Demo
popd