#define PackageName      "Dynamic Fire Fuel System"
#define PackageNameLong  "Dynamic Fire Fuel System"
#define Version          "2.0.5"
#define ReleaseType      "official"
#define ReleaseNumber    "3"

#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""

#include "C:\BRM\LANDIS_II\code\deploy\package (Setup section) v6.0.iss"
#define ExtDir "C:\Program Files\LANDIS-II\v6\bin\extensions"
#define AppDir "C:\Program Files\LANDIS-II\v6\"

[Files]

; Dynamic Fire Fuel System v1.0 plug-in
Source: {#ExtDir}\Landis.Extension.DynamicFire.dll; DestDir: {#ExtDir}; Flags: replacesameversion
Source: {#ExtDir}\Landis.Extension.DynamicFuels.dll; DestDir: {#ExtDir}; Flags: replacesameversion

Source: docs\LANDIS-II Dynamic Fire System v2.0.3 User Guide.pdf; DestDir: {#AppDir}\docs
Source: docs\LANDIS-II Dynamic Fuel System v2.0.1 User Guide.pdf; DestDir: {#AppDir}\docs

#define DynFireSys "Dynamic Fire System 2.0.txt"
Source: {#DynFireSys}; DestDir: {#LandisPlugInDir}

#define DynFuelSys "Dynamic Fuel System 2.0.txt"
Source: {#DynFuelSys}; DestDir: {#LandisPlugInDir}

; All the example input-files for the in examples\dynamic-fire-fuel-system
Source: examples\*.txt; DestDir: {#AppDir}\examples\dynamic-fire-fuel-system
Source: examples\ecoregions.gis; DestDir: {#AppDir}\examples\dynamic-fire-fuel-system
Source: examples\slope.gis; DestDir: {#AppDir}\examples\dynamic-fire-fuel-system
Source: examples\upslope_azi.gis; DestDir: {#AppDir}\examples\dynamic-fire-fuel-system
Source: examples\fire-ecoregion.gis; DestDir: {#AppDir}\examples\dynamic-fire-fuel-system
Source: examples\initial-communities.gis; DestDir: {#AppDir}\examples\dynamic-fire-fuel-system
Source: examples\*.bat; DestDir: {#AppDir}\examples\dynamic-fire-fuel-system
Source: examples\Fire_Weather_Data.csv; DestDir: {#AppDir}\examples\dynamic-fire-fuel-system

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
#include "C:\BRM\LANDIS_II\code\deploy\package (Code section) v3.iss"

//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  //CurrVers_PostUninstall := @CurrentVersion_PostUninstall
  Result := True
end;
