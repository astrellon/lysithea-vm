#!/bin/sh
./buildRelease.sh
if [ $? -eq 0 ]; then
    cd ./Release
    ./standardLibraryTest
    cd ../
fi
