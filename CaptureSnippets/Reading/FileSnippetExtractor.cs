﻿using System;
using System.Collections.Generic;
using System.IO;
using NuGet.Versioning;

namespace CaptureSnippets
{
    /// <summary>
    /// Extracts <see cref="Snippet"/>s from a given input.
    /// </summary>
    public class FileSnippetExtractor
    {
        VersionRange fileVersion;
        string package;

        public static FileSnippetExtractor Build(VersionRange fileVersion, string package, bool isCurrent)
        {
            Guard.AgainstNull(fileVersion, nameof(fileVersion));
            Guard.AgainstNullAndEmpty(package, nameof(package));
            return new FileSnippetExtractor
            {
                fileVersion = fileVersion,
                package = package,
                isCurrent = isCurrent
            };
        }

        public static FileSnippetExtractor BuildShared()
        {
            return new FileSnippetExtractor
            {
                isShared = true
            };
        }

        static char[] invalidCharacters = { '“', '”', '—' };
        bool isShared;
        bool isCurrent;

        /// <summary>
        /// Read from a <see cref="TextReader"/>.
        /// </summary>
        /// <param name="textReader">The <see cref="TextReader"/> to read from.</param>
        /// <param name="path">The current path so extract <see cref="Snippet"/>s from.</param>
        public IEnumerable<Snippet> AppendFromReader(TextReader textReader, string path)
        {
            Guard.AgainstNull(textReader, nameof(textReader));
            Guard.AgainstNullAndEmpty(path, nameof(path));
            try
            {
                var reader = new IndexReader(textReader);
                return GetSnippets(reader, path);
            }
            catch (Exception exception)
            {
                throw new Exception($"Could not extract snippets from '{path}';", exception);
            }
        }

        static string GetLanguageFromPath(string path)
        {
            var extension = Path.GetExtension(path);
            return extension?.TrimStart('.') ?? string.Empty;
        }


        IEnumerable<Snippet> GetSnippets(IndexReader stringReader, string path)
        {
            var language = GetLanguageFromPath(path);

            var loopStack = new LoopStack();
            while (true)
            {
                var line = stringReader.ReadLine();
                if (line == null)
                {
                    if (loopStack.IsInSnippet)
                    {
                        var current = loopStack.Current;
                        yield return Snippet.BuildError(
                            error: "Snippet was not closed",
                            path: path,
                            lineNumberInError: current.StartLine + 1,
                            key: current.Key);
                    }
                    break;
                }

                var trimmedLine = line.Trim()
                    .Replace("  ", " ")
                    .ToLowerInvariant();
                string version;
                Func<string, bool> endFunc;
                string key;
                if (StartEndTester.IsStart(trimmedLine, out version, out key, out endFunc))
                {
                    loopStack.Push(endFunc, key, version, stringReader.Index);
                    continue;
                }
                if (loopStack.IsInSnippet)
                {
                    if (!loopStack.Current.EndFunc(trimmedLine))
                    {
                        loopStack.AppendLine(line);
                        continue;
                    }

                    yield return BuildSnippet(stringReader, path, loopStack.Current, language);
                    loopStack.Pop();
                }
            }
        }


        Snippet BuildSnippet(IndexReader stringReader, string path, LoopState loopState, string language)
        {
            var startRow = loopState.StartLine + 1;

            string error;
            if (isShared && loopState.Version != null)
            {
                return Snippet.BuildError(
                    error: "Shared snippets cannot contain a version",
                    path: path,
                    lineNumberInError: startRow,
                    key: loopState.Key);
            }
            VersionRange snippetVersion;
            if (!TryParseVersionAndPackage(loopState, out snippetVersion, out error))
            {
                return Snippet.BuildError(
                    error: error,
                    path: path,
                    lineNumberInError: startRow,
                    key: loopState.Key);
            }
            var value = loopState.GetLines();
            if (value.IndexOfAny(invalidCharacters) > -1)
            {
                var joinedInvalidChars = $@"'{string.Join("', '", invalidCharacters)}'";
                return Snippet.BuildError(
                    error: $"Snippet contains invalid characters ({joinedInvalidChars}). This was probably caused by copying code from MS Word or Outlook. Dont do that.",
                    path: path,
                    lineNumberInError: startRow,
                    key: loopState.Key);
            }
            if (isShared)
            {
                return Snippet.BuildShared(
                    startLine: startRow,
                    endLine: stringReader.Index,
                    key: loopState.Key,
                    value: value,
                    path: path,
                    language: language.ToLowerInvariant());
            }
            return Snippet.Build(
                startLine: startRow,
                endLine: stringReader.Index,
                key: loopState.Key,
                version: snippetVersion,
                value: value,
                path: path,
                language: language.ToLowerInvariant(),
                package: package,
                isCurrent: isCurrent);
        }

        bool TryParseVersionAndPackage(LoopState loopState, out VersionRange snippetVersion, out string error)
        {
            snippetVersion = null;
            if (loopState.Version == null)
            {
                snippetVersion = fileVersion;
                error = null;
                return true;
            }

            VersionRange version;
            if (VersionRangeParser.TryParseVersion(loopState.Version, out version))
            {
                if (fileVersion.IsPreRelease())
                {
                    error = $"Could not use '{loopState.Version}' since directory is flagged as Prerelease. FileVersion: {fileVersion.ToFriendlyString()}.";
                    return false;
                }
                snippetVersion = version;

                error = null;
                return true;
            }

            error = $"Expected '{loopState.Version}' to be parsable as a version.";
            return false;
        }

    }
}