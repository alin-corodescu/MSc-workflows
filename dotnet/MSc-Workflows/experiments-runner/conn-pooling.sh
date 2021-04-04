#!/bin/bash


# This experiment is going to run the all up experiments (number 6 and 7)
# This one needs to be run on 2 clusters:
# 1. With ConnectionPooling off
# 2. With ConnectionPooling of

# The data size -- doesn't acutally vary.
dataSizes=(1024)
baseFileName="cp-$1"
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
        DataCount="$1" \
        Iterations="10"

    echo "Sleeping for 20 seconds to allow the execution to finish"
    sleep 20

    cd $telReaderDir
    dotnet TelemetryReader.dll \
        outputPath="$fileName" \
        ArchiveTraces="true" 
done

cd $outDir