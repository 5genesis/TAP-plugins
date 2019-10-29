# 5Genesis TAP Plugins

This repository contains the TAP plugins generated for the 5Genesis project. The repository contains a *Main* plugin that is used to package all independent plugins into a single one. 

The *Main* plugin should not contain any code: Define other pugins to provide new functionality, and add references to this plugin to the *Main* one.

The packaged plugins can only depend on the standard TAP plugins, or on other 5Genesis TAP plugins.

## Requirements

 - [OpenTAP](https://www.opentap.io/) (Tested on TAP 9.4)
 - [Visual Studio 2015 or 2017](https://visualstudio.microsoft.com/) (For development)
 
## Packaging a new TAP Plugin

In order to create a new TAP plugin and package it under the *Main* plugin:

1. Create a new TAP plugin project as usual.
2. Copy the `File` element from the `package.xml` file of the new project into the `package.xml` file of the *Main* project.
3. Add the new project as a dependency of the *Main* project.
4. Delete the `package.xml` file of the new project.
5. Open the `.csproj` file of the new project, and delete the `UsingTask` and `Target` elements at the bottom.

All new files to package must be included in the `package.xml` file of the *Main* project.

## Included Plugins

### Tap.Plugins.5Genesis.InfluxDB

Provides a new result listener that is able to send all generated TAP results to an InfluxDB instance. 
All columns from the original results are sent as `fields`, while some information about the host machine and TAP instance are sent as `tags`.
The result listener is also able to send the generated TAP logs to InfluxDB, with a format compatible with [Chronograf](https://www.influxdata.com/time-series-platform/chronograf/)'s log viewer.  
It's also compatible with extra metadata (`ExecutionId` and `_iteration_`).

##### Timestamp format
The result listener expects to find a column named `Timestamp` (case ignored) in order to know the timestamp that corresponds to each row.

The format of this column can be the number of seconds since the Epoch as a floating point value, or the number of milliseconds as an integer.

If necessary, it's possible to define custom timestamp parsers for selected results by filling the `Datetime overrides` table (in the result listener settings). 
The result listener will then look for matching column names in the results and parse them using the defined format strings.
> Important: In this case the result listener assumes that the time is written in the local timezone instead of UTC.

##### Requirements

 - [InfluxDb](https://www.influxdata.com/) (Tested on version 1.7.6)

### Tap.Plugins.5Genesis.iPerfAgent

Provides an instrument an step for controlling an iPerf Agent installed on a remote machine.

##### Requirements

 - [iPerfAgent](https://gitlab.fokus.fraunhofer.de/5genesis/malaga-platform) version 1.0.2

### Tap.Plugins.5Genesis.Misc

 - Provides helper functionality for other plugins.
 - Multi-CSV result listener: This result-listener will create a separate CSV file for each of the generated result kinds, instead of a single, monolithic file as on the original TAP CSV result listener.
 It's also compatible with extra metadata (`ExecutionId` and `_iteration_`).

### Tap.Plugins.5Genesis.Monroe

Provides instruments and steps for handling a MONROE instance. Includes steps for deploying (and starting), stopping and retrieving results from experiments. Results will be published as TAP results.
> The current result parser is only prepared for working with `monroe/ping` experiments. 

##### Requirements

 - [MONROE-experiment-core](https://github.com/MONROE-PROJECT/monroe-experiment-core)

### Tap.Plugins.5Genesis.Prometheus

Provides an instrument and a step for retrieving results from a Prometheus instance. The step can be configured for performing any query using PromQL, and time range can be specified either as an absolute start/end or relative to the current time.

##### Requirements

 - [Prometheus](https://prometheus.io/)

### Tap.Plugins.5Genesis.SshInstrument

Provides functionality for sending commands through SSH and transferring files/folders from the remote machine using SCP. Usage samples can be found at `(TAPPath)/5Genesis/Samples/SSH_Sample.TapPlan`.

### Tap.Plugins.5Genesis.Y1Demo

Specific functionality for the year 1 review demo(s). Not expected to be used in general, logic included will probably be moved to other plugins and further refined in the future.

## Authors

* **Bruno Garcia Garcia**

## License

TBD