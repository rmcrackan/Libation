; Libation Windows installer (Inno Setup 6.x)
;
; Compile with ISCC and /D defines. See Scripts/Windows/README.md and
; Build-WindowsInstaller.ps1 for local builds; CI will pass the same defines.
;
; Required /D defines from build script:
;   MyVersion, MyReleaseName, MyArchitecture, MySetupIcon,
;   OutputBaseFilename, SourceDir, OutputDir, ArchitecturesAllowed

#ifndef MyVersion
  #define MyVersion "0.0.0-dev"
#endif
#ifndef MyReleaseName
  #define MyReleaseName "chardonnay"
#endif
#ifndef MyArchitecture
  #define MyArchitecture "x64"
#endif
#ifndef MySetupIcon
  #define MySetupIcon "libation.ico"
#endif
#ifndef OutputBaseFilename
  #define OutputBaseFilename "Libation.0.0.0-dev-windows-chardonnay-x64-setup"
#endif
#ifndef SourceDir
  #define SourceDir "."
#endif
#ifndef OutputDir
  #define OutputDir "."
#endif
#ifndef ArchitecturesAllowed
  #define ArchitecturesAllowed "x64compatible"
#endif

#if MyReleaseName == 'classic'
  #define InstallFolderName "Libation-Classic"
  #define AppName "Libation (Classic)"
#else
  #define InstallFolderName "Libation"
  #define AppName "Libation (Chardonnay)"
#endif

[Setup]
#if MyReleaseName == 'classic'
AppId={{B1C2D3E4-F5A6-4B7C-8D9E-0F1A2B3C4D5E}}
#elif MyArchitecture == 'arm64'
AppId={{C2D3E4F5-A6B7-4C8D-9E0F-1A2B3C4D5E6F}}
#else
AppId={{A7B8C9D0-E1F2-4A3B-8C9D-0E1F2A3B4C5D}}
#endif
AppName={#AppName}
AppVersion={#MyVersion}
AppVerName={#AppName} {#MyVersion}
AppPublisher=Libation
AppPublisherURL=https://getlibation.com/
AppSupportURL=https://github.com/rmcrackan/Libation/issues
AppUpdatesURL=https://github.com/rmcrackan/Libation/releases
DefaultDirName={localappdata}\{#InstallFolderName}
DefaultGroupName={#AppName}
PrivilegesRequired=lowest
OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFilename}
SourceDir={#SourceDir}
ArchitecturesAllowed={#ArchitecturesAllowed}
ArchitecturesInstallIn64BitMode=x64compatible
DisableProgramGroupPage=no
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
SetupIconFile={#MySetupIcon}
UninstallDisplayIcon={app}\Libation.exe
UninstallDisplayName={#AppName}
VersionInfoVersion={#MyVersion}
VersionInfoProductName={#AppName}
VersionInfoCompany=Libation

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Libation"; Filename: "{app}\Libation.exe"; Comment: "Libation audiobook manager"
Name: "{group}\Hangover"; Filename: "{app}\Hangover.exe"; Comment: "Hangover license viewer"
Name: "{group}\Libation CLI"; Filename: "{app}\LibationCli.exe"; Comment: "Libation command-line tools"
Name: "{autodesktop}\Libation"; Filename: "{app}\Libation.exe"; Tasks: desktopicon; Comment: "Libation audiobook manager"

[Run]
Filename: "{app}\Libation.exe"; Description: "{cm:LaunchProgram,{#StringChange(AppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent