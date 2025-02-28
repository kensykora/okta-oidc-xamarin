﻿// <copyright file="OktaContext.cs" company="Okta, Inc">
// Copyright (c) 2020 - present Okta, Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Okta.Xamarin.Services;
using Okta.Xamarin.TinyIoC;

namespace Okta.Xamarin
{
    /// <summary>
    /// A high level container providing access to Okta functionality.
    /// </summary>
    public class OktaContext
    {
        private static Lazy<OktaContext> current = new Lazy<OktaContext>(() => new OktaContext());
        private static Lazy<TinyIoCContainer> container = new Lazy<TinyIoCContainer>(() => new TinyIoCContainer());
        private Lazy<IOktaStateManager> stateManager = new Lazy<IOktaStateManager>(() => new OktaStateManager());
        private OAuthException oAuthException;

        /// <summary>
        /// Initializes a new instance of the <see cref="OktaContext"/> class.
        /// </summary>
        public OktaContext()
        {
        }

        /// <summary>
        /// The event that is raised when initialization of services is started.
        /// </summary>
        public event EventHandler<InitServicesEventArgs> InitServicesStarted;

        /// <summary>
        /// The event that is raised when initialization of services completes.
        /// </summary>
        public event EventHandler<InitServicesEventArgs> InitServicesCompleted;

        /// <summary>
        /// The event that is raised when an exception occurs during initialization of services.
        /// </summary>
        public event EventHandler<InitServicesEventArgs> InitServicesException;

        /// <summary>
        /// The event that is raised when LoadStateAsync is started.
        /// </summary>
        public event EventHandler<SecureStorageEventArgs> LoadStateStarted;

        /// <summary>
        /// The event that is raised when LoadStateAsync completes.
        /// </summary>
        public event EventHandler<SecureStorageEventArgs> LoadStateCompleted;

        /// <summary>
        /// The event that is raised when an exception occurs in LoadStateAsync.
        /// </summary>
        public event EventHandler<SecureStorageExceptionEventArgs> LoadStateException;

        /// <summary>
        /// The event that is raised before writing to secure storage.
        /// </summary>
        public event EventHandler<SecureStorageEventArgs> SecureStorageWriteStarted;

        /// <summary>
        /// The event that is raised when writing to secure storage completes.
        /// </summary>
        public event EventHandler<SecureStorageEventArgs> SecureStorageWriteCompleted;

        /// <summary>
        /// The event that is raised when an exception occurrs writing to secure storage.
        /// </summary>
        public event EventHandler<SecureStorageExceptionEventArgs> SecureStorageWriteException;

        /// <summary>
        /// The event that is raised before reading from secure storage.
        /// </summary>
        public event EventHandler<SecureStorageEventArgs> SecureStorageReadStarted;

        /// <summary>
        /// The event that is raised when reading from secure storage completes.
        /// </summary>
        public event EventHandler<SecureStorageEventArgs> SecureStorageReadCompleted;

        /// <summary>
        /// The event that is raised when an exception occurrs reading from secure storage.
        /// </summary>
        public event EventHandler<SecureStorageExceptionEventArgs> SecureStorageReadException;

        /// <summary>
        /// The event that is raised when a non success status code is received from the API.
        /// </summary>
        public event EventHandler<RequestExceptionEventArgs> RequestException;

        /// <summary>
        /// The event that is raised when sign in is started.
        /// </summary>
        public event EventHandler<SignInEventArgs> SignInStarted;

        /// <summary>
        /// The event that is raised when sign in completes.
        /// </summary>
        public event EventHandler<SignInEventArgs> SignInCompleted;

        /// <summary>
        /// The event that is raised when sign out is started.
        /// </summary>
        public event EventHandler<SignOutEventArgs> SignOutStarted;

        /// <summary>
        /// The event that is raised when sign out completes.
        /// </summary>
        public event EventHandler<SignOutEventArgs> SignOutCompleted;

        /// <summary>
        /// The event that is raised when authentication fails.
        /// </summary>
        public event EventHandler<AuthenticationFailedEventArgs> AuthenticationFailed;

        /// <summary>
        /// The event that is raised when revoking a token is started.
        /// </summary>
        public event EventHandler<RevokeEventArgs> RevokeStarted;

        /// <summary>
        /// The event that is raised when a token revocation completes.
        /// </summary>
        public event EventHandler<RevokeEventArgs> RevokeCompleted;

        /// <summary>
        /// The event that is raised when an exception occurs during revocation.
        /// </summary>
        public event EventHandler<RevokeExceptionEventArgs> RevokeException;

