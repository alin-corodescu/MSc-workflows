# Big data workflow orchestration framework

The current repository hosts the code for a proof of concept big data workflow orchestration tool, realized as part of a master thesis project.

The novelty of the proposed framework is:
* Built-in data locality
* Ability to integrate different data management solutions through software containers
* Using long-lived containers to host the processing logic of the steps.

The framework is implemented in C#, using ASP.NET, is packaged using Docker containers and is run on Kubernetes clusters.

# Folder structure

* `/dotnet/MSc-Worfklows` is the root folder of C# solution.
* `/dotnet/MSc-Worfklows/deploy` contains a set of Kubernetes YAML files and deployments scripts to assist with the creation of the proposed solution in a Kubernetes cluster. The Docker images of the different components are publicly available on DockerHub. An additional configuration is provided to set up a multi-node KinD Kubernetes cluster on the local machine.
* `/dotnet/MSc-Worfklows/experiments-runner` contains helper scripts for benchmarking the solution.
* `/dotnet/MSc-Worfklows/build_docker_images.sh` is a helper script that packages the outputs of Debug builds of the projects into Docker images and pushes them to DockerHub.