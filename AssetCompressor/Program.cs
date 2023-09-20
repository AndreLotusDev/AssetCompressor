using System;
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using MimeDetective;
using MimeDetective.Storage;

//Get bytes from this path @"C:\Users\andrs\OneDrive\Área de Trabalho\rock3.mp4";
var bytes = System.IO.File.ReadAllBytes(@"PLACEHOLDER");

var inspector = new ContentInspectorBuilder()
{
    Definitions = MimeDetective.Definitions.Default.All()
}.Build();

var fileType = inspector.Inspect(bytes);

if (fileType != null)
{
    var mime = fileType.FirstOrDefault()?.Definition.File.MimeType;
    var extensionFormats = string.Join(", ", fileType.FirstOrDefault()?.Definition.File.Extensions);

    Console.WriteLine("File extensions: " + extensionFormats);
    Console.WriteLine("File mime type: " + mime);

    if (mime.StartsWith("image/"))
    {
        Console.WriteLine("This is an image file.");
    }
    else if (mime.StartsWith("video/"))
    {
        try
        {
            var outputFile = @"PLACEHOLDER";

            using var stream = new MemoryStream(bytes);
            using (var outputStream = new MemoryStream())
            {
                var pipe = new StreamPipeSink(outputStream);

                StreamPipeSource streamPipeSource = new StreamPipeSource(stream);

                FFMpegArguments.FromPipeInput(streamPipeSource)
                    .OutputToFile(outputFile, true, options => options
                        .WithCustomArgument("-probesize 100M")
                        .WithCustomArgument("-analyzeduration 200M")
                        .WithVideoCodec("libx264")
                        .ForceFormat(VideoType.MpegTs)
                        .ForcePixelFormat("yuv420p")
                        .WithConstantRateFactor(28))
                    .ProcessSynchronously();


                //FFMpegArguments.FromPipeInput(streamPipeSource)
                //    .OutputToPipe(pipe, options => options
                //        .WithCustomArgument("-probesize 50M")
                //        .WithCustomArgument("-analyzeduration 100M")
                //        .WithVideoCodec("libx265")
                //        .ForceFormat(VideoType.MpegTs)
                //        .WithConstantRateFactor(28))
                //    .ProcessSynchronously();

                ////Dump stream to outputfile 
                //using (var fileStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
                //{
                //    outputStream.WriteTo(fileStream);
                //}
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    else
    {
        Console.WriteLine("This file is neither an image nor a video.");
    }
}

