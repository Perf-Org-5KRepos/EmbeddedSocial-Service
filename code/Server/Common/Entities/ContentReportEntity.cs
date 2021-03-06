﻿// <copyright file="ContentReportEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace SocialPlus.Server.Entities
{
    using System;

    using Microsoft.CTStore;
    using SocialPlus.Models;

    /// <summary>
    /// Content report entity interface
    /// </summary>
    public class ContentReportEntity : ObjectEntity, IContentReportEntity
    {
        /// <summary>
        /// Gets or sets the type of content being reported on
        /// </summary>
        public ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the content being reported on
        /// </summary>
        public string ContentHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user that created the content.
        /// This can be null if a user did not create the content.
        /// </summary>
        public string ContentUserHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the user that is doing the reporting
        /// </summary>
        public string ReportingUserHandle { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the app that the content came from
        /// </summary>
        public string AppHandle { get; set; }

        /// <summary>
        /// Gets or sets the complaint that the user has about this content
        /// </summary>
        public ReportReason Reason { get; set; }

        /// <summary>
        /// Gets or sets the time at which the report was received from the user
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
}
