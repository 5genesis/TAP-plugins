**30/05/2019**

 - Support for using selected column names and formats for the generation of the result's timestamps (InfluxDb result listener).

**21/05/2019**

 - Implemented Experiment ID functionality:
    - Optionally add `Experiment ID` value as tag in InfluxDb result listener.
    - Added test step for setting `Experiment ID` value

**16/05/2019**

 - Implementation of the InflusDb result listener. It's able to send TAP logs and results to an InfluxDB instance.
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
