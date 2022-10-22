(define testArray (function ()
    (define arr (0 1 2))

    (set arr (array.set arr 1 "b"))
    (set arr (array.insertFlatten arr -1 ("c" "d")))
    (set arr (array.insert arr 0 "a"))
    (set arr (array.removeAt arr 1))
    (set arr (array.remove arr 2))

    (define arrlen (array.length arr))

    (define sub (array.sublist arr 0 2))

    (set sub (array.sublist arr 0 -1))
    (set sub (array.sublist arr -1 -1))
    (set sub (array.sublist arr -2 -1))
    (set sub (array.sublist arr -3 2))

    (define name "Foo")
    (define age 30)
    (define address ("location 1" "location 2"))

    (define arr2 (array.join name age address))
))

(testArray)
