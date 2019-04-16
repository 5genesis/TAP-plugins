
'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 04.03.2019 '''

__author__ = 'rsh'
__version__ = '0.0.1'

from .imports import *
from .instrument import *

@Attribute(Keysight.Tap.DisplayAttribute, "OpenBaton Resources Destroy", Description="Destroy Resources", Group="Berlin Platform")
@Attribute(Keysight.Tap.AllowAnyChildAttribute)
class OpenBatonDestroyResource(TestStep):
    def __init__(self):
        super(OpenBatonDestroyResource, self).__init__()
        
        # Add the Instrument
        self.AddProperty("Instrument", None, Instrument).AddAttribute(Keysight.Tap.DisplayAttribute, "Instrument", "Instrument", Group="Instrument", Order=1)
        

    def Run(self):
        super(OpenBatonDestroyResource, self).Run()
        # Delete NSR
        response_delete_nsr = self.Instrument.delete_nsr()
        if response_delete_nsr['status'] == RESPONSE_STATUS.OK:
            time.sleep(1)
            # Delete NSD
            response_delete_nsd = self.Instrument.delete_nsd()
            if response_delete_nsd['status'] == RESPONSE_STATUS.OK:
                time.sleep(1)        
                # Delete VNF Packages
                response_delete_vnf_pkgs = self.Instrument.delete_multi_vnf_packages()
                if response_delete_vnf_pkgs['status'] == RESPONSE_STATUS.OK:
                    time.sleep(1)
                    # Delete VIM Instance
                    response_delete_vim_instance = self.Instrument.delete_vim_instance()
                    if response_delete_vim_instance['status'] == RESPONSE_STATUS.OK:
                        self.Info('OpenBaton Resources Destroy completed successfully.')
                        self.UpgradeVerdict(Keysight.Tap.Verdict.Pass)
                    else:
                        self.Error('Cannot delete VIM Instance.')
                        self.UpgradeVerdict(Keysight.Tap.Verdict.Error)
                else:
                    self.Error('Cannot delete VNF Packages.')
            else:
                self.Error('Cannot delete NSD.')
        else:
            self.Error('Cannot delete NSR.')