'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 27.04.2019 '''

__author__ = 'rsh'
__version__ = '0.0.1'

import requests
import grequests
import json
from .imports import RESPONSE_STATUS

class OB_RESOURCES_DEALLOCATE:
	def __init__(self, ob_credentials, Debug, Info, Error):
		# Open Baton
		self.openbaton_ip = ob_credentials["openbaton_ip"]
		self.openbaton_port = ob_credentials["openbaton_port"]

		# Log levels
		self.Debug = Debug
		self.Info = Info
		self.Error = Error

	def set_header_configs(self, token, project_id):
		self.token = token
		self.project_id = project_id

	''' Delete the NSR '''
	def delete_nsr(self, nsr_ids):
		def exception_handler(request, exception):
			print ("Request failed")

		headers = {
			'Content-Type': 'application/json',
			'Authorization': ' '.join(['Bearer', self.token]),
			'project-id': self.project_id,
		}

		part_url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/ns-records/']) 

		requests_list = []
		for nsr_id in nsr_ids:
			requests_list.append(grequests.delete(''.join([part_url, nsr_id]), headers=headers))

		responses = grequests.map(requests_list, exception_handler=exception_handler)

		response_states = [True for r in responses if r.status_code == requests.codes.ok or r.status_code == 204]
		#response = requests.delete(url, headers=headers)
		print("Delete NSR {}".format(response_states))

		if len(response_states) and response_states[1:] == response_states[:-1] and response_states[0]:
			self.Info("NSR deleted. No payload in response.")
			return {
				"status": RESPONSE_STATUS.OK
			}
		else:
			return {
				"status": RESPONSE_STATUS.ERROR
			}


	''' Delete the NSD '''
	def delete_nsd(self, nsd_ids):
		def exception_handler(request, exception):
			print ("Request failed")

		headers = {
			"Authorization": ' '.join(["Bearer", self.token]),
			"project-id": self.project_id,
		}

		part_url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/ns-descriptors/']) 

		requests_list = []
		for nsd_id in nsd_ids:
			requests_list.append(grequests.delete(''.join([part_url, nsd_id]), headers=headers))

		responses = grequests.map(requests_list, exception_handler=exception_handler)

		response_states = [True for r in responses if r.status_code == requests.codes.ok or r.status_code == 204]
        
		#response = requests.delete(url, headers=headers)
		print("Delete NSD {}".format(response_states))
        
		if len(response_states) and response_states[1:] == response_states[:-1] and response_states[0]:
			self.Info('NSD deleted. No payload in response.')
			return {
				"status": RESPONSE_STATUS.OK
			}
		else:
			return {
				"status": RESPONSE_STATUS.ERROR
			}


	''' Delete multiple vnf packages '''
	def delete_vnf_packages(self, vnf_pkg_ids):
		def exception_handler(request, exception):
			print ("Request failed")

		headers = {
			"Content-Type": "application/json",
			"Authorization": ' '.join(["Bearer", self.token]),
			"project-id": self.project_id,
		}

		part_url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/vnf-packages/'])

		requests_list = []
		for vnf_pkg_id in vnf_pkg_ids:
			requests_list.append(grequests.delete(''.join([part_url, vnf_pkg_id]), headers=headers))

		responses = grequests.map(requests_list, exception_handler=exception_handler)

		response_states = [True for r in responses if r.status_code == requests.codes.ok or r.status_code == 204]

		print("Delete VNF-packages {}".format(response_states))

		if len(response_states) and response_states[1:] == response_states[:-1] and response_states[0]:
			self.Info("VNF-Packages deleted. No payload in response")
			return {
				"status": RESPONSE_STATUS.OK
			}
		else:
			return {
				"status": RESPONSE_STATUS.ERROR
			}


	''' Delete Vim Instance '''
	def delete_vim_instance(self, vim_inst_ids):
		def exception_handler(request, exception):
			print ("Request failed")

		headers = {
			"Authorization": ' '.join(["Bearer", self.token]),
			"project-id": self.project_id,
		}

		part_url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/datacenters/'])

		requests_list = []
		for vim_inst_id in vim_inst_ids:
			requests_list.append(grequests.delete(''.join([part_url, vim_inst_id]), headers=headers))

		responses = grequests.map(requests_list, exception_handler=exception_handler)

		response_states = [True for r in responses if r.status_code == requests.codes.ok or r.status_code == 204]
		        
		print("Delete VIM instance {}".format(response_states))
        
		if len(response_states) and response_states[1:] == response_states[:-1] and response_states[0]:
			self.Info('VIM instance deleted. No payload in response.')
			return {
				"status": RESPONSE_STATUS.OK
			}
		else:
			return {
				"status": RESPONSE_STATUS.ERROR
			}