#!/bin/bash

echo "Creating the elastic search service...\n"

kubectl apply -f elastic-search-service.yaml

sleep 5

echo "Creating the elastic search deployment...\n"

kubectl apply -f elastic-search-deployment.yaml

sleep 5

echo "Creating the kibana service + deployment...\n"

kubectl apply -f kibana.yaml

sleep 5

echo "Creating the fluentd config map\n"

kubectl apply -f fluentd-config-map-containerd.yml

sleep 5

echo "Creating the fluend daemonset..\n"

kubectl apply -f fluent.yaml

sleep 5
