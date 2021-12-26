using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
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
            if (!verbose) { return; }
            Console.WriteLine(msg, Color.Blue);
        }

        public static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Parser.Default.ParseArguments<Options>(args)
              .WithParsed(opts => RunOptionsAndReturnExitCode(opts))
              .WithNotParsed((errs) => HandleParseError(errs));
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            errs = errs.Where(e => e.Tag != ErrorType.HelpRequestedError);
            if (!errs.Any())
            {
                return;
            }
            PrintError("One or more error happened during the parsing of the arguments.\nErros: ");
            foreach (var err in errs.Where(e => e.Tag != ErrorType.HelpRequestedError))
            {
                Console.WriteLine(err.Tag);
            }
            Environment.Exit(-1);
        }

        private static void RunOptionsAndReturnExitCode(Options opts)
        {
            verbose = opts.Verbose;
            try
            {
                if (opts.Directory != null && !Directory.Exists(opts.Directory))
                {
                    PrintError("Invalid files directory.");
                    Environment.Exit(-1);
                }
            }
            catch
            {
                PrintError("Invalid files directory.");
                Environment.Exit(-1);
            }

            Log("Building file list.");
            List<string> files = new(1000);
            var searchDir = new DirectoryInfo(opts.Directory ?? Environment.CurrentDirectory);
            Log($"Searching for files in {searchDir}");

            foreach (var pattern in opts.InputFiles)
            {
                try
                {
                    if (File.Exists(pattern))
                    {
                        files.Add(pattern);
                        continue;
                    }
                }
                catch { }
                var searchResult = searchDir
                .EnumerateFiles(pattern, SearchOption.TopDirectoryOnly)
                .Select(f => f.FullName);
                if (!searchResult.Any())
                {
                    PrintError($"File: {pattern} doesn't exist in directory.");
                }
                files.AddRange(searchResult);
            }
            Log($"List of files to process:\n\t{string.Join($"{Environment.NewLine}\t", files)}");
            Encoding fileOriginalEncoding = Encoding.GetEncoding(1256);
            Encoding fileNewEncoding = new UTF8Encoding();
            Parallel.ForEach(files, file =>
            {
                Log($"Processing file {file}");
                File.WriteAllText(file, File.ReadAllText(file, fileOriginalEncoding), fileNewEncoding);
            });
            Console.WriteLine("Fixed encoding of all files!", Color.Green);
            Environment.Exit(0);
        }
    }
}
