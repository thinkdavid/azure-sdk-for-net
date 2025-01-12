﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Azure.Core;
using Azure.Messaging.ServiceBus.Amqp.Framing;
using Microsoft.Azure.Amqp;
using Microsoft.Azure.Amqp.Encoding;
using Microsoft.Azure.Amqp.Framing;
using Azure.Messaging.ServiceBus.Primitives;
using SBMessage = Azure.Messaging.ServiceBus.ServiceBusMessage;
using Azure.Messaging.ServiceBus.Management;

namespace Azure.Messaging.ServiceBus.Amqp
{
    internal static class AmqpMessageConverter
    {
        private const string EnqueuedTimeUtcName = "x-opt-enqueued-time";
        private const string ScheduledEnqueueTimeUtcName = "x-opt-scheduled-enqueue-time";
        private const string SequenceNumberName = "x-opt-sequence-number";
        private const string EnqueueSequenceNumberName = "x-opt-enqueue-sequence-number";
        private const string LockedUntilName = "x-opt-locked-until";
        private const string PartitionKeyName = "x-opt-partition-key";
        private const string PartitionIdName = "x-opt-partition-id";
        private const string ViaPartitionKeyName = "x-opt-via-partition-key";
        private const string DeadLetterSourceName = "x-opt-deadletter-source";
        private const string TimeSpanName = AmqpConstants.Vendor + ":timespan";
        private const string UriName = AmqpConstants.Vendor + ":uri";
        private const string DateTimeOffsetName = AmqpConstants.Vendor + ":datetime-offset";
        private const int GuidSize = 16;

        /// <summary>The size, in bytes, to use as a buffer for stream operations.</summary>
        private const int StreamBufferSizeInBytes = 512;

        public static AmqpMessage BatchSBMessagesAsAmqpMessage(IEnumerable<SBMessage> source)
        {
            Argument.AssertNotNull(source, nameof(source));
            return BuildAmqpBatchFromMessage(source);
        }

        /// <summary>
        ///   Builds a batch <see cref="AmqpMessage" /> from a set of <see cref="SBMessage" />
        ///   optionally propagating the custom properties.
        /// </summary>
        ///
        /// <param name="source">The set of messages to use as the body of the batch message.</param>
        ///
        /// <returns>The batch <see cref="AmqpMessage" /> containing the source messages.</returns>
        ///
        private static AmqpMessage BuildAmqpBatchFromMessage(IEnumerable<SBMessage> source)
        {
            AmqpMessage firstAmqpMessage = null;
            SBMessage firstMessage = null;

            return BuildAmqpBatchFromMessages(
                source.Select(sbMessage =>
                {
                    if (firstAmqpMessage == null)
                    {
                        firstAmqpMessage = SBMessageToAmqpMessage(sbMessage);
                        firstMessage = sbMessage;
                        return firstAmqpMessage;
                    }
                    else
                    {
                        return SBMessageToAmqpMessage(sbMessage);
                    }
                }), firstMessage);
        }

        /// <summary>
        ///   Builds a batch <see cref="AmqpMessage" /> from a set of <see cref="AmqpMessage" />.
        /// </summary>
        ///
        /// <param name="source">The set of messages to use as the body of the batch message.</param>
        /// <param name="firstMessage"></param>
        ///
        /// <returns>The batch <see cref="AmqpMessage" /> containing the source messages.</returns>
        ///
        private static AmqpMessage BuildAmqpBatchFromMessages(
            IEnumerable<AmqpMessage> source,
            SBMessage firstMessage = null)
        {
            AmqpMessage batchEnvelope;

            var batchMessages = source.ToList();

            if (batchMessages.Count == 1)
            {
                batchEnvelope = batchMessages[0];
            }
            else
            {
                batchEnvelope = AmqpMessage.Create(batchMessages.Select(message =>
                {
                    message.Batchable = true;
                    using var messageStream = message.ToStream();
                    return new Data { Value = ReadStreamToArraySegment(messageStream) };
                }));

                batchEnvelope.MessageFormat = AmqpConstants.AmqpBatchedMessageFormat;
            }

            if (firstMessage?.MessageId != null)
            {
                batchEnvelope.Properties.MessageId = firstMessage.MessageId;
            }
            if (firstMessage?.SessionId != null)
            {
                batchEnvelope.Properties.GroupId = firstMessage.SessionId;
            }

            if (firstMessage?.PartitionKey != null)
            {
                batchEnvelope.MessageAnnotations.Map[AmqpMessageConverter.PartitionKeyName] =
                    firstMessage.PartitionKey;
            }

            if (firstMessage?.ViaPartitionKey != null)
            {
                batchEnvelope.MessageAnnotations.Map[AmqpMessageConverter.ViaPartitionKeyName] =
                    firstMessage.ViaPartitionKey;
            }

            batchEnvelope.Batchable = true;
            return batchEnvelope;
        }

