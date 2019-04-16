
'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 15.02.2019 '''

# Ping Test


import sys
import tap
import clr
clr.AddReference("System.Collections")
from System.Collections.Generic import List
from tap import *

import Keysight.Tap
import math
from Keysight.Tap import Log, AvailableValuesAttribute, EnabledIfAttribute
from System import Array, Double, Byte, Int32, String, Boolean #Import types to reference for generic methods
import System
import requests

from System.ComponentModel import BrowsableAttribute # BrowsableAttribute can be used to hide things from the user.

#This is how attributes are used:
@Attribute(Keysight.Tap.DisplayAttribute, "FOKUS Ping Test", "Conduct a ping test", "Ping Test New")
@Attribute(Keysight.Tap.AllowAnyChildAttribute)
class PingTest(TestStep):
	def __init__(self):
		super(PingTest, self).__init__() # The base class initializer must be invoked.
	   
		prop1 = self.AddProperty("host", None, String)
		prop1.AddAttribute(Keysight.Tap.DisplayAttribute, "Host to ping to", "Host")

		prop2 = self.AddProperty("no_of_reqs", 5, Int32)
		prop2.AddAttribute(Keysight.Tap.DisplayAttribute, "No of requests", "No of requests")

		self.AddProperty("instrument", None, PingInstrument)
		
	def Run(self):
		self.instrument.setup(self.host, self.no_of_reqs)
		super(PingTest, self).Run()
		# Send the ping request
		self.instrument.send_request(self.UpgradeVerdict)

class PingInstrument(Instrument):
	def __init__(self):
		super(PingInstrument, self).__init__()

		prop = self.AddProperty("IPAddress", None, String)
		prop.AddAttribute(Keysight.Tap.DisplayAttribute, "IP Address to connect", "IP address")
		# Add Port
		self.AddProperty("Port", None, Int32)

		self.RegisterMethod("setup", None)
		self.RegisterMethod("send_request", None)

	def setup(self, host, no_of_reqs):
		self.host = host
		self.no_of_reqs = no_of_reqs

	def send_request(self, verdict):
		url = ''.join(['http://', self.IPAddress, ':', str(self.Port), "/api/ping"])
		print(url)
		params = {
			"host": self.host,
			"pingReqNum": self.no_of_reqs
		}
		response = requests.get(url, params=params)
		if response.status_code == requests.codes.ok:
			resp_data = response.json()
			if 'rtt_avg' in resp_data:
				verdict(Keysight.Tap.Verdict.Pass)
				res_str = ' '.join(["The RTT average is", str(resp_data['rtt_avg']), "ms"])
			else:
				verdict(Keysight.Tap.Verdict.Error)
				res_str = 'The RTT average could not be calculated'
		
		print (res_str)

	def Open(self):
		self.Info("Ping Instrument opened")

	def Close(self):
		self.Info("Ping Instrument closed")


		

		