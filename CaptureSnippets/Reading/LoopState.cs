﻿using System;
using System.Diagnostics;
using System.Text;

[DebuggerDisplay("Key={Key}, Version={Version}")]
    class LoopState
    {

    public string GetLines()
    {
        if (builder == null)
        {
            return "";
        }
        builder.TrimEnd();
        return builder.ToString();
    }

    public void AppendLine(string line)
    {
        if (builder == null)
        {
            builder = new StringBuilder();
        }
        if (builder.Length == 0)
        {
            if (line.IsWhiteSpace())
            {
                return;
            }
            CheckWhiteSpace(line, ' ');
            CheckWhiteSpace(line, '\t');
        }
        else
        {
            builder.AppendLine();
        }
        var paddingToRemove = line.LastIndexOfSequence(paddingChar, paddingLength);

        builder.Append(line, paddingToRemove, line.Length - paddingToRemove);
    }

    void CheckWhiteSpace(string line, char whiteSpace)
    {
        var c = line[0];
        if (c != whiteSpace)
        {
            return;
        }
        paddingChar = whiteSpace;
        for (var index = 1; index < line.Length; index++)
        {
            paddingLength++;
            var ch = line[index];
            if (ch != whiteSpace)
            {
                break;
            }
        }
    }

    StringBuilder builder;
    public string Key;
    char paddingChar;
    int paddingLength;

    public string Version;
    public Func<string, bool> EndFunc;
    public int StartLine;
}