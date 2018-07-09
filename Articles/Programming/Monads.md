Monads are "programmable semicolons."
doThis();
doThat()
Do you just want to do input and output? That's the IO monad.
Do you want a Cartesian product of all the results returned from both computations, with guards to weed out bad combinations? That's the list monad.
Do you want the second action to be completely ignored if the first action fails? That's the Maybe monad.
Do you want the second action to not fire if the first one fails, but you also want to remember why the first one failed? That's the Either monad.