        /// <summary>
        ///   Converts a stream to an <see cref="ArraySegment{T}" /> representation.
        /// </summary>
        ///
        /// <param name="stream">The stream to read and capture in memory.</param>
        ///
        /// <returns>The <see cref="ArraySegment{T}" /> containing the stream data.</returns>
        ///
        private static ArraySegment<byte> ReadStreamToArraySegment(Stream stream)
        {
            if (stream == null)
            {
                return new ArraySegment<byte>();
            }

            using var memStream = new MemoryStream(StreamBufferSizeInBytes);
            stream.CopyTo(memStream, StreamBufferSizeInBytes);

            return new ArraySegment<byte>(memStream.ToArray());
        }

        public static AmqpMessage SBMessageToAmqpMessage(SBMessage sbMessage)
        {
            ReadOnlyMemory<byte> bodyBytes = sbMessage.Body.AsBytes();
            var body = new ArraySegment<byte>((bodyBytes.IsEmpty) ? Array.Empty<byte>() : bodyBytes.ToArray());
            var amqpMessage = AmqpMessage.Create(new Data { Value = body });
            amqpMessage.Properties.MessageId = sbMessage.MessageId;
            amqpMessage.Properties.CorrelationId = sbMessage.CorrelationId;
            amqpMessage.Properties.ContentType = sbMessage.ContentType;
            amqpMessage.Properties.Subject = sbMessage.Label;
            amqpMessage.Properties.To = sbMessage.To;
            amqpMessage.Properties.ReplyTo = sbMessage.ReplyTo;
            amqpMessage.Properties.GroupId = sbMessage.SessionId;
            amqpMessage.Properties.ReplyToGroupId = sbMessage.ReplyToSessionId;

            if (sbMessage.TimeToLive != TimeSpan.MaxValue)
            {
                amqpMessage.Header.Ttl = (uint)sbMessage.TimeToLive.TotalMilliseconds;
                amqpMessage.Properties.CreationTime = DateTime.UtcNow;

                if (AmqpConstants.MaxAbsoluteExpiryTime - amqpMessage.Properties.CreationTime.Value > sbMessage.TimeToLive)
                {
                    amqpMessage.Properties.AbsoluteExpiryTime = amqpMessage.Properties.CreationTime.Value + sbMessage.TimeToLive;
                }
                else
                {
                    amqpMessage.Properties.AbsoluteExpiryTime = AmqpConstants.MaxAbsoluteExpiryTime;
                }
            }

            if ((sbMessage.ScheduledEnqueueTime != null) && sbMessage.ScheduledEnqueueTime > DateTimeOffset.MinValue)
            {
                amqpMessage.MessageAnnotations.Map.Add(ScheduledEnqueueTimeUtcName, sbMessage.ScheduledEnqueueTime.UtcDateTime);
            }

            if (sbMessage.PartitionKey != null)
            {
                amqpMessage.MessageAnnotations.Map.Add(PartitionKeyName, sbMessage.PartitionKey);
            }

            if (sbMessage.ViaPartitionKey != null)
            {
                amqpMessage.MessageAnnotations.Map.Add(ViaPartitionKeyName, sbMessage.ViaPartitionKey);
            }

            if (sbMessage.Properties != null && sbMessage.Properties.Count > 0)
            {
                if (amqpMessage.ApplicationProperties == null)
                {
                    amqpMessage.ApplicationProperties = new ApplicationProperties();
                }

                foreach (var pair in sbMessage.Properties)
                {
                    if (TryGetAmqpObjectFromNetObject(pair.Value, MappingType.ApplicationProperty, out var amqpObject))
                    {
                        amqpMessage.ApplicationProperties.Map.Add(pair.Key, amqpObject);
                    }
                    else
                    {
                        throw new NotSupportedException(Resources.InvalidAmqpMessageProperty.FormatForUser(pair.Key.GetType()));
                    }
                }
            }

            return amqpMessage;
        }

