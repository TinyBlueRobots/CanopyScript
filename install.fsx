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
let demo = currentDir + "\\demo.fsx"
let chromeDriverSource = "http://chromedriver.storage.googleapis.com/2.8/chromedriver_win32.zip"
let ieDriverSource = "https://selenium.googlecode.com/files/IEDriverServer_x64_2.39.0.zip"
let zipFile = currentDir + "\\zipfile.zip"

let writeFile file (line : string) = 
  use fileWriter = File.AppendText file
  fileWriter.WriteLine line

let startProcess fileName arguments = 
  let processStartInfo = 
    ProcessStartInfo(FileName = fileName, Arguments = arguments, UseShellExecute = false, CreateNoWindow = true)
  let pr = Process.Start processStartInfo
  pr.WaitForExit()

let downloadPackage package = 
  printfn "Installing '%s'" package
  startProcess nuget <| sprintf "install %s -ExcludeVersion -OutputDirectory %s" package packages

let deleteIfExists path = 
  printfn "Deleting '%s'" path
  if (Directory.Exists path) then Directory.Delete(path, true)
  else File.Delete path

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
deleteIfExists demo
deleteIfExists packages
download "https://nuget.org/nuget.exe" nuget
downloadPackage "canopy"
downloadPackage "phantomjs.exe"
deleteIfExists nuget
printfn "Installing 'ChromeDriver'"
downloadAndUnzip chromeDriverSource
printfn "Installing 'IEDriverServer'"
downloadAndUnzip ieDriverSource
printfn "Creating 'refs.fsx'"

let refsText = sprintf """
#r @"packages\Newtonsoft.Json\lib\net40\Newtonsoft.Json.dll"
#r @"packages\Selenium.WebDriver\lib\net40\WebDriver.dll"
#r @"packages\Selenium.Support\lib\net40\WebDriver.Support.dll"
#r @"packages\SizSelCsZzz\lib\SizSelCsZzz.dll"
#r @"packages\canopy\lib\canopy.dll"
open canopy
configuration.chromeDir <- "%s"
configuration.ieDir <- "%s"
configuration.phantomJSDir <- @"%s\phantomjs.exe\tools\phantomjs" """ packages packages packages

writeFile refs refsText
printfn "Creating 'demo.fsx'"

let demoText = sprintf """
#load "refs.fsx"
open canopy
start chrome
url "http://google.com"
"""

writeFile demo demoText
printfn "Done"
