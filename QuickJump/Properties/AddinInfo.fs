namespace QuickJump
open System

open Mono.Addins
open Mono.Addins.Description

[<assembly: Addin(
    "QuickJump",
    Namespace = "QuickJump",
    Version = "1.0.0.2"
)>]

[<assembly: AddinName("QuickJump")>]
[<assembly: AddinCategory("IDE extensions")>]
[<assembly: AddinDescription("An EasyMotion clone for Xamarin Studio")>]
[<assembly: AddinUrl("https://github.com/nosami/QuickJump")>]
[<assembly: AddinAuthor("jason")>]
()
