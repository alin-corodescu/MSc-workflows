#!/bin/bash


# This experiment is going to run the all up experiments (number 6 and 7)
# This one needs to be run on 1 cluster - with data locality on (at least) and daemonset connection to edge1.

# The data count varies
dataCounts=(3 6 9 12)
baseFileName="continuum-$1"
outDir=`pwd`

binDir="/home/alin/projects/MSc-workflows/dotnet/MSc-Workflows/tests/LoadGenerator/bin/Debug/net5.0"
telReaderDir="/home/alin/projects/MSc-workflows/dotnet/MSc-Workflows/tests/TelemetryReader/bin/Debug/net5.0"


for d in ${dataCounts[@]}; do
    echo "Running experiment for dataSize = $d"
    fileName="$outDir/$baseFileName-c$d.csv"
    echo $fileName

    cd $binDir
    dotnet LoadGenerator.dll \
        DataInjectorUrl="localhost:5432" \
        DataSize="1048576" \
        DataCount="$d" \
        Iterations="1"

    echo "Sleeping for 20 seconds to allow the execution to finish"
    sleep 20

    cd $telReaderDir
    dotnet TelemetryReader.dll \
        outputPath="$fileName" \
        ArchiveTraces="true" 
done

cd $outDir