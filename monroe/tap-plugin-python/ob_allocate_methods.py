'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 27.04.2019 '''

__author__ = 'rsh'
__version__ = '0.0.1'

import requests
import grequests
import json
import time
from .imports import RESPONSE_STATUS

class OB_RESOURCES_ALLOCATE:
	def __init__(self, ob_credentials, os_credentials, zabbix_config, Debug, Info, Error):
		# Open Baton
		self.openbaton_username = ob_credentials["openbaton_username"]
		self.openbaton_password = ob_credentials["openbaton_password"]
		self.openbaton_ip = ob_credentials["openbaton_ip"]
		self.openbaton_port = ob_credentials["openbaton_port"]

		# OpenStack
		self.openstack_ip = os_credentials["openstack_ip"]
		self.openstack_port = os_credentials["openstack_port"]
		self.openstack_username = os_credentials["openstack_username"]
		self.openstack_password = os_credentials["openstack_password"]
		self.openstack_tenant_id = os_credentials["openstack_tenant_id"]
		self.openstack_keypair_id = os_credentials["openstack_keypair_id"]

		# Zabbix
		self.allow_zabbix_srv = zabbix_config["allow_zabbix_srv"]
		self.zabbix_srv_ip = zabbix_config["zabbix_srv_ip"]

		# Log levels
		self.Debug = Debug
		self.Info = Info
		self.Error = Error


	''' Get the Open Baton token '''
	def get_openbaton_token(self,):
		self.openbaton_token_response = {}

		headers = {
			'Accept': 'application/json',
		}

		data = {
    		'username': self.openbaton_username, #admin
			'password': self.openbaton_password, #Openbaton1
			'grant_type': 'password'
		}

		url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/oauth/token'])

		auth = ('openbatonOSClient', 'secret')

		response = requests.post(url, headers=headers, data=data, auth=auth)

		if response.status_code == requests.codes.ok: # 200
			resp_json = response.json()
			self.Info("The openbaton token is {0}", resp_json['value']) # Displays the token
			self.openbaton_token_response = {
				"status": RESPONSE_STATUS.OK,
				"result": {
					"token": resp_json['value']
				}
			}
		else:
			self.openbaton_token_response = {
				"status": RESPONSE_STATUS.ERROR,
			}

		return self.openbaton_token_response

	def set_header_configs(self, ob_token, project_id):
		self.token = ob_token
		self.project_id = project_id

	''' Get VIM Instance '''
	def get_vim_instance(self, vim_instance_name="5genesis-vim-tap"):
		headers = {
			"Content-Type": "application/json",
			"Authorization": ' '.join(["Bearer", self.token]),
			"project-id": self.project_id
		}

		url = ''.join(['http://', self.openbaton_ip, ':', 
						str(self.openbaton_port), '/api/v1/datacenters/search/findByName?name=', vim_instance_name])

		response = requests.get(url, headers=headers)

		return response


	''' Load VIM instance in openbaton '''
	def create_vim_instance(self, vim_instance_name="5genesis-vim-tap"):
		self.openbaton_vim_instance_response = {}

		headers = {
			"Content-Type": "application/json",
			"Authorization": ' '.join(["Bearer", self.token]),
			"project-id": self.project_id
		}

		data = json.dumps({
			"name": vim_instance_name,
			"authUrl": ''.join(['http://', self.openstack_ip, ':', str(self.openstack_port), '/v3']),
			"tenant": self.openstack_tenant_id, #"3f26abc8928049838ad011e7e0455182",
			"username": self.openstack_username, #"admin",
			"password": self.openstack_password, #"b527085a6c8c486e",
			"keyPair": self.openstack_keypair_id, #"5genesis",
			"securityGroups": [
				"default"
			],
			"type":"openstack",
			"location":  {
				"name":"Berlin",
				"latitude":"52.525876",
				"longitude":"13.314400"
			}
		})

		url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/datacenters'])

		response = requests.post(url, headers=headers, data=data)
        
		if response.status_code == requests.codes.ok or response.status_code == 201:
			resp_data = response.json()
            
			self.openbaton_vim_instance_response = {
				"status": RESPONSE_STATUS.OK,
				"result": {
					"vim_inst_id": resp_data['id'], 
					"vim_name": resp_data['name']
				}
			}
		else:
			self.openbaton_vim_instance_response = {
				"status": RESPONSE_STATUS.ERROR
			}

		return self.openbaton_vim_instance_response


	''' Load vnf-packages (*.tar) to openbaton 
		Multiple async requests are sent if there are multiple vnf-packages
	'''
	def upload_vnf_package(self, file):
		self.openbaton_vnf_pkg_response = {}

		def exception_handler(request, exception):
			print ("Request failed")

		headers = {
			"Authorization": ' '.join(["Bearer", self.token]),
			"project-id": self.project_id
		}

		file = {
			"file": (file, open(file, 'rb'))
		}

		url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/vnf-packages'])

		response = requests.post(url, headers=headers, files=file)

		if response.status_code == requests.codes.ok:
			resp_data = response.json()
			vnfd_id = resp_data["id"]
			vnf_pkg_id = resp_data["vnfPackageLocation"]
			vnf_pkg_name = resp_data["name"]

			self.openbaton_vnf_pkg_response = {
				"status": RESPONSE_STATUS.OK,
				"result": {
					"vnfd_id": vnfd_id,
					"vnf_pkg_id": vnf_pkg_id,
					"vnf_pkg_name": vnf_pkg_name,
				}
			}
		else:
			self.openbaton_vnf_pkg_response = {
				"status": RESPONSE_STATUS.ERROR
			}

		return self.openbaton_vnf_pkg_response


	''' Create NSD in openbaton '''
	def create_nsd(self, vnfd_ids):
		self.openbaton_nsd_response = {}

		headers = {
			"Content-Type": "application/json",
			"Authorization": ' '.join(["Bearer", self.token]),
			"project-id": self.project_id
        }
		vnfds = [{"id": v} for v in vnfd_ids if v is not None]
		data = json.dumps({
			"name": "monroe",
			"vendor": "fokus",
			"version": "1.0",
			"vld": [],
			"vnfd": vnfds
		})
		url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/ns-descriptors'])

		response = requests.post(url, headers=headers, data=data)
    
		if response.status_code == requests.codes.ok or response.status_code == 201:
			resp_data = response.json()
			self.Info('The NSD Id is {0}', resp_data['id'])
			self.openbaton_nsd_response = {
				"status": RESPONSE_STATUS.OK,
				"result": {
					"nsd_id": resp_data['id']
				}
			}
		else:
			self.openbaton_nsd_response = {
				"status": RESPONSE_STATUS.ERROR
			}
        
		return self.openbaton_nsd_response

	''' Launch NSD in openbaton '''
	def launch_nsd(self, nsd_id, vnf, vnf_pkg_names):
		self.openbaton_nsd_launch_response = {}

		headers = {
			'Content-Type': 'application/json',
			'Authorization': ' '.join(['Bearer', self.token]),
			'project-id': self.project_id,
		}

		vnf_machine = [''.join([vnf['vim_name'], ":", vnf['zone']])]

		# adds -1 to the vnf pkg name to follow naming convention in openbaton.
		def convert_vnf_pkg_names(vnf_pkg_name):
			return ''.join([vnf_pkg_name, '-1'])

		updated_vnf_pkg_names = map(convert_vnf_pkg_names, vnf_pkg_names)

		vduVimInstances = {}
		vduVimInstances[updated_vnf_pkg_names[0]] = vnf_machine
        
		if self.allow_zabbix_srv:
			data = json.dumps({
				"keys": [],
				"configurations": {},
				"vduVimInstances": vduVimInstances,
				"monitoringIp" : self.zabbix_srv_ip # zabbix server ip
			})
		else:
			data = json.dumps({
				"keys": [],
				"configurations": {},
				"vduVimInstances": vduVimInstances
            })

		url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/ns-records/', nsd_id])        
        
		response = requests.post(url, headers=headers, data=data)
		resp_data = response.json()
        
		#nsr_status = resp_data['status']
		nsr_id = resp_data['id'] # id of newly created nsr instance

		get_nsr_data = self.check_nsr_status(headers, nsr_id)
		if get_nsr_data['status']:
			self.openbaton_nsd_launch_response = {
				"status": RESPONSE_STATUS.OK,
				"result": {
					"nsr_id": get_nsr_data['nsr_id'],
					"vnf_ip": get_nsr_data['vnf_ip'],
					"vnf_floating_ip": get_nsr_data['vnf_floating_ip'],
				}
			}
		elif get_nsr_data['status'] is False:
			self.openbaton_nsd_launch_response = {
				"status": RESPONSE_STATUS.ERROR
			}

		return self.openbaton_nsd_launch_response


	''' Check the status of NSR from NULL => ACTIVE 
		Continuous requests are sent in intervals to track the state of NSR in openbaton
	'''
	def check_nsr_status(self, headers, nsr_id):
		self.openbaton_nsr_response = {}

		url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/ns-records/', nsr_id])        
		attempt = 1
		while True:
			response = requests.get(url, headers=headers)
			response_data = response.json()

			if 'status' in response_data:
				self.Info('Checking the status of NSR: {0}, Attempt No.: {1}', response_data['status'], attempt)
				status = response_data['status'] 
				if status and status == 'ACTIVE':
					for k, _ in enumerate(response_data['vnfr']):
						if self.openbaton_vnf_pkg_response['result']['vnf_pkg_name'] == response_data['vnfr'][k]['name']:
							vnf_ip = response_data['vnfr'][k]['vdu'][0]['vnfc_instance'][0]['ips'][0]['ip']
							vnf_floating_ip = response_data['vnfr'][k]['vdu'][0]['vnfc_instance'][0]['floatingIps'][0]['ip']
							self.Info('The VNF private ip is {0}', vnf_ip)
							self.Info('The VNF floating ip {0}', vnf_floating_ip)
                    
					self.Info('Status of NSR is {0}', status)
					self.openbaton_nsr_response = {
						"vnf_ip": vnf_ip,
						"vnf_floating_ip": vnf_floating_ip,
						"status": True,
						"nsr_id": nsr_id,
					}
					break
				elif status == 'ERROR':
					self.openbaton_nsr_response = {
						"status": False
					}
					break
				else:  
					time.sleep(2)
				attempt = attempt + 1
			else:
				self.Error('NSR status is not available.')
				self.openbaton_nsr_response = {
					"status": False
				}
				break

		return self.openbaton_nsr_response