#!/bin/bash

# Permissions for the orchestrator to read other pods
kubectl apply -f cluster-role.yaml

sleep 3

# The datamaster service
kubectl apply -f datamaster.yaml

sleep 3
# The orchestrator service
kubectl apply -f orchestrator-service.yaml

sleep 3
# The DaemonSet of storage adapters
kubectl apply -f storage-adapter.yaml

sleep 3
# The first step
kubectl apply -f step1.yaml

sleep 3
# The orchestrator pod
kubectl apply -f orchestrator-pod.yaml

sleep 3
# the data master pod
kubectl apply -f datamaster-pod.yaml