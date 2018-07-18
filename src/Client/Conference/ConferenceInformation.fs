namespace Conference.ConferenceInformation

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.Core.JsInterop

open Fulma
open Fulma.FontAwesome
open Fulma.Color
open Fulma.Size

module Types =
  type Model =
    {
      Title : string
      AvailableSlotsForTalks : string
    }

  type Errors =
    {
      Title : string option
      AvailableSlotsForTalks : string option
    }

  let isInteger str =
    str
    |> System.Int32.TryParse
    |> function | (true, _) -> true | _ -> false

  let validate (model : Model) : Errors =
    let title =
      if model.Title |> System.String.IsNullOrEmpty then
        "Should not be empty" |> Some
      else
        None

    let availableSlotsForTalks =
      if model.AvailableSlotsForTalks |> System.String.IsNullOrEmpty then
        "Should not be empty" |> Some
      elif model.AvailableSlotsForTalks |> isInteger |> not then
        "Must be an integer" |> Some
      else
        None

    {
      Title = title
      AvailableSlotsForTalks = availableSlotsForTalks
    }

  let title (model : Model) =
    model.Title

  let availableSlotsForTalks (model : Model) =
    model.AvailableSlotsForTalks
    |> System.Int32.TryParse
    |> function | (true, value) -> value | _ -> 0

  let isValid model =
    let errors =
      model |> validate

    errors.Title.IsNone && errors.AvailableSlotsForTalks.IsNone


  type Msg =
    | TitleChanged of string
    | AvailableSlotsForTalksChanged of string


module View =
  open Types
  let private typeAndIconAndError error =
    match error with
    | Some error ->
        let help =
          Help.help
            [ Help.Color IsDanger ]
            [ str error ]

        Input.Color IsDanger,Fa.I.Times,help

    | None ->
        Input.Color IsSuccess,Fa.I.Check,str ""

  let private viewFormField changeMsg field error label =
    let inputType,inputIcon,inputError =
      error |> typeAndIconAndError

    Field.div []
      [
        Label.label [] [ str label ]
        Control.div
          [
             Control.HasIconRight
          ]
          [
            Input.input
              [
                inputType
                Input.Type Input.Text
                Input.Placeholder label
                Input.Value field
                Input.Props [ OnChange (fun event -> !!event.target?value |>changeMsg) ]
              ]
            Icon.faIcon
              [
                Icon.Size IsSmall
                Icon.IsRight
              ]
              [ Fa.icon inputIcon ]

          ]
        inputError
      ]

  let view dispatch model =
    let errors =
      model |> validate

    form [ ]
      [
        viewFormField
          (TitleChanged>>dispatch)
          model.Title
          errors.Title
          "Conference Title"

        viewFormField
          (AvailableSlotsForTalksChanged>>dispatch)
          model.AvailableSlotsForTalks
          errors.AvailableSlotsForTalks
          "Available Slots For Talks"
    ]

module State =
  open Types

  let init title availableSlotsForTalks : Model =
    {
      Title = title
      AvailableSlotsForTalks = availableSlotsForTalks
    }

  let update msg model : Model =
    match msg with
    | TitleChanged title ->
        { model with Title = title }

    | AvailableSlotsForTalksChanged number ->
        { model with AvailableSlotsForTalks = number }

