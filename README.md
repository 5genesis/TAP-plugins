# 5Genesis TAP Plugins

This repository contains the TAP plugins generated for the 5Genesis project. The repository contains a *Main* plugin that is used to package all independent plugins into a single one. 

The *Main* plugin should not contain any code: Define other pugins to provide new functionality, and add references to this plugin to the *Main* one.

The packaged plugins can only depend on the standard TAP plugins, or on other 5Genesis TAP plugins.

## Packaging a new TAP Plugin

In order to create a new TAP plugin and package it under the *Main* plugin:

1. Create a new TAP plugin project as usual.
2. Copy the `File` element from the `package.xml` file of the new project into the `package.xml` file of the *Main* project.
3. Add the new project as a dependency of the *Main* project.
4. Delete the `package.xml` file of the new project.
5. Open the `.csproj` file of the new project, and delete the `UsingTask` and `Target` elements at the bottom.

All new files to package must be included in the `package.xml` file of the *Main* project.

## Included Plugins

### Tap.Plugins.5Genesis.SshInstrument

Provides functionality for sending commands through SSH and transfering files/folders from the remote machine using SCP.

## Authors

* **Bruno Garcia Garcia**

## License

TBD