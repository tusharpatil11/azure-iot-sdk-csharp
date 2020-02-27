﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Azure.Devices
{
    /// <summary>
    /// Holds the API version numbers required in data-plane calls to the service
    /// </summary>
    internal class ClientApiVersionHelper
    {
        private const string ApiVersionQueryPrefix = "api-version=";
        private const string ApiVersionGA = "2016-02-03";
        private const string ApiVersionLatest = "2020-03-13";

        /// <summary>
        /// The default API version to use for all data-plane service calls
        /// </summary>
        public const string ApiVersionQueryString = ApiVersionQueryPrefix + ApiVersionLatest;

        public const string ApiVersionQueryStringGA = ApiVersionQueryPrefix + ApiVersionGA;
    }
}
