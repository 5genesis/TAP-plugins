'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Adapted by Mohammad Rajiullah from Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) '''

# MONROE Experiment

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
import json

from System.ComponentModel import BrowsableAttribute # BrowsableAttribute can be used to hide things from the user.

#This is how attributes are used:
@Attribute(Keysight.Tap.DisplayAttribute, "MONROE Experiment", "Conduct a monroe test", "Monroe Test New")
@Attribute(Keysight.Tap.AllowAnyChildAttribute)

def is_json(myjson):
  try:
    json_object = json.loads(myjson)
  except ValueError, e:
    return False
  return True


class MonroeTest(TestStep):
	def __init__(self):
		super(MonroeTest, self).__init__() # The base class initializer must be invoked.
	   

		#prop1 = self.AddProperty("config", "{\"script\":\"monroe/ping\",\"start\":2,\"storage\":99999999,\"interfacename\":\"eth0\",\"server\":\"8.8.8.8\",\"interface_without_metadata\":[\"eth0\"]}" , String)
		#prop1.AddAttribute(Keysight.Tap.DisplayAttribute, "configuration", "configuration")

		prop1 = self.AddProperty("script","monroe/ping",String)
		prop1.AddAttribute(Keysight.Tap.DisplayAttribute, "Container to run", "script")

		prop2 = self.AddProperty("start", 2, Int32)
		prop2.AddAttribute(Keysight.Tap.DisplayAttribute, "start", "start")

		prop3 = self.AddProperty("storage", 99999999, Int32)
		prop3.AddAttribute(Keysight.Tap.DisplayAttribute, "storage", "storage")

		prop4 = self.AddProperty("interfacename", "eth0", String)
		prop4.AddAttribute(Keysight.Tap.DisplayAttribute, "interfacename", "interfacename")

		#prop5 = self.AddProperty("server", "8.8.8.8", String)
		#prop5.AddAttribute(Keysight.Tap.DisplayAttribute, "server", "server")
              
		prop6 = self.AddProperty("interface_without_metadata", "eth0", String)
		prop6.AddAttribute(Keysight.Tap.DisplayAttribute, "interface_without_metadata", "interface_without_metadata")

                prop5 = self.AddProperty("option","{\"server\":\"8.8.8.8\"}", String)
		prop5.AddAttribute(Keysight.Tap.DisplayAttribute, "Other options passed to the experiment (json string)", "option")
		
		prop7 = self.AddProperty("duration", 30, Int32)
		prop7.AddAttribute(Keysight.Tap.DisplayAttribute, "Duration of the experiment (s)", "duration")

		self.AddProperty("instrument", None, MonroeInstrument)
		
		
	def Run(self):

                configDict={}
                configDict["script"]=self.script
                configDict["start"]=str(self.start)
		configDict["storage"]=str(self.storage)
		configDict["interfacename"]=self.interfacename
                configDict["interface_without_metadata"]= [self.interface_without_metadata]

                if self.option != "":
                        if not is_json(self.option):
                                print "String given in option is not a valid json"
                                exit()
                        else:
                                json_object = json.loads(self.option)
                                for item in json_object:
                                        configDict[item]=json_object[item]
                                
                self.config=json.dumps(configDict)
                        
                
		#self.config=json.dumps({
		#	"script": self.script,
		#	"start": str(self.duration),
		#	"storage": str(self.storage),
		#	"interfacename": self.interfacename,
		#	"server": self.server,
		#	"interface_without_metadata": [self.interface_without_metadata]
#
#		})

		print ("The experiment configuration is {}".format(self.config))
		
		self.instrument.setup(self.config, self.duration)
		super(MonroeTest, self).Run()
		# Send the experiment request
		self.instrument.send_request(self.UpgradeVerdict)

class MonroeInstrument(Instrument):
	def __init__(self):
		super(MonroeInstrument, self).__init__()

		prop = self.AddProperty("IPAddress", None, String)
		prop.AddAttribute(Keysight.Tap.DisplayAttribute, "IP Address to connect", "IP address")
		# Add Port
		self.AddProperty("Port", None, Int32)

		self.RegisterMethod("setup", None)
		self.RegisterMethod("send_request", None)

	def setup(self,config,duration):
		self.config=config
		self.duration=duration

	def send_request(self, verdict):
		url = ''.join(['http://', self.IPAddress, ':', str(self.Port), "/api/monroe"])
		print(url)
		params = {
			"config": self.config,
                        "duration": self.duration
		}
		
		response = requests.get(url, params=params)
		if response.status_code == requests.codes.ok:
			resp_data = response.json()
			if 'results' in resp_data:
				verdict(Keysight.Tap.Verdict.Pass)
				res_str = ' '.join(["The results are available at", resp_data['results']])
			else:
				verdict(Keysight.Tap.Verdict.Error)
				res_str = 'The results location is not available.'
		
                        print (res_str)
                else:
                        resp_data = response.json()
			if 'reasons' in resp_data:
				verdict(Keysight.Tap.Verdict.Error)
				res_str = ' '.join(["FAILED: ", resp_data['reasons']])
			else:
				verdict(Keysight.Tap.Verdict.Error)
				res_str = 'Failing reasons are not available.'
		
                        print (res_str)
                        

	def Open(self):
		self.Info("Monroe Instrument opened")

	def Close(self):
		self.Info("Monroe Instrument closed")


		

		
