module FsharpTodo.Commands

open System
open Models
open Database
open Newtonsoft.Json
open Utils
open Tasks

let getCommand (args: string list) =
    match args.[0] with
    | "get" -> (Get, args.Tail)
    | "add" -> (Add, args.Tail)
    | "remove" -> (Remove, args.Tail)
    | "update" -> (Update, args.Tail)
    | _ -> (Invalid, args.Tail)
    
let parseFlags (command, args) =
    let rec parse (command, args) (acc: Map<string, string>) =
        match args with
            | ("-p" | "--priority") :: value :: tail ->
                parse (command, tail) (Map.add "priority" value acc)
            | ("-i" | "--incomplete") :: value :: tail ->
                parse (command, tail) (Map.add "incomplete" value acc)
            | ("-d" | "--description") :: value :: tail ->
                parse (command, tail) (Map.add "description" value acc)
            | ("-t" | "--take") :: value :: tail ->
                parse (command, tail) (Map.add "take" value acc)
            | _ -> (command, acc, args)
    parse (command, args) Map.empty
   
   
let getById (values, args, tasks) =
    try
        match args |> List.tryHead with
        | Some idStr ->
            let id = Int32.Parse(idStr)
            let task = [tasks |> List.find (fun t -> t.Id = id)]
            (values, args, task)
        | _ -> (values, args, tasks)
    with
    | :? FormatException as ex ->
        failwith $"Error parsing 'ID' value: %s{ex.Message}"
        
let selectTask (values, args, tasks) =
    try
        match args |> List.tryHead with
        | Some idStr ->
            let id = Int32.Parse(idStr)
            let selected = tasks |> List.find (fun t -> t.Id = id)
            (values, args, tasks, selected)
        | _ -> failwith $"Unable to find task with ID {id}"
    with
    | :? FormatException as ex ->
        failwith $"Error parsing 'ID' value: %s{ex.Message}"

let updateDescription (values, args, tasks, selected) =
    try 
        match values |> Map.tryFind "description" with
        | Some desc when not (String.IsNullOrWhiteSpace desc) ->
            let updatedTask = { selected with Description = desc }
            (values, args, tasks, updatedTask)
        | _ -> (values, args, tasks, selected)
    with
    | :? FormatException as ex ->
        raise ex

let executeGet (values: Map<string, string>, args: string list) : CommandResult =
    let tasks = readAndDeserializeFile dbPath

    let applyTake (values, _, tasks) =
            try
                match values |> Map.tryFind "take" with
                | Some countStr ->
                    let count = Int32.Parse(countStr)
                    let tasks = tasks |> Seq.truncate count |> Seq.toList
                    (values, args, tasks)
                | _ -> (values, args, tasks)
            with
            | :? FormatException as ex ->
                 failwith $"Error parsing 'take' value: %s{ex.Message}"

    let message =
        (values, args, tasks)
        |> getById
        |> applyTake
        |> (fun (_, _, tasks) -> serializeTasks tasks)

    (None, message)
      
            
let executeAdd (values: Map<string, string>, args) : CommandResult =
    let tasks = readAndDeserializeFile dbPath
    
    let createTask (values, args, tasks) =
        let newTask = {
            Id = List.length tasks + 1
            Description = ""
            Priority = 0
            CreatedOn = DateTime.Now
            ModifiedOn = DateTime.Now
            IsDone = false 
        }
        (values, args, tasks, newTask)
   
    let concatNewTask (_, _, tasks, newTask: Task) =
        (tasks @ [newTask], $"Task created with ID {newTask.Id}")

    let updatedTasks, message =
        (values, args, tasks)
        |> createTask
        |> updateDescription
        |> concatNewTask
        
    (Some updatedTasks, message)


let executeRemove (values: Map<string, string>, args) : CommandResult =
    let tasks = readAndDeserializeFile dbPath
    
    let removeTask (_, args, tasks) =
        try
            match args |> List.tryHead with
            | Some idStr ->
                let id = Int32.Parse(idStr)
                Some (List.filter (fun t -> t.Id = id) tasks), $"Task with ID {id} removed"
            | _ -> None, $"Unable to find task with ID {id}"
        with
        | :? FormatException as ex ->
            failwith $"Error parsing 'ID' value: %s{ex.Message}"
    
    let updatedTasks, message =
        (values, args, tasks)
        |> removeTask
        
    (updatedTasks, message)

let executeUpdate (values: Map<string, string>, args) : CommandResult =
    let tasks = readAndDeserializeFile dbPath
    
    let replaceTask (_, _, tasks, selected) =
        try
            let id = selected.Id
            let updatedTasks =
                tasks
                |> List.map (fun t -> if t.Id = id then selected else t)
            (updatedTasks, $"Task with ID {id} updated")
        with
        | ex -> raise ex
    
    let updatedTasks, message =
        (values, args, tasks)
        |> selectTask
        |> updateDescription
        |> replaceTask
       
    (Some updatedTasks, message)

let executeCommand (command: Command, values: Map<string,string>, args: string list) =
    match command with
    | Get -> executeGet (values, args)
    | Add -> executeAdd (values, args)
    | Remove -> executeRemove (values, args)
    | Update -> executeUpdate (values, args)
    | Invalid -> (None, $"Invalid command: {command}")
    
let printResponse (result: CommandResult) =
    printfn $"{snd result}"
    match fst result with
    | Some tasks -> Some tasks
    | None -> None
    
let handleUpdateTasks tasks: Task list option =
    match tasks with
    | Some tasks -> saveTasksToFile tasks; Some tasks
    | None -> None
        