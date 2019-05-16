'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 27.04.2019 '''

__author__ = 'rsh'
__version__ = '0.0.1'

from .imports import RESPONSE_STATUS

'''
It contains the class methods so that it can be accessed from any teststep irrespective of creating a particular instance.
'''
class STATIC_METHODS:
	uniqueId_Ips_mapping = {}
	ob_token = None
	project_id = None
	nsr_ids = {}
	nsd_ids = {}
	vnf_pkg_ids = {}
	vim_inst_ids = {}

	# UniqueID of VNF and associated IPs mapping
	@classmethod
	def uniqueID_IP(cls, key, value):
		cls.uniqueId_Ips_mapping[key] = value

	# Set the header configs
	@classmethod
	def set_header_configs(cls, ob_token, project_id):
		cls.ob_token = ob_token
		cls.project_id = project_id

	# Get the header configs
	@classmethod
	def get_header_configs(cls,):
		return cls.ob_token, cls.project_id


	# Set the Network Service Record(NSR) ids
	@classmethod
	def set_nsr_ids(cls, vnf_id, nsr_id):
		if nsr_id not in cls.nsr_ids:
			cls.nsr_ids[vnf_id] = []
		cls.nsr_ids[vnf_id].append(nsr_id)


	# Get the Network Service Record(NSR) ids
	@classmethod
	def get_nsr_ids(cls, vnf_id=None):
		if vnf_id is None:
			all_nsr_ids = []
			for item in cls.nsr_ids.values():
				all_nsr_ids.append(item[0])
			return all_nsr_ids
		elif vnf_id in cls.nsr_ids:
			return cls.nsr_ids[vnf_id]
		else:
			return []


	# Delete the Network Service Record(NSR) id
	@classmethod
	def del_nsr_id(cls, vnf_id=None):
		if vnf_id in cls.nsr_ids:
			del cls.nsr_ids[vnf_id]
		elif vnf_id is None:
			cls.nsr_ids = {}

	# Set the Network Service Descriptor(NSD)
	@classmethod
	def set_nsd_ids(cls, vnf_id, nsd_id):
		if nsd_id not in cls.nsd_ids:
			cls.nsd_ids[vnf_id] = []
		cls.nsd_ids[vnf_id].append(nsd_id)

	# Get the length of nsds'
	@classmethod
	def get_nsr_length(cls,):
		return len(cls.nsd_ids.keys())

	# Get the NSD ids
	@classmethod
	def get_nsd_ids(cls, vnf_id=None):
		if vnf_id is None:
			all_nsd_ids = []
			for item in cls.nsd_ids.values():
				all_nsd_ids.append(item[0])
			return all_nsd_ids
		elif vnf_id in cls.nsd_ids:
			return cls.nsd_ids[vnf_id]
		else:
			return []

	# Get the NSD id for deletion
	@classmethod
	def del_nsd_id(cls, vnf_id=None):
		if vnf_id in cls.nsd_ids:
			del cls.nsd_ids[vnf_id]
		elif vnf_id is None:
			cls.nsd_ids = {}

	# Set VNF package ids
	@classmethod
	def set_vnf_pkg_ids(cls, vnf_id, vnf_pkg_id):
		if vnf_pkg_id not in cls.vnf_pkg_ids:
			cls.vnf_pkg_ids[vnf_id] = []
		cls.vnf_pkg_ids[vnf_id].append(vnf_pkg_id)

	# Get VNF package ids
	@classmethod
	def get_vnf_pkg_ids(cls, vnf_id=None):
		if vnf_id is None:
			all_vnf_pkg_ids = []
			for item in cls.vnf_pkg_ids.values():
				all_vnf_pkg_ids.append(item[0])
			return all_vnf_pkg_ids
		elif vnf_id in cls.vnf_pkg_ids:
			return cls.vnf_pkg_ids[vnf_id]
		else:
			return []

	# Get the VNF package id for deletion
	@classmethod
	def del_vnf_pkg_id(cls, vnf_id=None):
		if vnf_id in cls.vnf_pkg_ids:
			del cls.vnf_pkg_ids[vnf_id]
		elif vnf_id is None:
			cls.vnf_pkg_ids = {}

	# Set the VIM instance ids
	@classmethod
	def set_vim_inst_ids(cls, vnf_id, vim_inst_id):
		if vim_inst_id not in cls.vim_inst_ids:
			cls.vim_inst_ids[vnf_id] = []
		cls.vim_inst_ids[vnf_id].append(vim_inst_id)

	# Get the VIM instance ids
	@classmethod
	def get_vim_inst_ids(cls, vnf_id=None):
		if vnf_id is None:
			all_vim_inst_ids = []
			for item in cls.vim_inst_ids.values():
				all_vim_inst_ids.append(item[0])
			return all_vim_inst_ids
		elif vnf_id in cls.vim_inst_ids:
			return cls.vim_inst_ids[vnf_id]
		else:
			return []

	# Get the VIM instance ids for deletion
	@classmethod
	def del_vim_inst_id(cls, vnf_id):
		if vnf_id in cls.vim_inst_ids:
			del cls.vim_inst_ids[vnf_id]
		elif vnf_id is None:
			cls.vim_inst_ids = {}

	'''
	Get the client IP from the mapping if Unique ID of the VNF is used in the setting
	'''
	@classmethod
	def get_client_ip(cls, client):
		response = {}
		if not client["status"]:
			try:
				res = cls.uniqueId_Ips_mapping[client["id"]]
				
				client_floating_ip = res["vnf_floating_ip"]
			except KeyError:
				print("Could not find the VNF ID {0} in the mapping.", client["id"])
				response = {
				    'status': RESPONSE_STATUS.ERROR,
				    'result': {
				        'str': ' '.join(["Could not find the VNF ID", client["id"], "in the mapping."])
				    }
				}
		else:
			client_floating_ip = client["id"]

		return client_floating_ip, response

	'''
	Get the server IP from the mapping if Unique ID of the VNF is used in the setting
	'''
	@classmethod
	def get_server_ip(cls, server):
		response = {}
		if not server["status"]:
			try:
				res = cls.uniqueId_Ips_mapping[server["id"]]
				server_ip = res["vnf_ip"]
				server_floating_ip = res["vnf_floating_ip"]
			except KeyError:
				print("Could not find the VNF ID {0} in the mapping", server["id"])
				monroe_expt_response = {
				    'status': RESPONSE_STATUS.ERROR,
				    'result': {
				        'str': ' '.join(["Could not find the VNF ID", server["id"], "in the mapping."])
				    }
				}
		else:
			server_ip = server["id"]
			server_floating_ip = server["id"]

		return server_ip, server_floating_ip, response