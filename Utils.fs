module FsharpTodo.Utils

open System
open Models
open Newtonsoft.Json

let printTask task =
    printfn $"Task %d{task.Id}: %s{task.Description} (Created: {task.CreatedOn}, Modified: {task.ModifiedOn}, Done: %b{task.IsDone})"
    
let printTasks taskList =
    for task in taskList do
        printTask task

let tryParseId (idStr: string) =
    match Int32.TryParse idStr with
    | (true, id) -> Some id
    | _ -> None
    
let serializeTasks tasks =
    tasks
    |> List.map (fun t -> JsonConvert.SerializeObject(t))
    |> String.concat Environment.NewLine