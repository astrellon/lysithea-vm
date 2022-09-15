cmake_minimum_required(VERSION 3.10)

# specify the C++ standard
set(CMAKE_C_COMPILER gcc)
set(CMAKE_CXX_COMPILER g++)
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED True)

set(CMAKE_CXX_FLAGS_RELEASE "-Ofast -funroll-loops -m64 -DRELEASE")

project(simple-stack-vm)

file(GLOB FILE_SRC
    "src/*.hpp"
    "src/*.cpp"
)

add_executable(perfTest ${FILE_SRC} perfTestMain.cpp)
add_executable(dialogueTree ${FILE_SRC} dialogueTreeMain.cpp)
add_executable(runCommands ${FILE_SRC} runCommandsMain.cpp)
add_executable(controlApp controlMain.cpp)