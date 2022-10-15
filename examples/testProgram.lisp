(define name "Global")
(define main (function ()
    (print "Started main")
    (print name)

    (set name "Set from scope")
    (print name)

    (define name "Created in scope")
    (print name)
    (print "End main")
))

(print name)
(main)
(print name)