        /// <summary>
        /// The event that is raised when getting user.
        /// </summary>
        public event EventHandler<GetUserEventArgs> GetUserStarted;

        /// <summary>
        /// The event that is raised when getting user is complete.
        /// </summary>
        public event EventHandler<GetUserEventArgs> GetUserCompleted;

        /// <summary>
        /// The event that is raised when introspection starts.
        /// </summary>
        public event EventHandler<IntrospectEventArgs> IntrospectStarted;

        /// <summary>
        /// The event that is raised when introspection completes.
        /// </summary>
        public event EventHandler<IntrospectEventArgs> IntrospectCompleted;

        /// <summary>
        /// The event that is raised when token renewal (refresh) is started.
        /// </summary>
        public event EventHandler<RenewEventArgs> RenewStarted;

        /// <summary>
        /// The event that is raised when token renewal (refresh) completes.
        /// </summary>
        public event EventHandler<RenewEventArgs> RenewCompleted;

        /// <summary>
        /// The event that is raised when an exception occurs during renewal (refresh).
        /// </summary>
        public event EventHandler<RenewExceptionEventArgs> RenewException;

        /// <summary>
        /// Gets or sets the current global context.
        /// </summary>
        public static OktaContext Current
        {
            get => current.Value;
            set { current = new Lazy<OktaContext>(() => value); }
        }

        /// <summary>
        /// Gets the access token.
        /// </summary>
        public static string AccessToken
        {
            get => Current?.StateManager?.AccessToken;
        }

        /// <summary>
        /// Gets the ID token.
        /// </summary>
        public static string IdToken
        {
            get => Current?.StateManager?.IdToken;
        }

        /// <summary>
        /// Gets the refresh token.
        /// </summary>
        public static string RefreshToken
        {
            get => Current?.StateManager?.RefreshToken;
        }

        /// <summary>
        /// Gets token expiration.
        /// </summary>
        public static DateTimeOffset? Expires
        {
            get => Current?.StateManager?.Expires;
        }

        /// <summary>
        /// Gets the exception that occurred during sign-in, if any.  May be null.
        /// </summary>
        public OAuthException OAuthException
        {
            get
            {
                return this.oAuthException;
            }

            private set
            {
                this.oAuthException = value;
                OnAuthenticationFailed(value);
            }
        }

        /// <summary>
        /// Raise the `AuthenticationFailed` event.
        /// </summary>
        /// <param name="value">The OAuthException that occurred.</param>
        protected void OnAuthenticationFailed(OAuthException value)
        {
            this.AuthenticationFailed?.Invoke(this, new AuthenticationFailedEventArgs(value));
        }

        /// <summary>
        /// Gets or sets the secure key-value store.
        /// </summary>
        public SecureKeyValueStore SecureKeyValueStore { get; set; }

        /// <summary>
        /// Gets or sets the default client.
        /// </summary>
        public IOidcClient OidcClient { get; set; }

        /// <summary>
        /// Gets or sets the default config.
        /// </summary>
        public IOktaConfig OktaConfig { get; set; }

        /// <summary>
        /// Gets or sets the default state.
        /// </summary>
        public IOktaStateManager StateManager
        {
            get => this.stateManager.Value;
            set
            {
                this.stateManager = new Lazy<IOktaStateManager>(() => value);
                if (this.stateManager.Value != null)
                {
                    this.stateManager.Value.RequestException += this.OnRequestException;
                    this.stateManager.Value.SecureStorageReadStarted += this.OnSecureStorageReadStarted;
                    this.stateManager.Value.SecureStorageReadCompleted += this.OnSecureStorageReadCompleted;
                    this.stateManager.Value.SecureStorageReadException += this.OnSecureStorageReadException;
                    this.stateManager.Value.SecureStorageWriteStarted += this.OnSecureStorageWriteStarted;
                    this.stateManager.Value.SecureStorageWriteCompleted += this.OnSecureStorageWriteCompleted;
                    this.stateManager.Value.SecureStorageWriteException += this.OnSecureStorageWriteException;
                }
            }
        }

        /// <summary>
        /// Raise the `RequestException` event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        protected void OnRequestException(object sender, RequestExceptionEventArgs args)
        {
            this.RequestException?.Invoke(sender, args);
        }

        /// <summary>
        /// Raise the `SecureStorageReadStarted` event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        protected void OnSecureStorageReadStarted(object sender, SecureStorageEventArgs args)
        {
            this.SecureStorageReadStarted?.Invoke(sender, args);
        }

