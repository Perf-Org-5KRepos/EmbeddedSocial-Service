// This file isn't generated, but this comment is necessary to exclude it from StyleCop analysis.
// <auto-generated/>

// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace SocialPlus.Server.WebRoleCommon.DependencyResolution
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;

    using Microsoft.WindowsAzure.ServiceRuntime;
    using SocialPlus.Logging;
    using SocialPlus.OAuth;
    using SocialPlus.Server.Entities;
    using SocialPlus.Server.KVLibrary;
    using SocialPlus.Server.Blobs;
    using SocialPlus.Server.Managers;
    using SocialPlus.Server.Metrics;
    using SocialPlus.Server.Principal;
    using SocialPlus.Server.Queues;
    using SocialPlus.Server.Tables;
    using SocialPlus.Server.WebRoleCommon.Versioning;
    using SocialPlus.Utils;
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;

    /// <summary>
    /// Default registry class
    /// </summary>
    public abstract class BaseRegistry : Registry
    {
        /// <summary>
        /// Initializes the IoC registry with default values
        /// </summary>
        /// <param name="serviceVersionInfo">info of version of service</param>
        protected BaseRegistry(IServiceVersionInfo serviceVersionInfo)
        {
            // Short-lived container needed to read clientID, certThumbprint, and vaultURL via ISettingsReader
            // This container injects its own settingsReader class.

            // OLD code for creating a "short-lived" container
            // Container shortLivedContainer = new Container(x => x.For<ISettingsReader>().Use<SettingsReader>());
            // SettingsReader settingsReader = (SettingsReader)shortLivedContainer.GetInstance<ISettingsReader>();
            // shortLivedContainer.Dispose();
            bool enableAzureSettingsReaderTracing = false;
            ISettingsReader settingsReader = new AzureSettingsReader(enableAzureSettingsReaderTracing);

            string clientID = settingsReader.ReadValue("AADEmbeddedSocialClientId");
            string vaultUrl = settingsReader.ReadValue("KeyVaultUri");
            string certThumbPrint = settingsReader.ReadValue("SocialPlusCertThumbprint");
            StoreLocation storeLocation = StoreLocation.LocalMachine;
            string signingKey = settingsReader.ReadValue("SigningKey");
            int serviceBusBatchIntervalMs = int.Parse(settingsReader.ReadValue("ServiceBusBatchIntervalMs"));
            enableAzureSettingsReaderTracing = bool.Parse(settingsReader.ReadValue("EnableAzureSettingsReaderTracing"));

            this.Scan(
                scan =>
                {
                    scan.AssembliesFromPath(Environment.CurrentDirectory);
                    scan.LookForRegistries();
                    scan.WithDefaultConventions();

                    // scan.TheCallingAssembly();
                    // scan.With(new ControllerConvention());
                });

            // initialize the ILog to use debug output in an emulated role, and event source output in Azure
            Log log;
            if (RoleEnvironment.IsEmulated)
            {
                log = new Log(LogDestination.Debug, Log.DefaultCategoryName);
            }
            else
            {
                log = new Log(LogDestination.EventSource, Log.DefaultCategoryName);
            }

            this.For<ILog>().Singleton().Use(log);

            // This SettingsReader does not use KV; it's used to read regular (not protected) cloud configuration settings
            this.For<ISettingsReader>().Singleton().Use<AzureSettingsReader>().Ctor<bool>("isTracingEnabled").Is(enableAzureSettingsReaderTracing);

            // Initialize KV, asynchronous SettingsReader, ConnectionStringProvider, CTStore, CBStore, QueueManagers
            this.For<ICertificateHelper>().Singleton().Use<CertificateHelper>().Ctor<string>("certThumbprint").Is(certThumbPrint)
                .Ctor<string>("clientID").Is(clientID)
                .Ctor<StoreLocation>("storeLocation").Is(storeLocation);
            this.For<IKeyVaultClient>().Singleton().Use<AzureKeyVaultClient>();
            this.For<IKV>().Singleton().Use<KV>().Ctor<string>("clientID").Is(clientID)
                .Ctor<string>("vaultUrl").Is(vaultUrl)
                .Ctor<string>("certThumbprint").Is(certThumbPrint)
                .Ctor<StoreLocation>("storeLocation").Is(storeLocation);
            this.For<ISettingsReaderAsync>().Singleton().Use<KVSettingsReader>();
            this.For<IConnectionStringProvider>().Singleton().Use<ConnectionStringProvider>();
            this.For<ICTStoreManager>().Singleton().Use<CTStoreManager>();
            this.For<ICBStoreManager>().Singleton().Use<CBStoreManager>();
            this.For<IQueueManager>().Singleton().Use<QueueManager>().Ctor<int>("batchIntervalMs").Is(serviceBusBatchIntervalMs);

            // Initialize the rest of the managers
            this.For<ISessionTokenManager>().Use<SessionTokenManager>();
            this.For<IAppsManager>().Use<AppsManager>();
            this.For<IIdentitiesManager>().Singleton().Use<IdentitiesManager>();
            this.For<IUsersManager>().Use<UsersManager>();
            this.For<ITopicsManager>().Use<TopicsManager>();
            this.For<IViewsManager>().Use<ViewsManager>();
            this.For<IRelationshipsManager>().Use<RelationshipsManager>();
            this.For<ILikesManager>().Use<LikesManager>();
            this.For<IPinsManager>().Use<PinsManager>();
            this.For<ICommentsManager>().Use<CommentsManager>();
            this.For<IRepliesManager>().Use<RepliesManager>();
            this.For<IActivitiesManager>().Use<ActivitiesManager>();
            this.For<INotificationsManager>().Use<NotificationsManager>();
            this.For<ISearchManager>().Singleton().Use<SearchManager>();
            this.For<IBlobsManager>().Use<BlobsManager>();
            this.For<IPushNotificationsManager>().Use<PushNotificationsManager>();
            this.For<IPopularTopicsManager>().Use<PopularTopicsManager>();
            this.For<IPopularUsersManager>().Use<PopularUsersManager>();
            this.For<ITopicNamesManager>().Use<TopicNamesManager>();
            this.For<IReportsManager>().Use<ReportsManager>();

            //Initialize the metric loggers
            this.For<IPerformanceMetrics>().Singleton().Use<LogPerformanceMetrics>();
            this.For<IApplicationMetrics>().Singleton().Use<LogApplicationMetrics>();
            this.For<IMetricsConfig>().Singleton().Use<LogMetricsConfig>();

            // Build a composite auth manager
            var aadAuthManager = this.For<IAuthManager>().Use<AADAuthManager>();
            var spAuthManager = this.For<IAuthManager>().Use<SocialPlusAuthManager>();
            var anonAuthManager = this.For<IAuthManager>().Use<AnonAuthManager>();
            var msaAuthManager = this.For<IAuthManager>().Use<OAuthManager>().Ctor<IdentityProviders>("identityProvider").Is(IdentityProviders.Microsoft);
            var fbAuthManager = this.For<IAuthManager>().Use<OAuthManager>().Ctor<IdentityProviders>("identityProvider").Is(IdentityProviders.Facebook);
            var gAuthManager = this.For<IAuthManager>().Use<OAuthManager>().Ctor<IdentityProviders>("identityProvider").Is(IdentityProviders.Google);
            var tAuthManager = this.For<IAuthManager>().Use<OAuthManager>().Ctor<IdentityProviders>("identityProvider").Is(IdentityProviders.Twitter);
            this.For<ICompositeAuthManager>().Singleton().Use<CompositeAuthManager>()
                    .Ctor<IAuthManager>("aadAuthManager").Is(aadAuthManager)
                    .Ctor<IAuthManager>("spAuthManager").Is(spAuthManager)
                    .Ctor<IAuthManager>("anonAuthManager").Is(anonAuthManager)
                    .Ctor<IAuthManager>("msaAuthManager").Is(msaAuthManager)
                    .Ctor<IAuthManager>("fbAuthManager").Is(fbAuthManager)
                    .Ctor<IAuthManager>("gAuthManager").Is(gAuthManager)
                    .Ctor<IAuthManager>("tAuthManager").Is(tAuthManager);

            this.For<IHandleGenerator>().Use<HandleGenerator>();

            this.For<ITopicsStore>().Use<TopicsStore>();
            this.For<IUsersStore>().Use<UsersStore>();
            this.For<IAppsStore>().Use<AppsStore>();
            this.For<ILikesStore>().Use<LikesStore>();
            this.For<IPinsStore>().Use<PinsStore>();
            this.For<IUserRelationshipsStore>().Use<UserRelationshipsStore>();
            this.For<ITopicRelationshipsStore>().Use<TopicRelationshipsStore>();
            this.For<ICommentsStore>().Use<CommentsStore>();
            this.For<IRepliesStore>().Use<RepliesStore>();
            this.For<INotificationsStore>().Use<NotificationsStore>();
            this.For<IActivitiesStore>().Use<ActivitiesStore>();
            this.For<IBlobsMetadataStore>().Use<BlobsMetadataStore>();
            this.For<IPushRegistrationsStore>().Use<PushRegistrationsStore>();
            this.For<IBlobsStore>().Use<BlobsStore>();
            this.For<ITopicNamesStore>().Use<TopicNamesStore>();
            this.For<IContentReportsStore>().Use<ContentReportsStore>();
            this.For<IUserReportsStore>().Use<UserReportsStore>();
            this.For<IAVERTStore>().Use<AVERTStore>();

            this.For<IFanoutTopicsQueue>().Use<FanoutTopicsQueue>();
            this.For<IFanoutActivitiesQueue>().Use<FanoutActivitiesQueue>();
            this.For<IFollowingImportsQueue>().Use<FollowingImportsQueue>();
            this.For<IResizeImagesQueue>().Use<ResizeImagesQueue>();
            this.For<ILikesQueue>().Use<LikesQueue>();
            this.For<IRelationshipsQueue>().Use<RelationshipsQueue>();
            this.For<IReportsQueue>().Use<ReportsQueue>();
            this.For<ISearchQueue>().Use<SearchQueue>();

            // Initialize the build service info and the service version info. For service version, use the existing one, using the fancy use lambda notation
            this.For<IBuildVersionInfo>().Use<BuildVersionInfo>();
            this.For<IServiceVersionInfo>().Singleton().Use(ctx => serviceVersionInfo);
        }
    }
}