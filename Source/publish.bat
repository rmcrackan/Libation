rmdir bin\Publish /S /Q
dotnet publish LibationWinForms\LibationWinForms.csproj -p:PublishProfile=LibationWinForms\Properties\PublishProfiles\FolderProfile.pubxml
dotnet publish LibationCli\LibationCli.csproj -p:PublishProfile=LibationCli\Properties\PublishProfiles\FolderProfile.pubxml
dotnet publish Hangover\Hangover.csproj -p:PublishProfile=Hangover\Properties\PublishProfiles\FolderProfile.pubxml