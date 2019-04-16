# Copyright (C) Fraunhofer FOKUS

# Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de)

__author__ = 'rsh'

import os, json
import sys

rtt_avg = 0.
count_rtt = 0

# Check if user has specified a directory name 
l = len(sys.argv) # count the arguments number
if l == 1:
	dirnames = [name for name in os.listdir(".") if os.path.isdir(name)]
elif l == 2:
	dirnames = [sys.argv[1]]
else:
	print ('Too many parameters. Terminating the program.')
	sys.exit(0)

print ('List of directories  {}'.format(dirnames))
for dirname in dirnames:
	json_files = [pos_json for pos_json in os.listdir(''.join(['./', dirname])) if pos_json.endswith('.json')]
	print('List of files in the folder {}: {} '.format(dirname, json_files))
	for filename in json_files:
		filepath = ''.join(['./', dirname, '/', filename])
		with open(filepath, "r") as f:
			for line in f:
				json_data = json.loads(line)
				if 'Rtt' in json_data:
					rtt_avg += json_data['Rtt']
					count_rtt += 1
			f.close()

if count_rtt != 0:
	print ("The average RTT is {} ms".format(rtt_avg/count_rtt))
	print ("Total number of items: {}".format(count_rtt))
else:
	print ("Cannot calculate average RTT value")
