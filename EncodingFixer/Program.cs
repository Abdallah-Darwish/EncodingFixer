using CommandLine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;
namespace EncodingFixer
{
    class Program
    {
        public static void PrintError(string error)
        {
            Console.WriteLine(error, Color.Red);
        }
        private static bool verbose = false;
        public static void Log(string msg)
        {
            if (verbose == false) { return; }
            Console.WriteLine($"Log: {msg}", Color.Yellow);
        }
        class Options
        {
            [Option('f', "files", Required = true, HelpText = "Text files to set their encoding, you can use patterns like *.srt.", Separator = ',')]
            public IEnumerable<string> InputFiles { get; set; }

            [Option('v', Default = false, HelpText = "Prints all debugging messages to standard output.")]
            public bool Verbose { get; set; }

        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(opts => RunOptionsAndReturnExitCode(opts))
              .WithNotParsed((errs) => HandleParseError(errs));
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            errs = errs.Where(e => e.Tag != ErrorType.HelpRequestedError);
            if (errs.Any() == false)
            {
                return;
            }
            PrintError("One or more error happened during the parsing of the command line.\nErros: ");
            foreach (var err in errs.Where(e => e.Tag != ErrorType.HelpRequestedError))
            {
                Console.WriteLine(err.Tag);
            }
            Environment.Exit(-1);
        }

        private static void RunOptionsAndReturnExitCode(Options opts)
        {
            verbose = opts.Verbose;
            Log("Building file list.");
            List<string> files = new List<string>(1000);
            var workingDir = new DirectoryInfo(Environment.CurrentDirectory);
            foreach (var pattern in opts.InputFiles)
            {
                var searchResult = workingDir.EnumerateFiles(pattern, SearchOption.TopDirectoryOnly).Select(f => f.FullName);
                if (searchResult.Any() == false)
                {
                    PrintError($"File: {pattern} doesnt exist.");
                }
                files.AddRange(searchResult);
            }
            Log($"List of files to process:\n{string.Join(Environment.NewLine, files)}");
            Encoding fileOriginalEncoding = Encoding.GetEncoding(1256);
            Encoding fileNewEncoding = new UTF8Encoding();
            Parallel.ForEach(files, file =>
            {
                Log($"Processing file {file}");
                File.WriteAllText(file, File.ReadAllText(file, fileOriginalEncoding), fileNewEncoding);
            });
            Console.WriteLine("Fixed encoding of all Files!", Color.Green);
            Environment.Exit(0);
        }
    }
}
