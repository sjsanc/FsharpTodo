module FsharpTodo.Database

open System
open System.IO
open System.Text.Json
open Models

let dbPath = "/home/sjsanc/work/FsharpTodo/db.json"
   
let readAndDeserializeFile (filePath: string) =
    use reader = new StreamReader(filePath)
    let fileContent = reader.ReadToEnd()
    if String.IsNullOrWhiteSpace(fileContent) then
        []
    else
        JsonSerializer.Deserialize<Task list>(fileContent)

let saveTasksToFile (taskList: Task list) =
        use writer = new StreamWriter(dbPath)
        writer.WriteLine(JsonSerializer.Serialize(taskList))
        