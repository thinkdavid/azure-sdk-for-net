﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Messaging.ServiceBus.Core;
using Azure.Messaging.ServiceBus.Diagnostics;

namespace Azure.Messaging.ServiceBus
{
    /// <summary>
    ///   A client responsible for sending <see cref="ServiceBusMessage" /> to a specific Service Bus entity
    ///   (Queue or Topic). It can be used for both session and non-session entities. It is constructed by calling <see cref="ServiceBusClient.CreateSender(string)"/>.
    /// </summary>
    ///
    public class ServiceBusSender : IAsyncDisposable
    {
        /// <summary>The minimum allowable size, in bytes, for a batch to be sent.</summary>
        internal const int MinimumBatchSizeLimit = 24;

        /// <summary>
        ///   The fully qualified Service Bus namespace that the producer is associated with.  This is likely
        ///   to be similar to <c>{yournamespace}.servicebus.windows.net</c>.
        /// </summary>
        ///
        public string FullyQualifiedNamespace => _connection.FullyQualifiedNamespace;

        /// <summary>
        ///   The path of the entity that the sender is connected to, specific to the
        ///   Service Bus namespace that contains it.
        /// </summary>
        ///
        public string EntityPath { get; }

        /// <summary>
        ///   Indicates whether or not this <see cref="ServiceBusSender"/> has been disposed.
        /// </summary>
        ///
        /// <value>
        ///   <c>true</c> if the client is disposed; otherwise, <c>false</c>.
        /// </value>
        ///
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        ///   The instance of <see cref="ServiceBusEventSource" /> which can be mocked for testing.
        /// </summary>
        ///
        internal ServiceBusEventSource Logger { get; set; } = ServiceBusEventSource.Log;

        /// <summary>
        /// In the case of a via-sender, the message is sent to <see cref="EntityPath"/> via <see cref="ViaEntityPath"/>; null otherwise.
        /// </summary>
        public string ViaEntityPath { get; }

        /// <summary>
        /// Gets the ID to identify this client. This can be used to correlate logs and exceptions.
        /// </summary>
        /// <remarks>Every new client has a unique ID.</remarks>
        internal string Identifier { get; private set; }

        /// <summary>
        ///   The policy to use for determining retry behavior for when an operation fails.
        /// </summary>
        ///
        private readonly ServiceBusRetryPolicy _retryPolicy;

        /// <summary>
        ///   The active connection to the Azure Service Bus service, enabling client communications for metadata
        ///   about the associated Service Bus entity and access to transport-aware consumers.
        /// </summary>
        ///
        private readonly ServiceBusConnection _connection;

        /// <summary>
        ///   An abstracted Service Bus entity transport-specific sender that is associated with the
        ///   Service Bus entity gateway rather than a specific partition; intended to perform delegated operations.
        /// </summary>
        ///
        private readonly TransportSender _innerSender;
        private readonly EntityScopeFactory _scopeFactory;

