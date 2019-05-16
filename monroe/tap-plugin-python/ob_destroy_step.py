
'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 04.03.2019 '''

__author__ = 'rsh'
__version__ = '0.0.1'

from .imports import *
from .instrument import *
from .ob_destroy_methods import OB_RESOURCES_DEALLOCATE
from .static_methods import STATIC_METHODS

@Attribute(Keysight.Tap.DisplayAttribute, "Destroy VNF", Description="Destroy VNF Resources", Group="Open Baton")
@Attribute(Keysight.Tap.AllowAnyChildAttribute)
class OpenBatonDestroyResource(TestStep):
    def __init__(self):
        super(OpenBatonDestroyResource, self).__init__()
        
        # Add the Instrument
        self.AddProperty("Instrument", None, OpenBatonInstrument).AddAttribute(Keysight.Tap.DisplayAttribute, "OpenBaton Instrument", "OpenBaton Instrument", Group="Instrument", Order=1)

        # VNF Configuration
        delete_all_vnf_prop = self.AddProperty("delete_all_vnfs", True, Boolean)
        delete_all_vnf_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "Release all VNFs", "Release all VNFs resources", Group="Release VNF", Order=2.1)

        delete_vnf_prop = self.AddProperty("delete_vnf_id", None, String)
        delete_vnf_prop.AddAttribute(Keysight.Tap.DisplayAttribute, "VNF ID", "VNF ID of resource to release", Group="Release VNF", Order=2.2)
        delete_vnf_prop.AddAttribute(Keysight.Tap.EnabledIfAttribute, "delete_all_vnfs", False, HideIfDisabled=True)
        

    def Run(self):
        super(OpenBatonDestroyResource, self).Run()

        ob_credentials, _, _ = self.Instrument.get_all_credentials()
        ob_res_deallocate = OB_RESOURCES_DEALLOCATE(ob_credentials, self.Debug, self.Info, self.Error)
        ob_token, project_id = STATIC_METHODS.get_header_configs()

        ob_res_deallocate.set_header_configs(ob_token, project_id)

        if self.delete_all_vnfs:
            self.delete_vnf_id = None

        # Delete NSR
        response_delete_nsr = ob_res_deallocate.delete_nsr(STATIC_METHODS.get_nsr_ids(self.delete_vnf_id))
        if response_delete_nsr['status'] == RESPONSE_STATUS.OK:
            STATIC_METHODS.del_nsr_id(self.delete_vnf_id)
            time.sleep(1)
            # Delete NSD
            response_delete_nsd = ob_res_deallocate.delete_nsd(STATIC_METHODS.get_nsd_ids(self.delete_vnf_id))
            if response_delete_nsd['status'] == RESPONSE_STATUS.OK:
                STATIC_METHODS.del_nsd_id(self.delete_vnf_id)
                time.sleep(1)        
                # Delete VNF Packages
                response_delete_vnf_pkgs = ob_res_deallocate.delete_vnf_packages(STATIC_METHODS.get_vnf_pkg_ids(self.delete_vnf_id))
                if response_delete_vnf_pkgs['status'] == RESPONSE_STATUS.OK:
                    STATIC_METHODS.del_vnf_pkg_id(self.delete_vnf_id)
                    time.sleep(1)
                    # Delete VIM Instance
                    if STATIC_METHODS.get_nsr_length() == 0:
                        response_delete_vim_instance = ob_res_deallocate.delete_vim_instance(STATIC_METHODS.get_vim_inst_ids(self.delete_vnf_id))
                        if response_delete_vim_instance['status'] == RESPONSE_STATUS.OK:
                            STATIC_METHODS.del_vim_inst_id(self.delete_vnf_id)
                            self.Info('OpenBaton Resources Destroy completed successfully.')
                            self.UpgradeVerdict(Keysight.Tap.Verdict.Pass)
                        else:
                            self.Error('Cannot delete VIM Instance.')
                            self.UpgradeVerdict(Keysight.Tap.Verdict.Error)
                    else:
                        self.Info("Active NSR is still present bound to current VIM Instance ID. So, could not delete VIM instance ID.")
                        self.UpgradeVerdict(Keysight.Tap.Verdict.Pass)
                else:
                    self.Error('Cannot delete VNF Packages.')
            else:
                self.Error('Cannot delete NSD.')
        else:
            self.Error('Cannot delete NSR.')