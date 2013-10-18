#define PackageName      "Dynamic Fire Fuel System"
#define PackageNameLong  "Dynamic Fire Fuel System"
#define Version          "3.0"
#define ReleaseType      "beta"
#define ReleaseNumber    "1"

#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""

#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Setup section) v6.0.iss"


[Files]

; Dynamic Fire Fuel System v3.0 plug-in
Source: C:\Program Files\LANDIS-II\v6\bin\extensions\Landis.Extension.DynamicFire.dll; DestDir: {app}\bin; Flags: replacesameversion
Source: C:\Program Files\LANDIS-II\v6\bin\extensions\Landis.Extension.DynamicFuels.dll; DestDir: {app}\bin; Flags: replacesameversion

Source: docs\LANDIS-II Dynamic Fire System v2.0.1 User Guide.pdf; DestDir: {app}\docs
Source: docs\LANDIS-II Dynamic Fuel System v2.0 User Guide.pdf; DestDir: {app}\docs

#define DynFireSys "Dynamic Fire System 3.0.txt"
Source: {#DynFireSys}; DestDir: {#LandisPlugInDir}

#define DynFuelSys "Dynamic Fuel System 2.0.txt"
Source: {#DynFuelSys}; DestDir: {#LandisPlugInDir}

; All the example input-files for the in examples\dynamic-fire-fuel-system
Source: examples\*; DestDir: {app}\examples\dynamic-fire-fuel-system; Flags: recursesubdirs

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
#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Code section) v3.iss"

//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  //CurrVers_PostUninstall := @CurrentVersion_PostUninstall
  Result := True
end;
