'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 27.04.2019 '''

__author__ = 'rsh'
__version__ = '0.0.1'

class STATIC_METHODS:
	uniqueId_Ips_mapping = {}
	ob_token = None
	project_id = None
	nsr_ids = {}
	nsd_ids = {}
	vnf_pkg_ids = {}
	vim_inst_ids = {}

	''' UniqueID of VNF and associated IPs mapping '''
	@classmethod
	def uniqueID_IP(cls, key, value):
		cls.uniqueId_Ips_mapping[key] = value

	@classmethod
	def set_header_configs(cls, ob_token, project_id):
		cls.ob_token = ob_token
		cls.project_id = project_id

	@classmethod
	def get_header_configs(cls,):
		return cls.ob_token, cls.project_id

	@classmethod
	def set_nsr_ids(cls, vnf_id, nsr_id):
		if nsr_id not in cls.nsr_ids:
			cls.nsr_ids[vnf_id] = []
		cls.nsr_ids[vnf_id].append(nsr_id)

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

	@classmethod
	def del_nsr_id(cls, vnf_id=None):
		if vnf_id in cls.nsr_ids:
			del cls.nsr_ids[vnf_id]
		elif vnf_id is None:
			cls.nsr_ids = {}

	@classmethod
	def set_nsd_ids(cls, vnf_id, nsd_id):
		if nsd_id not in cls.nsd_ids:
			cls.nsd_ids[vnf_id] = []
		cls.nsd_ids[vnf_id].append(nsd_id)

	@classmethod
	def get_nsr_length(cls,):
		return len(cls.nsd_ids.keys())

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

	@classmethod
	def del_nsd_id(cls, vnf_id=None):
		if vnf_id in cls.nsd_ids:
			del cls.nsd_ids[vnf_id]
		elif vnf_id is None:
			cls.nsd_ids = {}

	@classmethod
	def set_vnf_pkg_ids(cls, vnf_id, vnf_pkg_id):
		if vnf_pkg_id not in cls.vnf_pkg_ids:
			cls.vnf_pkg_ids[vnf_id] = []
		cls.vnf_pkg_ids[vnf_id].append(vnf_pkg_id)

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

	@classmethod
	def del_vnf_pkg_id(cls, vnf_id=None):
		if vnf_id in cls.vnf_pkg_ids:
			del cls.vnf_pkg_ids[vnf_id]
		elif vnf_id is None:
			cls.vnf_pkg_ids = {}

	@classmethod
	def set_vim_inst_ids(cls, vnf_id, vim_inst_id):
		if vim_inst_id not in cls.vim_inst_ids:
			cls.vim_inst_ids[vnf_id] = []
		cls.vim_inst_ids[vnf_id].append(vim_inst_id)

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

	@classmethod
	def del_vim_inst_id(cls, vnf_id):
		if vnf_id in cls.vim_inst_ids:
			del cls.vim_inst_ids[vnf_id]
		elif vnf_id is None:
			cls.vim_inst_ids = {}