// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System.Collections;
using System.Collections.Generic;

namespace Azure.Management.Resources.Models
{
    /// <summary> General metadata for the parameter. </summary>
    public partial class ParameterDefinitionsValueMetadata : IDictionary<string, object>
    {
        /// <summary> Initializes a new instance of ParameterDefinitionsValueMetadata. </summary>
        public ParameterDefinitionsValueMetadata()
        {
            AdditionalProperties = new Dictionary<string, object>();
        }

        /// <summary> Initializes a new instance of ParameterDefinitionsValueMetadata. </summary>
        /// <param name="displayName"> The display name for the parameter. </param>
        /// <param name="description"> The description of the parameter. </param>
        /// <param name="additionalProperties"> . </param>
        internal ParameterDefinitionsValueMetadata(string displayName, string description, IDictionary<string, object> additionalProperties)
        {
            DisplayName = displayName;
            Description = description;
            AdditionalProperties = additionalProperties ?? new Dictionary<string, object>();
        }

        /// <summary> The display name for the parameter. </summary>
        public string DisplayName { get; set; }
        /// <summary> The description of the parameter. </summary>
        public string Description { get; set; }
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
