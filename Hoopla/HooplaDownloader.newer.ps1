<#
.SYNOPSIS
    Downloads, decrypts and repackages content from Hoopla

.DESCRIPTION
    Uses a HooplaDigital.com account to download DRM-free copies of ebooks, comics,
    and/or audiobooks available on the platform. Content that is not already borrowed
    on the account will be borrowed if slots are available. Content that is not borrowed
    cannot be downloaded.

    * E-Books are downloaded to epub files (most) or cbz (rare, picture books).
    * Comic books are downloaded to cbz files.
    * Audiobooks are downloaded to m4a files. (single file, and very little metadata available
      from Hoopla, such as chapters)

.PARAMETER Credential
    Credential to use for logging into Hoopla site.
    (Cannot be used with Username and Password parameters)

.PARAMETER Username
    Username to use for logging into Hoopla site.
    (Cannot be used with Credential parameter)

.PARAMETER Password
    Password to use for logging into Hoopla site.
    (Cannot be used with Credential parameter)

.PARAMETER TitleId
    Specifies one or more title IDs of content to download.

.PARAMETER OutputFolder
    Sets the output folder for downloaded content. Defaults to current directory.

.PARAMETER PatronId
    Override default patron id for Hoopla. (This is rarely required as most user accounts are only tied
    to a single patron).

.PARAMETER EpubZipBin
    Specifies path to epubzip binary. Else look for one beside script, or in system path.

.PARAMETER FfmpegBin
    Specifies path to ffmpeg binary. Else look for one beside script, or in system path.

.PARAMETER KeepDecryptedData
    If set, don't delete the intermediary data after decryption, before final output file.
    For ebooks, this is xml, images, and the manifest. For comics, it is images. For audiobooks,
    it is mp4 ts files. This is typically only useful for development or troubleshooting.

.PARAMETER KeepEncryptedData
    If set, don't delete the encrypted data as downloaded from Hoopla's servers. This is typically
    only useful for development or troubleshooting.

.PARAMETER AllBorrowed
    This parameter is deprecated. If TitleId is not set, it is implied that all borrowed titles will
    be downloaded.

.PARAMETER AudioBookForceSingleFile
    If set, leave audiobook as single file, as if chapter data is not present.

.EXAMPLE
    .\Invoke-HooplaDownload.ps1 123456
    Downloads Hoopla content with title id 123456

.NOTES
    Author: kabutops728 - My Anonamouse
    Version: 2.9
#>

[CmdletBinding(DefaultParameterSetName='CredentialSingleTitle')]
param(
    [int64[]]
    $TitleId,

    [Parameter(Mandatory,ParameterSetName='CredentialSingleTitle')]
    [Management.Automation.PSCredential]
    $Credential,

    [Parameter(Mandatory,ParameterSetName='UserPassSingleTitle')]
    [string]
    $Username,

    [Parameter(Mandatory,ParameterSetName='UserPassSingleTitle')]
    [string]
    $Password,

    [ValidateScript({Test-Path -LiteralPath $_ -IsValid -PathType Container})]
    [string]$OutputFolder = $PSScriptRoot,

    [int64]$PatronId,

    [string]$EpubZipBin,

    [string]$FfmpegBin,

    [switch]$KeepDecryptedData,

    [switch]$KeepEncryptedData,

    [switch]$AudioBookForceSingleFile,

    # Deprecated
    [switch]$AllBorrowed
)

$USER_AGENT = 'Hoopla Android/4.27'

$HEADERS = @{
    'app' = 'ANDROID'
    'app-version' = '4.27.1'
    'device-module' = 'KFKAWI'
    'device-version' = ''
    'hoopla-verson' = '4.27.1'
    'kids-mode' = 'false'
    'os' = 'ANDROID'
    'os-version' = '6.0.1'
    'ws-api' = '2.1'
    'Host' = 'hoopla-ws.hoopladigital.com'
}

$URL_HOOPLA_WS_BASE = 'https://hoopla-ws.hoopladigital.com'
$URL_HOOPLA_LIC_BASE = 'https://hoopla-license2.hoopladigital.com'

$COMIC_IMAGE_EXTS = @('.jpg','.png','.jpeg','.gif','.bmp','.tif','.tiff')

