(define fib (function (n)
    (if (lessEquals n 1)
        (return n)
        (return (add (fib (sub n 2)) (fib (sub n 1))))
    )
))

(print (fib 9))