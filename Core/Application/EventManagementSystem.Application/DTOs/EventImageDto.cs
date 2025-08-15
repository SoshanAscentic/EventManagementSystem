// <copyright file="EventImageDto.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.DTOs
{
    public class EventImageDto
    {
        public int Id { get; set; }

        public int EventId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public bool IsPrimary { get; set; }

        public DateTime UploadedAt { get; set; }
    }
}
