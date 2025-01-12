// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Azure.Analytics.Synapse.Artifacts.Models
{
    /// <summary> Notebook. </summary>
    public partial class Notebook : IDictionary<string, object>
    {
        /// <summary> Initializes a new instance of Notebook. </summary>
        /// <param name="metadata"> Notebook root-level metadata. </param>
        /// <param name="nbformat"> Notebook format (major number). Incremented between backwards incompatible changes to the notebook format. </param>
        /// <param name="nbformatMinor"> Notebook format (minor number). Incremented for backward compatible changes to the notebook format. </param>
        /// <param name="cells"> Array of cells of the current notebook. </param>
        public Notebook(NotebookMetadata metadata, int nbformat, int nbformatMinor, IEnumerable<NotebookCell> cells)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }
            if (cells == null)
            {
                throw new ArgumentNullException(nameof(cells));
            }

            Metadata = metadata;
            Nbformat = nbformat;
            NbformatMinor = nbformatMinor;
            Cells = cells.ToArray();
            AdditionalProperties = new Dictionary<string, object>();
        }

        /// <summary> Initializes a new instance of Notebook. </summary>
        /// <param name="description"> The description of the notebook. </param>
        /// <param name="bigDataPool"> Big data pool reference. </param>
        /// <param name="sessionProperties"> Session properties. </param>
        /// <param name="metadata"> Notebook root-level metadata. </param>
        /// <param name="nbformat"> Notebook format (major number). Incremented between backwards incompatible changes to the notebook format. </param>
        /// <param name="nbformatMinor"> Notebook format (minor number). Incremented for backward compatible changes to the notebook format. </param>
        /// <param name="cells"> Array of cells of the current notebook. </param>
        /// <param name="additionalProperties"> . </param>
        internal Notebook(string description, BigDataPoolReference bigDataPool, NotebookSessionProperties sessionProperties, NotebookMetadata metadata, int nbformat, int nbformatMinor, IList<NotebookCell> cells, IDictionary<string, object> additionalProperties)
        {
            Description = description;
            BigDataPool = bigDataPool;
            SessionProperties = sessionProperties;
            Metadata = metadata;
            Nbformat = nbformat;
            NbformatMinor = nbformatMinor;
            Cells = cells;
            AdditionalProperties = additionalProperties ?? new Dictionary<string, object>();
        }

        /// <summary> The description of the notebook. </summary>
        public string Description { get; set; }
        /// <summary> Big data pool reference. </summary>
        public BigDataPoolReference BigDataPool { get; set; }
        /// <summary> Session properties. </summary>
        public NotebookSessionProperties SessionProperties { get; set; }
        /// <summary> Notebook root-level metadata. </summary>
        public NotebookMetadata Metadata { get; set; }
        /// <summary> Notebook format (major number). Incremented between backwards incompatible changes to the notebook format. </summary>
        public int Nbformat { get; set; }
        /// <summary> Notebook format (minor number). Incremented for backward compatible changes to the notebook format. </summary>
        public int NbformatMinor { get; set; }
        /// <summary> Array of cells of the current notebook. </summary>
        public IList<NotebookCell> Cells { get; set; }
        internal IDictionary<string, object> AdditionalProperties { get; }
        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => AdditionalProperties.GetEnumerator();
        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => AdditionalProperties.GetEnumerator();
        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) => AdditionalProperties.TryGetValue(key, out value);
        /// <inheritdoc />
        public bool ContainsKey(string key) => AdditionalProperties.ContainsKey(key);
        /// <inheritdoc />
        public ICollection<string> Keys => AdditionalProperties.Keys;
        /// <inheritdoc />
        public ICollection<object> Values => AdditionalProperties.Values;
        /// <inheritdoc />
        int ICollection<KeyValuePair<string, object>>.Count => AdditionalProperties.Count;
        /// <inheritdoc />
        public void Add(string key, object value) => AdditionalProperties.Add(key, value);
        /// <inheritdoc />
        public bool Remove(string key) => AdditionalProperties.Remove(key);
        /// <inheritdoc />
        bool ICollection<KeyValuePair<string, object>>.IsReadOnly => AdditionalProperties.IsReadOnly;
        /// <inheritdoc />
        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> value) => AdditionalProperties.Add(value);
        /// <inheritdoc />
        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> value) => AdditionalProperties.Remove(value);
        /// <inheritdoc />
        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> value) => AdditionalProperties.Contains(value);
        /// <inheritdoc />
        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] destination, int offset) => AdditionalProperties.CopyTo(destination, offset);
        /// <inheritdoc />
        void ICollection<KeyValuePair<string, object>>.Clear() => AdditionalProperties.Clear();
        /// <inheritdoc />
        public object this[string key]
        {
            get => AdditionalProperties[key];
            set => AdditionalProperties[key] = value;
        }
    }
}
