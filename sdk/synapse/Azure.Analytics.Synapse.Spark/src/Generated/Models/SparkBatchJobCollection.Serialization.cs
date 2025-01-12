// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System.Collections.Generic;
using System.Text.Json;
using Azure.Core;

namespace Azure.Analytics.Synapse.Spark.Models
{
    public partial class SparkBatchJobCollection
    {
        internal static SparkBatchJobCollection DeserializeSparkBatchJobCollection(JsonElement element)
        {
            int @from = default;
            int total = default;
            IReadOnlyList<SparkBatchJob> sessions = default;
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("from"))
                {
                    @from = property.Value.GetInt32();
                    continue;
                }
                if (property.NameEquals("total"))
                {
                    total = property.Value.GetInt32();
                    continue;
                }
                if (property.NameEquals("sessions"))
                {
                    if (property.Value.ValueKind == JsonValueKind.Null)
                    {
                        continue;
                    }
                    List<SparkBatchJob> array = new List<SparkBatchJob>();
                    foreach (var item in property.Value.EnumerateArray())
                    {
                        if (item.ValueKind == JsonValueKind.Null)
                        {
                            array.Add(null);
                        }
                        else
                        {
                            array.Add(SparkBatchJob.DeserializeSparkBatchJob(item));
                        }
                    }
                    sessions = array;
                    continue;
                }
            }
            return new SparkBatchJobCollection(@from, total, sessions);
        }
    }
}
