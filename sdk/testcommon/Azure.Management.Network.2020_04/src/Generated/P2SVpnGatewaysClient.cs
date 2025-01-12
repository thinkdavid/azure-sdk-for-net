// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Management.Network.Models;

namespace Azure.Management.Network
{
    /// <summary> The P2SVpnGateways service client. </summary>
    public partial class P2SVpnGatewaysClient
    {
        private readonly ClientDiagnostics _clientDiagnostics;
        private readonly HttpPipeline _pipeline;
        internal P2SVpnGatewaysRestClient RestClient { get; }
        /// <summary> Initializes a new instance of P2SVpnGatewaysClient for mocking. </summary>
        protected P2SVpnGatewaysClient()
        {
        }
        /// <summary> Initializes a new instance of P2SVpnGatewaysClient. </summary>
        /// <param name="clientDiagnostics"> The handler for diagnostic messaging in the client. </param>
        /// <param name="pipeline"> The HTTP pipeline for sending and receiving REST requests and responses. </param>
        /// <param name="subscriptionId"> The subscription credentials which uniquely identify the Microsoft Azure subscription. The subscription ID forms part of the URI for every service call. </param>
        /// <param name="endpoint"> server parameter. </param>
        internal P2SVpnGatewaysClient(ClientDiagnostics clientDiagnostics, HttpPipeline pipeline, string subscriptionId, Uri endpoint = null)
        {
            RestClient = new P2SVpnGatewaysRestClient(clientDiagnostics, pipeline, subscriptionId, endpoint);
            _clientDiagnostics = clientDiagnostics;
            _pipeline = pipeline;
        }

