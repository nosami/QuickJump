namespace QuickJump
open System

open Mono.Addins
open Mono.Addins.Description

[<assembly: Addin(
    "QuickJump",
    Namespace = "QuickJump",
    Version = "1.0"
)>]

[<assembly: AddinName("QuickJump")>]
[<assembly: AddinCategory("IDE extensions")>]
[<assembly: AddinDescription("QuickJump")>]
[<assembly: AddinAuthor("jason")>]
()