        public static ServiceBusReceivedMessage AmqpMessageToSBMessage(AmqpMessage amqpMessage, bool isPeeked = false)
        {
            Argument.AssertNotNull(amqpMessage, nameof(amqpMessage));

            ServiceBusReceivedMessage sbMessage;

            if ((amqpMessage.BodyType & SectionFlag.AmqpValue) != 0
                && amqpMessage.ValueBody.Value != null)
            {
                sbMessage = new ServiceBusReceivedMessage();

                if (TryGetNetObjectFromAmqpObject(amqpMessage.ValueBody.Value, MappingType.MessageBody, out var dotNetObject))
                {
                    sbMessage.BodyObject = dotNetObject;
                }
                else
                {
                    sbMessage.BodyObject = amqpMessage.ValueBody.Value;
                }
            }
            else if ((amqpMessage.BodyType & SectionFlag.Data) != 0
                && amqpMessage.DataBody != null)
            {
                var dataSegments = new List<byte>();
                foreach (var data in amqpMessage.DataBody)
                {
                    if (data.Value is byte[] byteArrayValue)
                    {
                        dataSegments.AddRange(byteArrayValue);
                    }
                    else if (data.Value is ArraySegment<byte> arraySegmentValue)
                    {
                        byte[] byteArray;
                        if (arraySegmentValue.Count == arraySegmentValue.Array.Length)
                        {
                            byteArray = arraySegmentValue.Array;
                        }
                        else
                        {
                            byteArray = new byte[arraySegmentValue.Count];
                            Array.ConstrainedCopy(arraySegmentValue.Array, arraySegmentValue.Offset, byteArray, 0, arraySegmentValue.Count);
                        }
                        dataSegments.AddRange(byteArray);
                    }
                }
                sbMessage = new ServiceBusReceivedMessage(dataSegments.ToArray());
            }
            else
            {
                sbMessage = new ServiceBusReceivedMessage();
            }

            var sections = amqpMessage.Sections;
            if ((sections & SectionFlag.Header) != 0)
            {
                if (amqpMessage.Header.Ttl != null)
                {
                    sbMessage.SentMessage.TimeToLive = TimeSpan.FromMilliseconds(amqpMessage.Header.Ttl.Value);
                }

                if (amqpMessage.Header.DeliveryCount != null)
                {
                    sbMessage.DeliveryCount = isPeeked ? (int)(amqpMessage.Header.DeliveryCount.Value) : (int)(amqpMessage.Header.DeliveryCount.Value + 1);
                }
            }

            if ((sections & SectionFlag.Properties) != 0)
            {
                if (amqpMessage.Properties.MessageId != null)
                {
                    sbMessage.SentMessage.MessageId = amqpMessage.Properties.MessageId.ToString();
                }

                if (amqpMessage.Properties.CorrelationId != null)
                {
                    sbMessage.SentMessage.CorrelationId = amqpMessage.Properties.CorrelationId.ToString();
                }

                if (amqpMessage.Properties.ContentType.Value != null)
                {
                    sbMessage.SentMessage.ContentType = amqpMessage.Properties.ContentType.Value;
                }

                if (amqpMessage.Properties.Subject != null)
                {
                    sbMessage.SentMessage.Label = amqpMessage.Properties.Subject;
                }

                if (amqpMessage.Properties.To != null)
                {
                    sbMessage.SentMessage.To = amqpMessage.Properties.To.ToString();
                }

                if (amqpMessage.Properties.ReplyTo != null)
                {
                    sbMessage.SentMessage.ReplyTo = amqpMessage.Properties.ReplyTo.ToString();
                }

                if (amqpMessage.Properties.GroupId != null)
                {
                    sbMessage.SentMessage.SessionId = amqpMessage.Properties.GroupId;
                }

                if (amqpMessage.Properties.ReplyToGroupId != null)
                {
                    sbMessage.SentMessage.ReplyToSessionId = amqpMessage.Properties.ReplyToGroupId;
                }
            }

            // Do application properties before message annotations, because the application properties
            // can be updated by entries from message annotation.
            if ((sections & SectionFlag.ApplicationProperties) != 0)
            {
                foreach (var pair in amqpMessage.ApplicationProperties.Map)
                {
                    if (TryGetNetObjectFromAmqpObject(pair.Value, MappingType.ApplicationProperty, out var netObject))
                    {
                        sbMessage.SentMessage.Properties[pair.Key.ToString()] = netObject;
                    }
                }
            }

            if ((sections & SectionFlag.MessageAnnotations) != 0)
            {
                foreach (var pair in amqpMessage.MessageAnnotations.Map)
                {
                    var key = pair.Key.ToString();
                    switch (key)
                    {
                        case EnqueuedTimeUtcName:
                            sbMessage.EnqueuedTime = (DateTime)pair.Value;
                            break;
                        case ScheduledEnqueueTimeUtcName:
                            sbMessage.SentMessage.ScheduledEnqueueTime = (DateTime)pair.Value;
                            break;
                        case SequenceNumberName:
                            sbMessage.SequenceNumber = (long)pair.Value;
                            break;
                        case EnqueueSequenceNumberName:
                            sbMessage.EnqueuedSequenceNumber = (long)pair.Value;
                            break;
                        case LockedUntilName:
                            sbMessage.LockedUntil = (DateTime)pair.Value >= DateTimeOffset.MaxValue.UtcDateTime ?
                                DateTimeOffset.MaxValue : (DateTime)pair.Value;
                            break;
                        case PartitionKeyName:
                            sbMessage.SentMessage.PartitionKey = (string)pair.Value;
                            break;
                        case PartitionIdName:
                            sbMessage.PartitionId = (short)pair.Value;
                            break;
                        case ViaPartitionKeyName:
                            sbMessage.SentMessage.ViaPartitionKey = (string)pair.Value;
                            break;
                        case DeadLetterSourceName:
                            sbMessage.DeadLetterSource = (string)pair.Value;
                            break;
                        default:
                            if (TryGetNetObjectFromAmqpObject(pair.Value, MappingType.ApplicationProperty, out var netObject))
                            {
                                sbMessage.SentMessage.Properties[key] = netObject;
                            }
                            break;
                    }
                }
            }

            if (amqpMessage.DeliveryTag.Count == GuidSize)
            {
                var guidBuffer = new byte[GuidSize];
                Buffer.BlockCopy(amqpMessage.DeliveryTag.Array, amqpMessage.DeliveryTag.Offset, guidBuffer, 0, GuidSize);
                sbMessage.LockTokenGuid = new Guid(guidBuffer);
            }

            amqpMessage.Dispose();

            return sbMessage;
        }

