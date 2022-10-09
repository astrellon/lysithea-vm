(define main (procedure ()
    (say "Here is the test dialogue tree")
    (say "What is your name? ")
    (getPlayerName)
    (say "Hello {playerName}, hope you are well.")

    (:start)

    (if (isShopEnabled)
        (
            (randomSay ("Welcome to my shop {playerName}" "Come on in! What can I do for you {playerName}?"))
            (choice "Open Shop" openShop)
        )
        (randomSay ("Welcome stranger" "Hey, come on in"))
    )

    (choice "Questions" questions)
    (choice "Good bye" goodBye)
    (waitForChoice)
))

(define questions (procedure ()
    (:start)

    (say "What do you want to know?")
    (choice "Who are you?" (procedure ()
        (say "I'm just a person")
        (moveTo questions :start)
    ))
    (choice "Where am I?" (procedure ()
        (say "Approximately here")
        (moveTo questions :start)
    ))

    (unless (isShopEnabled)
        (choice "Can you open the shop?" (procedure ()
            (say "Yea sure thing")
            (openTheShop)
            (moveTo questions :start)
        ))
    )

    (choice "I've asked enough questions" (procedure ()
        (moveTo main :start)
    ))
    (waitForChoice)
))

(define goodBye (procedure ()
    (randomSay ("Catch you around!" "See you later {playerName}!"))
))

(main)