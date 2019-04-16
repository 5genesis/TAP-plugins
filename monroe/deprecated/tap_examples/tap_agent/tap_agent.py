'''

Copyright (C) 2019 Fraunhofer FOKUS

'''

# Install packages

# apt install python-pip

# pip install flask
# pip install flask_restful
# pip install pingparsing
# pip install iperf3

# debian specific
# apt-get install libiperf0

''' Port Info

REST API Listener at port 5000 (but can be configured)
Iperf server listens at port 5201
'''


''' Created by Ranjan Shrestha (ranjan.shrestha@fokus.fraunhofer.de) on 10.1.2019 '''


from flask import Flask, request, jsonify
from flask_restful import Api, Resource, reqparse
import json
import pingparsing
import iperf3


AGENT_PORT = 5000

app = Flask(__name__)
api = Api(app)


''' Ping Experiment '''
class Ping(Resource):
	def __init__(self):
		pass

	'''
	GET request is handled 
	@return object that contains only RTT average; but can be further extended if required
	'''
	def get(self):
		print 'Ping Experiment: A GET request is received'
		host = request.args.get('host')
		req_count = request.args.get('pingReqNum')
		result = self.get_ping_result(host, req_count)
		
		return jsonify({'rtt_avg': result['rtt_avg']})
	
	'''
	Produces a set of ping results as a json object
	'''
	def get_ping_result(self, host, count = 5):
		ping_parser = pingparsing.PingParsing()
		transmitter = pingparsing.PingTransmitter()
		transmitter.destination_host = host
		transmitter.count = count
		result = transmitter.ping()
		parse_result = ping_parser.parse(result).as_dict()

		return parse_result


''' Iperf Experiment '''
class Iperf(Resource):
	def __init__(self):
		pass

	def get (self):
		pass


	def post(self):
		srv_ip = None
		cli_ip = None
		set_as = None

		if 'srv_ip' in request.form:
			srv_ip = request.form['srv_ip']

		if 'cli_ip' in request.form:
			cli_ip = request.form['cli_ip']

		if 'set_as' in request.form:
			set_as = request.form['set_as']

		if set_as == "client":
			response = self.client(srv_ip)
			return jsonify({'throughput': response})
		elif set_as == "server":
			response = self.server()
			return response
		else:
			print ('Unknown args')
			return -1
		
	# Iperf client
	def client(self, srv_ip):
		if srv_ip == None:
			print ("Invalid parameter")
			return -1

		print ("The Iperf client is running...")
		client = iperf3.Client()
		client.duration = 1
		client.server_hostname = srv_ip
		client.port = 5201
		result = client.run()

		if result.error:
			print(result.error)
			return -1
		else:	
			print("The throughput is " + str(result.sent_Mbps) + " Mbps")
			return result.sent_Mbps


	# Iperf server
	def server(self):
		print ("The Iperf server is running...")
		server = iperf3.Server()
		result = server.run()
		
# Register the paths
api.add_resource(Ping, "/api/ping")
api.add_resource(Iperf, "/api/iperf")

# Agent listens on port AGENT_PORT
if __name__ == "__main__":
	app.run(host='0.0.0.0', port=AGENT_PORT, debug=True, threaded=True)
