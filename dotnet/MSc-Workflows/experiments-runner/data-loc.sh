#!/bin/bash


# This experiment is going to run the all up experiments (number 6 and 7)
# This one needs to be run on two clusters:
#  1. Everything off, 4 step
#  2. DL on, rest off, 4 step.

# The data count varies
dataCounts=(3 6 9 12)
baseFileName="data-loc-$1"
outDir=`pwd`

binDir="/home/alin/projects/MSc-workflows/dotnet/MSc-Workflows/tests/LoadGenerator/bin/Debug/net5.0"
telReaderDir="/home/alin/projects/MSc-workflows/dotnet/MSc-Workflows/tests/TelemetryReader/bin/Debug/net5.0"


for d in ${dataCounts[@]}; do
    echo "Running experiment for data count = $d"
    fileName="$outDir/$baseFileName-c$d.csv"
    echo $fileName

    cd $binDir
    dotnet LoadGenerator.dll \
        DataInjectorUrl="localhost:5432" \
        DataSize="1048576" \
        DataCount="$d" \
        Iterations="3"

    read -p "Press enter once JaegerUI has all the traces it needs"

    cd $telReaderDir
    dotnet TelemetryReader.dll \
        outputPath="$fileName" \
        ArchiveTraces="true" 

    read -p "Press enter to go to the next dataSize/count"
done

cd $outDir