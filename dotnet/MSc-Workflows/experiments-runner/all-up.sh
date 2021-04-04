#!/bin/bash


# This experiment is going to run the all up experiments (number 2 and 5)
# This one needs to be run on two clusters:
#  1. Everything off, 4 step
#  2. Everything off, 4 step.

# The data size will be varying.

# 1MB, 10MB, 50MB.
# dataSizes=(1048576 10485760 104857600)
dataSizes=(1024)
baseFileName="all-up-$1"
outDir=`pwd`

binDir="/home/alin/projects/MSc-workflows/dotnet/MSc-Workflows/tests/LoadGenerator/bin/Debug/net5.0"
telReaderDir="/home/alin/projects/MSc-workflows/dotnet/MSc-Workflows/tests/TelemetryReader/bin/Debug/net5.0"


for d in ${dataSizes[@]}; do
    echo "Running experiment for dataSize = $d"
    fileName="$outDir/$baseFileName-d$d.csv"
    echo $fileName

    cd $binDir
    dotnet LoadGenerator.dll \
        DataInjectorUrl="localhost:5432" \
        DataSize="$d" \
        DataCount="3" \
        Iterations="3"

    echo "Sleeping for 20 seconds to allow the execution to finish"
    sleep 20

    cd $telReaderDir
    dotnet TelemetryReader.dll \
        outputPath="$fileName" \
        ArchiveTraces="true" 
done

cd $outDir