        /// <summary> Retrieves the details of a virtual wan p2s vpn gateway. </summary>
        /// <param name="resourceGroupName"> The resource group name of the P2SVpnGateway. </param>
        /// <param name="gatewayName"> The name of the gateway. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual async Task<Response<P2SVpnGateway>> GetAsync(string resourceGroupName, string gatewayName, CancellationToken cancellationToken = default)
        {
            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.Get");
            scope.Start();
            try
            {
                return await RestClient.GetAsync(resourceGroupName, gatewayName, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Retrieves the details of a virtual wan p2s vpn gateway. </summary>
        /// <param name="resourceGroupName"> The resource group name of the P2SVpnGateway. </param>
        /// <param name="gatewayName"> The name of the gateway. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual Response<P2SVpnGateway> Get(string resourceGroupName, string gatewayName, CancellationToken cancellationToken = default)
        {
            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.Get");
            scope.Start();
            try
            {
                return RestClient.Get(resourceGroupName, gatewayName, cancellationToken);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Updates virtual wan p2s vpn gateway tags. </summary>
        /// <param name="resourceGroupName"> The resource group name of the P2SVpnGateway. </param>
        /// <param name="gatewayName"> The name of the gateway. </param>
        /// <param name="p2SVpnGatewayParameters"> Parameters supplied to update a virtual wan p2s vpn gateway tags. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual async Task<Response<P2SVpnGateway>> UpdateTagsAsync(string resourceGroupName, string gatewayName, TagsObject p2SVpnGatewayParameters, CancellationToken cancellationToken = default)
        {
            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.UpdateTags");
            scope.Start();
            try
            {
                return await RestClient.UpdateTagsAsync(resourceGroupName, gatewayName, p2SVpnGatewayParameters, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Updates virtual wan p2s vpn gateway tags. </summary>
        /// <param name="resourceGroupName"> The resource group name of the P2SVpnGateway. </param>
        /// <param name="gatewayName"> The name of the gateway. </param>
        /// <param name="p2SVpnGatewayParameters"> Parameters supplied to update a virtual wan p2s vpn gateway tags. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual Response<P2SVpnGateway> UpdateTags(string resourceGroupName, string gatewayName, TagsObject p2SVpnGatewayParameters, CancellationToken cancellationToken = default)
        {
            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.UpdateTags");
            scope.Start();
            try
            {
                return RestClient.UpdateTags(resourceGroupName, gatewayName, p2SVpnGatewayParameters, cancellationToken);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Lists all the P2SVpnGateways in a resource group. </summary>
        /// <param name="resourceGroupName"> The resource group name of the P2SVpnGateway. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual AsyncPageable<P2SVpnGateway> ListByResourceGroupAsync(string resourceGroupName, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }

            async Task<Page<P2SVpnGateway>> FirstPageFunc(int? pageSizeHint)
            {
                using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.ListByResourceGroup");
                scope.Start();
                try
                {
                    var response = await RestClient.ListByResourceGroupAsync(resourceGroupName, cancellationToken).ConfigureAwait(false);
                    return Page.FromValues(response.Value.Value, response.Value.NextLink, response.GetRawResponse());
                }
                catch (Exception e)
                {
                    scope.Failed(e);
                    throw;
                }
            }
            async Task<Page<P2SVpnGateway>> NextPageFunc(string nextLink, int? pageSizeHint)
            {
                using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.ListByResourceGroup");
                scope.Start();
                try
                {
                    var response = await RestClient.ListByResourceGroupNextPageAsync(nextLink, resourceGroupName, cancellationToken).ConfigureAwait(false);
                    return Page.FromValues(response.Value.Value, response.Value.NextLink, response.GetRawResponse());
                }
                catch (Exception e)
                {
                    scope.Failed(e);
                    throw;
                }
            }
            return PageableHelpers.CreateAsyncEnumerable(FirstPageFunc, NextPageFunc);
        }

        /// <summary> Lists all the P2SVpnGateways in a resource group. </summary>
        /// <param name="resourceGroupName"> The resource group name of the P2SVpnGateway. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual Pageable<P2SVpnGateway> ListByResourceGroup(string resourceGroupName, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }

            Page<P2SVpnGateway> FirstPageFunc(int? pageSizeHint)
            {
                using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.ListByResourceGroup");
                scope.Start();
                try
                {
                    var response = RestClient.ListByResourceGroup(resourceGroupName, cancellationToken);
                    return Page.FromValues(response.Value.Value, response.Value.NextLink, response.GetRawResponse());
                }
                catch (Exception e)
                {
                    scope.Failed(e);
                    throw;
                }
            }
            Page<P2SVpnGateway> NextPageFunc(string nextLink, int? pageSizeHint)
            {
                using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.ListByResourceGroup");
                scope.Start();
                try
                {
                    var response = RestClient.ListByResourceGroupNextPage(nextLink, resourceGroupName, cancellationToken);
                    return Page.FromValues(response.Value.Value, response.Value.NextLink, response.GetRawResponse());
                }
                catch (Exception e)
                {
                    scope.Failed(e);
                    throw;
                }
            }
            return PageableHelpers.CreateEnumerable(FirstPageFunc, NextPageFunc);
        }

        /// <summary> Lists all the P2SVpnGateways in a subscription. </summary>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual AsyncPageable<P2SVpnGateway> ListAsync(CancellationToken cancellationToken = default)
        {
            async Task<Page<P2SVpnGateway>> FirstPageFunc(int? pageSizeHint)
            {
                using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.List");
                scope.Start();
                try
                {
                    var response = await RestClient.ListAsync(cancellationToken).ConfigureAwait(false);
                    return Page.FromValues(response.Value.Value, response.Value.NextLink, response.GetRawResponse());
                }
                catch (Exception e)
                {
                    scope.Failed(e);
                    throw;
                }
            }
            async Task<Page<P2SVpnGateway>> NextPageFunc(string nextLink, int? pageSizeHint)
            {
                using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.List");
                scope.Start();
                try
                {
                    var response = await RestClient.ListNextPageAsync(nextLink, cancellationToken).ConfigureAwait(false);
                    return Page.FromValues(response.Value.Value, response.Value.NextLink, response.GetRawResponse());
                }
                catch (Exception e)
                {
                    scope.Failed(e);
                    throw;
                }
            }
            return PageableHelpers.CreateAsyncEnumerable(FirstPageFunc, NextPageFunc);
        }

        /// <summary> Lists all the P2SVpnGateways in a subscription. </summary>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual Pageable<P2SVpnGateway> List(CancellationToken cancellationToken = default)
        {
            Page<P2SVpnGateway> FirstPageFunc(int? pageSizeHint)
            {
                using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.List");
                scope.Start();
                try
                {
                    var response = RestClient.List(cancellationToken);
                    return Page.FromValues(response.Value.Value, response.Value.NextLink, response.GetRawResponse());
                }
                catch (Exception e)
                {
                    scope.Failed(e);
                    throw;
                }
            }
            Page<P2SVpnGateway> NextPageFunc(string nextLink, int? pageSizeHint)
            {
                using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.List");
                scope.Start();
                try
                {
                    var response = RestClient.ListNextPage(nextLink, cancellationToken);
                    return Page.FromValues(response.Value.Value, response.Value.NextLink, response.GetRawResponse());
                }
                catch (Exception e)
                {
                    scope.Failed(e);
                    throw;
                }
            }
            return PageableHelpers.CreateEnumerable(FirstPageFunc, NextPageFunc);
        }

        /// <summary> Creates a virtual wan p2s vpn gateway if it doesn&apos;t exist else updates the existing gateway. </summary>
        /// <param name="resourceGroupName"> The resource group name of the P2SVpnGateway. </param>
        /// <param name="gatewayName"> The name of the gateway. </param>
        /// <param name="p2SVpnGatewayParameters"> Parameters supplied to create or Update a virtual wan p2s vpn gateway. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual async Task<P2SVpnGatewaysCreateOrUpdateOperation> StartCreateOrUpdateAsync(string resourceGroupName, string gatewayName, P2SVpnGateway p2SVpnGatewayParameters, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (gatewayName == null)
            {
                throw new ArgumentNullException(nameof(gatewayName));
            }
            if (p2SVpnGatewayParameters == null)
            {
                throw new ArgumentNullException(nameof(p2SVpnGatewayParameters));
            }

            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.StartCreateOrUpdate");
            scope.Start();
            try
            {
                var originalResponse = await RestClient.CreateOrUpdateAsync(resourceGroupName, gatewayName, p2SVpnGatewayParameters, cancellationToken).ConfigureAwait(false);
                return new P2SVpnGatewaysCreateOrUpdateOperation(_clientDiagnostics, _pipeline, RestClient.CreateCreateOrUpdateRequest(resourceGroupName, gatewayName, p2SVpnGatewayParameters).Request, originalResponse);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Creates a virtual wan p2s vpn gateway if it doesn&apos;t exist else updates the existing gateway. </summary>
        /// <param name="resourceGroupName"> The resource group name of the P2SVpnGateway. </param>
        /// <param name="gatewayName"> The name of the gateway. </param>
        /// <param name="p2SVpnGatewayParameters"> Parameters supplied to create or Update a virtual wan p2s vpn gateway. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual P2SVpnGatewaysCreateOrUpdateOperation StartCreateOrUpdate(string resourceGroupName, string gatewayName, P2SVpnGateway p2SVpnGatewayParameters, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (gatewayName == null)
            {
                throw new ArgumentNullException(nameof(gatewayName));
            }
            if (p2SVpnGatewayParameters == null)
            {
                throw new ArgumentNullException(nameof(p2SVpnGatewayParameters));
            }

            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.StartCreateOrUpdate");
            scope.Start();
            try
            {
                var originalResponse = RestClient.CreateOrUpdate(resourceGroupName, gatewayName, p2SVpnGatewayParameters, cancellationToken);
                return new P2SVpnGatewaysCreateOrUpdateOperation(_clientDiagnostics, _pipeline, RestClient.CreateCreateOrUpdateRequest(resourceGroupName, gatewayName, p2SVpnGatewayParameters).Request, originalResponse);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Deletes a virtual wan p2s vpn gateway. </summary>
        /// <param name="resourceGroupName"> The resource group name of the P2SVpnGateway. </param>
        /// <param name="gatewayName"> The name of the gateway. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual async Task<P2SVpnGatewaysDeleteOperation> StartDeleteAsync(string resourceGroupName, string gatewayName, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (gatewayName == null)
            {
                throw new ArgumentNullException(nameof(gatewayName));
            }

            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.StartDelete");
            scope.Start();
            try
            {
                var originalResponse = await RestClient.DeleteAsync(resourceGroupName, gatewayName, cancellationToken).ConfigureAwait(false);
                return new P2SVpnGatewaysDeleteOperation(_clientDiagnostics, _pipeline, RestClient.CreateDeleteRequest(resourceGroupName, gatewayName).Request, originalResponse);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Deletes a virtual wan p2s vpn gateway. </summary>
        /// <param name="resourceGroupName"> The resource group name of the P2SVpnGateway. </param>
        /// <param name="gatewayName"> The name of the gateway. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual P2SVpnGatewaysDeleteOperation StartDelete(string resourceGroupName, string gatewayName, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (gatewayName == null)
            {
                throw new ArgumentNullException(nameof(gatewayName));
            }

            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.StartDelete");
            scope.Start();
            try
            {
                var originalResponse = RestClient.Delete(resourceGroupName, gatewayName, cancellationToken);
                return new P2SVpnGatewaysDeleteOperation(_clientDiagnostics, _pipeline, RestClient.CreateDeleteRequest(resourceGroupName, gatewayName).Request, originalResponse);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Generates VPN profile for P2S client of the P2SVpnGateway in the specified resource group. </summary>
        /// <param name="resourceGroupName"> The name of the resource group. </param>
        /// <param name="gatewayName"> The name of the P2SVpnGateway. </param>
        /// <param name="parameters"> Parameters supplied to the generate P2SVpnGateway VPN client package operation. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual async Task<P2SVpnGatewaysGenerateVpnProfileOperation> StartGenerateVpnProfileAsync(string resourceGroupName, string gatewayName, P2SVpnProfileParameters parameters, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (gatewayName == null)
            {
                throw new ArgumentNullException(nameof(gatewayName));
            }
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.StartGenerateVpnProfile");
            scope.Start();
            try
            {
                var originalResponse = await RestClient.GenerateVpnProfileAsync(resourceGroupName, gatewayName, parameters, cancellationToken).ConfigureAwait(false);
                return new P2SVpnGatewaysGenerateVpnProfileOperation(_clientDiagnostics, _pipeline, RestClient.CreateGenerateVpnProfileRequest(resourceGroupName, gatewayName, parameters).Request, originalResponse);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Generates VPN profile for P2S client of the P2SVpnGateway in the specified resource group. </summary>
        /// <param name="resourceGroupName"> The name of the resource group. </param>
        /// <param name="gatewayName"> The name of the P2SVpnGateway. </param>
        /// <param name="parameters"> Parameters supplied to the generate P2SVpnGateway VPN client package operation. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual P2SVpnGatewaysGenerateVpnProfileOperation StartGenerateVpnProfile(string resourceGroupName, string gatewayName, P2SVpnProfileParameters parameters, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (gatewayName == null)
            {
                throw new ArgumentNullException(nameof(gatewayName));
            }
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.StartGenerateVpnProfile");
            scope.Start();
            try
            {
                var originalResponse = RestClient.GenerateVpnProfile(resourceGroupName, gatewayName, parameters, cancellationToken);
                return new P2SVpnGatewaysGenerateVpnProfileOperation(_clientDiagnostics, _pipeline, RestClient.CreateGenerateVpnProfileRequest(resourceGroupName, gatewayName, parameters).Request, originalResponse);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Gets the connection health of P2S clients of the virtual wan P2SVpnGateway in the specified resource group. </summary>
        /// <param name="resourceGroupName"> The name of the resource group. </param>
        /// <param name="gatewayName"> The name of the P2SVpnGateway. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual async Task<P2SVpnGatewaysGetP2SVpnConnectionHealthOperation> StartGetP2SVpnConnectionHealthAsync(string resourceGroupName, string gatewayName, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (gatewayName == null)
            {
                throw new ArgumentNullException(nameof(gatewayName));
            }

            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.StartGetP2SVpnConnectionHealth");
            scope.Start();
            try
            {
                var originalResponse = await RestClient.GetP2SVpnConnectionHealthAsync(resourceGroupName, gatewayName, cancellationToken).ConfigureAwait(false);
                return new P2SVpnGatewaysGetP2SVpnConnectionHealthOperation(_clientDiagnostics, _pipeline, RestClient.CreateGetP2SVpnConnectionHealthRequest(resourceGroupName, gatewayName).Request, originalResponse);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Gets the connection health of P2S clients of the virtual wan P2SVpnGateway in the specified resource group. </summary>
        /// <param name="resourceGroupName"> The name of the resource group. </param>
        /// <param name="gatewayName"> The name of the P2SVpnGateway. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual P2SVpnGatewaysGetP2SVpnConnectionHealthOperation StartGetP2SVpnConnectionHealth(string resourceGroupName, string gatewayName, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (gatewayName == null)
            {
                throw new ArgumentNullException(nameof(gatewayName));
            }

            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.StartGetP2SVpnConnectionHealth");
            scope.Start();
            try
            {
                var originalResponse = RestClient.GetP2SVpnConnectionHealth(resourceGroupName, gatewayName, cancellationToken);
                return new P2SVpnGatewaysGetP2SVpnConnectionHealthOperation(_clientDiagnostics, _pipeline, RestClient.CreateGetP2SVpnConnectionHealthRequest(resourceGroupName, gatewayName).Request, originalResponse);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Gets the sas url to get the connection health detail of P2S clients of the virtual wan P2SVpnGateway in the specified resource group. </summary>
        /// <param name="resourceGroupName"> The name of the resource group. </param>
        /// <param name="gatewayName"> The name of the P2SVpnGateway. </param>
        /// <param name="request"> Request parameters supplied to get p2s vpn connections detailed health. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual async Task<P2SVpnGatewaysGetP2SVpnConnectionHealthDetailedOperation> StartGetP2SVpnConnectionHealthDetailedAsync(string resourceGroupName, string gatewayName, P2SVpnConnectionHealthRequest request, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (gatewayName == null)
            {
                throw new ArgumentNullException(nameof(gatewayName));
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.StartGetP2SVpnConnectionHealthDetailed");
            scope.Start();
            try
            {
                var originalResponse = await RestClient.GetP2SVpnConnectionHealthDetailedAsync(resourceGroupName, gatewayName, request, cancellationToken).ConfigureAwait(false);
                return new P2SVpnGatewaysGetP2SVpnConnectionHealthDetailedOperation(_clientDiagnostics, _pipeline, RestClient.CreateGetP2SVpnConnectionHealthDetailedRequest(resourceGroupName, gatewayName, request).Request, originalResponse);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Gets the sas url to get the connection health detail of P2S clients of the virtual wan P2SVpnGateway in the specified resource group. </summary>
        /// <param name="resourceGroupName"> The name of the resource group. </param>
        /// <param name="gatewayName"> The name of the P2SVpnGateway. </param>
        /// <param name="request"> Request parameters supplied to get p2s vpn connections detailed health. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual P2SVpnGatewaysGetP2SVpnConnectionHealthDetailedOperation StartGetP2SVpnConnectionHealthDetailed(string resourceGroupName, string gatewayName, P2SVpnConnectionHealthRequest request, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (gatewayName == null)
            {
                throw new ArgumentNullException(nameof(gatewayName));
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.StartGetP2SVpnConnectionHealthDetailed");
            scope.Start();
            try
            {
                var originalResponse = RestClient.GetP2SVpnConnectionHealthDetailed(resourceGroupName, gatewayName, request, cancellationToken);
                return new P2SVpnGatewaysGetP2SVpnConnectionHealthDetailedOperation(_clientDiagnostics, _pipeline, RestClient.CreateGetP2SVpnConnectionHealthDetailedRequest(resourceGroupName, gatewayName, request).Request, originalResponse);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Disconnect P2S vpn connections of the virtual wan P2SVpnGateway in the specified resource group. </summary>
        /// <param name="resourceGroupName"> The name of the resource group. </param>
        /// <param name="p2SVpnGatewayName"> The name of the P2S Vpn Gateway. </param>
        /// <param name="request"> The parameters are supplied to disconnect p2s vpn connections. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual async Task<P2SVpnGatewaysDisconnectP2SVpnConnectionsOperation> StartDisconnectP2SVpnConnectionsAsync(string resourceGroupName, string p2SVpnGatewayName, P2SVpnConnectionRequest request, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (p2SVpnGatewayName == null)
            {
                throw new ArgumentNullException(nameof(p2SVpnGatewayName));
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.StartDisconnectP2SVpnConnections");
            scope.Start();
            try
            {
                var originalResponse = await RestClient.DisconnectP2SVpnConnectionsAsync(resourceGroupName, p2SVpnGatewayName, request, cancellationToken).ConfigureAwait(false);
                return new P2SVpnGatewaysDisconnectP2SVpnConnectionsOperation(_clientDiagnostics, _pipeline, RestClient.CreateDisconnectP2SVpnConnectionsRequest(resourceGroupName, p2SVpnGatewayName, request).Request, originalResponse);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        /// <summary> Disconnect P2S vpn connections of the virtual wan P2SVpnGateway in the specified resource group. </summary>
        /// <param name="resourceGroupName"> The name of the resource group. </param>
        /// <param name="p2SVpnGatewayName"> The name of the P2S Vpn Gateway. </param>
        /// <param name="request"> The parameters are supplied to disconnect p2s vpn connections. </param>
        /// <param name="cancellationToken"> The cancellation token to use. </param>
        public virtual P2SVpnGatewaysDisconnectP2SVpnConnectionsOperation StartDisconnectP2SVpnConnections(string resourceGroupName, string p2SVpnGatewayName, P2SVpnConnectionRequest request, CancellationToken cancellationToken = default)
        {
            if (resourceGroupName == null)
            {
                throw new ArgumentNullException(nameof(resourceGroupName));
            }
            if (p2SVpnGatewayName == null)
            {
                throw new ArgumentNullException(nameof(p2SVpnGatewayName));
            }
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using var scope = _clientDiagnostics.CreateScope("P2SVpnGatewaysClient.StartDisconnectP2SVpnConnections");
            scope.Start();
            try
            {
                var originalResponse = RestClient.DisconnectP2SVpnConnections(resourceGroupName, p2SVpnGatewayName, request, cancellationToken);
                return new P2SVpnGatewaysDisconnectP2SVpnConnectionsOperation(_clientDiagnostics, _pipeline, RestClient.CreateDisconnectP2SVpnConnectionsRequest(resourceGroupName, p2SVpnGatewayName, request).Request, originalResponse);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }
    }
}