enum HooplaKind
{
    EBOOK = 5
    MUSIC = 6
    MOVIE = 7
    AUDIOBOOK = 8
    TELEVISION = 9
    COMIC = 10
}

$SUPPORTED_KINDS = @([HooplaKind]::EBOOK, [HooplaKind]::COMIC, [HooplaKind]::AUDIOBOOK)

Function Connect-Hoopla
{
    param(
        [Parameter(Mandatory)][Management.Automation.PSCredential]$Credential
    )

    $username = $Credential.UserName
    $password = $Credential.GetNetworkCredential().Password

    $res = Invoke-RestMethod -Uri "$URL_HOOPLA_WS_BASE/tokens" -Method Post -Headers $HEADERS -UserAgent $USER_AGENT -Body @{username = $username; password = $password}

    if ($res.tokenStatus -ne 'SUCCESS')
    {
        throw $res.message
    }

    $res.token
}

Function Get-HooplaUsers
{
    param(
        [Parameter(Mandatory)][string]$Token
    )

    $h = $HEADERS.Clone()
    $h['Authorization'] = "Bearer $Token"

    Invoke-RestMethod -Uri "$URL_HOOPLA_WS_BASE/users" -Method Get -Headers $h -UserAgent $USER_AGENT
}

Function Get-HooplaTitleInfo
{
    param(
        [Parameter(Mandatory)][int64]$PatronId,
        [Parameter(Mandatory)][string]$Token,
        [Parameter(Mandatory)][int64]$TitleId
    )

    $h = $HEADERS.Clone()
    $h['Authorization'] = "Bearer $Token"
    $h['patron-id'] = $PatronId

    Invoke-RestMethod -Uri "$URL_HOOPLA_WS_BASE/v2/titles/$TitleId" -Method Get -Headers $h -UserAgent $USER_AGENT
}

Function Get-HooplaBorrowsRemaining
{
    param(
        [Parameter(Mandatory)][string]$UserId,
        [Parameter(Mandatory)][int64]$PatronId,
        [Parameter(Mandatory)][string]$Token
    )

    $h = $HEADERS.Clone()
    $h['Authorization'] = "Bearer $Token"
    $h['patron-id'] = $PatronId

    Invoke-RestMethod -Uri "$URL_HOOPLA_WS_BASE/users/$UserId/patrons/$PatronId/borrows-remaining" -Method Get -Headers $h -UserAgent $USER_AGENT
}

Function Get-HooplaBorrowedTitles
{
    param(
        [Parameter(Mandatory)][string]$UserId,
        [Parameter(Mandatory)][int64]$PatronId,
        [Parameter(Mandatory)][string]$Token
    )

    $h = $HEADERS.Clone()
    $h['Authorization'] = "Bearer $Token"
    $h['patron-id'] = $PatronId

    Invoke-RestMethod -Uri "$URL_HOOPLA_WS_BASE/users/$UserId/borrowed-titles" -Method Get -Headers $h -UserAgent $USER_AGENT
}

Function Invoke-HooplaBorrow
{
    param(
        [Parameter(Mandatory)][string]$UserId,
        [Parameter(Mandatory)][int64]$PatronId,
        [Parameter(Mandatory)][string]$Token,
        [Parameter(Mandatory)][int64]$TitleId
    )

    $h = $HEADERS.Clone()
    $h['Authorization'] = "Bearer $Token"
    $h['patron-id'] = $PatronId

    Invoke-RestMethod -Uri "$URL_HOOPLA_WS_BASE/users/$UserId/patrons/$PatronId/borrowed-titles/$TitleId" -Method Post -Headers $h -UserAgent $USER_AGENT
}

Function Invoke-HooplaZipDownload
{
    param(
        [Parameter(Mandatory)][int64]$PatronId,
        [Parameter(Mandatory)][string]$Token,
        [Parameter(Mandatory)][int64]$CircId,
        [Parameter(Mandatory)][ValidateScript({Test-Path -LiteralPath $_ -IsValid -PathType Leaf})][string]$OutFile
    )

    $h = $HEADERS.Clone()
    $h['Authorization'] = "Bearer $Token"
    $h['patron-id'] = $PatronId

    $res = Invoke-WebRequest -Uri "$URL_HOOPLA_WS_BASE/patrons/downloads/$CircId/url" -Method Get -Headers $h -UserAgent $USER_AGENT -UseBasicParsing

    if ($PSVersionTable.PSVersion.Major -ge 6)
    {
        Invoke-WebRequest -Uri $res.Headers['Location'][0] -Method Get -UseBasicParsing -OutFile $OutFile
    }
    else
    {
        Invoke-WebRequest -Uri $res.Headers['Location'] -Method Get -UseBasicParsing -OutFile $OutFile
    }
}

