
'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 04.03.2019 '''

__author__ = 'rsh'
__version__ = '0.0.1'

from .imports import *
import json
import time
import grequests  # async http calls


''' Berlin Platform TAP Instrument class
    Base class is Instrument from TAP
'''

@Attribute(Keysight.Tap.DisplayAttribute, "Instrument", "Interface to OpenBaton and OpenStack", "Berlin Platform")        
class Instrument(Instrument):
    def __init__(self):
        super(Instrument, self).__init__()

        # Openbaton IP Address
        ob_ip = self.AddProperty("openbaton_ip", "127.0.0.1", String)
        ob_ip.AddAttribute(Keysight.Tap.DisplayAttribute, "IP Address", "OpenBaton IP Address", Group="Open Baton", Order=1.1)
    
        # Openbaton Port
        ob_port = self.AddProperty("openbaton_port", 8080, Int32)
        ob_port.AddAttribute(Keysight.Tap.DisplayAttribute, "Port", "OpenBaton Port", Group="Open Baton", Order=1.2)

        # OpenStack IP Address
        os_ip = self.AddProperty("openstack_ip", "127.0.0.1", String)
        os_ip.AddAttribute(Keysight.Tap.DisplayAttribute, "IP Address", "OpenStack IP Address", Group="Open Stack", Order=2.1)
    
        # OpenStack Port
        os_port = self.AddProperty("openstack_port", 5000, Int32)
        os_port.AddAttribute(Keysight.Tap.DisplayAttribute, "Port", "OpenStack Port", Group="Open Stack", Order=2.2)  

        # Zabbix Server
        allow_zabbix_srv = self.AddProperty("allow_zabbix_srv", False, Boolean)
        allow_zabbix_srv.AddAttribute(Keysight.Tap.DisplayAttribute, "Enable Zabbix Server", "Enable Zabbix Server", Group="Monitoring Server", Order=3.1)

        zs_ip = self.AddProperty("zabbix_srv_ip", "127.0.0.1", String)
        zs_ip.AddAttribute(Keysight.Tap.DisplayAttribute, "Server IP", "Zabbix Server IP", Group="Monitoring Server", Order=3.2)
        zs_ip.AddAttribute(Keysight.Tap.EnabledIfAttribute, "allow_zabbix_srv", HideIfDisabled=True) 

    ''' Get the openbaton token '''
    def get_openbaton_token(self):
        self.openbaton_token_response = {}

        headers = {
            'Accept': 'application/json',
        }

        data = {
          'username': 'xxx',
          'password': 'xxx',
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

    '''' Load VIM instance in openbaton '''
    def create_vim_instance(self, token, project_id):
        self.openbaton_vim_instance_response = {}
        self.project_id = project_id

        headers = {
            "Content-Type": "application/json",
            "Authorization": ' '.join(["Bearer", token]),
            "project-id": project_id
        }

        data = json.dumps({
            "name": "5genesis-vim-tap",
            "authUrl": ''.join(['http://', self.openstack_ip, ':', str(self.openstack_port), '/v3']),
            "tenant": "xxx",
            "username": "xxx",
            "password": "xxx",
            "keyPair":"5genesis",
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
    def upload_vnf_package(self, token, project_id, files):
        self.openbaton_vnf_pkg_response = {}

        def exception_handler(request, exception):
            print ("Request failed")

        headers = {
            "Authorization": ' '.join(["Bearer", token]),
            "project-id": project_id
        }

        files_list = []

        for file in files:
            files_list.append(
                {
                    "file": (''.join([file]), open(''.join([file]), 'rb')),
                }
            )

        url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/vnf-packages'])

        requests_list = []
        for file in files_list:
            requests_list.append(grequests.post(url, headers=headers, files=file))
        
        responses = grequests.map(requests_list, exception_handler=exception_handler)
        
        resp_data = [r.json() for r in responses if r.status_code == requests.codes.ok]
        
        vnfd_ids = [r['id'] for r in resp_data if r is not None]
        vnf_pkg_ids = [r['vnfPackageLocation'] for r in resp_data if r is not None]
        vnf_pkg_names = [r['name'] for r in resp_data if r is not None]
        print('The vnfd ids {}'.format(vnfd_ids))
        print('The vnf-pkg ids {}'.format(vnf_pkg_ids))
        
        vnf_pkg_names = vnf_pkg_names[::-1] # reversing the list to avoid conflict with the specified Availability Zones in TAP GUI.
        print('The vnf-pkg names {}'.format(vnf_pkg_names))

        self.openbaton_vnf_pkg_response = {
            "status": RESPONSE_STATUS.OK,
            "result": {
                "vnfd_ids": vnfd_ids,
                "vnf_pkg_ids": vnf_pkg_ids,
                "vnf_pkg_names": vnf_pkg_names
            }
        }
        
        return self.openbaton_vnf_pkg_response

    ''' Create NSD in openbaton '''
    def create_nsd(self, token, project_id, vnfd_ids):
        self.openbaton_nsd_response = {}

        headers = {
            'Content-Type': 'application/json',
            "Authorization": ' '.join(["Bearer", token]),
            "project-id": project_id
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
    def launch_nsd(self, token, nsd_id, project_id, client, server, vnf_pkg_names):
        self.openbaton_nsd_launch_response = {}

        headers = {
            'Content-Type': 'application/json',
            'Authorization': ' '.join(['Bearer', token]),
            'project-id': project_id,
        }

        client = [''.join([client['vim_name'], ":", client['zone']])]
        if server is not None:
            server = [''.join([server['vim_name'], ":", server['zone']])]
        
        print("The client is {}".format(client))
        print("The server is {}".format(server))

        # adds -1 to the vnf pkg name to follow naming convention in openbaton.
        def convert_vnf_pkg_names(vnf_pkg_name):
            return ''.join([vnf_pkg_name, '-1'])

        updated_vnf_pkg_names = map(convert_vnf_pkg_names, vnf_pkg_names)

        vduVimInstances = {}
        vduVimInstances[updated_vnf_pkg_names[0]] = client
        if server is not None:
            vduVimInstances[updated_vnf_pkg_names[1]] = server

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

        server_status = True if server is not None else False

        get_nsr_data = self.check_nsr_status(headers, nsr_id, server_status)
        if get_nsr_data['status']:
            self.openbaton_nsd_launch_response = {
                "status": RESPONSE_STATUS.OK,
                "result": {
                    "nsr_id": get_nsr_data['nsr_id']
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
    def check_nsr_status(self, headers, nsr_id, server_status):
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
                        if self.openbaton_vnf_pkg_response['result']['vnf_pkg_names'][0] == response_data['vnfr'][k]['name']:
                            client_ip = response_data['vnfr'][k]['vdu'][0]['vnfc_instance'][0]['ips'][0]['ip']
                            client_floating_ip = response_data['vnfr'][k]['vdu'][0]['vnfc_instance'][0]['floatingIps'][0]['ip']
                            self.Info('The Client ip is {0}', client_ip)
                            self.Info('The Client floating ip {0}', client_floating_ip)
                        
                        if server_status:
                            if self.openbaton_vnf_pkg_response['result']['vnf_pkg_names'][1] == response_data['vnfr'][k]['name']:
                                server_ip = response_data['vnfr'][k]['vdu'][0]['vnfc_instance'][0]['ips'][0]['ip']
                                server_floating_ip = response_data['vnfr'][k]['vdu'][0]['vnfc_instance'][0]['floatingIps'][0]['ip']
                                self.Info('The Server ip {0}', server_ip)
                                self.Info('The Server floating ip is {0}', server_floating_ip)
                        else:
                            self.Debug('Server/Responder is not deployed by OpenBaton.')
                            server_ip = None
                            server_floating_ip = None
                    
                    self.Info('Status of NSR is {0}', status)
                    self.openbaton_nsr_response = {
                        "client_ip": client_ip,
                        "server_ip": server_ip,
                        "client_floating_ip": client_floating_ip,
                        "server_floating_ip": server_floating_ip,
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


    ''' Send request to Monroe Client '''
    def send_request(self, configs):
        monroe_expt_response = {}
        url = None

        if configs['enable_manual_config']:
            url = ''.join(['http://', str(configs['monroe_vm_ip']), ':', str(configs['agent_port']), "/api/monroe"])
            self.Debug("{0}", url)

            params = {
                "config": configs['config'],
                "duration": configs['duration']
            }
            params['config'] = json.dumps(params['config'])

        elif 'status' in self.openbaton_nsr_response and self.openbaton_nsr_response['status']:
            if self.openbaton_nsr_response['client_floating_ip'] is not None:
                url = ''.join(['http://', self.openbaton_nsr_response['client_floating_ip'], ':', str(configs['agent_port']), "/api/monroe"])
                self.Debug("{0}", url)
                
                params = {
                    "config": configs['config'],
                    "duration": configs['duration']
                }
                
                if 'server_ip' in self.openbaton_nsr_response and self.openbaton_nsr_response['server_ip'] is not None:
                   params['config']['server'] = self.openbaton_nsr_response['server_ip']

                params['config'] = json.dumps(params['config'])
            else:
                monroe_expt_response = {
                    'status': RESPONSE_STATUS.ERROR,
                    'result': {
                        'str': 'Client Floating IP is not available.'
                    }
                }
        else:
            monroe_expt_response = {
                'status': RESPONSE_STATUS.ERROR,
                'result': {
                    'str': 'OpenBaton NSR status not available or has Error.'
                }
            }

        if url is not None:
            self.Info('The final experiment configuration is {0}'.format(params))

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
            self.Debug('The url is not set.')
            monroe_expt_response = {
                'status': RESPONSE_STATUS.ERROR,
                'result': {
                    'str': 'The request url is not set.'
                }
            }

        return monroe_expt_response
                        
    ''' Delete the NSR '''
    def delete_nsr(self):
        headers = {
            'Content-Type': 'application/json',
            'Authorization': ' '.join(['Bearer', self.openbaton_token_response['result']['token']]),
            'project-id': self.project_id,
        }

        url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/ns-records/', 
                                                            self.openbaton_nsd_launch_response['result']['nsr_id']]) 
        
        response = requests.delete(url, headers=headers)
        print("Delete NSR {}".format(response))
        
        if response.status_code == requests.codes.ok or response.status_code == 204:
            self.Info('NSR deleted. No payload in response.')
            return {
                "status": RESPONSE_STATUS.OK
            }
        else:
            return {
                "status": RESPONSE_STATUS.ERROR
            }

    ''' Delete the NSD '''
    def delete_nsd(self):
        headers = {
            "Authorization": ' '.join(["Bearer", self.openbaton_token_response['result']['token']]),
            "project-id": self.project_id,
        }

        url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), 
                                '/api/v1/ns-descriptors/', self.openbaton_nsd_response['result']['nsd_id']]) 
        
        response = requests.delete(url, headers=headers)
        print("Delete NSD {}".format(response))
        
        if response.status_code == requests.codes.ok or response.status_code == 204:
            self.Info('NSD deleted. No payload in response.')
            return {
                "status": RESPONSE_STATUS.OK
            }
        else:
            return {
                "status": RESPONSE_STATUS.ERROR
            }
            

    ''' Delete the VNF packages : To be updated: not in use for now'''
    '''
    def delete_vnf_packages(self, token, project_id, vnf_packages_ids):
        headers = {
            "Authorization": ' '.join(["Bearer", self.openbaton_token_response['result']['token']]),
            "project-id": self.project_id,
        }
        
        urls = []
        for id in vnf_packages_ids:
            urls.append(''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/vnf-packages/', id]))
        
        requests = []
        for url in urls:
            print (url)
            requests.append(grequests.delete(url, headers=headers))

        responses = grequests.map(requests)
        print ("Delete VNF-Packages", responses)
        
        for response in responses:
            if response.status_code == 200:
                print (response.json())
            elif response.status_code == 204:
                print ('VNF-Packages deleted. No payload in response.') '''

    ''' Delete multiple vnf packages '''
    def delete_multi_vnf_packages(self,):
        headers = {
            "Content-Type": "application/json",
            "Authorization": ' '.join(["Bearer", self.openbaton_token_response['result']['token']]),
            "project-id": self.project_id,
        }

        data = json.dumps(self.openbaton_vnf_pkg_response['result']['vnf_pkg_ids'])

        url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), '/api/v1/vnf-packages/multipledelete'])

        response = requests.post(url, headers=headers, data=data)
        print("Delete multi VNF-packages {}".format(response))

        if response.status_code == requests.codes.ok or response.status_code == 204:
            self.Info("VNF-Packages deleted. No payload in response")
            return {
                "status": RESPONSE_STATUS.OK
            }
        else:
            return {
                "status": RESPONSE_STATUS.ERROR
            }


    ''' Delete Vim Instance '''
    def delete_vim_instance(self):
        headers = {
            "Authorization": ' '.join(["Bearer", self.openbaton_token_response['result']['token']]),
            "project-id": self.project_id,
        }

        url = ''.join(['http://', self.openbaton_ip, ':', str(self.openbaton_port), 
                                    '/api/v1/datacenters/', self.openbaton_vim_instance_response['result']['vim_inst_id']])
        
        response = requests.delete(url, headers=headers)
        print("Delete VIM instance {}".format(response))
        
        if response.status_code == requests.codes.ok or response.status_code == 204:
            self.Info('VIM instance deleted. No payload in response.')
            return {
                "status": RESPONSE_STATUS.OK
            }
        else:
            return {
                "status": RESPONSE_STATUS.ERROR
            }

    ''' Opens the instrument
        Overriden function
    '''
    def Open(self):
        self.Info("Instrument opened")

    ''' Close the instrument
        Overriden function
    '''
    def Close(self):
        self.Info("Instrument closed")
        
