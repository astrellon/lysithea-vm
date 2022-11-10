# Standard Misc Library

A handful of generic functions that aren't operators.

### `(print ...args)`
```lisp
; Prints all function arguments to the console.
; @param ...args value[]
; @returns nothing

(define name "Alan")
(print "Hello " 5 ": " name)

; Outputs
Hello 5: Alan
```

### `(typeof value)`
```lisp
; Returns the type name of the value
; @param input value
; @returns string

(define name "Alan")
(define year 2022)
(print (typeof name) ": " (typeof year))

; Outputs
string: number
```

### `(toString value)`
```lisp
; Turns the argument into a string
; @param input value
; @returns string

(define year 2022)
(print year ": " (typeof year))

(define yearStr (toString year))
(print yearStr ": " (typeof yearStr))

; Outputs
2022: number
2022: string
```

### `(compareTo value1 value2)`
```lisp
; Compares two values returning either -1, 0 or 1.
; Can be used for sorting and checking if two values are the same.
; @param value1 value
; @param value2 value
; @returns number

(print (compareTo 5 10))
(print (compareTo 10 5))
(print (compareTo 10 10))

; Outputs
-1
1
0
```
