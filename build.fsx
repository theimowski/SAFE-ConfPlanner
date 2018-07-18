#r @"paket: groupref Build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
  #r "netstandard" // Temp fix for https://github.com/fsharp/FAKE/issues/1985
  #r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open System
open System.IO

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators

open SAFE.Build

let SAFE = SAFEBuild (fun x ->
    { x with
        JsDeps = Yarn
    } )


let testsPath = "./src/Domain.Tests" |> Path.getFullName

let supportPath = "./src/Support" |> Path.getFullName


// Pattern specifying assemblies to be tested using expecto
// let clientTestExecutables = "test/UITests/**/bin/**/*Tests*.exe"


// --------------------------------------------------------------------------------------
// END TODO: The rest of the file includes standard build steps
// --------------------------------------------------------------------------------------


let dotnetExePath = "dotnet"

let runDotnet workingDir args =
    let result =
        Process.execSimple (fun info ->
            { info with
                FileName = dotnetExePath
                WorkingDirectory = workingDir
                Arguments = args }
        ) TimeSpan.MaxValue
    if result <> 0 then failwithf "dotnet %s failed" args

// --------------------------------------------------------------------------------------
// Clean build results

Target.create "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj/"
    ++ "test/**/bin"
    ++ "test/**/obj/"
    |> Shell.cleanDirs

    !! "src/**/obj/*.nuspec"
    ++ "test/**/obj/*.nuspec"
    |> Seq.iter Shell.rm

    Shell.cleanDirs ["bin"; "temp"; "docs/output"; "./src/Clientpublic/bundle"]
)


Target.create "InstallDotNetCore" (fun _ ->
    let dotnetcliVersion = DotNet.getSDKVersionFromGlobalJson()
    let setParams (options : DotNet.CliInstallOptions) =
        { options with
            Version = DotNet.Version dotnetcliVersion }

    DotNet.install setParams |> ignore
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target.create "BuildTests" (fun _ ->
    runDotnet testsPath "build"
)

Target.create "RunTests" (fun _ ->
    runDotnet testsPath "test"
)

Target.create "InstallClient" (fun _ ->
    SAFE.RestoreClient ()
)

Target.create "BuildClient" (fun _ ->
    SAFE.BuildClient ()
)

Target.create "RunFixtures" (fun _ ->
    runDotnet supportPath "run"
)


// --------------------------------------------------------------------------------------
// Run the Website

let ipAddress = "localhost"
let port = 8080

// FinalTarget "KillProcess" (fun _ ->
//     killProcess "dotnet"
//     killProcess "dotnet.exe"
// )


Target.create "Run" (fun _ ->
    [ SAFE.RunServer; SAFE.RunClient; SAFE.RunBrowser ]
    |> SAFE.RunInParallel
)


// -------------------------------------------------------------------------------------
Target.create "Build" ignore
Target.create "All" ignore

open Fake.Core.TargetOperators

"Clean"
  ==> "InstallDotNetCore"
  ==> "InstallClient"
  ==> "BuildClient"
  ==> "RunTests"
  ==> "All"

"BuildClient"
  ==> "Build"

"InstallClient"
  ==> "Run"

"BuildTests"
  ==> "RunTests"

Target.runOrDefault "All"