        /// <summary>
        /// Raise the `SecureStorageReadCompleted` event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        protected void OnSecureStorageReadCompleted(object sender, SecureStorageEventArgs args)
        {
            this.SecureStorageReadCompleted?.Invoke(sender, args);
        }

        /// <summary>
        /// Raise the `SecureStorageReadException` event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        protected void OnSecureStorageReadException(object sender, SecureStorageExceptionEventArgs args)
        {
            this.SecureStorageReadException?.Invoke(sender, args);
        }

        /// <summary>
        /// Raise the `SecureStorageWriteStarted` event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        protected void OnSecureStorageWriteStarted(object sender, SecureStorageEventArgs args)
        {
            this.SecureStorageWriteStarted?.Invoke(sender, args);
        }

        /// <summary>
        /// Raise the `SecureStorageWriteCompleted` event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        protected void OnSecureStorageWriteCompleted(object sender, SecureStorageEventArgs args)
        {
            this.SecureStorageWriteCompleted?.Invoke(sender, args);
        }

        /// <summary>
        /// Raise the `SecureStorageWriteException` event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The arguments.</param>
        protected void OnSecureStorageWriteException(object sender, SecureStorageExceptionEventArgs args)
        {
            this.SecureStorageWriteException?.Invoke(sender, args);
        }

        /// <summary>
        /// Gets or sets the IoC containter.
        /// </summary>
        public TinyIoCContainer IoCContainer
        {
            get => container.Value;
            set { container = new Lazy<TinyIoCContainer>(() => value); }
        }

        /// <summary>
        /// Write the current state to secure storage.
        /// </summary>
        public async Task SaveStateAsync()
        {
            await this.StateManager?.WriteToSecureStorageAsync();
        }

        /// <summary>
        /// Write the current state to secure storage.
        /// </summary>
        /// <param name="oktaContext">OktaContext whose state is saved.  Default is OktaContext.Current.</param>
        public static async Task SaveStateAsync(OktaContext oktaContext = null)
        {
            oktaContext = oktaContext ?? Current;
            await oktaContext.SaveStateAsync();
        }

        /// <summary>
        /// Load state from secure storage.
        /// </summary>
        /// <returns>A value indicating if state was loaded successfully.</returns>
        public async Task<bool> LoadStateAsync()
        {
            if (this.StateManager == null)
            {
                this.StateManager = new OktaStateManager();
            }

            try
            {
                this.OnLoadStateStarted();
                OktaStateManager stateManager = await this.StateManager.ReadFromSecureStorageAsync();
                if (stateManager != null && !string.IsNullOrEmpty(stateManager.AccessToken))
                {
                    this.StateManager = stateManager;
                }
                else
                {
                    stateManager = this.StateManager as OktaStateManager;
                }

                this.OnLoadStateCompleted(stateManager);
                return stateManager != null;
            }
            catch (Exception ex)
            {
                this.OnLoadStateException(ex);
                return false;
            }
        }

        /// <summary>
        /// Raise the `LoadStateException` event.
        /// </summary>
        /// <param name="ex">The exception</param>
        protected void OnLoadStateException(Exception ex)
        {
            this.LoadStateException?.Invoke(this, new SecureStorageExceptionEventArgs { Exception = ex });
        }

        /// <summary>
        /// Raise the `LoadStateCompleted` event.
        /// </summary>
        /// <param name="stateManager">The state manager.</param>
        protected void OnLoadStateCompleted(OktaStateManager stateManager)
        {
            this.LoadStateCompleted?.Invoke(this, new SecureStorageEventArgs { OktaStateManager = stateManager });
        }

        /// <summary>
        /// Raise the `LoadStateStarted` event.
        /// </summary>
        protected void OnLoadStateStarted()
        {
            this.LoadStateStarted?.Invoke(this, new SecureStorageEventArgs { OktaStateManager = this.StateManager });
        }

        /// <summary>
        /// Load state from secure storage.
        /// </summary>
        /// <param name="oktaContext">The context to load state for.</param>
        /// <returns>A value indicating if state was loaded successfully.</returns>
        public static async Task<bool> LoadStateAsync(OktaContext oktaContext = null)
        {
            oktaContext = oktaContext ?? Current;
            return await oktaContext.LoadStateAsync();
        }

        /// <summary>
        /// Clear state data from secure storage.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ClearStateAsync()
        {
            await this.StateManager?.ClearSecureStorageStateAsync();
        }

        /// <summary>
        /// Clear state data from secure storage.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task ClearStateAsync(OktaContext oktaContext = null)
        {
            oktaContext = oktaContext ?? Current;
            await oktaContext.ClearStateAsync();
        }

