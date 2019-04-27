
'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 04.03.2019 '''

__author__ = 'rsh'
__version__ = '0.0.1'

import uuid
from .imports import *
from .instrument import *
from .ob_allocate_methods import OB_RESOURCES_ALLOCATE
from .static_methods import STATIC_METHODS


@Attribute(Keysight.Tap.DisplayAttribute, "Deploy VNF", Description="Allocate VNF Resources", Group="Open Baton")
@Attribute(Keysight.Tap.AllowAnyChildAttribute)
class OpenBatonAllocateResource(TestStep):
    def __init__(self):
        super(OpenBatonAllocateResource, self).__init__()

        # Project ID
        project_id = self.AddProperty("project_id", "ec3e72a0-3897-49d6-a5ed-3cc60376f19a", String)
        project_id.AddAttribute(Keysight.Tap.DisplayAttribute, "Project ID", Description="Project ID", Group="OpenBaton Project ID", Order=2, Collapsed=False)

        # Create a Availability Zones dot net Array property and make it hidden
        availability_zones = ["AZ1", "AZ2"]
        availability_zones_dot_net = Array[str](availability_zones)
        self.AddProperty("az_hidden", availability_zones_dot_net, Array).AddAttribute(BrowsableAttribute, False) # Hide the property

        ''' Load VNF Package '''
        # VNF package path
        vnf_pkg_path_prop = self.AddProperty("vnf_pkg_path", None, String)
        vnf_pkg_path_prop.AddAttribute(FilePathAttribute, FilePathAttribute.BehaviorChoice.Open, "tar")
        vnf_pkg_path_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "VNF Package path", "VNF Package path", Group="Upload VNF Package", Order=4.1)
        # Availability Zone
        availability_zone_prop = self.AddProperty("availability_zone", availability_zones[0], String)
        availability_zone_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "Deployment Zone", "Deployment Zone", Group="Upload VNF Package", Order=4.2)
        availability_zone_prop.AddAttribute(Keysight.Tap.AvailableValuesAttribute, "az_hidden")
        # VNF Unique ID
        generated_id = str(uuid.uuid4().hex)
        vnf_pkg_unique_id_prop = self.AddProperty("vnf_pkg_unique_id", generated_id, String)
        vnf_pkg_unique_id_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "VNF Unique ID", "Provide a VNF Unique ID", Group="Upload VNF Package", Order=4.3)
        
        # Add the Instrument
        self.AddProperty("Instrument", None, Instrument).AddAttribute(Keysight.Tap.DisplayAttribute, "Instrument", "Instrument", Group="Instrument", Order=1)

    def PrePlanRun(self,):
        super(OpenBatonAllocateResource, self).PrePlanRun()

        status = False
        self.Info('OpenBatonAllocateResource: Preliminary check started.')
        
        if not self.project_id:
            self.Error('Project Id not present or of not string type')
            status = True
                
        if status:
            self.UpgradeVerdict(Keysight.Tap.Verdict.Error)
            sys.exit("Error has occurred. Exiting the program.")
        else:
            self.Info('OpenBatonAllocateResource: Preliminary check completed.')


    def Run(self):
        super(OpenBatonAllocateResource, self).Run()

        openbaton_credentials, openstack_credentials, zabbix_config = self.Instrument.get_all_credentials()

        ob_allocate = OB_RESOURCES_ALLOCATE(openbaton_credentials, 
                                    openstack_credentials, 
                                    zabbix_config, self.Debug, self.Info, self.Error)

    	response_token = ob_allocate.get_openbaton_token()

        if response_token['status'] == RESPONSE_STATUS.OK:
            token_id = response_token['result']['token']
            ob_allocate.set_header_configs(token_id, self.project_id)
            STATIC_METHODS.set_header_configs(token_id, self.project_id)
            # Check if VIM Instance already exist
            response_check_vim_instance = ob_allocate.get_vim_instance()
            self.Debug("Checking if a VIM Instance already exist.")
            if response_check_vim_instance.status_code == 404: # VIM instance not found case
                # Create VIM instance
                self.Debug("No VIM instance found. Now creating a new VIM instance.")
                response_vim_instance = ob_allocate.create_vim_instance()
            elif response_check_vim_instance.status_code == 200: # VIM instance found case
                # VIM instance already exists
                self.Debug("The VIM instance already exists. Using the already available VIM instance.")
                response_vim_instance = {}
                response_vim_instance['status'] = RESPONSE_STATUS.OK
                response_vim_instance['result'] = {}
                response_check_vim_instance_json = response_check_vim_instance.json()
                
                response_vim_instance['result']['vim_name'] = response_check_vim_instance_json['name']
                response_vim_instance["result"]["vim_inst_id"] = response_check_vim_instance_json['id']
            
            if response_vim_instance['status'] == RESPONSE_STATUS.OK:
                # Create VNF-packages
                STATIC_METHODS.set_vim_inst_ids(self.vnf_pkg_unique_id, response_vim_instance["result"]["vim_inst_id"])
                file = self.vnf_pkg_path
                response_vnf_pkg = ob_allocate.upload_vnf_package(file)
                
                if response_vnf_pkg['status'] == RESPONSE_STATUS.OK:    
                    # Create NSD #nsd_id
                    STATIC_METHODS.set_vnf_pkg_ids(self.vnf_pkg_unique_id, response_vnf_pkg['result']['vnf_pkg_id'])
                    response_nsd = ob_allocate.create_nsd([response_vnf_pkg['result']['vnfd_id']])
                    
                    if response_nsd['status'] == RESPONSE_STATUS.OK:
                        # Launching NSD (=> creating VMs based on the package)  
                        vnf = {
                            "vim_name": response_vim_instance['result']['vim_name'], 
                            "zone": self.availability_zone
                        }
                        STATIC_METHODS.set_nsd_ids(self.vnf_pkg_unique_id, response_nsd["result"]["nsd_id"])
                        response_launch_nsd = ob_allocate.launch_nsd(response_nsd['result']['nsd_id'], 
                                                    vnf, 
                                                    [response_vnf_pkg['result']['vnf_pkg_name']])
                        
                        if response_launch_nsd['status'] == RESPONSE_STATUS.OK:
                            self.Info('OpenBaton VNF deployment completed successfully.')
                            STATIC_METHODS.uniqueID_IP(self.vnf_pkg_unique_id, response_launch_nsd['result'])
                            STATIC_METHODS.set_nsr_ids(self.vnf_pkg_unique_id, response_launch_nsd["result"]["nsr_id"])
                            self.UpgradeVerdict(Keysight.Tap.Verdict.Pass)
                        else:
                            self.Error('Error occurred while launching NSD')
                            self.UpgradeVerdict(Keysight.Tap.Verdict.Error)
                    else:
                        self.Error('Error occurred while creating NSD')
                        self.UpgradeVerdict(Keysight.Tap.Verdict.Error)
                else:
                    self.Error('Error occurred while creating VNF Packages')
                    self.UpgradeVerdict(Keysight.Tap.Verdict.Error)
            else:
                self.Error('Error occurred while creating VIM Instance')
                self.UpgradeVerdict(Keysight.Tap.Verdict.Error)
        else:
            self.Error('Token not found from OpenBaton. Cannot proceed.')
            self.UpgradeVerdict(Keysight.Tap.Verdict.Error)