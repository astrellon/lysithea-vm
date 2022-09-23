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

file(GLOB STD_LIB_FILE_SRC
    "src/standard_library/*.hpp"
    "src/standard_library/*.cpp"
)

add_executable(perfTest ${FILE_SRC} perf_test_main.cpp)
add_executable(dialogueTree ${FILE_SRC} dialogue_tree_main.cpp)
add_executable(standardLibraryTest ${FILE_SRC} ${STD_LIB_FILE_SRC} standard_library_main.cpp)
add_executable(controlApp control_main.cpp)