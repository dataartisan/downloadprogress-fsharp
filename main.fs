open System
open System.Net
open System.Threading
open Microsoft.FSharp.Control.WebExtensions

// URLs to try in increasing size
// http://msdn.microsoft.com/"
// http://launchpad.net/nunitv2/2.5/2.5.8/+download/NUnit-2.5.8.10295.zip
// http://ftp.ussg.iu.edu/eclipse/technology/epp/downloads/release/helios/SR1/eclipse-jee-helios-SR1-linux-gtk-x86_64.tar.gz

let webClient = new WebClient()

let fetchAsync(name, url:string) =
    async { 
        try
            let uri = new System.Uri(url)
            let contentLength = ref 0L
            let bytesReceived = ref 0L
            webClient.DownloadProgressChanged.Add(
                fun args ->
                    let contentLengthString =
                        webClient.ResponseHeaders.Get "Content-Length"
                    if !contentLength = 0L && contentLengthString <> null then
                        contentLength :=
                            Int64.Parse contentLengthString
                    bytesReceived := !bytesReceived + args.BytesReceived
                    if !contentLength > 0L then
                        printfn "received %A of %A bytes (%A %%)"
                            !bytesReceived !contentLength
                            (int (100L * !bytesReceived / !contentLength))
                    else
                        printfn "received %A bytes" !bytesReceived
            )
            let! html = webClient.AsyncDownloadString(uri)
            printfn "Read %d characters for %s" html.Length name
        with
            | ex -> printfn "%s" (ex.Message);
    }

let args = System.Environment.GetCommandLineArgs()

let usage() =
    printfn "usage: %s url" args.[0]

let main() =
    if args.Length <> 2 then
        usage()
        System.Environment.Exit 1
    let url = args.[1]
    printfn "downloading %s" url
    printfn "press return to cancel..."
    Thread.Sleep 500
    let ts = new System.Threading.CancellationTokenSource()
    Async.Start(fetchAsync(url, url), ts.Token)
    System.Console.ReadLine() |> ignore
    if webClient.IsBusy then
        printfn "canceling..."
        ts.Cancel()

main()
