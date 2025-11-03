param (
    [Parameter(Mandatory = $true)][string]$name
)

# Check if dotnet ef is available
$efAvailable = $false
try {
    $null = dotnet ef --version 2>&1
    if ($LASTEXITCODE -eq 0) {
        $efAvailable = $true
    }
}
catch {
    $efAvailable = $false
}

# Only restore if dotnet ef is not available
if (-not $efAvailable) {
    Write-Host "dotnet ef not found. Running dotnet restore..."
    dotnet restore
}

dotnet ef migrations --project ./DataLayer.Postgres/DataLayer.Postgres.csproj add $name
dotnet ef migrations --project ./DataLayer.Sqlite/DataLayer.Sqlite.csproj add $name