        /// <summary>
        ///   Initializes a new instance of the <see cref="ServiceBusSender"/> class.
        /// </summary>
        /// <param name="entityPath">The entity path to send the message to.</param>
        /// <param name="options">The set of <see cref="ServiceBusSenderOptions"/> to use for configuring
        /// this <see cref="ServiceBusSender"/>.</param>
        /// <param name="connection">The connection for the sender.</param>
        ///
        internal ServiceBusSender(
            string entityPath,
            ServiceBusSenderOptions options,
            ServiceBusConnection connection)
        {
            Logger.ClientCreateStart(typeof(ServiceBusSender), connection?.FullyQualifiedNamespace, entityPath);
            try
            {
                Argument.AssertNotNull(connection, nameof(connection));
                Argument.AssertNotNull(connection.RetryOptions, nameof(connection.RetryOptions));
                Argument.AssertNotNullOrWhiteSpace(entityPath, nameof(entityPath));
                connection.ThrowIfClosed();

                options = options?.Clone() ?? new ServiceBusSenderOptions();
                EntityPath = entityPath;
                ViaEntityPath = options.ViaQueueOrTopicName;
                Identifier = DiagnosticUtilities.GenerateIdentifier(EntityPath);
                _connection = connection;
                _retryPolicy = _connection.RetryOptions.ToRetryPolicy();
                _innerSender = _connection.CreateTransportSender(
                    entityPath,
                    ViaEntityPath,
                    _retryPolicy,
                    Identifier);
                _scopeFactory = new EntityScopeFactory(EntityPath, FullyQualifiedNamespace);
            }
            catch (Exception ex)
            {
                Logger.ClientCreateException(typeof(ServiceBusSender), connection?.FullyQualifiedNamespace, entityPath, ex);
                throw;
            }
            Logger.ClientCreateComplete(typeof(ServiceBusSender), Identifier);
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="ServiceBusSender"/> class for mocking.
        /// </summary>
        ///
        protected ServiceBusSender()
        {
        }

        /// <summary>
        ///   Sends a message to the associated entity of Service Bus.
        /// </summary>
        ///
        /// <param name="message">A messsage to send.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        ///
        public virtual async Task SendAsync(
            ServiceBusMessage message,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(message, nameof(message));
            await SendAsync(
                new ServiceBusMessage[] { message },
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///   Sends a set of messages to the associated Service Bus entity using a batched approach.
        ///   If the size of the messages exceed the maximum size of a single batch,
        ///   an exception will be triggered and the send will fail. In order to ensure that the messages
        ///   being sent will fit in a batch, use <see cref="SendAsync(ServiceBusMessageBatch, CancellationToken)"/> instead.
        /// </summary>
        ///
        /// <param name="messages">The set of messages to send.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        ///
        public virtual async Task SendAsync(
            IEnumerable<ServiceBusMessage> messages,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(messages, nameof(messages));
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusSender));
            IList<ServiceBusMessage> messageList = messages.ToList();
            if (messageList.Count == 0)
            {
                return;
            }

            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.SendMessageStart(Identifier, messageCount: messageList.Count);
            using DiagnosticScope scope = CreateDiagnosticScope(messages, DiagnosticProperty.SendActivityName);
            scope.Start();

            try
            {
                await _innerSender.SendAsync(
                    messageList,
                    cancellationToken).ConfigureAwait(false);
            }

            catch (Exception exception)
            {
                Logger.SendMessageException(Identifier, exception.ToString());
                scope.Failed(exception);
                throw;
            }
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.SendMessageComplete(Identifier);
        }

        private DiagnosticScope CreateDiagnosticScope(IEnumerable<ServiceBusMessage> messages, string activityName)
        {
            InstrumentMessages(messages);

            // create a new scope for the specified operation
            DiagnosticScope scope = _scopeFactory.CreateScope(
                activityName,
                DiagnosticProperty.ClientKind);

            scope.SetMessageData(messages);
            return scope;
        }

        /// <summary>
        ///   Performs the actions needed to instrument a set of messages.
        /// </summary>
        ///
        /// <param name="messages">The messages to instrument.</param>
        ///
        private void InstrumentMessages(IEnumerable<ServiceBusMessage> messages)
        {
            foreach (ServiceBusMessage message in messages)
            {
                if (!message.Properties.ContainsKey(DiagnosticProperty.DiagnosticIdAttribute))
                {
                    using DiagnosticScope messageScope = _scopeFactory.CreateScope(
                        DiagnosticProperty.MessageActivityName,
                        DiagnosticProperty.SenderKind);
                    messageScope.Start();

                    Activity activity = Activity.Current;
                    if (activity != null)
                    {
                        message.Properties[DiagnosticProperty.DiagnosticIdAttribute] = activity.Id;
                    }
                }
            }
        }

        /// <summary>
        ///   Creates a size-constraint batch to which <see cref="ServiceBusMessage" /> may be added using
        ///   a <see cref="ServiceBusMessageBatch.TryAdd"/>. If a message would exceed the maximum
        ///   allowable size of the batch, the batch will not allow adding the message and signal that
        ///   scenario using it return value.
        ///
        ///   Because messages that would violate the size constraint cannot be added, publishing a batch
        ///   will not trigger an exception when attempting to send the messages to the Queue/Topic.
        /// </summary>
        ///
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns>An <see cref="ServiceBusMessageBatch" /> with the default batch options.</returns>
        ///
        /// <seealso cref="CreateBatchAsync(CreateBatchOptions, CancellationToken)" />
        ///
        public virtual ValueTask<ServiceBusMessageBatch> CreateBatchAsync(CancellationToken cancellationToken = default) => CreateBatchAsync(null, cancellationToken);

        /// <summary>
        ///   Creates a size-constraint batch to which <see cref="ServiceBusMessage" /> may be added using a try-based pattern.  If a message would
        ///   exceed the maximum allowable size of the batch, the batch will not allow adding the message and signal that scenario using its
        ///   return value.
        ///
        ///   Because messages that would violate the size constraint cannot be added, publishing a batch will not trigger an exception when
        ///   attempting to send the messages to the Queue/Topic.
        /// </summary>
        ///
        /// <param name="options">The set of options to consider when creating this batch.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns>An <see cref="ServiceBusMessageBatch" /> with the requested <paramref name="options"/>.</returns>
        ///
        /// <seealso cref="CreateBatchAsync(CreateBatchOptions, CancellationToken)" />
        ///
        public virtual async ValueTask<ServiceBusMessageBatch> CreateBatchAsync(
            CreateBatchOptions options,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusSender));
            options = options?.Clone() ?? new CreateBatchOptions();
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.CreateMessageBatchStart(Identifier);
            ServiceBusMessageBatch batch;
            try
            {
                TransportMessageBatch transportBatch = await _innerSender.CreateBatchAsync(options, cancellationToken).ConfigureAwait(false);
                batch = new ServiceBusMessageBatch(transportBatch);
            }
            catch (Exception ex)
            {
                Logger.CreateMessageBatchException(Identifier, ex.ToString());
                throw;
            }
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.CreateMessageBatchComplete(Identifier);
            return batch;
        }

