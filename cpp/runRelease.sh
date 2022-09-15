#!/bin/sh
./buildRelease.sh
if [ $? -eq 0 ]; then
    cd ./Release
    ./dialogueTree
    cd ../
fi
