#!/bin/bash

# Permissions for the orchestrator to read other pods
kubectl apply -f cluster-role.yaml

# The Orchestrator and the Data master service
kubectl apply -f services.yaml

sleep 3

# The Orchestration deployments
kubectl apply -f orchestration-deployments.yaml

# The DaemonSet of storage adapters
kubectl apply -f storage-adapter-edge1.yaml
kubectl apply -f storage-adapter-cloud1.yaml

# The first step
kubectl apply -f step1-deployment-edge1.yaml
kubectl apply -f step1-deployment-cloud1.yaml

# The second step
kubectl apply -f step2-deployment-edge1.yaml
kubectl apply -f step2-deployment-cloud1.yaml

# The third step
kubectl apply -f step3-deployment-edge1.yaml
kubectl apply -f step3-deployment-cloud1.yaml

# The fourth step.
kubectl apply -f step4-deployment-cloud1.yaml