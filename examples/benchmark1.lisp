(define testString (function ()
    (define str "012 hello there")
    (set str (string.set str 1 "b"))
    (set str (string.insert str 1 "a"))
    (set str (string.removeAt str 0))
    (set str (string.removeAll str "2"))
    (set str (string.insert str -12 "c"))

    (define str1 (string.substring str 0 3))
    (define str2 (string.substring str -1 1))
    (define str3 (string.substring str -11 5))
))

(define i 0)
(loop (< i 100000)
    (testString)
    (set i (+ i 1))
)
