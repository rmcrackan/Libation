rmdir bin\Publish /S /Q
dotnet publish -c Release LibationWinForms\LibationWinForms.csproj -p:PublishProfile=LibationWinForms\Properties\PublishProfiles\FolderProfile.pubxml
dotnet publish -c Release LibationCli\LibationCli.csproj -p:PublishProfile=LibationCli\Properties\PublishProfiles\FolderProfile.pubxml
dotnet publish -c Release Hangover\Hangover.csproj -p:PublishProfile=Hangover\Properties\PublishProfiles\FolderProfile.pubxml