﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Messaging.ServiceBus.Core;
using Azure.Messaging.ServiceBus.Diagnostics;
using Azure.Messaging.ServiceBus.Primitives;

namespace Azure.Messaging.ServiceBus
{
    /// <summary>
    /// The <see cref="ServiceBusReceiver" /> is responsible for receiving
    /// <see cref="ServiceBusReceivedMessage" /> and settling messages from Queues and Subscriptions.
    /// It is constructed by calling <see cref="ServiceBusClient.CreateReceiver(string, ServiceBusReceiverOptions)"/>.
    /// </summary>
    public class ServiceBusReceiver : IAsyncDisposable
    {
        /// <summary>
        /// The fully qualified Service Bus namespace that the receiver is associated with.  This is likely
        /// to be similar to <c>{yournamespace}.servicebus.windows.net</c>.
        /// </summary>
        public string FullyQualifiedNamespace => _connection.FullyQualifiedNamespace;

        /// <summary>
        /// The path of the Service Bus entity that the receiver is connected to, specific to the
        /// Service Bus namespace that contains it.
        /// </summary>
        public string EntityPath { get; }

        /// <summary>
        /// The <see cref="ReceiveMode"/> used to specify how messages are received. Defaults to PeekLock mode.
        /// </summary>
        public ReceiveMode ReceiveMode { get; }

        /// <summary>
        /// Indicates whether the receiver entity is session enabled.
        /// </summary>
        internal bool IsSessionReceiver { get; private set; }

        /// <summary>
        /// The number of messages that will be eagerly requested from Queues or Subscriptions and queued locally without regard to
        /// whether a processing is currently active, intended to help maximize throughput by allowing the receiver to receive
        /// from a local cache rather than waiting on a service request.
        /// </summary>
        public int PrefetchCount { get; }

        /// <summary>
        /// Gets the ID to identify this client. This can be used to correlate logs and exceptions.
        /// </summary>
        /// <remarks>Every new client has a unique ID.</remarks>
        internal string Identifier { get; private set; }

        /// <summary>
        ///   Indicates whether or not this <see cref="ServiceBusReceiver"/> has been disposed.
        /// </summary>
        ///
        /// <value>
        /// <c>true</c> if the client is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// The policy to use for determining retry behavior for when an operation fails.
        /// </summary>
        ///
        private readonly ServiceBusRetryPolicy _retryPolicy;

        /// <summary>
        /// The active connection to the Azure Service Bus service, enabling client communications for metadata
        /// about the associated Service Bus entity and access to transport-aware receivers.
        /// </summary>
        ///
        private readonly ServiceBusConnection _connection;

        /// <summary>
        /// An abstracted Service Bus transport-specific receiver that is associated with the
        /// Service Bus entity gateway; intended to perform delegated operations.
        /// </summary>
        internal readonly TransportReceiver InnerReceiver;
        internal readonly EntityScopeFactory _scopeFactory;

