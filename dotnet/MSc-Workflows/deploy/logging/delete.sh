#!/bin/bash

echo "Deleting the elastic search service...\n"

kubectl delete -f elastic-search-service.yaml

sleep 1

echo "Deleting the elastic search deployment...\n"

kubectl delete -f elastic-search-deployment.yaml

sleep 1

echo "Deleting the kibana service + deployment...\n"

kubectl delete -f kibana.yaml

sleep 1

echo "Deleting the fluentd config map\n"

kubectl delete -f fluentd-config-map.yml

sleep 1

echo "Deleting the fluend daemonset..\n"

kubectl delete -f fluent.yaml

sleep 1