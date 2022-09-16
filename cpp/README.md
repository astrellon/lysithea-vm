Simple Stack Virtual Machine C++
=
The C++ port of the simple stack VM. Makes use of `std::variant` and `std::optional` which does give it the minimum requirement of C++17. Currently it's only been tested on GCC and Clang on Linux and macOS.

Release Build
-
```sh
$ ./buildRelease.sh
```

Then under the `Release` folder there should be several executables. The `controlApp` is a small test program to vaguely compare the performance difference between `perfTest` and a pure C++ program. It's not written in a way that really makes sense for a purely C++ program but it attempts to look similar to the simple stack program.

Debug Build
-
To debug with VSCode you'll have to build the debug binaries, then the launch tasks will work.
```sh
$ ./buildDebug.sh
```
