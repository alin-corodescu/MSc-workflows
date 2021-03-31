#!/bin/bash

# Permissions for the orchestrator to read other pods
kubectl delete -f cluster-role.yaml

sleep 3

# The datamaster service
kubectl delete -f datamaster.yaml

sleep 3
# The orchestrator service
kubectl delete -f orchestrator-service.yaml

sleep 3
# The DaemonSet of storage adapters
kubectl delete -f storage-adapter.yaml

sleep 3
# The first step
kubectl delete -f step1.yaml

sleep 3
# The orchestrator pod
kubectl delete -f orchestrator-pod.yaml

sleep 3
# the data master pod
kubectl delete -f datamaster-pod.yaml