using System.Collections.Generic;
using CommandLine;

namespace EncodingFixer;

public class Options
{
    [Option('f', "files", Required = true, HelpText = "Names (not paths) of text files to fix their encoding, you can use patterns like *.srt, seperate files by a comma(,).", Separator = ',')]
    public IEnumerable<string> InputFiles { get; set; }

    [Option('d', "directory", Required = false, HelpText = "Parent directory of input files, if ignored it will be working directory.")]
    public string? Directory { get; set; }

    [Option('v', Default = false, HelpText = "Prints all debugging messages to standard output.")]
    public bool Verbose { get; set; }
}