Function Get-HooplaKey
{
    param(
        [Parameter(Mandatory)][int64]$PatronId,
        [Parameter(Mandatory)][string]$Token,
        [Parameter(Mandatory)][int64]$CircId
    )

    $h = $HEADERS.Clone()
    $h['Authorization'] = "Bearer $Token"
    $h['patron-id'] = $PatronId

    Invoke-RestMethod -Uri "$URL_HOOPLA_LIC_BASE/downloads/$CircId/key" -Method Get -Headers $h -UserAgent $USER_AGENT
}

Function Get-FileKeyKey
{
    param(
        [Parameter(Mandatory)][int64]$CircId,
        [Parameter(Mandatory)][DateTime]$Due,
        [Parameter(Mandatory)][int64]$PatronId
    )

    $combined = '{0:yyyyMMddHHmmss}:{1}:{2}' -f $Due, $PatronId, $CircId

    [Security.Cryptography.HashAlgorithm]::Create('SHA1').ComputeHash([Text.Encoding]::UTF8.GetBytes($combined)) | Select-Object -First 16
}

Function Decrypt-FileKey
{
    param(
        [Parameter(Mandatory)][byte[]]$FileKeyEnc,
        [Parameter(Mandatory)][byte[]]$FileKeyKey
    )

    $aesManaged = New-Object "System.Security.Cryptography.AesManaged"
    $aesManaged.Mode = [Security.Cryptography.CipherMode]::ECB
    $aesManaged.Padding = [Security.Cryptography.PaddingMode]::PKCS7
    $aesManaged.BlockSize = 128
    $aesManaged.KeySize = 128
    $aesManaged.Key = $FileKeyKey

    $decryptor = $aesManaged.CreateDecryptor();

    $unencryptedData = $decryptor.TransformFinalBlock($FileKeyEnc, 0, $FileKeyEnc.Length);
    $aesManaged.Dispose()

    $unencryptedData
}

Function Decrypt-File
{
    param(
        [Parameter(Mandatory)][byte[]]$FileKey,
        [Parameter(Mandatory)][string]$MediaKey,
        [Parameter(Mandatory)][string]$InputFileName,
        [Parameter(Mandatory)][string]$OutputFileName
    )

    $aesManaged = New-Object "System.Security.Cryptography.AesManaged"
    $aesManaged.Mode = [Security.Cryptography.CipherMode]::CBC
    $aesManaged.Padding = [Security.Cryptography.PaddingMode]::PKCS7
    $aesManaged.BlockSize = 128
    $aesManaged.KeySize = 256
    $aesManaged.Key = $FileKey
    $aesManaged.IV = [Text.Encoding]::UTF8.GetBytes($MediaKey) | Select-Object -First 16


    $fileStreamReader = New-Object -TypeName 'System.IO.FileStream' -ArgumentList $InputFileName, ([IO.FileMode]::Open), ([IO.FileShare]::Read)
    $fileStreamWriter = New-Object -TypeName 'System.IO.FileStream' -ArgumentList $OutputFileName, ([IO.FileMode]::Create)

    $FileStreamReader.Seek(0, [IO.SeekOrigin]::Begin) | Out-Null

    $decryptor = $aesManaged.CreateDecryptor()
    $cryptoStream = New-Object -TypeName 'System.Security.Cryptography.CryptoStream' -ArgumentList $fileStreamWriter, $decryptor, ([Security.Cryptography.CryptoStreamMode]::Write)
    $fileStreamReader.CopyTo($cryptoStream)

    $cryptoStream.FlushFinalBlock()
    $cryptoStream.Close()
    $fileStreamReader.Close()
    $fileStreamWriter.Close()

    $aesManaged.Dispose()
}

