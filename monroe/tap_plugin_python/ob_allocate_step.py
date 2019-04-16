
'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 04.03.2019 '''

__author__ = 'rsh'
__version__ = '0.0.1'

from .imports import *
from .instrument import *
from array import array

@Attribute(Keysight.Tap.DisplayAttribute, "OpenBaton Resources Allocate", Description="Allocate Resources", Group="Berlin Platform")
@Attribute(Keysight.Tap.AllowAnyChildAttribute)
class OpenBatonAllocateResource(TestStep):
    def __init__(self):
        super(OpenBatonAllocateResource, self).__init__()

        # Project ID
        project_id = self.AddProperty("project_id", "ec3e72a0-3897-49d6-a5ed-3cc60376f19a", String)
        project_id.AddAttribute(Keysight.Tap.DisplayAttribute, "Project ID", Description="Project ID", Group="OpenBaton Project ID", Order=2, Collapsed=False)

        # Load Server-Client Boolean Property
        load_srv_cli_bool = self.AddProperty("load_srv_cli", True, Boolean)
        load_srv_cli_bool.AddAttribute(Keysight.Tap.DisplayAttribute, "Load Client and Server", "Load Client and Server", Group="Load Client & Server", Order=3)

        # Create a Availability Zones dot net Array property and make it hidden
        availability_zones = ["AZ1", "AZ2"]
        availability_zones_dot_net = Array[str](availability_zones)
        self.AddProperty("az_hidden", availability_zones_dot_net, Array).AddAttribute(BrowsableAttribute, False) # Hide the property
        # Availability Zone
        availability_zone1 = self.AddProperty("client_zone", availability_zones[0], String)
        availability_zone1.AddAttribute(Keysight.Tap.DisplayAttribute, "Client", "Client Availability Zone", Group="Availability Zone", Order=4.1)
        availability_zone1.AddAttribute(Keysight.Tap.AvailableValuesAttribute, "az_hidden")

        availability_zone2 = self.AddProperty("server_zone", availability_zones[0], String)
        availability_zone2.AddAttribute(Keysight.Tap.DisplayAttribute, "Server", "Server Availability Zone", Group="Availability Zone", Order=4.2)
        availability_zone2.AddAttribute(Keysight.Tap.EnabledIfAttribute, "load_srv_cli", HideIfDisabled=True)
        availability_zone2.AddAttribute(Keysight.Tap.AvailableValuesAttribute, "az_hidden")

        # Load VNF Packages
        load_vnf_pkg_client = self.AddProperty("client_vnf_pkg_path", None, String);
        load_vnf_pkg_client.AddAttribute(FilePathAttribute, FilePathAttribute.BehaviorChoice.Open, "tar")
        load_vnf_pkg_client.AddAttribute(Keysight.Tap.DisplayAttribute, "VNF Package Client", "VNF Package Client", Group="OpenBaton VNF Packages", Order=5.1)
        
        load_vnf_pkg_server = self.AddProperty("server_vnf_pkg_path", None, String);
        load_vnf_pkg_server.AddAttribute(FilePathAttribute, FilePathAttribute.BehaviorChoice.Open, "tar")
        load_vnf_pkg_server.AddAttribute(Keysight.Tap.DisplayAttribute, "VNF Package Server", "VNF Package Server", Group="OpenBaton VNF Packages", Order=5.2)
        load_vnf_pkg_server.AddAttribute(Keysight.Tap.EnabledIfAttribute, "load_srv_cli", HideIfDisabled=True)
        
        # Add the Instrument
        self.AddProperty("Instrument", None, Instrument).AddAttribute(Keysight.Tap.DisplayAttribute, "Instrument", "Instrument", Group="Instrument", Order=1)

    def PrePlanRun(self,):
        super(OpenBatonAllocateResource, self).PrePlanRun()

        status = False
        self.Info('OpenBatonAllocateResource: Preliminary check started.')
        
        if not self.project_id:
            self.Error('Project Id not present or of not string type')
            status = True

        if self.load_srv_cli:
            if not (self.client_zone and self.server_zone):
                self.Error('Availability zones are not present.')
                status = True

            if not (self.client_vnf_pkg_path and self.server_vnf_pkg_path):
                self.Error('VNF Package paths are not available.')
                status = True
        else:
            if not self.client_zone:
                self.Error('Availability zone is not present.')
                status = True

            if not self.client_vnf_pkg_path:
                self.Error('VNF Package path is not available.')
                status = True
                
        if status:
            self.UpgradeVerdict(Keysight.Tap.Verdict.Error)
            sys.exit("Error has occurred. Exiting the program.")
        else:
            self.Info('OpenBatonAllocateResource: Preliminary check completed.')

    def Run(self):
        super(OpenBatonAllocateResource, self).Run()
        
    	response_token = self.Instrument.get_openbaton_token()
        if response_token['status'] == RESPONSE_STATUS.OK:
            # Create VIM instance
            response_vim_instance = self.Instrument.create_vim_instance(response_token['result']['token'], self.project_id)
            if response_vim_instance['status'] == RESPONSE_STATUS.OK:
                # Create VNF-packages
                if self.load_srv_cli:
                    files = [self.server_vnf_pkg_path, self.client_vnf_pkg_path]
                else:
                    files = [self.client_vnf_pkg_path]
                response_vnf_pkg = self.Instrument.upload_vnf_package(response_token['result']['token'], self.project_id, files)
                
                if response_vnf_pkg['status'] == RESPONSE_STATUS.OK:    
                    # Create NSD #nsd_id
                    response_nsd = self.Instrument.create_nsd(response_token['result']['token'], self.project_id, response_vnf_pkg['result']['vnfd_ids'])
                    
                    if response_nsd['status'] == RESPONSE_STATUS.OK:
                        # Launching NSD (=> creating VMs based on the package)  
                        client = {"vim_name": response_vim_instance['result']['vim_name'], "zone": self.client_zone}
                        if self.load_srv_cli:
                            server = {"vim_name": response_vim_instance['result']['vim_name'], "zone": self.server_zone}
                        else:
                            server = None
                        
                        response_launch_nsd = self.Instrument.launch_nsd(response_token['result']['token'], 
                                                    response_nsd['result']['nsd_id'], 
                                                    self.project_id, 
                                                    client, server, 
                                                    response_vnf_pkg['result']['vnf_pkg_names'])
                        if response_launch_nsd['status'] == RESPONSE_STATUS.OK:
                            self.Info('OpenBaton Resources Allocation completed successfully.')
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