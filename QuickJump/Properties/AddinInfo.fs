namespace QuickJump
open System

open Mono.Addins
open Mono.Addins.Description

[<assembly: Addin(
    "QuickJump",
    Namespace = "QuickJump",
    Version = "1.0.0.3"
)>]

[<assembly: AddinName("QuickJump")>]
[<assembly: AddinCategory("IDE extensions")>]
[<assembly: AddinDescription("An EasyMotion clone for Xamarin Studio. Quickly jump anywhere on any open document.")>]
[<assembly: AddinUrl("https://github.com/nosami/QuickJump")>]
[<assembly: AddinAuthor("jason")>]
()
