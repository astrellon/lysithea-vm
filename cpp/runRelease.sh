#!/bin/sh
./buildRelease.sh
if [ $? -eq 0 ]; then
    cd ./Release
    ./perfTest
    # ./standardLibraryTest
    # ./dialogueTree
    cd ../
fi
