#!/bin/bash

# Permissions for the orchestrator to read other pods
kubectl delete -f cluster-role.yaml

# The Orchestrator and the Data master service
kubectl delete -f services.yaml

sleep 3

# The Orchestration deployments
kubectl delete -f orchestration-deployments.yaml

# The DaemonSet of storage adapters
kubectl delete -f storage-adapter-edge1.yaml
kubectl delete -f storage-adapter-cloud1.yaml

# The first step
kubectl delete -f step1-deployment-edge1.yaml
kubectl delete -f step1-deployment-cloud1.yaml

# The second step
kubectl delete -f step2-deployment-edge1.yaml
kubectl delete -f step2-deployment-cloud1.yaml

# The third step
kubectl delete -f step3-deployment-edge1.yaml
kubectl delete -f step3-deployment-cloud1.yaml

# The fourth step.
kubectl delete -f step4-deployment-cloud1.yaml