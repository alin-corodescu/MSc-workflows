#!/bin/bash

mkdir -p -m 777 /mnt/store/inputs
mkdir -p -m 777 /mnt/store/perm_storage
mkdir -p -m 777 /mnt/store/outputs

chmod 777 /mnt/store

ln -s /mnt/store /store

