// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System.Collections.Generic;

namespace Azure.Analytics.Synapse.Artifacts.Models
{
    /// <summary> Azure Synapse top-level resource. </summary>
    internal partial class Resource
    {
        /// <summary> Initializes a new instance of Resource. </summary>
        internal Resource()
        {
        }

        /// <summary> Initializes a new instance of Resource. </summary>
        /// <param name="id"> The resource identifier. </param>
        /// <param name="name"> The resource name. </param>
        /// <param name="type"> The resource type. </param>
        /// <param name="location"> The resource location. </param>
        /// <param name="tags"> The resource tags. </param>
        /// <param name="eTag"> Etag identifies change in the resource. </param>
        internal Resource(string id, string name, string type, string location, IReadOnlyDictionary<string, string> tags, string eTag)
        {
            Id = id;
            Name = name;
            Type = type;
            Location = location;
            Tags = tags;
            ETag = eTag;
        }

        /// <summary> The resource identifier. </summary>
        public string Id { get; }
        /// <summary> The resource name. </summary>
        public string Name { get; }
        /// <summary> The resource type. </summary>
        public string Type { get; }
        /// <summary> The resource location. </summary>
        public string Location { get; set; }
        /// <summary> The resource tags. </summary>
        public IReadOnlyDictionary<string, string> Tags { get; set; }
        /// <summary> Etag identifies change in the resource. </summary>
        public string ETag { get; }
    }
}