Function Test-Mp4
{
    param(
        [Parameter(Mandatory, Position=0)]
        [Alias('LiteralPath')]
        [string]$Path
    )

    $fileStream = New-Object -TypeName 'System.IO.FileStream' -ArgumentList $Path, ([IO.FileMode]::Open), ([IO.FileShare]::Read)
    $fileReader = New-Object -TypeName 'System.IO.BinaryReader' -ArgumentList $fileStream -ErrorAction Stop
    $head = $fileReader.ReadBytes(8)
    
    $fileReader.Dispose()
    $fileStream.Dispose()

    return [Text.Encoding]::ASCII.GetString(($head | Select-Object -Skip 4)) -eq 'ftyp'
}

Function Remove-InvalidFileNameChars
{
    param(
        [Parameter(Mandatory,Position=0,
        ValueFromPipeline=$true,
        ValueFromPipelineByPropertyName=$true)]
      [String]$Name
    )

    $invalidChars = [IO.Path]::GetInvalidFileNameChars() -join ''
    $re = "[{0}]" -f [RegEx]::Escape($invalidChars)
    $Name -replace $re, '_'
}

Function Convert-HooplaDecryptedToEpub
{
    param(
        [Parameter(Mandatory)][string]$InputFolder,
        [Parameter(Mandatory)][string]$OutFolder
    )

    $container = [xml](Get-Content -LiteralPath (Join-Path -Path $InputFolder -ChildPath 'META-INF\container.xml') -Raw)
    $rootFile = $container.container.rootfiles.rootfile | Select-Object -ExpandProperty Full-Path
    $contentFile = (Join-Path -Path $InputFolder -ChildPath $rootFile).Trim()
    $contentRoot = Get-Item -LiteralPath $contentFile | Select-Object -ExpandProperty Directory
    $content = [xml](Get-Content -LiteralPath $contentFile)

    $fileList = $content.package.manifest.item | Select-Object -ExpandProperty href | ForEach-Object -Process { (Join-Path -Path $contentRoot -ChildPath ([Web.HttpUtility]::UrlDecode($_))).Trim() }
    $fileList += $contentFile
    $fileList = $fileList | Sort-Object -Unique

    $title = $content.package.metadata.title | Select-Object -First 1
    if ($title.GetType() -ne [String])
    {
        $title = $content.package.metadata.title | Select-Object -First 1 | Select-Object -ExpandProperty '#text'
    }

    $author = $content.package.metadata.creator | Select-Object -First 1
    if ($author.GetType() -ne [String])
    {
        $author = $content.package.metadata.creator | Select-Object -First 1 | Select-Object -ExpandProperty '#text'
    }
    
    # Usually, content root is a subfolder of the input folder. But sometimes, they are the same. Make sure we declutter the input root if they differ, and always keep the mimetype file.
    $mimeTypeFile = Join-Path -Path $InputFolder -ChildPath 'mimetype'

    $extra = @(Get-ChildItem -LiteralPath $contentRoot -File -Recurse | Where-Object -FilterScript { ($_.FullName -notin $fileList) -and ($_.FullName -ne $mimeTypeFile) })
    $extra += Get-ChildItem -LiteralPath $InputFolder -File | Where-Object -FilterScript { ($_.FullName -notin $fileList) -and ($_.FullName -ne $mimeTypeFile) }

    $extra = $extra | Sort-Object -Property FullName -Unique
    $extra | Remove-Item

    $containerXmlFolder = Join-Path -Path $contentRoot.FullName -ChildPath 'META-INF'
    $containerXmlPath = Join-Path -Path $containerXmlFolder -ChildPath 'container.xml'
    if (!(Test-Path -LiteralPath $containerXmlPath -PathType Leaf))
    {
        New-Item -Path $containerXmlFolder -ItemType Directory -Force | Out-Null
        $xml = @"
<?xml version="1.0"?>
<container version="1.0" xmlns="urn:oasis:names:tc:opendocument:xmlns:container">
   <rootfiles>
      <rootfile full-path="content.opf" media-type="application/oebps-package+xml"/>
   </rootfiles>
</container>

"@
        $xml | Out-File -LiteralPath $containerXmlPath -Encoding ascii
    }

    $finalFile = ('{0} - {1}.epub' -f $title, $author) | Remove-InvalidFileNameChars

    Push-Location
    Set-Location -LiteralPath $InputFolder

    $finalFileFullPath = (Join-Path -Path $OutFolder -ChildPath $finalFile)
    if ($VerbosePreference -eq 'Continue')
    {
        & $EpubZipBin $finalFileFullPath
    }
    else
    {
        & $EpubZipBin $finalFileFullPath >$null 2>&1
    }
    Pop-Location

    Get-Item -LiteralPath $finalFileFullPath
}