        public static AmqpMap GetRuleDescriptionMap(RuleDescription description)
        {
            var ruleDescriptionMap = new AmqpMap();

            switch (description.Filter)
            {
                case SqlRuleFilter sqlRuleFilter:
                    var filterMap = GetSqlRuleFilterMap(sqlRuleFilter);
                    ruleDescriptionMap[ManagementConstants.Properties.SqlRuleFilter] = filterMap;
                    break;
                case CorrelationRuleFilter correlationFilter:
                    var correlationFilterMap = GetCorrelationRuleFilterMap(correlationFilter);
                    ruleDescriptionMap[ManagementConstants.Properties.CorrelationRuleFilter] = correlationFilterMap;
                    break;
                default:
                    throw new NotSupportedException(
                        Resources.RuleFilterNotSupported.FormatForUser(
                            description.Filter.GetType(),
                            nameof(SqlRuleFilter),
                            nameof(CorrelationRuleFilter)));
            }

            var amqpAction = GetRuleActionMap(description.Action as SqlRuleAction);
            ruleDescriptionMap[ManagementConstants.Properties.SqlRuleAction] = amqpAction;
            ruleDescriptionMap[ManagementConstants.Properties.RuleName] = description.Name;

            return ruleDescriptionMap;
        }

        public static RuleDescription GetRuleDescription(AmqpRuleDescriptionCodec amqpDescription)
        {
            var filter = GetFilter(amqpDescription.Filter);
            var ruleAction = GetRuleAction(amqpDescription.Action);

            var ruleDescription = new RuleDescription(amqpDescription.RuleName, filter)
            {
                Action = ruleAction
            };

            return ruleDescription;
        }

