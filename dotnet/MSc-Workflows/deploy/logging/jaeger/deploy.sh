#!/bin/bash

kubectl apply -f jaegertracing.io_jaegers_crd.yaml

kubectl apply -f service_account.yaml

kubectl apply -f role.yaml

kubectl apply -f role_binding.yaml

kubectl apply -f operator.yaml

kubectl apply -f cluster_role.yaml

kubectl apply -f cluster_role_binding.yaml