Function Convert-HooplaDecryptedToCbz
{
    param(
        [Parameter(Mandatory)][string]$InputFolder,
        [Parameter(Mandatory)][string]$OutFolder,
        [Parameter(Mandatory)][string]$Name
    )

    $fileName = $Name | Remove-InvalidFileNameChars
    $tempOutFile = Join-Path -Path $OutFolder -ChildPath "$fileName.zip"
    $finalOutFile = Join-Path -Path $OutFolder -ChildPath "$fileName.cbz"

    Compress-Archive -Path (
        Get-ChildItem -LiteralPath $InputFolder | Where-Object -FilterScript { $_.Extension -in $COMIC_IMAGE_EXTS } | Select-Object -ExpandProperty FullName
    ) -CompressionLevel Fastest -DestinationPath $tempOutFile

    Rename-Item -LiteralPath $tempOutFile -NewName $finalOutFile

    Get-Item $finalOutFile
}

Function Convert-HooplaDecryptedToM4a
{
    param(
        [Parameter(Mandatory)][string]$InputFolder,
        [Parameter(Mandatory)][string]$OutFolder,
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][string]$Title,
        [Parameter(Mandatory)][string]$Author,
        [Parameter(Mandatory)][int]$Year,
        [string]$Subtitle,
        [object]$ChapterData
        
    )

    if ($Author)
    {
        $baseFileName = ('{0} - {1}' -f $Name, $Author) | Remove-InvalidFileNameChars
    }
    else
    {
        $baseFileName = $Name | Remove-InvalidFileNameChars
    }

    $finalOutFile = Join-Path -Path $OutFolder -ChildPath ('{0}.m4a' -f $baseFileName)
    $inFile = Get-ChildItem -LiteralPath $InputFolder -Filter '*.m3u8' | Select-Object -First 1 | Select-Object -ExpandProperty FullName

    Push-Location
    Set-Location $InputFolder
    $ffArgs = @(
        '-y',
        '-i', $infile,
        '-metadata', ('title="{0}"' -f $Title),
        '-metadata', ('year="{0}"'  -f $Year),
        '-metadata', ('author="{0}"'  -f $Author),
        '-metadata', 'genre="Audiobook"'
    )

    if ($Subtitle)
    {
        $ffArgs += '-metadata', ('subtitle="{0}"' -f $Subtitle)
    }

    $ffArgs += @(
        '-c:a', 'copy',
        $finalOutFile
    )

    if ($VerbosePreference -eq 'Continue')
    {
        & $FfmpegBin @ffArgs
        #& $FfmpegBin -y -i $inFile -metadata "title=`"$Title`"" -metadata "year=`"$Year`"" -metadata "author=`"$Author`"" -metadata "genre=`"Audiobook`"" '-c:a' copy $finalOutFile
    }
    else
    {
        & $FfmpegBin @ffArgs >$null 2>&1
        #& $FfmpegBin -y -i $inFile -metadata "title=`"$Title`"" -metadata "year=`"$Year`"" -metadata "author=`"$Author`"" -metadata "genre=`"Audiobook`"" '-c:a' copy $finalOutFile >$null 2>&1
    }

    if ($ChapterData -and (!$AudioBookForceSingleFile))
    {
        $outDir = New-Item -Path (Join-Path -Path $OutFolder -ChildPath $baseFileName) -ItemType Directory
        $chapterCount = $ChapterData | Select-Object -ExpandProperty chapter | Measure-Object -Maximum | Select-Object -ExpandProperty Maximum
        $ChapterData | ForEach-Object -Process {
            $ffArgs = @(
                '-y',
                '-i', $finalOutFile,
                '-ss', $_.start,
                '-t', $_.duration,
                '-metadata', ('title="{0}"' -f $_.title),
                '-metadata', ('album="{0}"' -f $Title),
                '-metadata', ('year="{0}"'  -f $Year),
                '-metadata', ('author="{0}"'  -f $Author),
                '-metadata', 'genre="Audiobook"'
                '-metadata', ('track={0}/{1}' -f $_.ordinal, $chapterCount)
            )

            if ($Subtitle)
            {
                $ffArgs += '-metadata', ('subtitle="{0}"' -f $Subtitle)
            }

            $ffArgs += @(
                '-c', 'copy',
                (Join-Path -Path $outDir.FullName -ChildPath ('{0} - {1} - {2}.m4a' -f $baseFileName, $_.ordinal, ($_.title | Remove-InvalidFileNameChars)))
            )

            if ($VerbosePreference -eq 'Continue')
            {
                & $FfmpegBin @ffArgs
            }
            else
            {
                & $FfmpegBin @ffArgs >$null 2>&1
            }
        }

        Remove-Item $finalOutFile
        $finalOutFile = $outDir
    }

    Pop-Location

    Get-Item $finalOutFile
}

