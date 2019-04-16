#!/usr/bin/env python

'''

Copyright (C) 2019 Fraunhofer FOKUS

'''

''' Adapted by Mohammad Rajiullah from Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) '''

# Install packages

# apt install python-flask

''' Port Info

REST API Listener at port 8080 (but can be configured)
'''




from flask import Flask, request, jsonify
from flask_restful import Api, Resource, reqparse
import json
import uuid
import time
from subprocess import check_output, CalledProcessError

AGENT_PORT = 8080

app = Flask(__name__)
api = Api(app)


''' Monroe Experiment '''
class Monroe(Resource):
	def __init__(self):
		pass

	'''
	GET request is handled 
	@return object that contains only RTT average; but can be further extended if required
	'''
	def get(self):
		print 'Monroe Experiment: A GET request is received'
                exp_name = "kau-virtualnode-dev-" + str(uuid.uuid4())
                print request.args.get('config')
                config_file = open("/experiments/user/"+exp_name+".conf", "w")


                config_file.write("%s" % request.args.get('config'))
                config_file.close()
		
                ##Monroe name space 
                try:
                    cmd=['/usr/bin/monroe-experiments']
                    output=check_output(cmd)
                except CalledProcessError as e:
                    resp=jsonify({'status': 503,'reasons': "Monroe name space is not available: " +exp_name })
                    resp.status_code = 503
                    return resp

		##Deploy 
                try:
                    cmd=['/usr/bin/container-deploy.sh',exp_name]
                    output=check_output(cmd)
                except CalledProcessError as e:
                    #return jsonify({'results': "Deploy failed: " +exp_name })
                    resp=jsonify({'status': 503,'reasons': "Deploy failed: " +exp_name })
                    resp.status_code = 503
                    return resp

		##Start
                try:
                    cmd=['/usr/bin/container-start.sh',exp_name]
                    output=check_output(cmd)
                except CalledProcessError as e:
                    resp=jsonify({'status': 503,'reasons': "Start failed: " +exp_name })
                    resp.status_code = 503
                    return resp

		time.sleep( int(request.args.get('duration')) ) 

		##Stop
                try:
                    cmd=['/usr/bin/container-stop.sh',exp_name]
                    output=check_output(cmd)
                except CalledProcessError as e:
                    resp=jsonify({'status': 503, 'reasons': "Stop failed: " +exp_name })
                    resp.status_code = 503
                    return resp


		#
		result = self.get_result(exp_name)
                message = {
                          'status': 200,
		          'results': "http://monroe-system.eu/user/"+exp_name
                          }
                resp = jsonify(message)
                return resp


	'''
	Produces a set of exp results as a json object
	'''
	def get_result(self, exp_name):
		#ping_parser = pingparsing.MonroeParsing()
		#transmitter = pingparsing.MonroeTransmitter()
		#transmitter.destination_host = host
		#transmitter.count = count
		#result = transmitter.ping()
		#parse_result = ping_parser.parse(result).as_dict()

		#return parse_result
                pass

		
# Register the paths
api.add_resource(Monroe, "/api/monroe")

# Agent listens on port AGENT_PORT
if __name__ == "__main__":
	app.run(host='0.0.0.0', port=AGENT_PORT, debug=True, threaded=True)