        public static RuleFilter GetFilter(AmqpRuleFilterCodec amqpFilter)
        {
            RuleFilter filter;

            switch (amqpFilter.DescriptorCode)
            {
                case AmqpSqlRuleFilterCodec.Code:
                    var amqpSqlFilter = (AmqpSqlRuleFilterCodec)amqpFilter;
                    filter = new SqlRuleFilter(amqpSqlFilter.Expression);
                    break;

                case AmqpTrueRuleFilterCodec.Code:
                    filter = new TrueRuleFilter();
                    break;

                case AmqpFalseRuleFilterCodec.Code:
                    filter = new FalseRuleFilter();
                    break;

                case AmqpCorrelationRuleFilterCodec.Code:
                    var amqpCorrelationFilter = (AmqpCorrelationRuleFilterCodec)amqpFilter;
                    var correlationFilter = new CorrelationRuleFilter
                    {
                        CorrelationId = amqpCorrelationFilter.CorrelationId,
                        MessageId = amqpCorrelationFilter.MessageId,
                        To = amqpCorrelationFilter.To,
                        ReplyTo = amqpCorrelationFilter.ReplyTo,
                        Label = amqpCorrelationFilter.Label,
                        SessionId = amqpCorrelationFilter.SessionId,
                        ReplyToSessionId = amqpCorrelationFilter.ReplyToSessionId,
                        ContentType = amqpCorrelationFilter.ContentType
                    };

                    foreach (var property in amqpCorrelationFilter.Properties)
                    {
                        correlationFilter.Properties.Add(property.Key.Key.ToString(), property.Value);
                    }

                    filter = correlationFilter;
                    break;

                default:
                    throw new NotSupportedException($"Unknown filter descriptor code: {amqpFilter.DescriptorCode}");
            }

            return filter;
        }

        private static RuleAction GetRuleAction(AmqpRuleActionCodec amqpAction)
        {
            RuleAction action;

            if (amqpAction.DescriptorCode == AmqpEmptyRuleActionCodec.Code)
            {
                action = null;
            }
            else if (amqpAction.DescriptorCode == AmqpSqlRuleActionCodec.Code)
            {
                var amqpSqlRuleAction = (AmqpSqlRuleActionCodec)amqpAction;
                var sqlRuleAction = new SqlRuleAction(amqpSqlRuleAction.SqlExpression);

                action = sqlRuleAction;
            }
            else
            {
                throw new NotSupportedException($"Unknown action descriptor code: {amqpAction.DescriptorCode}");
            }

            return action;
        }

