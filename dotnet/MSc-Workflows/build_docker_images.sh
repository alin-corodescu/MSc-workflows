#!/bin/bash

# eval $(minikube -p minikube docker-env)

cd ComputeSteps/Compute
pwd
docker build . -t compute:latest

kind load docker-image compute:latest --name=workflow-cluster

cd ../../Orchestrator/OrchestratorService
pwd
docker build . -t orchestrator-service:latest
kind load docker-image orchestrator-service:latest --name=workflow-cluster

cd ../Sidecar
pwd
docker build . -t sidecar:latest
kind load docker-image sidecar:latest --name=workflow-cluster

cd ../../StorageAdapters/Adapters
pwd
docker build . -t storage-adapter:latest
kind load docker-image storage-adapter:latest --name=workflow-cluster

cd ../DataMaster
pwd
docker build . -t data-master:latest
kind load docker-image data-master:latest --name=workflow-cluster