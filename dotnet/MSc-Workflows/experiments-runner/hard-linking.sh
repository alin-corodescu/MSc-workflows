#!/bin/bash


# This experiment is going to run the all up experiments (number 6 and 7)
# This one needs to be run on 2 clusters:
# 1. With Hard Linking off
# 2. With Hard Linking on

# The data size varies
dataSizes=(1048576 10485760 52428800)
baseFileName="hl-$1"
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
        Iterations="3"

    read -p "Press enter once JaegerUI has all the traces it needs"

    cd $telReaderDir
    dotnet TelemetryReader.dll \
        outputPath="$fileName" \
        ArchiveTraces="true" 
done

cd $outDir