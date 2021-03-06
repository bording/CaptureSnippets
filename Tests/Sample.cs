﻿//using System.IO;
//using CaptureSnippets;
//// ReSharper disable UnusedMember.Local
//// ReSharper disable UnusedParameter.Local

//class Sample
//{

//    void UseSnippetExtractor()
//    {
//        // setup version convention and extract snippets from files
//        var snippetExtractor = new DirectorySnippetExtractor(
//            directoryFilter: IncludeDirectory,
//            fileFilter: IncludeFile);
//        var snippets = snippetExtractor.ReadComponents(@"C:\path");


//        //In this case the text will be extracted from a file path
//        ProcessResult result;
//        var markdownProcessor = new MarkdownProcessor(
//            snippets: snippets,
//            appendSnippetGroup: SimpleSnippetMarkdownHandling.AppendGroup);
//        using (var reader = File.OpenText(@"C:\path\myInputMarkdownFile.md"))
//        using (var writer = File.CreateText(@"C:\path\myOutputMarkdownFile.md"))
//        {
//            result = markdownProcessor.Apply(reader, writer);
//        }

//        // List of all snippets that the markdown file expected but did not exist in the input snippets
//        var missingSnippets = result.MissingSnippets;

//        // List of all snippets that the markdown file used
//        var usedSnippets = result.UsedSnippets;

//    }

//    bool IncludeDirectory(string directorypath)
//    {
//        throw new System.NotImplementedException();
//    }

//    Package InferPackage(string path, string parent)
//    {
//        throw new System.NotImplementedException();
//    }

//    bool IncludeFile(string filepath)
//    {
//        return filepath.EndsWith(".vm") || filepath.EndsWith(".cs");
//    }

//}