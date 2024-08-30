# AutoCAD Loader

AutoCAD Loader is a native desktop application which is used to configure AutoCAD to company standards.

## Installer requirements

### Registry

#### Registry hive location:

HKEY_LOCAL_MACHINE\SOFTWARE\Arcadis\AutoCAD Loader

#### Registry keys:

| Key Name               | Type   | Default Data                                                                                                        | Description                                                                  |
| ---------------------- | ------ | ------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- |
| DirectoryAccessTimeout | DWORD  | 20                                                                                                                  | Number of seconds to try accessing a directory before timing out.            |
| LocationsCentral       | REG_SZ | I:\_TechSTND_Arcadis\AutoCAD;\\anf-scus-03ce.arcadis-nl.local\proddata501\AIBI_TECHSTND_NA_TechSTND_Arcadis\AutoCAD | Semicolon delimited list of central directory paths, in order of preference. |
| UrlApiOffices          | REG_SZ | https://dt.arcadis.com/Api/officeapi/DriveInfo                                                                      | URL for the offices data API.                                                |
| Version                | REG_SZ | [Populated by installer]                                                                                            | Version number of the AutoCAD Loader                                         |

### External files

| Directory path                                | File                              | Description                                                                                                               |
| --------------------------------------------- | --------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| %PROGRAMDATA%\AutoCAD Loader\Cache\Fonts      | [Standards snapshot - Fonts]      | A snapshot of the current standards Fonts directory.                                                                      |
| %PROGRAMDATA%\AutoCAD Loader\Cache\Packages   | [Standards snapshot - Packages]   | A snapshot of the current standards Packages directory.                                                                   |
| %PROGRAMDATA%\AutoCAD Loader\Cache\Pats       | [Standards snapshot - Pats]       | A snapshot of the current standards Pats directory.                                                                       |
| %PROGRAMDATA%\AutoCAD Loader\Cache\PlotStyles | [Standards snapshot - PlotStyles] | A snapshot of the current standards PlotStyles directory.                                                                 |
| %PROGRAMDATA%\AutoCAD Loader\Cache\Plotters   | [Standards snapshot - Plotters]   | A snapshot of the current standards Plotters directory.                                                                   |
| %PROGRAMDATA%\Arcadis\AutoCAD Loader\Settings | offices.json                      | JSON formatted list of Azure directory data for offices. (will be overridden by data from DFS, then server API).          |
| %PROGRAMDATA%\Arcadis\AutoCAD Loader\Settings | Packages.xml                      | XML formatted list of bundle definitions.                                                                                 |
| %PROGRAMDATA%\Arcadis\AutoCAD Loader\Settings | Plotter.txt                       | Template for registry injection into profile. (will be overridden by data from DFS).                                      |
| %PROGRAMDATA%\Arcadis\AutoCAD Loader\Settings | VersionInfo.xml                   | XML formatted list of AutoCAD/Civil 3D application and plugin definitions (will be overridden by configuration from DFS). |

## Command parameters

AutoCAD Loader can be launched with the following command parameters:

### -log

Records information level entries to the Event Viewer log.
