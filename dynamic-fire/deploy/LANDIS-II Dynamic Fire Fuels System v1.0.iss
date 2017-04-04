#define PackageName      "Dynamic Fire Fuel System"
#define PackageNameLong  "Dynamic Fire Fuel System"
#define Version          "1.0"
#define ReleaseType      "official"
#define ReleaseNumber    "1"

#define CoreVersion      "5.1"
#define CoreReleaseAbbr  ""

#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Setup section).iss"

;#include "..\package (Setup section).iss"


[Files]

; Dynamic Fire Fuel System v1.0 plug-in and auxiliary libs (Troschuetz Random)
Source: {#LandisBuildDir}\disturbanceextensions\dynamic-fire\build\release\Landis.Extension.DynamicFire.dll; DestDir: {app}\bin; Flags: replacesameversion
Source: {#LandisBuildDir}\disturbanceextensions\dynamic-fire\Troschuetz.Random.dll; DestDir: {app}\bin; Flags: replacesameversion
Source: {#LandisBuildDir}\disturbanceextensions\dynamic-fuels\build\release\Landis.Extension.DynFuelSystem.dll; DestDir: {app}\bin; Flags: replacesameversion

Source: docs\LANDIS-II Dynamic Fire System v1.0 User Guide.pdf; DestDir: {app}\docs
Source: docs\LANDIS-II Dynamic Fuel System v1.0 User Guide.pdf; DestDir: {app}\docs

#define DynFireSys "Dynamic Fire System 1.0.txt"
Source: {#DynFireSys}; DestDir: {#LandisPlugInDir}

#define DynFuelSys "Dynamic Fuel System 1.0.txt"
Source: {#DynFuelSys}; DestDir: {#LandisPlugInDir}

; All the example input-files for the in examples\dynamic-fire-fuel-system
Source: {#LandisBuildDir}\disturbanceextensions\dynamic-fire\deploy\examples\*; DestDir: {app}\examples\dynamic-fire-fuel-system; Flags: recursesubdirs

[Run]
;; Run plug-in admin tool to add entries for each plug-in
#define PlugInAdminTool  CoreBinDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""Dynamic Fire System"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#DynFireSys}"" "; WorkingDir: {#LandisPlugInDir}

Filename: {#PlugInAdminTool}; Parameters: "remove ""Dynamic Fuel System"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#DynFuelSys}"" "; WorkingDir: {#LandisPlugInDir}

[UninstallRun]
;; Run plug-in admin tool to remove entries for each plug-in

[Code]
#include AddBackslash(LandisDeployDir) + "package (Code section) v3.iss"

//-----------------------------------------------------------------------------

//function CurrentVersion_PostUninstall(currentVersion: TInstalledVersion): Integer;
//begin
  // Remove the plug-in name from database
//  if StartsWith(currentVersion.Version, '1.0') then
//    begin
//      Exec('{#PlugInAdminTool}', 'remove "Dynamic Fire System"',
//           ExtractFilePath('{#PlugInAdminTool}'),
//		   SW_HIDE, ewWaitUntilTerminated, Result);
//      Exec('{#PlugInAdminTool}', 'remove "Dynamic Fuel System"',
//           ExtractFilePath('{#PlugInAdminTool}'),
//		   SW_HIDE, ewWaitUntilTerminated, Result);
//	end
//  else
//    Result := 0;
//end;

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  //CurrVers_PostUninstall := @CurrentVersion_PostUninstall
  Result := True
end;