if (!$Credential)
{
    $ssPassword = ConvertTo-SecureString $Password -AsPlainText -Force
    $Credential = New-Object -TypeName 'System.Management.Automation.PSCredential' -ArgumentList $Username, $ssPassword
}

if ((!$AllBorrowed) -and ($null -eq $TitleId))
{
    Write-Warning 'No -TitleId specified. All currently-borrowed titles will be downloaded.'
    $AllBorrowed = $true
}

$AppExtension = ''
if (($PSVersionTable.PSVersion -lt '6.0') -or $IsWindows)
{
    $AppExtension = '.exe'
}

$cmd = ''
if ($EpubZipBin)
{
    $cmd = Get-Command -Name $EpubZipBin -ErrorAction SilentlyContinue
    if (!$cmd)
    {
        Write-Warning "Epubzip binary specified was not found ($EpubZipBin). Will try to use alternate version if available."
    }
}

if (!$cmd)
{
    $cmd = Get-Command -Name (Join-Path -Path $PSScriptRoot -ChildPath "epubzip$AppExtension") -ErrorAction SilentlyContinue
    if (!$cmd)
    {
        $cmd = Get-Command -Name "epubzip$AppExtension" -ErrorAction SilentlyContinue
        if (!$cmd)
        {
            Write-Warning "Epubzip binary not found ($EpubZipBin). If you are downloading ebooks (rather than comics or audiobooks), you may wish to download the binary from https://github.com/dino-/epub-tools/releases, specify a different path with -EpubZipBin, or specify -KeepDecryptedData so that you can manually pack afterward."
        }
    }

    $EpubZipBin = $cmd.Source
}

Write-Verbose ('Using epubzip bin: "{0}"' -f $EpubZipBin)

$cmd = ''
if ($FfmpegBin)
{
    $cmd = Get-Command -Name $FfmpegBin -ErrorAction SilentlyContinue
    if (!$cmd)
    {
        Write-Warning "FFMpeg binary specified was not found ($FfmpegBin). Will try to use alternate version if available."
    }
}

if (!$cmd)
{
    $cmd = Get-Command -Name (Join-Path -Path $PSScriptRoot -ChildPath "ffmpeg$AppExtension") -ErrorAction SilentlyContinue
    if (!$cmd)
    {
        $cmd = Get-Command -Name "ffmpeg$AppExtension" -ErrorAction SilentlyContinue
        if (!$cmd)
        {
            Write-Warning "FFmpeg binary not found. If you are downloading audiobooks (rather than ebooks or comics), you may wish to download the binary from https://ffmpeg.zeranoe.com/builds/, specify a different path with -FfmpegBin, or specify -KeepDecryptedData so that you can manually convert afterward."
        }
    }

    $FfmpegBin = $cmd.Source
}

Write-Verbose ('Using ffpmeg bin: "{0}"' -f $FfmpegBin)

if (!(Test-Path -LiteralPath $OutputFolder -PathType Container))
{
    Write-Warning "Output folder doesn't exist. Creating."
    New-Item -Path $OutputFolder -ItemType Directory | Out-Null
}

$OutputFolder = Get-Item -LiteralPath $OutputFolder | Select-Object -ExpandProperty $_.FullName

$token = Connect-Hoopla -Credential $Credential
Write-Verbose "Logged in. Received token $($token -replace '\-.*', '-****-****-****-************')"

$users = Get-HooplaUsers $token
Write-Verbose "Found $($users.patrons.Count) patrons"

$userId = $users.id
if (!$PatronId)
{
    if ($users.patrons.Count -eq 0)
    {
        throw "No patrons found on account. Account may not be correctly set up with library."
    }
    elseif ($users.patrons.Count -gt 1)
    {
        Write-Warning (
            "Multiple patrons found on account. Using first one, {0} ({1}). You can specify -PatronId to override" -f $users.patrons[0].id, $users.patrons[0].libraryName
        )
    }

    $PatronId = $users.patrons[0].id
    Write-Verbose "Using PatronId $PatronId"
}

$borrowedRaw = Get-HooplaBorrowedTitles -Token $token -UserId $userId -PatronId $PatronId
$borrowed = $borrowedRaw | Where-Object -FilterScript { $_.kind.id -in $SUPPORTED_KINDS }
Write-Verbose "Found $($borrowed.Count) ($($borrowedRaw.Count)) titles already borrowed"
$toDownload = @()

if ($AllBorrowed)
{
    $toDownload = $borrowed
}
else
{
    $toDownload = $borrowed | Where-Object -FilterScript { $_.id -in $TitleId }

    $allBorrowedTitles = $borrowed | Select-Object -ExpandProperty id
    $toBorrow = $TitleId | Where-Object -FilterScript { $_ -notin $allBorrowedTitles }
    
    if ($toBorrow)
    {
        $borrowsRemainingData = Get-HooplaBorrowsRemaining -UserId $userId -PatronId $PatronId -Token $token
        Write-Host $borrowsRemainingData.borrowsRemainingMessage

        $borrowsRemaining = $borrowsRemainingData.borrowsRemaining

        $toBorrow | ForEach-Object -Process {
            Write-Host "Title $_ is not already borrowed or is not a supported kind. Looking up data about it."
            $titleInfo = Get-HooplaTitleInfo -PatronId $PatronId -Token $token -TitleId $_
            if ($titleInfo.kind.id -in $SUPPORTED_KINDS)
            {
                if ((--$borrowsRemaining) -le 0)
                {
                    Write-Warning "Title $_ ($($titleInfo.Title)) not borrowed already, but we're out of remaining borrows allowed. Skipping..."
                }
                else
                {
                    Write-Host "Borrowing title $_ ($($titleInfo.Title))..."
                    $res = Invoke-HooplaBorrow -UserId $userId -PatronId $PatronId -Token $token -TitleId $titleInfo.id
                    Write-Host "Response: $($res.message)"
                    $newToDownload = $res.titles | Where-Object -FilterScript { $_.id -eq $titleInfo.id }
                    if ($newToDownload)
                    {
                        $toDownload += $newToDownload
                    }
                    else
                    {
                        Write-Warning "Failed to borrow title $_ ($($titleInfo.Title))..."
                    }
                }
            }
            else
            {
                Write-Warning "Title $_ is not a supported kind ($($titleInfo.kind.name)). Skipping..."
            }
        }
    } 
}

$tempFolder = [IO.Path]::GetTempPath()

$now = Get-Date

