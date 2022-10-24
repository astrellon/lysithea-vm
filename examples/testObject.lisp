; A comment!
(define testPerson (function ()
    ; A second comment
    (define personObj { ; A third comment
        name "Alan"
        age 33
        address ("location 1" "location 2")
    })
    (print "PersonObj: " (toString personObj))

    (define person1 (newPerson "Alan" 33 ("location 1" "location 2")))

    (print person1.name " Hello " (+ 5 person1.age) " at " person1.address.1)

    (define person2 (newPerson "Pri" 30 ("location 3" "location 4")))

    (print person2.name " Hello " (+ 5 person2.age) " at " person2.address.0)

    (define person3 (combinePerson person1 person2))

    (print person3.name " HelloComb " (+ 5 person3.age) " at " person3.address.2)
    (print (toString person3))
))

(define main (function()
    (define pos (newVector 1 2 3))
    (define offset (newVector 5 10 15))

    (define result (pos.add offset))
    (define result2 (result.add offset))

    (print "Add Name: " (toString pos.add))
    (print "Pos: " pos)
    (print "Offset: " offset)
    (print "Result: " result)
    (print "Result2: " result2)
))

(main)