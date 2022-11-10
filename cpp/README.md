# C++
The C++ port of the Lysithea VM. Does not make use of any fancy C++ features beyond `std::shared_ptr`, mostly for the sake of simplicity.

This is likely to it's detriment as it's a mostly one-to-one port of the C# code apart from how it handles primitive values. As such it performs fairly similarly to the C# and almost certainly not as well as it could.

## Release Build
```sh
$ ./buildRelease.sh
```

Then under the `Release` folder there should be several executables. The `controlApp` is a small test program to vaguely compare the performance difference between `perfTest` and a pure C++ program. It's not written in a way that really makes sense for a purely C++ program but it attempts to look similar to the simple stack program.

## Debug Build
To debug with VSCode you'll have to build the debug binaries, then the launch tasks will work.
```sh
$ ./buildDebug.sh
```
