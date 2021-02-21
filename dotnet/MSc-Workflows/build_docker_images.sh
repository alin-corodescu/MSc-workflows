#!/bin/bash

eval $(minikube -p minikube docker-env)

cd ComputeSteps/Compute
pwd
docker build . -t compute:latest

cd ../../Orchestrator/OrchestratorService
pwd
docker build . -t orchestrator-service:latest

cd ../Sidecar
pwd
docker build . -t sidecar:latest

cd ../../StorageAdapters/Adapters
pwd
docker build . -t storage-adapter:latest

cd ../DataMaster
pwd
docker build . -t data-master:latest