        /// <summary>
        ///   The instance of <see cref="ServiceBusEventSource" /> which can be mocked for testing.
        /// </summary>
        ///
        internal ServiceBusEventSource Logger { get; set; } = ServiceBusEventSource.Log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusReceiver"/> class.
        /// </summary>
        ///
        /// <param name="connection">The <see cref="ServiceBusConnection" /> connection to use for communication with the Service Bus service.</param>
        /// <param name="entityPath"></param>
        /// <param name="isSessionEntity"></param>
        /// <param name="options">A set of options to apply when configuring the consumer.</param>
        /// <param name="sessionId">An optional session Id to scope the receiver to. If not specified,
        /// the next available session returned from the service will be used.</param>
        ///
        internal ServiceBusReceiver(
            ServiceBusConnection connection,
            string entityPath,
            bool isSessionEntity,
            ServiceBusReceiverOptions options,
            string sessionId = default)
        {
            Type type = GetType();
            Logger.ClientCreateStart(type, connection?.FullyQualifiedNamespace, entityPath);
            try
            {
                Argument.AssertNotNull(connection, nameof(connection));
                Argument.AssertNotNull(connection.RetryOptions, nameof(connection.RetryOptions));
                Argument.AssertNotNullOrWhiteSpace(entityPath, nameof(entityPath));
                connection.ThrowIfClosed();
                options = options?.Clone() ?? new ServiceBusReceiverOptions();
                Identifier = DiagnosticUtilities.GenerateIdentifier(entityPath);
                _connection = connection;
                _retryPolicy = connection.RetryOptions.ToRetryPolicy();
                ReceiveMode = options.ReceiveMode;
                PrefetchCount = options.PrefetchCount;
                EntityPath = entityPath;
                IsSessionReceiver = isSessionEntity;
                InnerReceiver = _connection.CreateTransportReceiver(
                    entityPath: EntityPath,
                    retryPolicy: _retryPolicy,
                    receiveMode: ReceiveMode,
                    prefetchCount: (uint)PrefetchCount,
                    identifier: Identifier,
                    sessionId: sessionId,
                    isSessionReceiver: IsSessionReceiver);
                _scopeFactory = new EntityScopeFactory(EntityPath, FullyQualifiedNamespace);
                if (!isSessionEntity)
                {
                    // don't log client completion for session receiver here as it is not complete until
                    // the link is opened.
                    Logger.ClientCreateComplete(type, Identifier);
                }
            }
            catch (Exception ex)
            {
                Logger.ClientCreateException(type, connection?.FullyQualifiedNamespace, entityPath, ex);
                throw;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusReceiver"/> class for mocking.
        /// </summary>
        ///
        protected ServiceBusReceiver() { }

        /// <summary>
        /// Receives a batch of <see cref="ServiceBusReceivedMessage" /> from the entity using <see cref="ReceiveMode"/> mode. <see cref="ReceiveMode"/> defaults to PeekLock mode.
        /// This method doesn't guarantee to return exact `maxMessages` messages, even if there are `maxMessages` messages available in the queue or topic.
        /// </summary>
        ///
        /// <param name="maxMessages">The maximum number of messages that will be received.</param>
        /// <param name="maxWaitTime">An optional <see cref="TimeSpan"/> specifying the maximum time to wait for the first message before returning an empty list if no messages have been received.
        /// If not specified, the <see cref="ServiceBusRetryOptions.TryTimeout"/> will be used.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns>List of messages received. Returns an empty list if no message is found.</returns>
        public virtual async Task<IList<ServiceBusReceivedMessage>> ReceiveBatchAsync(
           int maxMessages,
           TimeSpan? maxWaitTime = default,
           CancellationToken cancellationToken = default)
        {
            Argument.AssertAtLeast(maxMessages, 1, nameof(maxMessages));
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusReceiver));
            if (maxWaitTime.HasValue)
            {
                Argument.AssertPositive(maxWaitTime.Value, nameof(maxWaitTime));
            }

            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.ReceiveMessageStart(Identifier, maxMessages);
            using DiagnosticScope scope = _scopeFactory.CreateScope(
                DiagnosticProperty.ReceiveActivityName,
                requestedMessageCount: maxMessages);
            scope.Start();

            IList<ServiceBusReceivedMessage> messages = null;

            try
            {
                messages = await InnerReceiver.ReceiveBatchAsync(
                    maxMessages,
                    maxWaitTime,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.ReceiveMessageException(Identifier, exception.ToString());
                scope.Failed(exception);
                throw;
            }

            Logger.ReceiveMessageComplete(Identifier, messages.Count);
            scope.SetMessageData(messages);

            return messages;
        }

        /// <summary>
        /// Receives a <see cref="ServiceBusReceivedMessage" /> from the entity using <see cref="ReceiveMode"/> mode. <see cref="ReceiveMode"/> defaults to PeekLock mode
        /// </summary>
        /// <param name="maxWaitTime">An optional <see cref="TimeSpan"/> specifying the maximum time to wait for a message before returning a null if no messages are available.
        /// If not specified, the <see cref="ServiceBusRetryOptions.TryTimeout"/> will be used.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns>The message received. Returns null if no message is found.</returns>
        public virtual async Task<ServiceBusReceivedMessage> ReceiveAsync(
            TimeSpan? maxWaitTime = default,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<ServiceBusReceivedMessage> result = await ReceiveBatchAsync(
                maxMessages: 1,
                maxWaitTime: maxWaitTime,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            foreach (ServiceBusReceivedMessage message in result)
            {
                return message;
            }
            return null;
        }

        /// <summary>
        /// Fetches the next active message without changing the state of the receiver or the message source.
        /// </summary>
        ///
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// The first call to <see cref="PeekAsync(CancellationToken)"/> fetches the first active message for this receiver. Each subsequent call fetches the subsequent message in the entity.
        /// Unlike a received message, peeked message will not have a lock token associated with it, and hence it cannot be Completed/Abandoned/Deferred/Deadlettered/Renewed.
        /// Also, unlike <see cref="ReceiveAsync(TimeSpan?, CancellationToken)"/>, this method will fetch even Deferred messages (but not Deadlettered message).
        /// </remarks>
        ///
        /// <returns>The <see cref="ServiceBusReceivedMessage" /> that represents the next message to be read. Returns null when nothing to peek.</returns>
        public virtual async Task<ServiceBusReceivedMessage> PeekAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<ServiceBusReceivedMessage> result = await PeekBatchAtInternalAsync(
                sequenceNumber: null,
                maxMessages: 1,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            foreach (ServiceBusReceivedMessage message in result)
            {
                return message;
            }
            return null;
        }

        /// <summary>
        /// Fetch the next message without changing the state of the receiver or the message source.
        /// </summary>
        ///
        /// <param name="sequenceNumber">The sequence number from where to peek the message.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns></returns>
        public virtual async Task<ServiceBusReceivedMessage> PeekAtAsync(
            long sequenceNumber,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<ServiceBusReceivedMessage> result = await PeekBatchAtAsync(
                sequenceNumber: sequenceNumber,
                maxMessages: 1,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            foreach (ServiceBusReceivedMessage message in result)
            {
                return message;
            }
            return null;
        }

        /// <summary>
        /// Fetches the next batch of active messages without changing the state of the receiver or the message source.
        /// </summary>
        ///
        /// <param name="maxMessages">The maximum number of messages that will be fetched.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// The first call to <see cref="PeekBatchAsync(int, CancellationToken)"/> fetches the first active message for this receiver. Each subsequent call
        /// fetches the subsequent message in the entity.
        /// Unlike a received message, peeked message will not have lock token associated with it, and hence it cannot be Completed/Abandoned/Deferred/Deadlettered/Renewed.
        /// Also, unlike <see cref="ReceiveAsync(TimeSpan?, CancellationToken)"/>, this method will fetch even Deferred messages (but not Deadlettered messages).
        /// </remarks>
        ///
        /// <returns>An <see cref="IList{ServiceBusReceivedMessage}" /> of messages that were peeked.</returns>
        public virtual async Task<IList<ServiceBusReceivedMessage>> PeekBatchAsync(
            int maxMessages,
            CancellationToken cancellationToken = default) =>
            await PeekBatchAtInternalAsync(
                sequenceNumber: null,
                maxMessages: maxMessages,
                cancellationToken: cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Fetches the next batch of active messages without changing the state of the receiver or the message source.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number from where to peek the message.</param>
        /// <param name="maxMessages">The maximum number of messages that will be fetched.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        /// <returns>An <see cref="IList{ServiceBusReceivedMessage}" /> of messages that were peeked.</returns>
        public virtual async Task<IList<ServiceBusReceivedMessage>> PeekBatchAtAsync(
            long sequenceNumber,
            int maxMessages,
            CancellationToken cancellationToken = default) =>
            await PeekBatchAtInternalAsync(
                sequenceNumber: sequenceNumber,
                maxMessages: maxMessages,
                cancellationToken: cancellationToken)
                .ConfigureAwait(false);

        /// <summary>
        /// Fetches the next batch of active messages without changing the state of the receiver or the message source.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number from where to peek the message.</param>
        /// <param name="maxMessages">The maximum number of messages that will be fetched.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        /// <returns>An <see cref="IList{ServiceBusReceivedMessage}" /> of messages that were peeked.</returns>
        private async Task<IList<ServiceBusReceivedMessage>> PeekBatchAtInternalAsync(
            long? sequenceNumber,
            int maxMessages,
            CancellationToken cancellationToken)
        {
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusReceiver));
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.PeekMessageStart(Identifier, sequenceNumber, maxMessages);
            using DiagnosticScope scope = _scopeFactory.CreateScope(
                DiagnosticProperty.PeekActivityName,
                requestedMessageCount: maxMessages);
            scope.Start();

            IList<ServiceBusReceivedMessage> messages = new List<ServiceBusReceivedMessage>();
            try
            {
                messages = await InnerReceiver.PeekBatchAtAsync(
                    sequenceNumber,
                    maxMessages,
                    cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.PeekMessageException(Identifier, exception.ToString());
                scope.Failed(exception);
                throw;
            }

            Logger.PeekMessageComplete(Identifier, messages.Count);
            scope.SetMessageData(messages);
            return messages;
        }

        /// <summary>
        /// Opens an AMQP link for use with receiver operations.
        /// </summary>
        ///
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        internal async Task OpenLinkAsync(CancellationToken cancellationToken) =>
            await InnerReceiver.OpenLinkAsync(cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Completes a <see cref="ServiceBusReceivedMessage"/>. This will delete the message from the service.
        /// </summary>
        /// <param name="message">The message to complete.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// This operation can only be performed on a message that was received by this receiver
        /// when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// </remarks>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public virtual async Task CompleteAsync(
            ServiceBusReceivedMessage message,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(message, nameof(message));
            await CompleteAsync(message.LockToken, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Completes a <see cref="ServiceBusReceivedMessage"/>. This will delete the message from the service.
        /// </summary>
        ///
        /// <param name="lockToken">The lock token of the <see cref="ServiceBusReceivedMessage"/> message to complete.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// This operation can only be performed on a message that was received by this receiver
        /// when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// </remarks>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public virtual async Task CompleteAsync(
            string lockToken,
            CancellationToken cancellationToken = default)
        {
            ThrowIfLockTokenIsEmpty(lockToken);
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusReceiver));
            Argument.AssertNotNullOrEmpty(lockToken, nameof(lockToken));
            ThrowIfNotPeekLockMode();
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.CompleteMessageStart(
                Identifier,
                1,
                lockToken);
            using DiagnosticScope scope = _scopeFactory.CreateScope(
                DiagnosticProperty.CompleteActivityName,
                lockToken: lockToken);
            scope.Start();

            try
            {
                await InnerReceiver.CompleteAsync(
                    lockToken,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.CompleteMessageException(Identifier, exception.ToString());
                scope.Failed(exception);
                throw;
            }

            Logger.CompleteMessageComplete(Identifier);
        }

        /// <summary>
        /// Abandons a <see cref="ServiceBusReceivedMessage"/>.This will make the message available again for immediate processing as the lock on the message held by the receiver will be released.
        /// </summary>
        ///
        /// <param name="message">The <see cref="ServiceBusReceivedMessage"/> to abandon.</param>
        /// <param name="propertiesToModify">The properties of the message to modify while abandoning the message.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// Abandoning a message will increase the delivery count on the message.
        /// This operation can only be performed on messages that were received by this receiver
        /// when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// </remarks>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public virtual async Task AbandonAsync(
            ServiceBusReceivedMessage message,
            IDictionary<string, object> propertiesToModify = null,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(message, nameof(message));
            await AbandonAsync(
                message.LockToken,
                propertiesToModify,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Abandons a <see cref="ServiceBusReceivedMessage"/>. This will make the message available again for processing.
        /// </summary>
        ///
        /// <param name="lockToken">The lock token <see cref="ServiceBusReceivedMessage"/> to abandon.</param>
        /// <param name="propertiesToModify">The properties of the message to modify while abandoning the message.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// Abandoning a message will increase the delivery count on the message.
        /// This operation can only be performed on messages that were received by this receiver
        /// when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// </remarks>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public virtual async Task AbandonAsync(
            string lockToken,
            IDictionary<string, object> propertiesToModify = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfLockTokenIsEmpty(lockToken);
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusReceiver));
            ThrowIfNotPeekLockMode();
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.AbandonMessageStart(Identifier, 1, lockToken);
            using DiagnosticScope scope = _scopeFactory.CreateScope(
                DiagnosticProperty.AbandonActivityName,
                lockToken: lockToken);
            scope.Start();

            try
            {
                await InnerReceiver.AbandonAsync(
                    lockToken,
                    propertiesToModify,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.AbandonMessageException(Identifier, exception.ToString());
                scope.Failed(exception);
                throw;
            }

            Logger.AbandonMessageComplete(Identifier);
        }

        /// <summary>
        /// Moves a message to the deadletter sub-queue.
        /// </summary>
        ///
        /// <param name="message">The <see cref="ServiceBusReceivedMessage"/> to deadletter.</param>
        /// <param name="propertiesToModify">The properties of the message to modify while moving to sub-queue.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// In order to receive a message from the deadletter queue, you can call
        /// <see cref="ServiceBusClient.CreateDeadLetterReceiver(string, ServiceBusReceiverOptions)"/>
        /// or <see cref="ServiceBusClient.CreateDeadLetterReceiver(string, string, ServiceBusReceiverOptions)"/>
        /// to create a receiver for the queue or subscription.
        /// This operation can only be performed when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// </remarks>
        public virtual async Task DeadLetterAsync(
            ServiceBusReceivedMessage message,
            IDictionary<string, object> propertiesToModify = null,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(message, nameof(message));
            await DeadLetterAsync(
                lockToken: message.LockToken,
                propertiesToModify: propertiesToModify,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Moves a message to the deadletter sub-queue.
        /// </summary>
        ///
        /// <param name="lockToken">The lock token of the <see cref="ServiceBusReceivedMessage"/> to deadletter.</param>
        /// <param name="propertiesToModify">The properties of the message to modify while moving to sub-queue.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// In order to receive a message from the deadletter queue, you can call
        /// <see cref="ServiceBusClient.CreateDeadLetterReceiver(string, ServiceBusReceiverOptions)"/>
        /// or <see cref="ServiceBusClient.CreateDeadLetterReceiver(string, string, ServiceBusReceiverOptions)"/>
        /// to create a receiver for the queue or subscription.
        /// This operation can only be performed when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// </remarks>
        public virtual async Task DeadLetterAsync(
            string lockToken,
            IDictionary<string, object> propertiesToModify = null,
            CancellationToken cancellationToken = default) =>
            await DeadLetterInternalAsync(
                lockToken: lockToken,
                propertiesToModify: propertiesToModify,
                cancellationToken: cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Moves a message to the deadletter sub-queue.
        /// </summary>
        ///
        /// <param name="message">The <see cref="ServiceBusReceivedMessage"/> to deadletter.</param>
        /// <param name="deadLetterReason">The reason for deadlettering the message.</param>
        /// <param name="deadLetterErrorDescription">The error description for deadlettering the message.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// A lock token can be found in <see cref="ServiceBusReceivedMessage.LockToken"/>,
        /// only when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// In order to receive a message from the deadletter queue, you will need a new <see cref="ServiceBusReceiver"/>, with the corresponding path.
        /// You can use EntityNameHelper.FormatDeadLetterPath(string) to help with this.
        /// This operation can only be performed on messages that were received by this receiver.
        /// </remarks>
        public virtual async Task DeadLetterAsync(
            ServiceBusReceivedMessage message,
            string deadLetterReason,
            string deadLetterErrorDescription = default,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(message, nameof(message));
            await DeadLetterAsync(
                lockToken: message.LockToken,
                deadLetterReason: deadLetterReason,
                deadLetterErrorDescription: deadLetterErrorDescription,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Moves a message to the deadletter sub-queue.
        /// </summary>
        ///
        /// <param name="lockToken">The lock token of the <see cref="ServiceBusReceivedMessage"/> to deadletter.</param>
        /// <param name="deadLetterReason">The reason for deadlettering the message.</param>
        /// <param name="deadLetterErrorDescription">The error description for deadlettering the message.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// A lock token can be found in <see cref="ServiceBusReceivedMessage.LockToken"/>,
        /// only when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// In order to receive a message from the deadletter queue, you will need a new <see cref="ServiceBusReceiver"/>, with the corresponding path.
        /// You can use EntityNameHelper.FormatDeadLetterPath(string) to help with this.
        /// This operation can only be performed on messages that were received by this receiver.
        /// </remarks>
        public virtual async Task DeadLetterAsync(
            string lockToken,
            string deadLetterReason,
            string deadLetterErrorDescription = null,
            CancellationToken cancellationToken = default) =>
            await DeadLetterInternalAsync(
                lockToken: lockToken,
                deadLetterReason: deadLetterReason,
                deadLetterErrorDescription: deadLetterErrorDescription,
                cancellationToken: cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Moves a message to the deadletter sub-queue.
        /// </summary>
        ///
        /// <param name="lockToken">The lock token <see cref="ServiceBusReceivedMessage"/> to deadletter.</param>
        /// <param name="deadLetterReason">The reason for deadlettering the message.</param>
        /// <param name="deadLetterErrorDescription">The error description for deadlettering the message.</param>
        /// <param name="propertiesToModify">The properties of the message to modify while deferring the message.</param>
        /// <param name="cancellationToken"></param>
        ///
        /// <remarks>
        /// A lock token can be found in <see cref="ServiceBusReceivedMessage.LockToken"/>,
        /// only when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// In order to receive a message from the deadletter queue, you will need a new <see cref="ServiceBusReceiver"/>, with the corresponding path.
        /// You can use EntityNameHelper.FormatDeadLetterPath(string) to help with this.
        /// This operation can only be performed on messages that were received by this receiver.
        /// </remarks>
        private async Task DeadLetterInternalAsync(
            string lockToken,
            string deadLetterReason = default,
            string deadLetterErrorDescription = default,
            IDictionary<string, object> propertiesToModify = default,
            CancellationToken cancellationToken = default)
        {
            ThrowIfLockTokenIsEmpty(lockToken);
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusReceiver));
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            ThrowIfNotPeekLockMode();
            Logger.DeadLetterMessageStart(Identifier, 1, lockToken);
            using DiagnosticScope scope = _scopeFactory.CreateScope(
                DiagnosticProperty.DeadLetterActivityName,
                lockToken: lockToken);
            scope.Start();

            try
            {
                await InnerReceiver.DeadLetterAsync(
                    lockToken: lockToken,
                    deadLetterReason: deadLetterReason,
                    deadLetterErrorDescription: deadLetterErrorDescription,
                    propertiesToModify: propertiesToModify,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.DeadLetterMessageException(Identifier, exception.ToString());
                scope.Failed(exception);
                throw;
            }

            Logger.DeadLetterMessageComplete(Identifier);
        }

        /// <summary> Indicates that the receiver wants to defer the processing for the message.</summary>
        ///
        /// <param name="message">The <see cref="ServiceBusReceivedMessage"/> to defer.</param>
        /// <param name="propertiesToModify">The properties of the message to modify while deferring the message.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// A lock token can be found in <see cref="ServiceBusReceivedMessage.LockToken"/>,
        /// only when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// In order to receive this message again in the future, you will need to save the <see cref="ServiceBusReceivedMessage.SequenceNumber"/>
        /// and receive it using <see cref="ReceiveDeferredMessageAsync(long, CancellationToken)"/>.
        /// Deferring messages does not impact message's expiration, meaning that deferred messages can still expire.
        /// This operation can only be performed on messages that were received by this receiver.
        /// </remarks>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public virtual async Task DeferAsync(
            ServiceBusReceivedMessage message,
            IDictionary<string, object> propertiesToModify = null,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(message, nameof(message));
            await DeferAsync(
                message.LockToken,
                propertiesToModify,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary> Indicates that the receiver wants to defer the processing for the message.</summary>
        ///
        /// <param name="lockToken">The lockToken of the <see cref="ServiceBusReceivedMessage"/> to defer.</param>
        /// <param name="propertiesToModify">The properties of the message to modify while deferring the message.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <remarks>
        /// A lock token can be found in <see cref="ServiceBusReceivedMessage.LockToken"/>,
        /// only when <see cref="ReceiveMode"/> is set to <see cref="ReceiveMode.PeekLock"/>.
        /// In order to receive this message again in the future, you will need to save the <see cref="ServiceBusReceivedMessage.SequenceNumber"/>
        /// and receive it using <see cref="ReceiveDeferredMessageAsync(long, CancellationToken)"/>.
        /// Deferring messages does not impact message's expiration, meaning that deferred messages can still expire.
        /// This operation can only be performed on messages that were received by this receiver.
        /// </remarks>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        public virtual async Task DeferAsync(
            string lockToken,
            IDictionary<string, object> propertiesToModify = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfLockTokenIsEmpty(lockToken);
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusReceiver));
            ThrowIfNotPeekLockMode();
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.DeferMessageStart(Identifier, 1, lockToken);
            using DiagnosticScope scope = _scopeFactory.CreateScope(
                DiagnosticProperty.DeferActivityName,
                lockToken: lockToken);
            scope.Start();

            try
            {
                await InnerReceiver.DeferAsync(
                    lockToken,
                    propertiesToModify,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.DeferMessageException(Identifier, exception.ToString());
                scope.Failed(exception);
                throw;
            }

            Logger.DeferMessageComplete(Identifier);
        }

        /// <summary>
        /// Throws an InvalidOperationException when not in PeekLock mode.
        /// </summary>
        private void ThrowIfNotPeekLockMode()
        {
            if (ReceiveMode != ReceiveMode.PeekLock)
            {
                throw new InvalidOperationException(Resources.OperationNotSupported);
            }
        }

        /// <summary>
        /// Throws an InvalidOperationException when the lock token is empty.
        /// </summary>
        private static void ThrowIfLockTokenIsEmpty(string lockToken)
        {
            if (Guid.Parse(lockToken) == Guid.Empty)
            {
                throw new InvalidOperationException(Resources.SettlementOperationNotSupported);
            }
        }

        /// <summary>
        /// Receives a specific deferred message identified by <paramref name="sequenceNumber"/>.
        /// </summary>
        ///
        /// <param name="sequenceNumber">The sequence number of the message that will be received.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns>Message identified by sequence number <paramref name="sequenceNumber"/>. Returns null if no such message is found.
        /// Throws if the message has not been deferred.</returns>
        /// <seealso cref="DeferAsync(ServiceBusReceivedMessage, IDictionary{string, object}, CancellationToken)"/>
        /// <seealso cref="DeferAsync(string, IDictionary{string, object}, CancellationToken)"/>
        public virtual async Task<ServiceBusReceivedMessage> ReceiveDeferredMessageAsync(
            long sequenceNumber,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<ServiceBusReceivedMessage> result = await ReceiveDeferredMessageBatchAsync(sequenceNumbers: new long[] { sequenceNumber }).ConfigureAwait(false);
            foreach (ServiceBusReceivedMessage message in result)
            {
                return message;
            }
            return null;
        }

        /// <summary>
        /// Receives a <see cref="IList{Message}"/> of deferred messages identified by <paramref name="sequenceNumbers"/>.
        /// </summary>
        ///
        /// <param name="sequenceNumbers">An <see cref="IEnumerable{T}"/> containing the sequence numbers to receive.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        ///
        /// <returns>Messages identified by sequence number are returned. Returns null if no messages are found.
        /// Throws if the messages have not been deferred.</returns>
        /// <seealso cref="DeferAsync(ServiceBusReceivedMessage, IDictionary{string, object}, CancellationToken)"/>
        /// <seealso cref="DeferAsync(string, IDictionary{string, object}, CancellationToken)"/>
        public virtual async Task<IList<ServiceBusReceivedMessage>> ReceiveDeferredMessageBatchAsync(
            IEnumerable<long> sequenceNumbers,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusReceiver));
            Argument.AssertNotNullOrEmpty(sequenceNumbers, nameof(sequenceNumbers));
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            var sequenceNumbersList = sequenceNumbers.ToList();

            Logger.ReceiveDeferredMessageStart(Identifier, sequenceNumbersList.Count, sequenceNumbersList);
            using DiagnosticScope scope = _scopeFactory.CreateScope(DiagnosticProperty.ReceiveDeferredActivityName);
            scope.AddAttribute(
                DiagnosticProperty.SequenceNumbersAttribute,
                string.Join(",", sequenceNumbers));
            scope.Start();

            IList<ServiceBusReceivedMessage> deferredMessages = null;
            try
            {
                deferredMessages = await InnerReceiver.ReceiveDeferredMessageBatchAsync(
                    sequenceNumbersList,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.ReceiveDeferredMessageException(Identifier, exception.ToString());
                scope.Failed(exception);
                throw;
            }

            Logger.ReceiveDeferredMessageComplete(Identifier, deferredMessages.Count);
            scope.SetMessageData(deferredMessages);
            return deferredMessages;
        }

        /// <summary>
        /// Renews the lock on the message. The lock will be renewed based on the setting specified on the queue.
        /// </summary>
        ///
        /// <remarks>
        /// When a message is received in <see cref="ReceiveMode.PeekLock"/> mode, the message is locked on the server for this
        /// receiver instance for a duration as specified during the Queue/Subscription creation (LockDuration).
        /// If processing of the message requires longer than this duration, the lock needs to be renewed.
        /// For each renewal, it resets the time the message is locked by the LockDuration set on the Entity.
        /// </remarks>
        ///
        /// <param name="message">The <see cref="ServiceBusReceivedMessage"/> to renew the lock for.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        public virtual async Task RenewMessageLockAsync(
            ServiceBusReceivedMessage message,
            CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(message, nameof(message));
            DateTimeOffset lockedUntil = await RenewMessageLockAsync(
                message.LockToken,
                cancellationToken).ConfigureAwait(false);
            message.LockedUntil = lockedUntil;
        }

        /// <summary>
        /// Renews the lock on the message. The lock will be renewed based on the setting specified on the queue.
        /// </summary>
        ///
        /// <remarks>
        /// When a message is received in <see cref="ReceiveMode.PeekLock"/> mode, the message is locked on the server for this
        /// receiver instance for a duration as specified during the Queue/Subscription creation (LockDuration).
        /// If processing of the message requires longer than this duration, the lock needs to be renewed.
        /// For each renewal, it resets the time the message is locked by the LockDuration set on the Entity.
        /// </remarks>
        ///
        /// <param name="lockToken">The lockToken of the <see cref="ServiceBusReceivedMessage"/> to renew the lock for.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> instance to signal the request to cancel the operation.</param>
        public virtual async Task<DateTimeOffset> RenewMessageLockAsync(
            string lockToken,
            CancellationToken cancellationToken = default)
        {
            ThrowIfLockTokenIsEmpty(lockToken);
            Argument.AssertNotClosed(IsDisposed, nameof(ServiceBusReceiver));
            ThrowIfNotPeekLockMode();
            ThrowIfSessionReceiver();
            cancellationToken.ThrowIfCancellationRequested<TaskCanceledException>();
            Logger.RenewMessageLockStart(Identifier, 1, lockToken);
            using DiagnosticScope scope = _scopeFactory.CreateScope(
                DiagnosticProperty.RenewMessageLockActivityName,
                lockToken: lockToken);
            scope.Start();

            DateTimeOffset lockedUntil;
            try
            {
                lockedUntil = await InnerReceiver.RenewMessageLockAsync(
                    lockToken,
                    cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.RenewMessageLockException(Identifier, exception.ToString());
                scope.Failed(exception);
                throw;
            }

            Logger.RenewMessageLockComplete(Identifier);
            scope.AddAttribute(DiagnosticProperty.LockedUntilAttribute, lockedUntil);
            return lockedUntil;
        }

        /// <summary>
        /// Throws an exception if the receiver instance is a session receiver.
        /// </summary>
        private void ThrowIfSessionReceiver()
        {
            if (IsSessionReceiver)
            {
                throw new InvalidOperationException(Resources.CannotLockMessageOnSessionEntity);
            }
        }

        /// <summary>
        /// Performs the task needed to clean up resources used by the <see cref="ServiceBusReceiver" />.
        /// </summary>
        ///
        /// <returns>A task to be resolved on when the operation has completed.</returns>
        [SuppressMessage("Usage", "AZC0002:Ensure all service methods take an optional CancellationToken parameter.", Justification = "This signature must match the IAsyncDisposable interface.")]
        public virtual async ValueTask DisposeAsync()
        {
            IsDisposed = true;
            Type clientType = GetType();

            Logger.ClientDisposeStart(clientType, Identifier);
            try
            {
                await InnerReceiver.CloseAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.ClientDisposeException(clientType, Identifier, ex);
                throw;
            }

            Logger.ClientDisposeComplete(clientType, Identifier);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        ///
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        ///
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        ///
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Converts the instance to string representation.
        /// </summary>
        ///
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }
}
