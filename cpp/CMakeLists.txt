cmake_minimum_required(VERSION 3.10)

# specify the C++ standard
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED True)

project(simple-stack-vm)

file(GLOB FILE_SRC
    "src/*.hpp"
    "src/*.cpp"
)

add_executable(app ${FILE_SRC} main.cpp)