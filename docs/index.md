# What is the Dynamic Fire and Fuels Extensions?

The Dynamic Fire System simulates fire ignition, initiation, spread, and damage. Fire ignition and initiation follow Yang et al. 2004. Fire spread rates uses the equations from the Canadian Forest Fire Behavior Prediction System (FBPS; Forestry Canada Fire Danger Group 1992) and are dependent upon fuel type, weather, and topography. Weather data are input as comma-separated text files containing data compiled directly from weather stations. Actual spread across the landscape follows the elliptical algorithms from Mark Finney, similar to those found in FARSITE. The extension has the flexibility to accommodate fuel types from throughout the world. Mortality from fires generally follows the behavior of previous LANDIS fire extensions.

The Dynamic Fire System extension must be used in conjunction with a fuel extension. The Dynamic Fuels extension uses cohort species and age-ranges to determine a fuel type for each cell on the landscape. The fuel types should match the fuel types created in the Dynamic Fire System extension. Prescriptions from the Base Harvest extension and mortality from the BDA extension can optionally cause fuel types to change.

An alternative fuels extension that classifies fuel types based on species biomass is now available. See the [Dynamic Biomass Fuel System](LINK HERE) extension.

# Release Notes

- Dynamic Fire latest release: Version 3.0 — September 2018
- [View User Guide](https://github.com/LANDIS-II-Foundation/Extension-Dynamic-Fire-System/blob/master/docs/LANDIS-II%20Dynamic%20Fire%20System%20v3.0%20User%20Guide.pdf).

- Dynamic Fuels latest release: Version 3.0 - September 2018
- [View User Guide](https://github.com/LANDIS-II-Foundation/Extension-Dynamic-Fuel-System/blob/master/docs/LANDIS-II%20Dynamic%20Fuel%20System%20v3.0%20User%20Guide.pdf).

- Full release details found in the User Guide and on GitHub.

# Requirements

To use Dynamic Fire and Fuels, you need:

- The [LANDIS-II model v7.0](http://www.landis-ii.org/install) installed on your computer.
- Example files (see below)

# Download

- The latest version of Dynamic Fire can be [downloaded from GitHub](https://github.com/LANDIS-II-Foundation/Extension-Dynamic-Fire-System/blob/master/deploy/installer/LANDIS-II-V7%20Dynamic%20Fire%20System%203.0-setup.exe). To install it on your computer, launch the installer.
- The latest version of Dynamic Fuels can be [downloaded from GitHub](https://github.com/LANDIS-II-Foundation/Extension-Dynamic-Fuel-System/blob/master/deploy/installer/LANDIS-II-V7%20Dynamic%20Fuel%20System%203.0-setup.exe). To install it on your computer, launch the installer.

# Example Files

LANDIS-II requires a global parameter file for your scenario, and separate parameter files for each extension.

Example files can be [downloaded from GitHub](https://github.com/LANDIS-II-Foundation/Extension-Dynamic-Fire-System/blob/master/testings/version-tests/Core7-DynamicFire3.0/dynamic-fire-example.zip).

# Citation

 Sturtevant, B.R., R.M. Scheller, B.R. Miranda, D. Shinneman, A.D. Syphard. 2009. Simulating dynamic and mixed-severity fire regimes: A process-based fire extension for LANDIS-II. Ecological Modelling 220: 3380-3393.

# Support

If you have a question, please contact Brian Sturtevant. 
You can also ask for help in the [LANDIS-II users group](http://www.landis-ii.org/users).

If you come across any issue or suspected bug, please post about it in the [issue section of the Github repository](https://github.com/LANDIS-II-Foundation/Extension-Dynamic-Fire-System/issues) (GitHub ID required).

# Author

[The LANDIS-II Foundation](http://www.landis-ii.org)

Mail : brian.r.sturtevant@usda.gov
