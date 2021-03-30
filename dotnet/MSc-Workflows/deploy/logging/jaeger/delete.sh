#!/bin/bash

kubectl delete -f jaegertracing.io_jaegers_crd.yaml

kubectl delete -f service_account.yaml

kubectl delete -f role.yaml

kubectl delete -f role_binding.yaml

kubectl delete -f operator.yaml

kubectl delete -f cluster_role.yaml

kubectl delete -f cluster_role_binding.yaml