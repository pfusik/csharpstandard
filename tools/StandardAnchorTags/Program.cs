﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices.ComTypes;

namespace StandardAnchorTags
{
    public class Program
    {
        const char sectionReference = '§';

        private static readonly string[] frontMatter = new string[]
        {
                "foreword.md",
                "introduction.md"
        };
        private static readonly string[] mainBodyFiles = new string[]
        {
            "scope.md",
            "normative-references.md",
            "terms-and-definitions.md",
            "acronyms-and-abbreviations.md",
            "general-description.md",
            "conformance.md",
            "lexical-structure.md",
            "basic-concepts.md",
            "types.md",
            "variables.md",
            "conversions.md",
            "expressions.md",
            "statements.md",
            "namespaces.md",
            "classes.md",
            "structs.md",
            "arrays.md",
            "interfaces.md",
            "enums.md",
            "delegates.md",
            "exceptions.md",
            "attributes.md",
            "unsafe-code.md"
        };
        private static readonly string[] annexFiles = new string[]
        {
            "grammar.md",
            "portability-issues.md",
            "standard-library.md",
            "documentation-comments.md",
            "bibliography.md"
        };

        static async Task Main()
        {
            try
            {
                Console.WriteLine("=========================== Front Matter ===================================");
                var sectionMap = new TocSectionNumberBuilder(
                    "- [Foreword](foreword.md)",
                    "- [Introduction](introduction.md)");

                Console.WriteLine("================= GENERATE UPDATED SECTION NUMBERS =========================");
                Console.WriteLine("============================ Main text======================================");
                foreach (var file in mainBodyFiles)
                {
                    Console.WriteLine($" -- {file}");
                    await sectionMap.AddContentsToTOC(file);
                }
                Console.WriteLine("============================= Annexes ======================================");
                sectionMap.FinishMainSection();
                foreach (var file in annexFiles)
                {
                    Console.WriteLine($" -- {file}");
                    await sectionMap.AddContentsToTOC(file);
                }
                Console.WriteLine("Update TOC");
                var existingReadMe = await ReadExistingReadMe();
                using var readme = new StreamWriter("README.md", false);
                await readme.WriteAsync(existingReadMe);
                await readme.WriteAsync(sectionMap.Toc);
                var sectionLinkMap = sectionMap.LinkMap;

                Console.WriteLine("======================= UPDATE ALL REFERENCES ==============================");
                var fixup = new ReferenceUpdateProcessor(sectionMap.LinkMap);

                Console.WriteLine("=========================== Front Matter ===================================");
                foreach (var file in frontMatter)
                {
                    Console.WriteLine($" -- {file}");
                    await fixup.ReplaceReferences(file);

                }
                Console.WriteLine("============================ Main text======================================");
                foreach (var file in mainBodyFiles)
                {
                    Console.WriteLine($" -- {file}");
                    await fixup.ReplaceReferences(file);

                }
                Console.WriteLine("============================= Annexes ======================================");
                foreach (var file in annexFiles)
                {
                    Console.WriteLine($" -- {file}");
                    await fixup.ReplaceReferences(file);
                }
            } catch (InvalidOperationException e)
            {
                Console.WriteLine("\tError encountered:");
                Console.WriteLine(e.Message.ToString());
                Console.WriteLine("To recover, do the following:");
                Console.WriteLine("1. Discard all changes from the section numbering tool");
                Console.WriteLine("2. Fix the error noted above.");
                Console.WriteLine("3. Run the tool again.");
            }
        }

        private static async Task<string> ReadExistingReadMe()
        {
            using var reader = new StreamReader("README.md");
            var contents = await reader.ReadToEndAsync();

            // This is the first node in the TOC, so truncate here:
            var index = contents.IndexOf("- [Foreword]");

            return contents.Substring(0, index);

        }
    }
}