        /// <summary>
        ///   Sends a <see cref="ServiceBusMessageBatch"/>
        ///   containing a set of <see cref="ServiceBusMessage"/> to
        ///   the associated Service Bus entity.
        /// </summary>
        ///
        /// <param name="messageBatch">The batch of messages to send. A batch may be created using <see cref="CreateBatchAsync(CancellationToken)" />.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        ///
        public virtual async Task SendAsync(
            ServiceBusMessageBatch messageBatch,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(messageBatch, nameof(messageBatch));
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusSender));
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.SendMessageStart(Identifier, messageBatch.Count);
            using DiagnosticScope scope = CreateDiagnosticScope(
                messageBatch.AsEnumerable<ServiceBusMessage>(),
                DiagnosticProperty.SendActivityName);
            scope.Start();

            try
            {
                messageBatch.Lock();
                await _innerSender.SendBatchAsync(messageBatch, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.SendMessageException(Identifier, exception.ToString());
                scope.Failed(exception);
                throw;
            }
            finally
            {
                messageBatch.Unlock();
            }

            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.SendMessageComplete(Identifier);
        }

        /// <summary>
        /// Schedules a message to appear on Service Bus at a later time.
        /// </summary>
        ///
        /// <param name="message">The message to schedule.</param>
        /// <param name="scheduledEnqueueTime">The UTC time at which the message should be available for processing</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>Although the message will not be available to be received until the scheduledEnqueueTime, it can still be peeked before that time.</remarks>
        /// <returns>The sequence number of the message that was scheduled.</returns>
        public virtual async Task<long> ScheduleMessageAsync(
            ServiceBusMessage message,
            DateTimeOffset scheduledEnqueueTime,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(message, nameof(message));
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusSender));
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.ScheduleMessageStart(Identifier, scheduledEnqueueTime.ToString(CultureInfo.InvariantCulture));
            using DiagnosticScope scope = CreateDiagnosticScope(
                new ServiceBusMessage[] { message },
                DiagnosticProperty.ScheduleActivityName);
            scope.Start();

            long sequenceNumber;
            try
            {
                message.ScheduledEnqueueTime = scheduledEnqueueTime.UtcDateTime;
                sequenceNumber = await _innerSender.ScheduleMessageAsync(message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.ScheduleMessageException(Identifier, exception.ToString());
                scope.Failed(exception);
                throw;
            }

            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.ScheduleMessageComplete(Identifier);
            scope.AddAttribute(DiagnosticProperty.SequenceNumbersAttribute, sequenceNumber);
            return sequenceNumber;
        }

        /// <summary>
        /// Cancels a message that was scheduled.
        /// </summary>
        /// <param name="sequenceNumber">The <see cref="ServiceBusReceivedMessage.SequenceNumber"/> of the message to be cancelled.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        public virtual async Task CancelScheduledMessageAsync(
            long sequenceNumber,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusSender));
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.CancelScheduledMessageStart(Identifier, sequenceNumber);
            using DiagnosticScope scope = _scopeFactory.CreateScope(
                DiagnosticProperty.CancelActivityName,
                DiagnosticProperty.ClientKind);

            scope.AddAttribute(DiagnosticProperty.SequenceNumbersAttribute, sequenceNumber);
            scope.Start();
            try
            {
                await _innerSender.CancelScheduledMessageAsync(sequenceNumber, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.CancelScheduledMessageException(Identifier, ex.ToString());
                throw;
            }

            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.CancelScheduledMessageComplete(Identifier);
        }

        /// <summary>
        ///   Performs the task needed to clean up resources used by the <see cref="ServiceBusSender" />.
        /// </summary>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        ///
        [SuppressMessage("Usage", "AZC0002:Ensure all service methods take an optional CancellationToken parameter.", Justification = "This signature must match the IAsyncDisposable interface.")]
        public virtual async ValueTask DisposeAsync()
        {
            IsDisposed = true;

            Logger.ClientDisposeStart(typeof(ServiceBusSender), Identifier);

            try
            {
                await _innerSender.CloseAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.ClientDisposeException(typeof(ServiceBusSender), Identifier, ex);
                throw;
            }

            Logger.ClientDisposeComplete(typeof(ServiceBusSender), Identifier);
        }

        /// <summary>
        ///   Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        ///
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        ///
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        ///
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///   Returns a hash code for this instance.
        /// </summary>
        ///
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        ///
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        ///   Converts the instance to string representation.
        /// </summary>
        ///
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        ///
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }
}
