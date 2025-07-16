// <copyright file="EventImage.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Entities
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.ValueObjects;

    public class EventImage : BaseEntity
    {
        // Private constructor for EF Core
        private EventImage()
        {
            this.EventId = null!;
            this.FileName = string.Empty;
            this.FilePath = string.Empty;
        }

        public EventId EventId { get; private set; }

        public string FileName { get; private set; }

        public string FilePath { get; private set; }

        public long FileSize { get; private set; }

        public bool IsPrimary { get; private set; }

        public string? Alt { get; private set; }

        // Navigation property
        public Event? Event { get; private set; }

        // Factory method for creating new images
        public static EventImage Create(EventId eventId, string fileName, string filePath, long fileSize, bool isPrimary = false, string? alt = null)
        {
            return new EventImage
            {
                EventId = eventId,
                FileName = ValidateFileName(fileName),
                FilePath = ValidateFilePath(filePath),
                FileSize = ValidateFileSize(fileSize),
                IsPrimary = isPrimary,
                Alt = alt?.Trim(),
            };
        }

        // Business Methods
        public void SetPrimary(bool isPrimary)
        {
            this.IsPrimary = isPrimary;
            this.MarkAsUpdated();
        }

        public void UpdateAlt(string? alt)
        {
            this.Alt = alt?.Trim();
            this.MarkAsUpdated();
        }

        private static string ValidateFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            }

            if (fileName.Length > 255)
            {
                throw new ArgumentException("File name cannot exceed 255 characters", nameof(fileName));
            }

            return fileName.Trim();
        }

        private static string ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            if (filePath.Length > 500)
            {
                throw new ArgumentException("File path cannot exceed 500 characters", nameof(filePath));
            }

            return filePath.Trim();
        }

        private static long ValidateFileSize(long fileSize)
        {
            if (fileSize <= 0)
            {
                throw new ArgumentException("File size must be positive", nameof(fileSize));
            }

            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (fileSize > maxFileSize)
            {
                throw new ArgumentException($"File size cannot exceed {maxFileSize / (1024 * 1024)}MB", nameof(fileSize));
            }

            return fileSize;
        }
    }
}