        internal static bool TryGetAmqpObjectFromNetObject(object netObject, MappingType mappingType, out object amqpObject)
        {
            amqpObject = null;
            if (netObject == null)
            {
                return true;
            }

            switch (SerializationUtilities.GetTypeId(netObject))
            {
                case PropertyValueType.Byte:
                case PropertyValueType.SByte:
                case PropertyValueType.Int16:
                case PropertyValueType.Int32:
                case PropertyValueType.Int64:
                case PropertyValueType.UInt16:
                case PropertyValueType.UInt32:
                case PropertyValueType.UInt64:
                case PropertyValueType.Single:
                case PropertyValueType.Double:
                case PropertyValueType.Boolean:
                case PropertyValueType.Decimal:
                case PropertyValueType.Char:
                case PropertyValueType.Guid:
                case PropertyValueType.DateTime:
                case PropertyValueType.String:
                    amqpObject = netObject;
                    break;
                case PropertyValueType.Stream:
                    if (mappingType == MappingType.ApplicationProperty)
                    {
                        amqpObject = StreamToBytes((Stream)netObject);
                    }
                    break;
                case PropertyValueType.Uri:
                    amqpObject = new DescribedType((AmqpSymbol)UriName, ((Uri)netObject).AbsoluteUri);
                    break;
                case PropertyValueType.DateTimeOffset:
                    amqpObject = new DescribedType((AmqpSymbol)DateTimeOffsetName, ((DateTimeOffset)netObject).UtcTicks);
                    break;
                case PropertyValueType.TimeSpan:
                    amqpObject = new DescribedType((AmqpSymbol)TimeSpanName, ((TimeSpan)netObject).Ticks);
                    break;
                case PropertyValueType.Unknown:
                    if (netObject is Stream netObjectAsStream)
                    {
                        if (mappingType == MappingType.ApplicationProperty)
                        {
                            amqpObject = StreamToBytes(netObjectAsStream);
                        }
                    }
                    else if (mappingType == MappingType.ApplicationProperty)
                    {
                        throw new SerializationException(Resources.FailedToSerializeUnsupportedType.FormatForUser(netObject.GetType().FullName));
                    }
                    else if (netObject is byte[] netObjectAsByteArray)
                    {
                        amqpObject = new ArraySegment<byte>(netObjectAsByteArray);
                    }
                    else if (netObject is IList)
                    {
                        // Array is also an IList
                        amqpObject = netObject;
                    }
                    else if (netObject is IDictionary netObjectAsDictionary)
                    {
                        amqpObject = new AmqpMap(netObjectAsDictionary);
                    }
                    break;
            }

            return amqpObject != null;
        }

        private static bool TryGetNetObjectFromAmqpObject(object amqpObject, MappingType mappingType, out object netObject)
        {
            netObject = null;
            if (amqpObject == null)
            {
                return true;
            }

            switch (SerializationUtilities.GetTypeId(amqpObject))
            {
                case PropertyValueType.Byte:
                case PropertyValueType.SByte:
                case PropertyValueType.Int16:
                case PropertyValueType.Int32:
                case PropertyValueType.Int64:
                case PropertyValueType.UInt16:
                case PropertyValueType.UInt32:
                case PropertyValueType.UInt64:
                case PropertyValueType.Single:
                case PropertyValueType.Double:
                case PropertyValueType.Boolean:
                case PropertyValueType.Decimal:
                case PropertyValueType.Char:
                case PropertyValueType.Guid:
                case PropertyValueType.DateTime:
                case PropertyValueType.String:
                    netObject = amqpObject;
                    break;
                case PropertyValueType.Unknown:
                    if (amqpObject is AmqpSymbol amqpObjectAsAmqpSymbol)
                    {
                        netObject = (amqpObjectAsAmqpSymbol).Value;
                    }
                    else if (amqpObject is ArraySegment<byte> amqpObjectAsArraySegment)
                    {
                        ArraySegment<byte> binValue = amqpObjectAsArraySegment;
                        if (binValue.Count == binValue.Array.Length)
                        {
                            netObject = binValue.Array;
                        }
                        else
                        {
                            var buffer = new byte[binValue.Count];
                            Buffer.BlockCopy(binValue.Array, binValue.Offset, buffer, 0, binValue.Count);
                            netObject = buffer;
                        }
                    }
                    else if (amqpObject is DescribedType amqpObjectAsDescribedType)
                    {
                        if (amqpObjectAsDescribedType.Descriptor is AmqpSymbol)
                        {
                            var amqpSymbol = (AmqpSymbol)amqpObjectAsDescribedType.Descriptor;
                            if (amqpSymbol.Equals((AmqpSymbol)UriName))
                            {
                                netObject = new Uri((string)amqpObjectAsDescribedType.Value);
                            }
                            else if (amqpSymbol.Equals((AmqpSymbol)TimeSpanName))
                            {
                                netObject = new TimeSpan((long)amqpObjectAsDescribedType.Value);
                            }
                            else if (amqpSymbol.Equals((AmqpSymbol)DateTimeOffsetName))
                            {
                                netObject = new DateTimeOffset(new DateTime((long)amqpObjectAsDescribedType.Value, DateTimeKind.Utc));
                            }
                        }
                    }
                    else if (mappingType == MappingType.ApplicationProperty)
                    {
                        throw new SerializationException(Resources.FailedToSerializeUnsupportedType.FormatForUser(amqpObject.GetType().FullName));
                    }
                    else if (amqpObject is AmqpMap map)
                    {
                        var dictionary = new Dictionary<string, object>();
                        foreach (var pair in map)
                        {
                            dictionary.Add(pair.Key.ToString(), pair.Value);
                        }

                        netObject = dictionary;
                    }
                    else
                    {
                        netObject = amqpObject;
                    }
                    break;
            }

            return netObject != null;
        }

