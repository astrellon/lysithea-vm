#!/bin/sh
./buildRelease.sh
if [ $? -eq 0 ]; then
    cd ./Release
    # ./mapBenchmark
    # ./perfTest
    ./standardLibraryTest
    # ./dialogueTree
    cd ../
fi
