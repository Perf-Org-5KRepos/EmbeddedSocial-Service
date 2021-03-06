﻿// <copyright file="FanoutTopicActivityMessage.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Messages
{
    using System;

    using SocialPlus.Models;
    using SocialPlus.Server.Messaging;

    /// <summary>
    /// <c>Fanout</c> topic activity message
    /// </summary>
    public class FanoutTopicActivityMessage : QueueMessage, IFanoutTopicActivityMessage
    {
        /// <summary>
        /// Gets or sets topic handle
        /// </summary>
        public string TopicHandle { get; set; }

        /// <summary>
        /// Gets or sets app handle
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets activity handle
        /// </summary>
        public string ActivityHandle { get; set; }

        /// <summary>
        /// Gets or sets activity type
        /// </summary>
        public ActivityType ActivityType { get; set; }

        /// <summary>
        /// Gets or sets actor user handle
        /// </summary>
        public string ActorUserHandle { get; set; }

        /// <summary>
        /// Gets or sets acted on user handle
        /// </summary>
        public string ActedOnUserHandle { get; set; }

        /// <summary>
        /// Gets or sets acted on content type
        /// </summary>
        public ContentType ActedOnContentType { get; set; }

        /// <summary>
        /// Gets or sets acted on content handle
        /// </summary>
        public string ActedOnContentHandle { get; set; }

        /// <summary>
        /// Gets or sets created time
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
}