$toDownload | ForEach-Object -Process {
    $info = $_
    $contentKind = [HooplaKind]$_.kind.id
    if ($_.contents.mediaType)
    {
        $contentKind = [HooplaKind]$_.contents.mediaType
    }
    $contents = $info.contents
    $circId = $contents.circId
    $mediaKey = $contents.mediaKey
    $dueUnix = [Math]::Truncate($info.contents.due / 1000)
    $due = (New-Object DateTime 1970, 1, 1, 0, 0, 0, ([DateTimeKind]::Utc)).AddSeconds($dueUnix)

    $circFileName = (Join-Path -Path $tempFolder -ChildPath "$($circId).zip")

    Invoke-HooplaZipDownload -PatronId $patronId -Token $token -CircId $circId -OutFile $circFileName
    $keyData = Get-HooplaKey -PatronId $patronId -Token $token -CircId $circId

    $fileKeyKey = Get-FileKeyKey -CircId $circId -Due $due -PatronId $patronId
    $fileKey = Decrypt-FileKey -FileKeyEnc ([Convert]::FromBase64String($keyData."$mediaKey")) -FileKeyKey $fileKeyKey

    $encDir = Join-Path -Path $tempFolder -ChildPath ('enc-{0}-{1:yyyyMMddHHmmss}' -f $circId, $now)
    New-Item -Path $encDir -ItemType Directory | Out-Null
    Expand-Archive -LiteralPath $circFileName -DestinationPath $encDir

    Remove-Item -LiteralPath $circFileName

    $decDir = Join-Path -Path $tempFolder -ChildPath ('dec-{0}-{1:yyyyMMddHHmmss}' -f $circId, $now)
    New-Item -Path $decDir -ItemType Directory | Out-Null

    $activity = 'Decrypting Content ({0})' -f $_.title
    Write-Progress -Activity $activity -PercentComplete 0
    $zipFiles = Get-ChildItem $encDir -Recurse -File
    $decDone = 0
    $decTotal = $zipFiles.Count
    $zipFiles | ForEach-Object -Process {
        $outFile = $_.FullName.Replace($encDir, $decDir)
        $outDir = $_.DirectoryName.Replace($encDir, $decDir)
        
        if (!(Test-Path -LiteralPath $outDir))
        {
            New-Item -Path $outDir -ItemType Directory -Force | Out-Null
        }

        if (($contentKind -eq [HooplaKind]::AUDIOBOOK) -and ($_.Extension -eq '.m3u8'))
        {
            $lines = Get-Content -LiteralPath $_.FullName | Where-Object -FilterScript {$_ -notmatch '^#EXT-X-KEY'}
            # Out-File doesn't support utf8 w/o BOM
            [IO.File]::WriteAllLines($outFile, $lines)
            return
        }

        if ($_.Length)
        {
            # Hack. Some ebooks contain audio files that download as unencrypted
            if (($_.Extension -eq '.m4a') -and (Test-Mp4 -LiteralPath $_.FullName))
            {
                Write-Verbose -Message ('Coping unencrypted {0}' -f $_.FullName)
                Copy-Item -LiteralPath $_.FullName -Destination $outFile
            }
            else
            {
                Write-Verbose -Message ('Decrypting {0}' -f $_.FullName)
                Decrypt-File -FileKey $fileKey -MediaKey $mediaKey -InputFileName $_.FullName -OutputFileName $outFile
            }
        }
        else
        {
            Write-Verbose -Message ('Writing empty file {0}' -f $_.FullName)
            '' | Out-File -LiteralPath $outFile
        }

        Write-Progress -Activity $activity -PercentComplete ((++$decDone) / $decTotal * 100)
    }
    Write-Progress -Activity $activity -Completed

    switch ($contentKind)
    {
        ([HooplaKind]::EBOOK) {
            Convert-HooplaDecryptedToEpub -InputFolder $decDir -OutFolder $OutputFolder
        }

        ([HooplaKind]::COMIC) {
            $title = $contents.title
            $subtitle = $contents.subtitle
            $name = $title
            if ($subtitle) {
                $name += ", $subtitle"
            }
            Convert-HooplaDecryptedToCbz -InputFolder $decDir -OutFolder $OutputFolder -Name $name
        }

        ([HooplaKind]::AUDIOBOOK) {
            Convert-HooplaDecryptedToM4a -InputFolder $decDir -OutFolder $OutputFolder -Name $info.title -Title $info.title `
                -Year $info.year -Author $info.artist.name -Subtitle $contents.subtitle -ChapterData $contents.chapters
        }
    }    

    if (!$KeepDecryptedData)
    {
        Remove-Item -LiteralPath $decDir -Recurse
    }
    else
    {
        Write-Host ('Decrypted data for {0} ({1}) stored in {2}' -f $_.id, $_.title, $decDir)
    }

    if (!$KeepEncryptedData)
    {
        Remove-Item -LiteralPath $encDir -Recurse
    }
}

