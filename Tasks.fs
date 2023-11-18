module FsharpTodo.Tasks

open System
open Models

let addTask (taskList: Task list) (task: Task) =
    task :: taskList

let removeTask (taskId: int) (tasks: Task list) =
    tasks |> List.filter (fun t -> t.Id <> taskId)

let updateTask (task: Task) (tasks: Task list) =
    tasks
    |> List.map (fun t -> if t.Id = task.Id then task else t)

let createTask (description: string, taskList: Task list) : Task =
    {
        Id = List.length taskList + 1
        Description = description
        IsDone = false
        Priority = 0
        CreatedOn = DateTime.Now
        ModifiedOn = DateTime.Now
    }