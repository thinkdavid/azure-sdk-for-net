// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System.Collections.Generic;

namespace Azure.Management.AppConfiguration.Models
{
    /// <summary> A list of private link resources. </summary>
    public partial class PrivateLinkResourceListResult
    {
        /// <summary> Initializes a new instance of PrivateLinkResourceListResult. </summary>
        internal PrivateLinkResourceListResult()
        {
        }

        /// <summary> Initializes a new instance of PrivateLinkResourceListResult. </summary>
        /// <param name="value"> The collection value. </param>
        /// <param name="nextLink"> The URI that can be used to request the next set of paged results. </param>
        internal PrivateLinkResourceListResult(IReadOnlyList<PrivateLinkResource> value, string nextLink)
        {
            Value = value;
            NextLink = nextLink;
        }

        /// <summary> The collection value. </summary>
        public IReadOnlyList<PrivateLinkResource> Value { get; }
        /// <summary> The URI that can be used to request the next set of paged results. </summary>
        public string NextLink { get; }
    }
}
