// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System.Text.Json;
using Azure.Core;

namespace Azure.Management.Resources.Models
{
    public partial class PolicyDefinitionGroup : IUtf8JsonSerializable
    {
        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("name");
            writer.WriteStringValue(Name);
            if (DisplayName != null)
            {
                writer.WritePropertyName("displayName");
                writer.WriteStringValue(DisplayName);
            }
            if (Category != null)
            {
                writer.WritePropertyName("category");
                writer.WriteStringValue(Category);
            }
            if (Description != null)
            {
                writer.WritePropertyName("description");
                writer.WriteStringValue(Description);
            }
            if (AdditionalMetadataId != null)
            {
                writer.WritePropertyName("additionalMetadataId");
                writer.WriteStringValue(AdditionalMetadataId);
            }
            writer.WriteEndObject();
        }

        internal static PolicyDefinitionGroup DeserializePolicyDefinitionGroup(JsonElement element)
        {
            string name = default;
            string displayName = default;
            string category = default;
            string description = default;
            string additionalMetadataId = default;
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("name"))
                {
                    name = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("displayName"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    displayName = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("category"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    category = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("description"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    description = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("additionalMetadataId"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    additionalMetadataId = property.Value.GetString();
                    continue;
                }
            }
            return new PolicyDefinitionGroup(name, displayName, category, description, additionalMetadataId);
        }
    }
}