        private static ArraySegment<byte> StreamToBytes(Stream stream)
        {
            ArraySegment<byte> buffer;
            if (stream == null || stream.Length < 1)
            {
                buffer = default;
            }
            else
            {
                using (var memoryStream = new MemoryStream(512))
                {
                    stream.CopyTo(memoryStream, 512);
                    buffer = new ArraySegment<byte>(memoryStream.ToArray());
                }
            }

            return buffer;
        }

        private static Data ToData(AmqpMessage message)
        {
            ArraySegment<byte>[] payload = message.GetPayload();
            var buffer = new BufferListStream(payload);
            ArraySegment<byte> value = buffer.ReadBytes((int)buffer.Length);
            return new Data { Value = value };
        }

        internal static AmqpMap GetSqlRuleFilterMap(SqlRuleFilter sqlRuleFilter)
        {
            var amqpFilterMap = new AmqpMap
            {
                [ManagementConstants.Properties.Expression] = sqlRuleFilter.SqlExpression
            };
            return amqpFilterMap;
        }

        internal static AmqpMap GetCorrelationRuleFilterMap(CorrelationRuleFilter correlationRuleFilter)
        {
            var correlationRuleFilterMap = new AmqpMap
            {
                [ManagementConstants.Properties.CorrelationId] = correlationRuleFilter.CorrelationId,
                [ManagementConstants.Properties.MessageId] = correlationRuleFilter.MessageId,
                [ManagementConstants.Properties.To] = correlationRuleFilter.To,
                [ManagementConstants.Properties.ReplyTo] = correlationRuleFilter.ReplyTo,
                [ManagementConstants.Properties.Label] = correlationRuleFilter.Label,
                [ManagementConstants.Properties.SessionId] = correlationRuleFilter.SessionId,
                [ManagementConstants.Properties.ReplyToSessionId] = correlationRuleFilter.ReplyToSessionId,
                [ManagementConstants.Properties.ContentType] = correlationRuleFilter.ContentType
            };

            var propertiesMap = new AmqpMap();
            foreach (var property in correlationRuleFilter.Properties)
            {
                propertiesMap[new MapKey(property.Key)] = property.Value;
            }

            correlationRuleFilterMap[ManagementConstants.Properties.CorrelationRuleFilterProperties] = propertiesMap;

            return correlationRuleFilterMap;
        }

        internal static AmqpMap GetRuleActionMap(SqlRuleAction sqlRuleAction)
        {
            AmqpMap ruleActionMap = null;
            if (sqlRuleAction != null)
            {
                ruleActionMap = new AmqpMap { [ManagementConstants.Properties.Expression] = sqlRuleAction.SqlExpression };
            }

            return ruleActionMap;
        }
    }
}
