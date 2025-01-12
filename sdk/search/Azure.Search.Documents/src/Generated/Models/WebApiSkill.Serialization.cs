// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Azure.Core;

namespace Azure.Search.Documents.Indexes.Models
{
    public partial class WebApiSkill : IUtf8JsonSerializable
    {
        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("uri");
            writer.WriteStringValue(Uri);
            if (HttpHeaders != null && HttpHeaders.Any())
            {
                writer.WritePropertyName("httpHeaders");
                writer.WriteStartObject();
                foreach (var item in HttpHeaders)
                {
                    writer.WritePropertyName(item.Key);
                    writer.WriteStringValue(item.Value);
                }
                writer.WriteEndObject();
            }
            if (HttpMethod != null)
            {
                writer.WritePropertyName("httpMethod");
                writer.WriteStringValue(HttpMethod);
            }
            if (Timeout != null)
            {
                writer.WritePropertyName("timeout");
                writer.WriteStringValue(Timeout.Value, "P");
            }
            if (BatchSize != null)
            {
                writer.WritePropertyName("batchSize");
                writer.WriteNumberValue(BatchSize.Value);
            }
            if (DegreeOfParallelism != null)
            {
                writer.WritePropertyName("degreeOfParallelism");
                writer.WriteNumberValue(DegreeOfParallelism.Value);
            }
            writer.WritePropertyName("@odata.type");
            writer.WriteStringValue(ODataType);
            if (Name != null)
            {
                writer.WritePropertyName("name");
                writer.WriteStringValue(Name);
            }
            if (Description != null)
            {
                writer.WritePropertyName("description");
                writer.WriteStringValue(Description);
            }
            if (Context != null)
            {
                writer.WritePropertyName("context");
                writer.WriteStringValue(Context);
            }
            if (Inputs != null && Inputs.Any())
            {
                writer.WritePropertyName("inputs");
                writer.WriteStartArray();
                foreach (var item in Inputs)
                {
                    writer.WriteObjectValue(item);
                }
                writer.WriteEndArray();
            }
            if (Outputs != null && Outputs.Any())
            {
                writer.WritePropertyName("outputs");
                writer.WriteStartArray();
                foreach (var item in Outputs)
                {
                    writer.WriteObjectValue(item);
                }
                writer.WriteEndArray();
            }
            writer.WriteEndObject();
        }

        internal static WebApiSkill DeserializeWebApiSkill(JsonElement element)
        {
            string uri = default;
            IDictionary<string, string> httpHeaders = default;
            string httpMethod = default;
            TimeSpan? timeout = default;
            int? batchSize = default;
            int? degreeOfParallelism = default;
            string odataType = default;
            string name = default;
            string description = default;
            string context = default;
            IList<InputFieldMappingEntry> inputs = default;
            IList<OutputFieldMappingEntry> outputs = default;
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("uri"))
                {
                    uri = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("httpHeaders"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                    foreach (var property0 in property.Value.EnumerateObject())
                    {
                        if (property0.Value.ValueKind == JsonValueKind.Null)
                        {
                            dictionary.Add(property0.Name, null);
                        }
                        else
                        {
                            dictionary.Add(property0.Name, property0.Value.GetString());
                        }
                    }
                    httpHeaders = dictionary;
                    continue;
                }
                if (property.NameEquals("httpMethod"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    httpMethod = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("timeout"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    timeout = property.Value.GetTimeSpan("P");
                    continue;
                }
                if (property.NameEquals("batchSize"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    batchSize = property.Value.GetInt32();
                    continue;
                }
                if (property.NameEquals("degreeOfParallelism"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    degreeOfParallelism = property.Value.GetInt32();
                    continue;
                }
                if (property.NameEquals("@odata.type"))
                {
                    odataType = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("name"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    name = property.Value.GetString();
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
                if (property.NameEquals("context"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    context = property.Value.GetString();
                    continue;
                }
                if (property.NameEquals("inputs"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    List<InputFieldMappingEntry> array = new List<InputFieldMappingEntry>();
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Null)
                        {
                            array.Add(null);
                        }
                        else
                        {
                            array.Add(InputFieldMappingEntry.DeserializeInputFieldMappingEntry(item));
                        }
                    }
                    inputs = array;
                    continue;
                }
                if (property.NameEquals("outputs"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    List<OutputFieldMappingEntry> array = new List<OutputFieldMappingEntry>();
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Null)
                        {
                            array.Add(null);
                        }
                        else
                        {
                            array.Add(OutputFieldMappingEntry.DeserializeOutputFieldMappingEntry(item));
                        }
                    }
                    outputs = array;
                    continue;
                }
            }
            return new WebApiSkill(odataType, name, description, context, inputs, outputs, uri, httpHeaders, httpMethod, timeout, batchSize, degreeOfParallelism);
        }
    }
}
