**29/10/2019** [Version 1.2.1]

 - Migrated to OpenTAP
 - Fix exception on 'Set Execution Metadata' step

**11/09/2019** [Version 1.1]

 - Add plugin, step for year 1 review MCS demo
 - Add 'Set Execution Metadata' step
 - Rename ExperimentId to ExecutionId

**29/08/2019** [Version 1.0.0]

 - Bugfix, changed version nomenclature

**30/07/2019** [Version 300715]

 - Do not send null values to InfluxDb

**15/07/2019** [Version 190715]

 - Added Multi-CSV result listener.
 - Avoid sending measurement names with spaces (or symbols) to InfluxDb, added iteration metadata.

**27/06/2019** [Version 190627]

 - Implementation of the iPerfAgent plugin (code for the agent can be found in the [malaga-platform](https://gitlab.fokus.fraunhofer.de/5genesis/malaga-platform) repository).

**30/05/2019**

 - Support for using selected column names and formats for the generation of the result's timestamps (InfluxDb result listener).

**21/05/2019**

 - Implemented Experiment ID functionality:
    - Optionally add `Experiment ID` value as tag in InfluxDb result listener.
    - Added test step for setting `Experiment ID` value

**16/05/2019**

 - Implementation of the InfluxDb result listener. It's able to send TAP logs and results to an InfluxDB instance.
 - MONROE and Prometheus results are saved in their correct types if possible, instead of always string.
 - Prometheus results can be retrieved using relative times.

**10/05/2019**

 - Implementation of Prometheus HTTP API plugin.
 - MONROE Plugin options serialization fix.

**08/05/2019**

 - Updated MONROE TAP plugin for interfacing with the new TAP agent. 

**24/04/2019**

Initial merge from UMA repository:
 - SSH plugin
 - Initial MONROE plugin
