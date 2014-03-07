#r "System.IO.Compression.FileSystem.dll"
#r "System.IO.Compression.dll"

open System
open System.IO
open System.IO.Compression
open System.Net
open System.Diagnostics

let currentDir = Environment.CurrentDirectory
let packages = currentDir + "\\packages"
let nuget = currentDir + "\\nuget.exe"
let refs = currentDir + "\\refs.fsx"
let chromeDriverSource = "http://chromedriver.storage.googleapis.com/2.9/chromedriver_win32.zip"
let ieDriverSource = "http://selenium-release.storage.googleapis.com/2.40/IEDriverServer_x64_2.40.0.zip"
let zipFile = currentDir + "\\zipfile.zip"

let writeRefLine (line : string) =
    use fileWriter = File.AppendText refs
    fileWriter.WriteLine line

let startProcess fileName arguments =
    let processStartInfo = ProcessStartInfo(FileName = fileName, Arguments = arguments, UseShellExecute = false, CreateNoWindow = true)
    let pr = Process.Start processStartInfo
    pr.WaitForExit()

let downloadPackage package =
    printfn "Installing '%s'" package
    startProcess nuget <| sprintf "install %s -ExcludeVersion -OutputDirectory %s" package packages

let deleteIfExists path =
    printfn "Deleting '%s'" path
    if (Directory.Exists path) then Directory.Delete(path, true) else File.Delete path

let download source destination =
    printfn "Downloading '%s'" source
    use webClient = new WebClient(UseDefaultCredentials = true, Proxy = WebRequest.DefaultWebProxy)
    webClient.Proxy.Credentials <- CredentialCache.DefaultCredentials
    webClient.DownloadFile(source, destination)

let downloadAndUnzip source =
    download source zipFile
    ZipFile.ExtractToDirectory(zipFile, packages)
    deleteIfExists zipFile

deleteIfExists refs
deleteIfExists packages

download "https://nuget.org/nuget.exe" nuget
downloadPackage "canopy"
downloadPackage "phantomjs.exe"
downloadPackage "FSharp.Data"
deleteIfExists nuget

printfn "Installing 'ChromeDriver'"
downloadAndUnzip chromeDriverSource

printfn "Installing 'IEDriverServer'"
downloadAndUnzip ieDriverSource

printfn "Creating 'refs.fsx'"
let libs = [@"Newtonsoft.Json\lib\net40\Newtonsoft.Json.dll";
            @"Selenium.WebDriver\lib\net40\WebDriver.dll";
            @"Selenium.Support\lib\net40\WebDriver.Support.dll";
            @"SizSelCsZzz\lib\SizSelCsZzz.dll";
            @"canopy\lib\canopy.dll";
            @"FSharp.Data\lib\net40\FSharp.Data.dll";
            @"FSharp.Data\lib\net40\FSharp.Data.DesignTime.dll"]

libs |> Seq.map (fun x -> sprintf @"#r @""packages\%s""" x) |> Seq.iter writeRefLine

writeRefLine "open canopy"
writeRefLine <| sprintf @"configuration.chromeDir <- @""%s""" packages
writeRefLine <| sprintf @"configuration.ieDir <- @""%s""" packages
writeRefLine <| sprintf @"configuration.phantomJSDir <- @""%s\phantomjs.exe\tools\phantomjs""" packages

printfn "Done"
