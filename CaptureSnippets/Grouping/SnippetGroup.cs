using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CaptureSnippets
{
    /// <summary>
    /// A hierarchy of <see cref="SnippetSource"/>s grouped by Key > Package
    /// </summary>
    [DebuggerDisplay("Key={Key}, Language={Language}, Component={Component.ValueOrUndefined}")]
    public class SnippetGroup : IEnumerable<PackageGroup>
    {
        /// <summary>
        /// Initialise a new instance of <see cref="SnippetGroup"/>.
        /// </summary>
        public SnippetGroup(string key, string language, Component component, IReadOnlyList<PackageGroup> packages)
        {
            Guard.AgainstNull(key, nameof(key));
            Guard.AgainstNull(packages, nameof(packages));
            Guard.AgainstNull(language, nameof(language));
            Guard.AgainstNull(component, nameof(component));
            Guard.AgainstUpperCase(language, nameof(language));
            Key = key;
            Packages = packages;
            Language = language;
            Component = component;
        }

        /// <summary>
        /// The key that all child <see cref="SnippetGroup"/>s contain.
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// The language of the snippet, extracted from the file extension of the input file.
        /// </summary>
        public readonly string Language;

        /// <summary>
        /// The <see cref="Component"/> for this <see cref="SnippetGroup"/>.
        /// </summary>
        public readonly Component Component;

        /// <summary>
        /// All the <see cref="PackageGroup"/>s with a common key.
        /// </summary>
        public readonly IReadOnlyList<PackageGroup> Packages;

        /// <summary>
        /// Enumerates over <see cref="Packages"/>.
        /// </summary>
        public IEnumerator<PackageGroup> GetEnumerator()
        {
            return Packages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            return $@"SnippetGroup. 
  Key: {Key}
  Language: {Language}
  Component: {Component.ValueOrUndefined}
";
        }
    }
}