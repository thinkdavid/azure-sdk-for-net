// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

namespace Azure.Management.EventHub.Models
{
    /// <summary> The full ARM ID of an Event Hubs Namespace. </summary>
    public partial class EHNamespaceIdContainer
    {
        /// <summary> Initializes a new instance of EHNamespaceIdContainer. </summary>
        internal EHNamespaceIdContainer()
        {
        }

        /// <summary> Initializes a new instance of EHNamespaceIdContainer. </summary>
        /// <param name="id"> id parameter. </param>
        internal EHNamespaceIdContainer(string id)
        {
            Id = id;
        }

        /// <summary> id parameter. </summary>
        public string Id { get; }
    }
}
