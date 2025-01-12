// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;

namespace Azure.Analytics.Synapse.Artifacts.Models
{
    /// <summary> A list of rerun triggers. </summary>
    internal partial class RerunTriggerListResponse
    {
        /// <summary> Initializes a new instance of RerunTriggerListResponse. </summary>
        /// <param name="value"> List of rerun triggers. </param>
        internal RerunTriggerListResponse(IEnumerable<RerunTriggerResource> value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Value = value.ToArray();
        }

        /// <summary> Initializes a new instance of RerunTriggerListResponse. </summary>
        /// <param name="value"> List of rerun triggers. </param>
        /// <param name="nextLink"> The continuation token for getting the next page of results, if any remaining results exist, null otherwise. </param>
        internal RerunTriggerListResponse(IReadOnlyList<RerunTriggerResource> value, string nextLink)
        {
            Value = value;
            NextLink = nextLink;
        }

        /// <summary> List of rerun triggers. </summary>
        public IReadOnlyList<RerunTriggerResource> Value { get; set; }
        /// <summary> The continuation token for getting the next page of results, if any remaining results exist, null otherwise. </summary>
        public string NextLink { get; }
    }
}
