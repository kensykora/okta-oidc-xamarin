﻿// <copyright file="OidcClient.Test.cs" company="Okta, Inc">
// Copyright (c) 2019-present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Okta.Xamarin
{
    public class TestOidcClient  : OidcClient
    {
        /// <summary>
        /// A hook that is called when launching the browser
        /// </summary>
        public Action<string> OnLaunchBrowser { get; set; }
        /// <summary>
        /// A hook that is called when closing the browser
        /// </summary>
        public Action OnCloseBrowser { get; set; }

        /// <summary>
        /// Launches a browser to the specified url
        /// </summary>
        /// <param name="url">The url to launch in a Chrome custom tab</param>
        protected override void LaunchBrowser(string url)
        {
            OnLaunchBrowser?.Invoke(url);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OidcClient"/> class using the specified <see cref="OktaConfig"/>.
        /// </summary>
        /// <param name="config">The <see cref="OktaConfig"/> to use for this client.  The config must be valid at the time this is called.</param>
        public TestOidcClient(IOktaConfig config)
        {
            this.Config = config;
            validator.Validate(Config);
        }

        /// <summary>
        /// Called to close the browser used for login after the redirect
        /// </summary>
        protected override void CloseBrowser()
        {
            OnCloseBrowser?.Invoke();
        }

        /// <summary>
        /// Provides access to the internal OAuth <see cref="State"/> used to track requests from this client
        /// </summary>
        public string State_Internal
        {
            get
            {
                return this.State;
            }
        }

        public string CodeVerifier_Internal
        {
            get
            {
                return this.CodeVerifier;
            }
        }

        public string CodeChallenge_Internal
        {
            get
            {
                return this.CodeChallenge;
            }
        }

        public TaskCompletionSource<IOktaStateManager> CurrentTask_Accessor
        {
            get
            {
                return this.currentTask;
            }
        }

        /// <summary>
        /// Allows setting a custom <see cref="HttpMessageHandler"/> for use by this client's <see cref="HttpClient"/>, in order to mock of HTTP requests.
        /// </summary>
        /// <param name="handler">The message handler.</param>
        public void SetMockHttpMessageHandler(HttpMessageHandler handler)
        {
            if (handler == null)
                client = new HttpClient();
            else
                client = new HttpClient(handler);
        }

        /// <summary>
        /// Provides internal access to the function which determines the AuthorizeUrl including login query parameters based on the <see cref="Config"/>
        /// </summary>
        /// <returns>The url ready to be used for login.</returns>
        public string GenerateAuthorizeUrlTest()
        {
            return GenerateAuthorizeUrl();
        }

        /// <summary>
        /// Provided internal access to raise the RequestException event.
        /// </summary>
        /// <param name="requestExceptionEventArgs">The event arguments.</param>
        public void RaiseRequestExceptionEvent(RequestExceptionEventArgs requestExceptionEventArgs)
        {
            this.OnRequestException(requestExceptionEventArgs);
        }

        /// <summary>
        /// Provides internal access to the call the PerformAuthorizationServerRequestAsync method.
        /// </summary>
        /// <param name="httpMethod">The http method.</param>
        /// <param name="path">The path.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="formUrlEncodedContent">The content.</param>
        public void CallPerformAuthorizationServerRequestAsync(HttpMethod httpMethod, string path, Dictionary<string, string> headers, params KeyValuePair<string, string>[] formUrlEncodedContent)
        {
            base.PerformAuthorizationServerRequestAsync(httpMethod, path, headers, formUrlEncodedContent);
        }

        public async Task CallExchangeCodeForTokenAsync(string code)
        {
            await ExchangeAuthCodeForTokenAsync(code);
        }
    }
}
