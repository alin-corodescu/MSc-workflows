#!/bin/bash


# This experiment is going to run the all up experiments (number 2 and 5)
# This one needs to be run on two clusters:
#  1. Everything off, 4 step
#  2. Everything off, 4 step.

# The data size will be varying.

# 1MB, 10MB, 50MB.
# dataSizes=(1048576 10485760 104857600)
dataSizes=(104857600)
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
        DataCount="1" \
        Iterations="1"

    echo "Sleeping for 5s to allow Jaeger to collect the traces"
    sleep 5
    # read -p "Press enter once JaegerUI has all the traces it needs"

    cd $telReaderDir
    dotnet TelemetryReader.dll \
        outputPath="$fileName" \
        ArchiveTraces="true" 

    read -p "Press enter to go to the next dataSize/count"
done

cd $outDir