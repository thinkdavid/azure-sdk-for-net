// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.ComponentModel;

namespace Azure.Management.Resources.Models
{
    /// <summary> The operation type for the patch API. </summary>
    public readonly partial struct TagsPatchResourceOperation : IEquatable<TagsPatchResourceOperation>
    {
        private readonly string _value;

        /// <summary> Determines if two <see cref="TagsPatchResourceOperation"/> values are the same. </summary>
        public TagsPatchResourceOperation(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        private const string ReplaceValue = "Replace";
        private const string MergeValue = "Merge";
        private const string DeleteValue = "Delete";

        /// <summary> Replace. </summary>
        public static TagsPatchResourceOperation Replace { get; } = new TagsPatchResourceOperation(ReplaceValue);
        /// <summary> Merge. </summary>
        public static TagsPatchResourceOperation Merge { get; } = new TagsPatchResourceOperation(MergeValue);
        /// <summary> Delete. </summary>
        public static TagsPatchResourceOperation Delete { get; } = new TagsPatchResourceOperation(DeleteValue);
        /// <summary> Determines if two <see cref="TagsPatchResourceOperation"/> values are the same. </summary>
        public static bool operator ==(TagsPatchResourceOperation left, TagsPatchResourceOperation right) => left.Equals(right);
        /// <summary> Determines if two <see cref="TagsPatchResourceOperation"/> values are not the same. </summary>
        public static bool operator !=(TagsPatchResourceOperation left, TagsPatchResourceOperation right) => !left.Equals(right);
        /// <summary> Converts a string to a <see cref="TagsPatchResourceOperation"/>. </summary>
        public static implicit operator TagsPatchResourceOperation(string value) => new TagsPatchResourceOperation(value);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => obj is TagsPatchResourceOperation other && Equals(other);
        /// <inheritdoc />
        public bool Equals(TagsPatchResourceOperation other) => string.Equals(_value, other._value, StringComparison.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        /// <inheritdoc />
        public override string ToString() => _value;
    }
}
