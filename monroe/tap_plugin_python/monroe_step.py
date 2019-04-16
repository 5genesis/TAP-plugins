'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Adapted by Mohammad Rajiullah from Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) '''

# MONROE Experiment
from .imports import *
from .instrument import *

@Attribute(Keysight.Tap.DisplayAttribute, "Monroe Experiment", "Monroe Experiment", "Monroe Virtual Node")
@Attribute(Keysight.Tap.AllowAnyChildAttribute)
class MonroeExperiment(TestStep):
	def __init__(self):
		super(MonroeExperiment, self).__init__()
	   
		prop1 = self.AddProperty("script","monroe/ping", String)
		prop1.AddAttribute(Keysight.Tap.DisplayAttribute, "Container to run", "Script", Group="Monroe Configuration", Order=2.1)

		prop2 = self.AddProperty("start", 2, Int32)
		prop2.AddAttribute(Keysight.Tap.DisplayAttribute, "Start", "Start", Group="Monroe Configuration", Order=2.2)

		prop3 = self.AddProperty("storage", 99999999, Int32)
		prop3.AddAttribute(Keysight.Tap.DisplayAttribute, "Storage", "Storage", Group="Monroe Configuration", Order=2.3)

		prop4 = self.AddProperty("interfacename", "eth0", String)
		prop4.AddAttribute(Keysight.Tap.DisplayAttribute, "Interface Name", "Interface Name", Group="Monroe Configuration", Order=2.4)
              
		prop6 = self.AddProperty("interface_without_metadata", "eth0", String)
		prop6.AddAttribute(Keysight.Tap.DisplayAttribute, "Interface without metadata", "Interface without metadata", Group="Monroe Configuration", Order=2.5)

		prop5 = self.AddProperty("option","{\"server\":\"8.8.8.8\"}", String)
		prop5.AddAttribute(Keysight.Tap.DisplayAttribute, "Optional parameters", "Options passed to the experiment (json string)", Group="Monroe Configuration", Order=2.6)
		
		prop7 = self.AddProperty("duration", 30, Int32)
		prop7.AddAttribute(Keysight.Tap.DisplayAttribute, "Duration of the experiment (s)", "Duration of the experiment", Group="Monroe Configuration", Order=2.7)
		prop7.AddAttribute(Keysight.Tap.UnitAttribute, "s")

		prop8 = self.AddProperty("agent_port", 8080, Int32)
		prop8.AddAttribute(Keysight.Tap.DisplayAttribute, "Agent Port", "Agent Port", Group="TAP Agent", Order=4)

		prop9 = self.AddProperty("enable_manual_config", False, Boolean)
		prop9.AddAttribute(Keysight.Tap.DisplayAttribute, "Enable manual VM configuration", "Enable manual VM configuration", Group="Monroe VM Configuration", Order=3.1)

		prop10 = self.AddProperty("monroe_vm_ip", "127.0.0.1", String)
		prop10.AddAttribute(Keysight.Tap.DisplayAttribute, "Monroe VM IP Address", "Monroe VM IP Address", Group="Monroe VM Configuration", Order=3.2)
		prop10.AddAttribute(Keysight.Tap.EnabledIfAttribute, "enable_manual_config", HideIfDisabled=True)

		self.AddProperty("Instrument", None, Instrument).AddAttribute(Keysight.Tap.DisplayAttribute, "Instrument", "Instrument", Group="Instrument", Order=1)
		
	
	def PrePlanRun(self,):
		super(MonroeExperiment, self).PrePlanRun()
		self.Info('MonroeExperiment: Preliminary check started.')

		status = False   # error status
		
		if not self.script:
			self.Error('Script field is not present.')
			status = True
		
		if not self.start:
			self.Error('Start field is not present.')
			status = True
		
		if not self.storage:
			self.Error('Storage field is not present.')
			status = True

		if not self.interfacename:
			self.Error('Interface name is not present.')
			status = True

		if not self.interface_without_metadata:
			self.Error('Interface without metadata field is not present.')
			status = True

		if not self.option:
			self.Error('Option field is not present.')
			status = True

		if self.option and not self.is_json(self.option):
			self.Error('Option field string not in JSON format.')
			status = True

		if not self.duration:
			self.Error('Duration field is not present.')
			status = True

		if self.enable_manual_config and not self.monroe_vm_ip:
			self.Error('Monroe VM IP is not present.')
			status = True
		elif not self.enable_manual_config:
			self.Warning('Manual Monroe VM IP Address configuration is not enabled. OpenBaton is expected to be in use.')

		if not self.agent_port:
			self.Error('Agent port is not present.')
			status = True

		if status:
			self.UpgradeVerdict(Keysight.Tap.Verdict.Error)
			sys.exit("Error has occurred. Exiting the program.")
		else:
			self.Info('MonroeExperiment: Preliminary check completed.')


	def Run(self):
		configDict = {}
		configDict["script"] = self.script
		configDict["start"] = str(self.start)
		configDict["storage"] = str(self.storage)
		configDict["interfacename"] = self.interfacename
		configDict["interface_without_metadata"] = [self.interface_without_metadata]

		json_object = json.loads(self.option)
		for item in json_object:
			configDict[item]=json_object[item]

                        
		self.config = configDict

		#self.Info("The experiment configuration is {0}", self.config)
		
		super(MonroeExperiment, self).Run()
		request_monroe_config = {
			"config": self.config, 
			"duration": self.duration,
			"agent_port": self.agent_port,
			"enable_manual_config": self.enable_manual_config,
			"monroe_vm_ip": self.monroe_vm_ip
		}

		# Send the experiment request
		response = self.Instrument.send_request(request_monroe_config)

		if response['status'] == RESPONSE_STATUS.OK:
			self.Info('Monroe experiment is successful.')
			self.Info("{0}", response['result']['str'])
			self.UpgradeVerdict(Keysight.Tap.Verdict.Pass)
		else:
			self.Error('Monroe experiment is unsuccessful.')
			self.Error("{0}", response['result']['str'])
			self.UpgradeVerdict(Keysight.Tap.Verdict.Error)


	def is_json(self, myjson):
		try:
			json_object = json.loads(myjson)
		except ValueError, e:
			return False
		
		return True