        /// <summary>
        /// Get an instance of the specified generic type T from the underlying IoC container.
        /// </summary>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <returns>{T}.</returns>
        public static T GetService<T>()
            where T : class
        {
            return Current.IoCContainer.Resolve<T>();
        }

        /// <summary>
        /// Register default implementations of Okta services into the specified container.
        /// </summary>
        /// <param name="container">The container.</param>
        public static void RegisterOktaDefaults(TinyIoCContainer container)
        {
            // additional future service registrations go here
            container.Register<SecureKeyValueStore, OktaSecureKeyValueStore>();
            Current.IoCContainer = container;
        }

        /// <summary>
        /// Creates or replaces a registration for the specified interface with the specified implementation.
        /// </summary>
        /// <typeparam name="TInterfaceType">Type to register</typeparam>
        /// <typeparam name="TImplementationType">Type to instantiate that implements RegisterType</typeparam>
        public static void RegisterServiceImplementation<TInterfaceType, TImplementationType>()
            where TInterfaceType : class
            where TImplementationType : class, TInterfaceType
        {
            Current.IoCContainer.Register<TInterfaceType, TImplementationType>();
        }

        /// <summary>
        /// Creates or replaces a container class registration with a specific, strong referenced, instance.
        /// </summary>
        /// <typeparam name="TInterfaceType">The type to register.</typeparam>
        /// <param name="instance">Instance of RegisterType to register.</param>
        public static void RegisterServiceImplementation<TInterfaceType>(object instance)
        {
            Current.IoCContainer.Register(typeof(TInterfaceType), instance);
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.LoadStateStarted event.
        /// </summary>
        /// <param name="loadStateStartedEventHandler">The event handler.</param>
        public static void AddLoadStateStartedListener(EventHandler<SecureStorageEventArgs> loadStateStartedEventHandler)
        {
            Current.LoadStateStarted += loadStateStartedEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.LoadStateComplted event.
        /// </summary>
        /// <param name="loadStateCompletedEventHandler">The event handler.</param>
        public static void AddLoadStateCompletedListener(EventHandler<SecureStorageEventArgs> loadStateCompletedEventHandler)
        {
            Current.LoadStateCompleted += loadStateCompletedEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.LoadStateException event.
        /// </summary>
        /// <param name="loadStateExceptionEventHandler">The event handler.</param>
        public static void AddLoadStateExceptionListener(EventHandler<SecureStorageExceptionEventArgs> loadStateExceptionEventHandler)
        {
            Current.LoadStateException += loadStateExceptionEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.SecureStorageReadStarted event.
        /// </summary>
        /// <param name="secureStorageReadEventHandler">The event handler.</param>
        public static void AddSecureStorageReadStartedListener(EventHandler<SecureStorageEventArgs> secureStorageReadEventHandler)
        {
            Current.SecureStorageReadStarted += secureStorageReadEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.SecureStorageReadCompleted event.
        /// </summary>
        /// <param name="secureStorageReadEventHandler">The event handler.</param>
        public static void AddSecureStorageReadCompletedListener(EventHandler<SecureStorageEventArgs> secureStorageReadEventHandler)
        {
            Current.SecureStorageReadCompleted += secureStorageReadEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.SecureStorageReadException event.
        /// </summary>
        /// <param name="secureStorageExceptionHandler">The event handler.</param>
        public static void AddSecureStorageReadExceptionListener(EventHandler<SecureStorageExceptionEventArgs> secureStorageExceptionHandler)
        {
            Current.SecureStorageReadException += secureStorageExceptionHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.SecureStorageWriteStarted event.
        /// </summary>
        /// <param name="secureStorageWriteEventHandler">The event handler.</param>
        public static void AddSecureStorageWriteStartedListener(EventHandler<SecureStorageEventArgs> secureStorageWriteEventHandler)
        {
            Current.SecureStorageWriteStarted += secureStorageWriteEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.SecureStorageWriteCompleted event.
        /// </summary>
        /// <param name="secureStorageWriteEventHandler">The event handler.</param>
        public static void AddSecureStorageWriteCompletedListener(EventHandler<SecureStorageEventArgs> secureStorageWriteEventHandler)
        {
            Current.SecureStorageWriteCompleted += secureStorageWriteEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.SecureStorageWriteException event.
        /// </summary>
        /// <param name="secureStorageExceptionHandler">The event handler.</param>
        public static void AddSecureStorageWriteExceptionListener(EventHandler<SecureStorageExceptionEventArgs> secureStorageExceptionHandler)
        {
            Current.SecureStorageWriteException += secureStorageExceptionHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.SignInStarted event.
        /// </summary>
        /// <param name="signInStartedEventHandler">The event handler.</param>
        public static void AddSignInStartedListener(EventHandler<SignInEventArgs> signInStartedEventHandler)
        {
            Current.SignInStarted += signInStartedEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.SignInCompleted event.
        /// </summary>
        /// <param name="signInCompletedEventHandler">The event handler.</param>
        public static void AddSignInCompletedListener(EventHandler<SignInEventArgs> signInCompletedEventHandler)
        {
            Current.SignInCompleted += signInCompletedEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.AuthenticationFailed event.
        /// </summary>
        /// <param name="authenticationFailedEventHandler">The event handler.</param>
        public static void AddAuthenticationFailedListener(EventHandler<AuthenticationFailedEventArgs> authenticationFailedEventHandler)
        {
            Current.AuthenticationFailed += authenticationFailedEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.SignOutStarted event.
        /// </summary>
        /// <param name="signOutStartedEventHandler">The event handler.</param>
        public static void AddSignOutStartedListener(EventHandler<SignOutEventArgs> signOutStartedEventHandler)
        {
            Current.SignOutStarted += signOutStartedEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.SignOutCompleted event.
        /// </summary>
        /// <param name="signOutCompletedEventHandler">The event handler.</param>
        public static void AddSignOutCompletedListener(EventHandler<SignOutEventArgs> signOutCompletedEventHandler)
        {
            Current.SignOutCompleted += signOutCompletedEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.RevokedToken event.
        /// </summary>
        /// <param name="revokeStartedEventHandler">The event handler.</param>
        public static void AddRevokeStartedListener(EventHandler<RevokeEventArgs> revokeStartedEventHandler)
        {
            Current.RevokeStarted += revokeStartedEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.RevokeCompleted event.
        /// </summary>
        /// <param name="revokeCompletedEventHandler">The event handler.</param>
        public static void AddRevokeCompletedListener(EventHandler<RevokeEventArgs> revokeCompletedEventHandler)
        {
            Current.RevokeCompleted += revokeCompletedEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.RevokeException event.
        /// </summary>
        /// <param name="revokeExceptionHandler">The event handler.</param>
        public static void AddRevokeExceptionListener(EventHandler<RevokeExceptionEventArgs> revokeExceptionHandler)
        {
            Current.RevokeException += revokeExceptionHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.GetUserStarted event.
        /// </summary>
        /// <param name="getUserEventHandler">The event handler.</param>
        public static void AddGetUserStartedListener(EventHandler<GetUserEventArgs> getUserEventHandler)
        {
            Current.GetUserStarted += getUserEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.GetUsercompleted event.
        /// </summary>
        /// <param name="getUserEventHandler">The event handler.</param>
        public static void AddGetUserCompletedListener(EventHandler<GetUserEventArgs> getUserEventHandler)
        {
            Current.GetUserCompleted += getUserEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.IntrospectStarted event.
        /// </summary>
        /// <param name="introspectEventHandler">The event handler.</param>
        public static void AddIntrospectStartedListener(EventHandler<IntrospectEventArgs> introspectEventHandler)
        {
            Current.IntrospectStarted += introspectEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContenxt.Current.InstrospectCompleted event.
        /// </summary>
        /// <param name="introspectEventHandler">The event handler.</param>
        public static void AddIntrospectCompletedListener(EventHandler<IntrospectEventArgs> introspectEventHandler)
        {
            Current.IntrospectCompleted += introspectEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.RenewStarted event.
        /// </summary>
        /// <param name="renewEventHandler">The event handler.</param>
        public static void AddRenewStartedListener(EventHandler<RenewEventArgs> renewEventHandler)
        {
            Current.RenewStarted += renewEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.RenewCompleted event.
        /// </summary>
        /// <param name="renewEventHandler">The event handler.</param>
        public static void AddRenewCompletedListener(EventHandler<RenewEventArgs> renewEventHandler)
        {
            Current.RenewCompleted += renewEventHandler;
        }

        /// <summary>
        /// Convenience method to add a listener to the OktaContext.Current.RenewException event.
        /// </summary>
        /// <param name="renewExceptionEventHandler">The event handler.</param>
        public static void AddRenewExceptionListener(EventHandler<RenewExceptionEventArgs> renewExceptionEventHandler)
        {
            Current.RenewException += renewExceptionEventHandler;
        }

        /// <summary>
        /// Initialize OktaContext.Current services with the implementations in the specified inversion of control container.
        /// </summary>
        /// <param name="iocContainer">The inversion of control container.</param>
        public static void ServiceInit(TinyIoCContainer iocContainer)
        {
            Current.InitServices(iocContainer);
        }

        /// <summary>
        /// Initialize services using the specified container.
        /// </summary>
        /// <param name="iocContainer">The inversion of control container.</param>
        public void InitServices(TinyIoCContainer iocContainer)
        {
            try
            {
                this.InitServicesStarted?.Invoke(this, new InitServicesEventArgs { TinyIoCContainer = iocContainer });
                this.IoCContainer = iocContainer;

                this.OidcClient = iocContainer.Resolve<IOidcClient>();
                this.SecureKeyValueStore = iocContainer.Resolve<SecureKeyValueStore>();

                // future services
                // to be added here

                this.InitServicesCompleted?.Invoke(this, new InitServicesEventArgs { TinyIoCContainer = iocContainer });
            }
            catch (Exception ex)
            {
                this.InitServicesException?.Invoke(this, new InitServicesEventArgs { Exception = ex });
            }
        }

        /// <summary>
        /// Initialize OktaContext.Current with the specified client.
        /// </summary>
        /// <param name="oidcClient">The client.</param>
        public static void Init(IOidcClient oidcClient)
        {
            Current.OidcClient = oidcClient;
        }

        /// <summary>
        /// Sign in using the specified client.
        /// </summary>
        /// <param name="oidcClient">The client.</param>
        /// <returns>OktaState.</returns>
        public virtual async Task<IOktaStateManager> SignInAsync(IOidcClient oidcClient = null)
        {
            this.OnSignInStarted();
            oidcClient = oidcClient ?? this.OidcClient;
            this.OktaConfig = oidcClient?.Config ?? GetService<IOktaConfig>();
            try
            {
                this.StateManager = await oidcClient.SignInWithBrowserAsync();
                if (oidcClient.OAuthException != null)
                {
                    this.OAuthException = oidcClient.OAuthException;
                }
            }
            catch (OAuthException ex)
            {
                this.OAuthException = ex;
                return this.StateManager;
            }

            this.OnSignInCompleted();
            return this.StateManager;
        }

        /// <summary>
        /// Raise the `SingInCompleted` event.
        /// </summary>
        protected void OnSignInCompleted()
        {
            this.SignInCompleted?.Invoke(this, new SignInEventArgs { StateManager = this.StateManager });
        }

        /// <summary>
        /// Raise the `SignInStarted` event.
        /// </summary>
        protected void OnSignInStarted()
        {
            this.SignInStarted?.Invoke(this, new SignInEventArgs { StateManager = this.StateManager });
        }

        /// <summary>
        /// Sign out using the specified client.
        /// </summary>
        /// <param name="oidcClient">The client.</param>
        /// <returns>Task.</returns>
        public virtual async Task SignOutAsync(IOidcClient oidcClient = null)
        {
            this.OnSignOutStarted();
            oidcClient = oidcClient ?? this.OidcClient;
            this.StateManager = await oidcClient?.SignOutOfOktaAsync(this.StateManager);
            this.OnSignOutCompleted();
        }

        /// <summary>
        /// Raise the `SignOutCompleted` event.
        /// </summary>
        protected void OnSignOutCompleted()
        {
            this.SignOutCompleted?.Invoke(this, new SignOutEventArgs { StateManager = this.StateManager });
        }

        /// <summary>
        /// Raise the `SignOutStarted` event.
        /// </summary>
        protected void OnSignOutStarted()
        {
            this.SignOutStarted?.Invoke(this, new SignOutEventArgs { StateManager = this.StateManager });
        }

        /// <summary>
        /// Revoke the specified access token.
        /// </summary>
        /// <param name="accessToken">The token.</param>
        /// <returns>Task.</returns>
        public static async Task RevokeAccessTokenAsync(string accessToken = null)
        {
            await Current.RevokeAsync(TokenKind.AccessToken, accessToken);
        }

        /// <summary>
        /// Revoke the specified refresh token.
        /// </summary>
        /// <param name="refreshToken">The token.</param>
        /// <returns>Task.</returns>
        public static async Task RevokeRefreshTokenAsync(string refreshToken = null)
        {
            await Current.RevokeAsync(TokenKind.RefreshToken, refreshToken);
        }

        /// <summary>
        /// Revoke the access token.
        /// </summary>
        /// <returns>Task.</returns>
        public virtual async Task RevokeAsync()
        {
            await this.RevokeAsync(TokenKind.AccessToken);
        }

        /// <summary>
        /// Revoke token of the specified kind.
        /// </summary>
        /// <param name="tokenKind">The kind of token to revoke.</param>
        /// <param name="token">The token.</param>
        /// <returns>Task.</returns>
        public virtual async Task RevokeAsync(TokenKind tokenKind, string token = null)
        {
            try
            {
                token = token ?? this.StateManager.GetToken(tokenKind);
                this.OnRevokeStarted(tokenKind, token);

                switch (tokenKind)
                {
                    case Xamarin.TokenKind.AccessToken:
                        await this.StateManager.RevokeAccessTokenAsync(token);
                        break;
                    case Xamarin.TokenKind.RefreshToken:
                    default:
                        await this.StateManager.RevokeRefreshTokenAsync(token);
                        break;
                }

                this.OnRevokeCompleted(tokenKind);
            }
            catch (Exception ex)
            {
                this.OnRevokeException(tokenKind, ex);
            }
        }

        /// <summary>
        /// Raise the `RevokeException` event.
        /// </summary>
        /// <param name="tokenKind">The kind of token.</param>
        /// <param name="ex">The exception.</param>
        protected void OnRevokeException(TokenKind tokenKind, Exception ex)
        {
            this.RevokeException?.Invoke(this, new RevokeExceptionEventArgs { StateManager = this.StateManager, TokenKind = tokenKind, Exception = ex });
        }

        /// <summary>
        /// Raise the `RevokeCompleted` event.
        /// </summary>
        /// <param name="tokenKind">The kind of token.</param>
        protected void OnRevokeCompleted(TokenKind tokenKind)
        {
            this.RevokeCompleted?.Invoke(this, new RevokeEventArgs { StateManager = this.StateManager, TokenKind = tokenKind, Response = this.StateManager.LastApiResponse });
        }

        /// <summary>
        /// Raise the `RevokeStarted` event.
        /// </summary>
        /// <param name="tokenKind">The kind of token.</param>
        /// <param name="token">The token.</param>
        protected void OnRevokeStarted(TokenKind tokenKind, string token)
        {
            this.RevokeStarted?.Invoke(this, new RevokeEventArgs { StateManager = this.StateManager, TokenKind = tokenKind, Token = token });
        }

        /// <summary>
        /// Gets information about the specified kind of token.
        /// </summary>
        /// <param name="tokenKind">The kind of token to introspect.</param>
        /// <returns>Dictionary{string, object}.</returns>
        public virtual async Task<Dictionary<string, object>> IntrospectAsync(TokenKind tokenKind)
        {
            string token = this.StateManager.GetToken(tokenKind);
            this.OnIntrospectStarted(tokenKind, token);
            Dictionary<string, object> result = await this.StateManager.IntrospectAsync(tokenKind);
            this.OnIntrospectCompleted(tokenKind, token, result);
            return result;
        }

        /// <summary>
        /// Raise the `IntrospectCompleted` event.
        /// </summary>
        /// <param name="tokenKind">The kind of token.</param>
        /// <param name="token">The token.</param>
        /// <param name="result">The result.</param>
        protected void OnIntrospectCompleted(TokenKind tokenKind, string token, Dictionary<string, object> result)
        {
            this.IntrospectCompleted?.Invoke(this, new IntrospectEventArgs { StateManager = this.StateManager, Token = token, TokenKind = tokenKind, Response = result });
        }

        /// <summary>
        /// Raise the `IntrospectStarted` event.
        /// </summary>
        /// <param name="tokenKind">The kind of token.</param>
        /// <param name="token">The token.</param>
        protected void OnIntrospectStarted(TokenKind tokenKind, string token)
        {
            this.IntrospectStarted?.Invoke(this, new IntrospectEventArgs { StateManager = this.StateManager, Token = token, TokenKind = tokenKind });
        }

        /// <summary>
        /// Renew tokens.
        /// </summary>
        /// <param name="refreshIdToken">A value indicating whether to renew the ID token, the default is false.</param>
        /// <returns>A <see cref="Task{RenewResponse}"/> representing the result of the asynchronous operation.</returns>
        public static async Task<RenewResponse> RenewAsync(bool refreshIdToken)
        {
            return await Current.RenewAsync(RefreshToken, refreshIdToken);
        }

        /// <summary>
        /// Renew tokens.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="refreshIdToken">A value indicating whether to renew the ID token, the default is false.</param>
        /// <returns>A <see cref="Task{RenewResponse}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<RenewResponse> RenewAsync(string refreshToken = null, bool refreshIdToken = false)
        {
            try
            {
                refreshToken = refreshToken ?? this.StateManager?.RefreshToken;
                if (string.IsNullOrEmpty(refreshToken))
                {
                    throw new ArgumentNullException(nameof(refreshToken));
                }

                string authorizationServerId = this.StateManager?.Config?.AuthorizationServerId;

                this.OnRenewStarted(refreshToken, refreshIdToken, authorizationServerId);
                RenewResponse result = await this.StateManager.RenewAsync(refreshToken, refreshIdToken);
                result.EnsureSuccess();
                this.OnRenewCompleted(refreshToken, refreshIdToken, authorizationServerId, result);

                return result;
            }
            catch (Exception ex)
            {
                this.OnRenewException(ex);
                return new RenewResponse(ex);
            }
        }

        /// <summary>
        /// Raise the `RenewException` event.
        /// </summary>
        /// <param name="ex">The exception.</param>
        protected void OnRenewException(Exception ex)
        {
            this.RenewException?.Invoke(this, new RenewExceptionEventArgs { StateManager = this.StateManager, Exception = ex });
        }

        /// <summary>
        /// Raise the `RenewCompleted` event.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="refreshIdToken">A value indicating whether to refresh the id token.</param>
        /// <param name="authorizationServerId">The authorization server id.</param>
        /// <param name="result">The result.</param>
        protected void OnRenewCompleted(string refreshToken, bool refreshIdToken, string authorizationServerId, RenewResponse result)
        {
            this.RenewCompleted?.Invoke(this, new RenewEventArgs { StateManager = this.StateManager, RefreshToken = refreshToken, Response = result, RefreshIdToken = refreshIdToken, AuthorizationServerId = authorizationServerId });
        }

        /// <summary>
        /// Raise the `RenewStarted` event.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="refreshIdToken">A value indicating whether to refresh the id token.</param>
        /// <param name="authorizationServerId">The authorization server id.</param>
        protected void OnRenewStarted(string refreshToken, bool refreshIdToken, string authorizationServerId)
        {
            this.RenewStarted?.Invoke(this, new RenewEventArgs { StateManager = this.StateManager, RefreshToken = refreshToken, RefreshIdToken = refreshIdToken, AuthorizationServerId = authorizationServerId });
        }

        /// <summary>
        /// Gets an instance of the generic type T representing the current user.
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task<T> GetUserAsync<T>()
        {
            this.OnGetUserStarted();
            T user = await this.StateManager.GetUserAsync<T>();
            this.OnGetUserCompleted(user);
            return user;
        }

        /// <summary>
        /// Raise the `GetUserStarted` event.
        /// </summary>
        protected void OnGetUserStarted()
        {
            this.GetUserStarted?.Invoke(this, new GetUserEventArgs { StateManager = this.StateManager });
        }

        /// <summary>
        /// Gets information about the current user.
        /// </summary>
        /// <returns>Task{Dictionary{string,object}}.</returns>
        public virtual async Task<Dictionary<string, object>> GetUserAsync()
        {
            this.OnGetUserStarted();
            Dictionary<string, object> userInfo = await this.StateManager.GetUserAsync();
            this.OnGetUserCompleted(userInfo);
            return userInfo;
        }

        /// <summary>
        /// Raise the `GetUserCompleted` event.
        /// </summary>
        /// <param name="userInfo">The user information.</param>
        protected void OnGetUserCompleted(object userInfo)
        {
            this.GetUserCompleted?.Invoke(this, new GetUserEventArgs { StateManager = this.StateManager, UserInfo = userInfo });
        }

        /// <summary>
        /// Gets a ClaimsPrincipal representing the current user.
        /// </summary>
        /// <returns>ClaimsPrincipal.</returns>
        public virtual async Task<System.Security.Claims.ClaimsPrincipal> GetClaimsPrincipalAsync()
        {
            this.OnGetUserStarted();
            ClaimsPrincipal claimsPrincipal = await this.StateManager.GetClaimsPrincipalAsync();
            this.OnGetUserCompleted(claimsPrincipal);
            return claimsPrincipal;
        }

        /// <summary>
        /// Clears the local authentication state by removing tokens from the state manager.
        /// </summary>
        public void Clear()
        {
            this.StateManager.Clear();
        }

        /// <summary>
        /// Gets the token of the specified kind from the state manager.
        /// </summary>
        /// <param name="tokenKind">The kind of token to get.</param>
        /// <returns>token as string.</returns>
        public string GetToken(TokenKind tokenKind)
        {
            return this.StateManager.GetToken(tokenKind);
        }
    }
}
