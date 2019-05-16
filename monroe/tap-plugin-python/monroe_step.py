'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) '''

__author__ = 'rsh'
__version__ = '0.0.1'

# MONROE Experiment
from .imports import *
from netaddr import valid_ipv4
from .monroe_methods import MONROE

# install package: netaddr -> pip install netaddr

@Attribute(Keysight.Tap.DisplayAttribute, "Monroe Experiment", "Monroe Experiment", "Monroe Virtual Node")
@Attribute(Keysight.Tap.AllowAnyChildAttribute)
class MonroeExperiment(TestStep):
	def __init__(self):
		super(MonroeExperiment, self).__init__()
		# Monroe Configuration Settings
		self.monroe_containers = ["monroe/ping", "monroe/iperf:virt"]
		monroe_containers_dot_net = Array[str](self.monroe_containers)
		self.AddProperty("containers_hidden", monroe_containers_dot_net, Array).AddAttribute(BrowsableAttribute, False) # Hide the property

		script_prop = self.AddProperty("script", self.monroe_containers[0], String)
		script_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "Container to run", "Container to run", Group="Monroe Configuration", Order=2.1)
		script_prop.AddAttribute(Keysight.Tap.AvailableValuesAttribute, "containers_hidden")

		start_prop = self.AddProperty("start", 2, Int32)
		start_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "Start", "Start", Group="Monroe Configuration", Order=2.2)

		storage_prop = self.AddProperty("storage", 99999999, Int32)
		storage_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "Storage", "Storage", Group="Monroe Configuration", Order=2.3)

		intfname_prop = self.AddProperty("intfname", "eth0", String)
		intfname_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "Interface Name", "Interface Name", Group="Monroe Configuration", Order=2.4)
		
		intf_without_metadata_prop = self.AddProperty("interface_without_metadata", "eth0", String)
		intf_without_metadata_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "Interface without metadata", "Interface without metadata", Group="Monroe Configuration", Order=2.5)		

		duration_prop = self.AddProperty("duration", 30, Int32)
		duration_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "Duration of experiment", "Duration of experiment", Group="Monroe Configuration", Order=2.6)		
		duration_prop.AddAttribute(Keysight.Tap.UnitAttribute, "s")

		self.iperfversion_options = ["3", "2"]
		monroe_iperfversions_dot_net = Array[str](self.iperfversion_options)
		self.AddProperty("iperfver_options_hidden", monroe_iperfversions_dot_net, Array).AddAttribute(BrowsableAttribute, False)

		iperfv_prop = self.AddProperty("iperfv", self.iperfversion_options[0], String)
		iperfv_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "IPerf version", "IPerf version", Group="Monroe Configuration", Order=2.8)
		iperfv_prop.AddAttribute(Keysight.Tap.AvailableValuesAttribute, "iperfver_options_hidden")		
		iperfv_prop.AddAttribute(Keysight.Tap.EnabledIfAttribute, "script", self.monroe_containers[1], HideIfDisabled=True)

		self.iperfproto_options = ["TCP", "UDP"]
		monroe_iperfproto_dot_net = Array[str](self.iperfproto_options)
		self.AddProperty("iperfproto_options_hidden", monroe_iperfproto_dot_net, Array).AddAttribute(BrowsableAttribute, False)

		iperf_proto_prop = self.AddProperty("iperf_proto", self.iperfproto_options[0], String)
		iperf_proto_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "IPerf protocol", "IPerf protocol", Group="Monroe Configuration", Order=2.9)
		iperf_proto_prop.AddAttribute(Keysight.Tap.AvailableValuesAttribute, "iperfproto_options_hidden")
		iperf_proto_prop.AddAttribute(Keysight.Tap.EnabledIfAttribute, "script", self.monroe_containers[1], HideIfDisabled=True)

		# VNF configurations
		server_prop = self.AddProperty("server", None, String)
		server_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "Server ID or IP", "Provide Server unique ID or IP", Group="VNF Configuration", Order=3.1)

		client_prop = self.AddProperty("client", None, String)
		client_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "Client ID or IP", "Provide Client unique ID or IP", Group="VNF Configuration", Order=3.2)

		# TAP Agent Port
		agent_port_prop = self.AddProperty("agent_port", 8080, Int32)
		agent_port_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "Agent Port", "Agent Port", Group="TAP Agent", Order=4)
		
	
	def PrePlanRun(self,):
		super(MonroeExperiment, self).PrePlanRun()
		self.Info("MonroeExperiment: Preliminary check started.")

		status = False # error status

		if not self.script:
			self.Error("Script field is not present.")
			status = True

		if not self.start:
			self.Error("Start field is not present.")
			status = True

		if not self.storage:
			self.Error("Storage field is not present.")
			status = True

		if not self.intfname:
			self.Error("Interface name is not present.")
			status = True

		if not self.interface_without_metadata:
			self.Error("Interface without metadata field is not present.")
			status = True

		if not self.duration:
			self.Error("Duration field is not present.")
			status = True

		'''if not self.iperfv:
			self.Error("Iperf Version is not present.")
			status = True

		if not self.iperf_proto:
			self.Error("Iperf protocol is not present.")
			status = True

		if status:
			self.UpgradeVerdict(Keysight.Tap.Verdict.Error)
			sys.exit("Error has occurred. Exiting the program.")
		else:
			self.Info("MonroeExperiment: Preliminary check completed.") '''


	def Run(self):
		super(MonroeExperiment, self).Run()

		monroe_config = {
			"script": self.script,
			"start": self.start,
			"storage": self.storage,
			"interfacename": self.intfname,
			"interface_without_metadata": self.interface_without_metadata,
			"server": self.server
		}

		if self.script == self.monroe_containers[1]:
			monroe_config["iperfversion"] = self.iperfv
			monroe_config["protocol"] = self.iperf_proto

		client_ip_status = self.check_if_ip(self.client)
		server_ip_status = self.check_if_ip(self.server)

		client = {
			"id": self.client,
			"status": client_ip_status
		}

		server = {
			"id": self.server,
			"status": server_ip_status
		}

		# Send the experiment request
		response = MONROE.send_request(monroe_config, self.duration, self.agent_port, client, server)
		
		if response['status'] == RESPONSE_STATUS.OK:
			self.Info('Monroe experiment is successful.')
			self.Info("{0}", response['result']['str'])
			self.UpgradeVerdict(Keysight.Tap.Verdict.Pass)
		else:
			self.Error('Monroe experiment is unsuccessful.')
			self.Error("{0}", response['result']['str'])
			self.UpgradeVerdict(Keysight.Tap.Verdict.Error)

	''' Check if the object is a json object '''
	def is_json(self, json_obj):
		try:
			json_object = json.loads(json_obj)
		except ValueError, e:
			return False
		
		return True

	''' Validates a string as an IP '''
	def check_if_ip(self, inputstr):
		return valid_ipv4(inputstr)