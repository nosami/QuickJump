﻿namespace QuickJump

open System
open System.Collections.Generic
open System.Text.RegularExpressions
open MonoDevelop
open MonoDevelop.Components
open MonoDevelop.Components.Commands
open MonoDevelop.Core
open MonoDevelop.Core.Text
open MonoDevelop.Ide.Editor
open MonoDevelop.Ide.Editor.Extension
open Mono.TextEditor

type HintMarker(hintChar:char, matchChar:string, offset) =
    inherit TextSegmentMarker(offset, 1)

    let dummyEvent = DelegateEvent<_>()
    let tag = obj()

    override x.Draw (editor, g, metrics, _startOffset, _endOffset) =
        let line = editor.GetLineByOffset offset
        let location = editor.OffsetToLocation offset
        let x = editor.ColumnToX (line, location.Column) - editor.HAdjustment.Value + editor.TextViewMargin.XOffset + (editor.TextViewMargin.TextStartPosition |> float)
        let y = (editor.LineToY location.Line) - editor.VAdjustment.Value

        g.SetSourceColor(Cairo.Color(127.0, 0.0, 0.0))
        g.SelectFontFace("Source Code Pro", Cairo.FontSlant.Normal, Cairo.FontWeight.Bold)
        g.SetFontSize (editor.Options.Font.Size / 1024 |> float)
        let extent = g.TextExtents matchChar
        use layout = new Pango.Layout(editor.PangoContext)
        layout.FontDescription <- editor.Options.Font
        layout.SetText (hintChar |> string)
        let padding = 2.0
        g.Rectangle(x - padding , y, extent.Width + padding * 2.0, y + metrics.LineHeight)

        g.Fill ()

        g.SetSourceColor(Cairo.Color(255.0, 255.0, 255.0))
        g.MoveTo(x - 1.0, y)
        g.ShowLayout layout

    interface ITextSegmentMarker with
        member x.IsVisible with get() = false and set(_) = ()
        member x.Tag with get() = tag and set(_) = ()
        member x.Offset = offset
        member x.Length = 1
        member x.EndOffset = offset + 1
        [<CLIEvent>]
        member x.MousePressed = dummyEvent.Publish
        [<CLIEvent>]
        member x.MouseHover = dummyEvent.Publish

type QuickJumpState =
    | WaitingForTrigger
    | WaitingForInput
    | Input of cc:char

type QuickJump() as x =
    inherit TextEditorExtension()
    let mutable state: QuickJumpState = WaitingForTrigger
    let mutable markers = Dictionary<_,_>()

    let getEditorData() =
        x.Editor.GetContent<ITextEditorDataProvider>().GetTextEditorData()

    let lineMatches (lineNumber) =
        let line = x.Editor.GetLine lineNumber
        match state with
        | Input c ->
            let lineText = x.Editor.GetTextBetween(line.Offset, line.EndOffset)

            Regex.Matches(lineText, @"\b" + (c |> string), RegexOptions.IgnoreCase)
            |> Seq.cast
            |> Seq.map (fun (found:Match) -> line.Offset + found.Index)
        | _ -> Seq.empty

    let addMarker (hint:char, offset) =
        match state with
        | Input c ->
            let marker = HintMarker(hint, c |> string, offset)
            let doc = getEditorData().Document
            doc.AddMarker marker
            markers.Add(hint, marker)
        | _ -> ()

    let removeHints() =
        markers |> Seq.iter(fun kv -> x.Editor.RemoveMarker kv.Value |> ignore)
        markers.Clear()

    let hints = "abcdefghijklmnopqrstuvwxyz1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ"

    let addHints() =
        removeHints()
        let editor = getEditorData()
        let topVisibleLine = ((editor.VAdjustment.Value / editor.LineHeight) |> int) + 1
        let bottomVisibleLine =
            Math.Min(editor.LineCount - 1,
                topVisibleLine + ((editor.VAdjustment.PageSize / editor.LineHeight) |> int))

        [topVisibleLine..bottomVisibleLine]
        |> Seq.collect lineMatches
        |> Seq.sort
        |> Seq.zip hints
        |> Seq.iter addMarker

    [<CommandHandler("QuickJump.RunQuickJump")>]
    member x.RunQuickJump() = state <- WaitingForInput

    override x.KeyPress (descriptor:KeyDescriptor) =
        match descriptor.ModifierKeys, descriptor.KeyChar, state with
        | ModifierKeys.None, c, WaitingForInput ->
            state <- Input c
            addHints()
            false
        | ModifierKeys.None, c, Input _s ->
            if markers.ContainsKey(c) then
                x.Editor.CaretOffset <- markers.[c].Offset

            removeHints()
            state <- WaitingForTrigger
            false
        | _, _, _ ->
            removeHints()
            state <- WaitingForTrigger
            base.KeyPress descriptor
