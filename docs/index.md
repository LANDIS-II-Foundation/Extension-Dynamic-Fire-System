# What is the Dynamic Fire and Fuels Extensions?

The Dynamic Fire System simulates fire ignition, initiation, spread, and damage. Fire ignition and initiation follow Yang et al. 2004. Fire spread rates uses the equations from the Canadian Forest Fire Behavior Prediction System (Forestry Canada Fire Danger Group 1992) and are dependent upon fuel type, weather, and topography. Weather data are input as comma-separated text files containing data compiled directly from weather stations. Actual spread across the landscape follows the elliptical algorithms from Mark Finney, similar to those found in FARSITE. The extension has the flexibility to accommodate fuel types from throughout the world. Mortality from fires generally follows the behavior of previous LANDIS fire extensions.

The Dynamic Fire System extension is used in conjunction with the Biomass Fuels extension. The fuel types should match the fuel types created in the Dynamic Fire System extension. Prescriptions from the Harvest extension and mortality from the BDA extension can optionally cause fuel types to change.

# Release Notes

- Dynamic Fire latest release: Version 4.0, September 2024
- [View User Guide](https://github.com/LANDIS-II-Foundation/Extension-Dynamic-Fire-System/blob/master/docs/LANDIS-II%20Dynamic%20Fire%20System%20v4%20User%20Guide.pdf).
- Dynamic Biomass Fuels latest release: Version 4.0, September 2024
- [View User Guide and Installer](https://github.com/LANDIS-II-Foundation/Extension-Dynamic-Biomass-Fuels/blob/master/docs/index.md).
- Full release details found in the User Guide and on GitHub.
- Copyright The LANDIS-II Foundation

# Requirements

You need:

- The [LANDIS-II model v8.0](http://www.landis-ii.org/install) installed on your computer.
- Extension Installer (see below)
- Example files (see below)

# Download and Install the Extension

- The latest version can be [downloaded from GitHub](https://github.com/LANDIS-II-Foundation/Extension-Dynamic-Fire-System/blob/master/deploy/installer/LANDIS-II-V8%20Dynamic%20Fire%20System%204.0-setup.exe). (Look for the download icon in the upper right corner.)  Launch the installer.

# Example Files

Example files can be [downloaded from GitHub](https://downgit.github.io/#/home?url=https://github.com/LANDIS-II-Foundation/Extension-Dynamic-Fire-System/blob/master/testings/Core8-DynamicFire4.0).

# Citation

 Sturtevant, B.R., R.M. Scheller, B.R. Miranda, D. Shinneman, A.D. Syphard. 2009. Simulating dynamic and mixed-severity fire regimes: A process-based fire extension for LANDIS-II. Ecological Modelling 220: 3380-3393.

# Support

If you have a question, please contact Brian Sturtevant. 
You can also ask for help in the [LANDIS-II users group](http://www.landis-ii.org/users).

If you come across any issue or suspected bug, please post about it in the [issue section of the Github repository](https://github.com/LANDIS-II-Foundation/Extension-Dynamic-Fire-System/issues) (GitHub ID required).

# Author

[The LANDIS-II Foundation](http://www.landis-ii.org)

Mail : brian.r.sturtevant@usda.gov
