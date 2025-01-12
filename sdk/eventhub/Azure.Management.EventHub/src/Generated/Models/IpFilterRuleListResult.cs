// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System.Collections.Generic;

namespace Azure.Management.EventHub.Models
{
    /// <summary> The response from the List namespace operation. </summary>
    public partial class IpFilterRuleListResult
    {
        /// <summary> Initializes a new instance of IpFilterRuleListResult. </summary>
        internal IpFilterRuleListResult()
        {
        }

        /// <summary> Initializes a new instance of IpFilterRuleListResult. </summary>
        /// <param name="value"> Result of the List IpFilter Rules operation. </param>
        /// <param name="nextLink"> Link to the next set of results. Not empty if Value contains an incomplete list of IpFilter Rules. </param>
        internal IpFilterRuleListResult(IReadOnlyList<IpFilterRule> value, string nextLink)
        {
            Value = value;
            NextLink = nextLink;
        }

        /// <summary> Result of the List IpFilter Rules operation. </summary>
        public IReadOnlyList<IpFilterRule> Value { get; }
        /// <summary> Link to the next set of results. Not empty if Value contains an incomplete list of IpFilter Rules. </summary>
        public string NextLink { get; }
    }
}
