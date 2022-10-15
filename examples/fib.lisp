(define fib (function (n)
    (if (<= n 1)
        (return n)
        (return (+ (fib (- n 2)) (fib (- n 1))))
    )
))

(print (fib 9))