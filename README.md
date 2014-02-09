#CanopyScript

Canopy is a web testing framework, written in F#, that allows you to run Selenium tests using concise and simple language. We started using it to enable non-technical QAs to automate their tests and user journeys and it has proved extremely successful. Because it is F#, all you need is the compiler and notepad, and because its syntax is so simple, you don’t need any particular programming skill to get going.

First you’re going to need to install .NET 4.5:

http://www.microsoft.com/en-gb/download/confirmation.aspx?id=40779

And the F# compiler tools:

http://go.microsoft.com/fwlink/?LinkId=261286

Then create a directory for your scripts and save this file into it:

https://github.com/JonCanning/CanopyScript/releases/download/release/install.fsx

Run this script and it will download canopy and its supporting libraries, and create another file called refs.fsx. You will need to load this file and open the canopy library at the start of your scripts as shown in this sample that carries out a search on Google:

```
#load "refs.fsx"
open canopy

start chrome

describe "Go to Google.com and search for Attack of the Mutant Camels"
url "http://google.com"
"input[type=text]" << "Attack of the Mutant Camels"
press enter

describe "Check that the first result is the Wikipedia page"
"li h3" == "Attack of the Mutant Camels - Wikipedia, the free encyclopedia"

describe "Follow link to Wikipedia"
click "li h3"

describe "Check that the first paragraph mentions Jeff Minter"
"p" =~ "Jeff Minter"

quit()
```

Just save this as an .fsx file and run it. Boom.

If you want to organise your scripts into subdirectories, make sure that you modify the load command to read the refs file from the parent directory e.g. ```#load "..\\refs.fsx"```

Have a look the documentation for all the possible actions and assertions:

http://lefthandedgoat.github.io/canopy/

###Data Driven Tests
 
Our QAs have to test user journeys after a release using data from a script. It's mind numbing stuff, so here's a little scripting recipe to automate this.

Create a file called testRunner.fsx:
```
#load "refs.fsx"
open FSharp.Data
[<Literal>]
let path = "testData.csv"
type Csv = CsvProvider<path>
let rows = Csv.Load(path).Data
#load "test.fsx"
rows |> Seq.iter test.run
```

Put your test into another file called test.fsx:
```
module test
#load "refs.fsx"
open canopy

let run (row:Csv.Row) =

  start chrome
  
  describe <| "Go to Google.com and search for " + row.Search
  url "http://google.com"
  "input[type=text]" << row.Search
  press enter
  
  describe "Check that the first result is the Wikipedia page"
  "div.rc h3" == row.Result
  
  describe "Follow link to Wikipedia"
  click "div.rc h3"
  
  describe <| "Check that the first paragraph mentions " + row.Assert
  "p" =~ row.Assert
  
  quit()
```

Put your data into a file called testData.csv:
```
Search,Result,Assert
Attack of the Mutant Camels,"Attack of the Mutant Camels - Wikipedia, the free encyclopedia",Jeff Minter
Revenge of the Mutant Camels,"Revenge of the Mutant Camels - Wikipedia, the free encyclopedia",Commodore 64
```

Now execute testRunner.fsx and you'll see your test run for each row of data
