
'''
Copyright (C) 2019 Fraunhofer FOKUS
'''

''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 04.03.2019 '''

__author__ = 'rsh'
__version__ = '0.0.1'

import sys
import tap
import clr
clr.AddReference("System.Collections")
from System.Collections.Generic import List
from tap import *

import Keysight.Tap
import math
from Keysight.Tap import Log, AvailableValuesAttribute, EnabledIfAttribute, FilePathAttribute, AvailableValuesAttribute, ShortNameAttribute
from System import Array, Double, Byte, Int32, String, Boolean, Enum #Import types to reference for generic methods
import System
import requests

from System.ComponentModel import BrowsableAttribute # BrowsableAttribute can be used to hide things from the user.

import enum
import sys
from array import array

class RESPONSE_STATUS(enum.Enum):
	OK = 1
	ERROR = 2
