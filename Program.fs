
open FsharpTodo
open Commands
open Database

[<EntryPoint>]
let main argv =
    try
        getCommand (List.ofArray argv)
        |> parseFlags
        |> executeCommand
        |> printResponse
        |> handleUpdateTasks
        |> ignore
       
        0
    with
    | ex ->
        printfn $"Error: %s{ex.Message}"
        1


// add "test"
// add "test" 0
// add -d | --desc "test" -p | --priority 0
// get 1
// get -i | --incomplete
// list
// list 10
// list 10 -p | --priority >0 | <0 | 0
// remove 1
// update 1 -d | --desc "test -p | --priority 0
// complete 1
