(procedure main()
    (say "Here is the test dialogue tree")
    (say "What is your name? ")
    (getPlayerName)
    (say "Hello {playerName}, hope you are well.")

    (:start)

    (if (isShopEnabled)
        (
            (randomSay "Welcome to my shop" "Come on in! What can I do for you?")
            (choice "Open Shop" (async (openShop)))
        )
        (randomSay "Welcome stranger" "Hey, come on in")
    )

    (choice "Questions" (async (questions)))
    (choice "Good bye" (async (goodBye)))
    (waitForChoice)
)

(procedure questions()
    (:start)

    (say "What do you want to know?")
    (choice "Who are you?" (async
        (say "I'm just a person")
        (jump :start)
    ))
    (choice "Where am I?" (async
        (say "Approximately here")
        (jump :start)
    ))

    (if (isShopEnabled)
        (choice "Can you open the shop?" (async
            (say "Yea sure thing")
            (openTheShop)
            (jump :start)
        ))
    )

    (choice "I've asked enough questions" (async
        (jump main:start)
    ))
    (waitForChoice)
)

(procedure goodBye()
    (randomSay "Catch you around!" "See you later {playerName}!")
)

