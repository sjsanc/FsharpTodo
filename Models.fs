module FsharpTodo.Models

open System
open Newtonsoft.Json
open Newtonsoft.Json.Converters

type Task = {
    Id: int
    Description: string
    IsDone: bool
    Priority: int
    CreatedOn: DateTime
    ModifiedOn: DateTime
}

type CommandResult = Task list option * string

type ActionArgs = Map<string, string> * string list * Task list * Task option

type Command =
    | Get
    | Add
    | Remove
    | Update
    | Invalid
    
  