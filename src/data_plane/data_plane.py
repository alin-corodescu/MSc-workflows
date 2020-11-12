
import flask
from flask import request, jsonify
import uuid

app = flask.Flask(__name__)
app.config["DEBUG"] = True

data = {}

@app.route('/api/v1/data', methods = ['GET'])
def get_all():
    return jsonify(data)

@app.route('/api/v1/data', methods = ['POST'])
def post_new_chunk():
    unique_id = uuid.uuid1()
    data[str(unique_id)] = request.get_json()
    return jsonify(unique_id)

@app.route('/api/v1/data/<id>', methods = ['GET'])
def get_metadata(id):
    return jsonify(data[id])

app.run()