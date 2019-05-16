
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


''' Open Baton TAP Instrument class
    Base class is Instrument from TAP
'''

@Attribute(Keysight.Tap.DisplayAttribute, "OpenBaton", "Interface to OpenBaton and OpenStack", "Open Baton")
@Attribute(ShortNameAttribute, "OpenBaton")        
class OpenBatonInstrument(Instrument):
    def __init__(self):
        super(OpenBatonInstrument, self).__init__()

        # Openbaton IP Address
        ob_ip = self.AddProperty("openbaton_ip", "127.0.0.1", String)
        ob_ip.AddAttribute(Keysight.Tap.DisplayAttribute, "IP Address", "OpenBaton IP Address", Group="Open Baton", Order=1.1)
    
        # Openbaton Port
        ob_port = self.AddProperty("openbaton_port", 8080, Int32)
        ob_port.AddAttribute(Keysight.Tap.DisplayAttribute, "Port", "OpenBaton Port", Group="Open Baton", Order=1.2)

        # Open Baton Username
        ob_username = self.AddProperty("openbaton_username", "admin", String)
        ob_username.AddAttribute(Keysight.Tap.DisplayAttribute, "Username", "Open Baton Login: Username", Group="Open Baton", Order=1.3)

        # Open Baton Password
        ob_password = self.AddProperty("openbaton_password", None, Security.SecureString)
        ob_password.AddAttribute(Keysight.Tap.DisplayAttribute, "Password", "Open Baton Login: Password", Group="Open Baton", Order=1.4)

        # OpenStack IP Address
        os_ip = self.AddProperty("openstack_ip", "127.0.0.1", String)
        os_ip.AddAttribute(Keysight.Tap.DisplayAttribute, "IP Address", "OpenStack IP Address", Group="Open Stack", Order=2.1)
    
        # OpenStack Port
        os_port = self.AddProperty("openstack_port", 5000, Int32)
        os_port.AddAttribute(Keysight.Tap.DisplayAttribute, "Port", "OpenStack Port", Group="Open Stack", Order=2.2)  

        # OpenStack Username
        os_username = self.AddProperty("openstack_username", "admin", String)
        os_username.AddAttribute(Keysight.Tap.DisplayAttribute, "Username", "OpenStack Login: Username", Group="Open Stack", Order=2.3)

        # OpenStack Password
        os_password = self.AddProperty("openstack_password", None, Security.SecureString)
        os_password.AddAttribute(Keysight.Tap.DisplayAttribute, "Password", "OpenStack Login: Password", Group="Open Stack", Order=2.4)

        # OpenStack Tenant ID
        os_tenant_id = self.AddProperty("openstack_tenant_id", None, String)
        os_tenant_id.AddAttribute(Keysight.Tap.DisplayAttribute, "Tenant ID", "Tenant ID", Group="Open Stack", Order=2.5)

        # OpenStack KeyPair ID
        os_keypair_id = self.AddProperty("openstack_keypair_id", None, String)
        os_keypair_id.AddAttribute(Keysight.Tap.DisplayAttribute, "Keypair ID", "Keypair ID", Group="Open Stack", Order=2.6) 

        # Zabbix Server
        allow_zabbix_srv = self.AddProperty("allow_zabbix_srv", False, Boolean)
        allow_zabbix_srv.AddAttribute(Keysight.Tap.DisplayAttribute, "Enable Zabbix Server", "Enable Zabbix Server", Group="Monitoring Server", Order=3.1)

        zs_ip = self.AddProperty("zabbix_srv_ip", "127.0.0.1", String)
        zs_ip.AddAttribute(Keysight.Tap.DisplayAttribute, "Server IP", "Zabbix Server IP", Group="Monitoring Server", Order=3.2)
        zs_ip.AddAttribute(Keysight.Tap.EnabledIfAttribute, "allow_zabbix_srv", HideIfDisabled=True) 

    
    def get_all_credentials(self,):
        # convert the secure password to the plain string
        self.openstack_password = System.Net.NetworkCredential(System.String.Empty, self.openstack_password).Password
        self.openbaton_password = System.Net.NetworkCredential(System.String.Empty, self.openbaton_password).Password

        openbaton = {
            "openbaton_ip": self.openbaton_ip,
            "openbaton_port": self.openbaton_port,
            "openbaton_username": self.openbaton_username,
            "openbaton_password": self.openbaton_password,
        }

        openstack = {
            "openstack_ip": self.openstack_ip,
            "openstack_port": self.openstack_port,
            "openstack_username": self.openstack_username,
            "openstack_password": self.openstack_password,
            "openstack_tenant_id": self.openstack_tenant_id,
            "openstack_keypair_id": self.openstack_keypair_id,
        }

        zabbix = {
            "allow_zabbix_srv": self.allow_zabbix_srv,
            "zabbix_srv_ip": self.zabbix_srv_ip,
        }

        return openbaton, openstack, zabbix

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
        