#!/bin/bash

# eval $(minikube -p minikube docker-env)

cd ComputeSteps/Compute
pwd
docker build . -t compute:latest

docker tag compute:latest alincorodescu/msc-workflows-compute:latest
docker push alincorodescu/msc-workflows-compute:latest

# kind load docker-image compute:latest --name=workflow-cluster

cd ../../Orchestrator/OrchestratorService
pwd
docker build . -t orchestrator-service:latest

docker tag orchestrator-service:latest alincorodescu/msc-workflows-orchestrator:latest
docker push alincorodescu/msc-workflows-orchestrator:latest
# kind load docker-image orchestrator-service:latest --name=workflow-cluster

cd ../Sidecar
pwd
docker build . -t sidecar:latest
docker tag sidecar:latest alincorodescu/msc-workflows-sidecar:latest
docker push alincorodescu/msc-workflows-sidecar:latest
# kind load docker-image sidecar:latest --name=workflow-cluster

cd ../../StorageAdapters/Adapters
pwd
docker build . -t storage-adapter:latest
docker tag storage-adapter:latest alincorodescu/msc-workflows-storage-adapter:latest
docker push alincorodescu/msc-workflows-storage-adapter:latest
# kind load docker-image storage-adapter:latest --name=workflow-cluster

cd ../DataMaster
pwd
docker build . -t data-master:latest
docker tag data-master:latest alincorodescu/msc-workflows-data-master:latest
docker push alincorodescu/msc-workflows-data-master:latest
# kind load docker-image data-master:latest --name=workflow-cluster

cd ../../tests/BlobStorageTester
pwd
docker build . -t storage-tester:latest

docker tag storage-tester:latest alincorodescu/msc-workflows-storage-tester:latest
docker push alincorodescu/msc-workflows-storage-tester:latest

cd ../GrpcService
pwd
docker build . -t grpc-test:latest

docker tag grpc-test:latest alincorodescu/msc-workflows-grpc-test:latest
docker push alincorodescu/msc-workflows-grpc-test:latest