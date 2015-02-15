using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fody;

namespace CaptureSnippets
{

    /// <summary>
    /// Merges <see cref="SnippetGroup"/>s with an input file/text.
    /// </summary>
    public class MarkdownProcessor
    {

        /// <summary>
        /// Apply <paramref name="snippets"/> to <paramref name="textReader"/>.
        /// </summary>
        [ConfigureAwait(false)]
        public async Task<ProcessResult> Apply(List<SnippetGroup> snippets, TextReader textReader, TextWriter writer)
        {
            using (var reader = new IndexReader(textReader))
            {
                return await Apply(snippets, writer, reader);
            }
        }

        
        async Task<ProcessResult> Apply(List<SnippetGroup> availableSnippets, TextWriter writer, IndexReader reader)
        {
            var result = new ProcessResult();

            string line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                await writer.WriteLineAsync(line);

                string key;
                if (!ImportKeyReader.TryExtractKeyFromLine(line, out key))
                {
                    continue;
                }

                var snippetGroup = availableSnippets.FirstOrDefault(x => x.Key == key);
                if (snippetGroup == null)
                {
                    var missingSnippet = new MissingSnippet
                                         {
                                             Key = key,
                                             Line = reader.Index
                                         };
                    result.MissingSnippets.Add(missingSnippet);
                    await writer.WriteLineAsync(string.Format("** Could not find key '{0}' **", key));
                    continue;
                }

                await AppendGroup(snippetGroup, writer);
                if (result.UsedSnippets.All(x => x.Key != snippetGroup.Key))
                {
                    result.UsedSnippets.Add(snippetGroup);
                }
            }
            return result;
        }

        /// <summary>
        /// Method that cna be override to control how an individual <see cref="SnippetGroup"/> is rendered.
        /// </summary>
        [ConfigureAwait(false)]
        public async Task AppendGroup(SnippetGroup snippetGroup, TextWriter stringBuilder)
        {
            foreach (var versionGroup in snippetGroup)
            {
                if (versionGroup.Version != null)
                {
                    await stringBuilder.WriteLineAsync("#### Version " + versionGroup.Version);
                }
                foreach (var snippet in versionGroup)
                {
                    await AppendSnippet(snippet, stringBuilder);
                }
            }
        }

        [ConfigureAwait(false)]
        public async Task AppendSnippet(Snippet codeSnippet, TextWriter stringBuilder)
        {
            var format = string.Format(
@"```{0}
{1}
```", codeSnippet.Language, codeSnippet.Value);
            await stringBuilder.WriteLineAsync(format);
        }
    }
}