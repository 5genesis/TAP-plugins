'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 27.04.2019 '''

__author__ = 'rsh'
__version__ = '0.0.1'

import requests
import json
from .imports import RESPONSE_STATUS
from .static_methods import STATIC_METHODS

class MONROE:
	def __init__(self,):
		pass

	''' Send request to Monroe Client '''
	@staticmethod
	def send_request(monroe_config, duration_of_expt, agent_port, client, server):
		monroe_expt_response = {}
		url = None
		
		if not client["status"]:
			try:
				res = STATIC_METHODS.uniqueId_Ips_mapping[client["id"]]
				
				client_floating_ip = res["vnf_floating_ip"]
			except KeyError:
				print("Could not find the VNF ID {0} in the mapping.", client["id"])
				monroe_expt_response = {
				    'status': RESPONSE_STATUS.ERROR,
				    'result': {
				        'str': ' '.join(["Could not find the VNF ID", client["id"], "in the mapping."])
				    }
				}
		else:
			client_floating_ip = client["id"]


		if not server["status"]:
			try:
				res = STATIC_METHODS.uniqueId_Ips_mapping[server["id"]]
				server_ip = res["vnf_ip"]
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

		if "status" in monroe_expt_response and monroe_expt_response["status"] == RESPONSE_STATUS.ERROR:
			return monroe_expt_response

		url = ''.join(['http://', str(client_floating_ip), ":", str(agent_port), "/api/monroe"])

		monroe_config["server"] = server_ip # updating the server field

		params = {
			"config": monroe_config,
			"duration": duration_of_expt,
		}

		params["config"] = json.dumps(params["config"])

		if url is not None:
			print ('The URL is {}'.format(url))
			print('The final experiment configuration is {0}'.format(params))

			response = requests.get(url, params=params)
			
			if response.status_code == requests.codes.ok:
			    resp_data = response.json()
			    
			    if 'results' in resp_data:    
			        monroe_expt_response = {
			            'status': RESPONSE_STATUS.OK,
			            'result': {
			                'str': ' '.join(["The results are available at", resp_data['results']])
			            }
			        }
			    else:
			        monroe_expt_response = {
			            'status': RESPONSE_STATUS.ERROR,
			            'result': {
			                'str': 'The results location is not available.'
			            }
			        }
			else:
				resp_data = response.json()
				if 'reasons' in resp_data:
				    monroe_expt_response = {
				        'status': RESPONSE_STATUS.ERROR,
				        'result': {
				            'str': ' '.join(["FAILED: ", resp_data['reasons']])
				        }
				    }
				else:
				    monroe_expt_response = {
				        'status': RESPONSE_STATUS.ERROR,
				        'result': {
				            'str': 'Failing reasons are not available.'
				        }
				    }
		else:
		    print('The url is not set.')
		    monroe_expt_response = {
		        'status': RESPONSE_STATUS.ERROR,
		        'result': {
		            'str': 'The request url is not set.'
		        }
		    }

		return monroe_expt_response