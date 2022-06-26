<# You must enable running powershell scripts.

	Set-ExecutionPolicy -Scope CurrentUser Unrestricted
#>

$pubDir = "bin\Publish"
Remove-Item $pubDir -Recurse -Force

dotnet publish -c Release LibationWinForms\LibationWinForms.csproj -p:PublishProfile=LibationWinForms\Properties\PublishProfiles\FolderProfile.pubxml
dotnet publish -c Release LibationCli\LibationCli.csproj -p:PublishProfile=LibationCli\Properties\PublishProfiles\FolderProfile.pubxml
dotnet publish -c Release Hangover\Hangover.csproj -p:PublishProfile=Hangover\Properties\PublishProfiles\FolderProfile.pubxml

$verMatch = Select-String -Path 'AppScaffolding\AppScaffolding.csproj' -Pattern '<Version>(\d{0,3}\.\d{0,3}\.\d{0,3})\.\d{0,3}</Version>'
$archiveName = "bin\Libation."+$verMatch.Matches.Groups[1].Value+".zip"
Get-ChildItem -Path $pubDir -Recurse |
	Compress-Archive -DestinationPath $archiveName -Force