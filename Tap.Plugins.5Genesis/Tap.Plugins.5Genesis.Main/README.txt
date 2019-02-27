5Genesis Main TAP plugin
========================

This is an empty TAP plugin that is used package all the existing plugins into a single one.

Notes
-----

This plugin should not contain any code. Use other plugins to provide new functionality, and package them under this one.

The packaged plugins can only depend on the standard TAP plugins, or on other 5Genesis TAP plugins.

Packaging a new TAP plugin
--------------------------

To create a new TAP plugin and package it under the Main plugin:

1. Create a new TAP plugin project as usual
2. Copy the File element from the package.xml file of the new project into the package.xml file of the Main project
3. Add the new project as a dependency of the Main project
4. Delete the package.xml file of the new project
5. Open the .csproj file of the new project, and delete UsingTask and Target elements at the bottom

All new files to package must be included in the package.xml file of the Main project.
