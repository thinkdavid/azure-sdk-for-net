// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System.Text.Json;
using Azure.Core;

namespace Azure.Iot.Hub.Service.Models
{
    internal partial class PnpReported
    {
        internal static PnpReported DeserializePnpReported(JsonElement element)
        {
            object value = default;
            DesiredState desiredState = default;
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("value"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    value = property.Value.GetObject();
                    continue;
                }
                if (property.NameEquals("desiredState"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    desiredState = DesiredState.DeserializeDesiredState(property.Value);
                    continue;
                }
            }
            return new PnpReported(value, desiredState);
        }
    }
}