// <copyright file="EventImage.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Entities
{
    using EventManagementSystem.Domain.Common;
    using EventManagementSystem.Domain.ValueObjects;

    public class EventImage : BaseEntity
    {
        // Backing field for value object
        private int _eventId;

        // Private constructor for EF Core
        private EventImage()
        {
            this._eventId = 0;
            this.FileName = string.Empty;
            this.FilePath = string.Empty;
        }

        // Properties that EF Core will map directly
        public string FileName { get; private set; }

        public string FilePath { get; private set; }

        public long FileSize { get; private set; }

        public bool IsPrimary { get; private set; }

        public DateTime UploadedAt { get; private set; }

        // Value object property with backing field
        public EventId EventId
        {
            get => EventId.Create(this._eventId);
            private set => this._eventId = value.Value;
        }

        // Navigation property
        public Event? Event { get; private set; }

        // Factory method
        public static EventImage Create(EventId eventId, string fileName, string filePath, long fileSize, bool isPrimary = false)
        {
            var image = new EventImage
            {
                FileName = ValidateFileName(fileName),
                FilePath = ValidateFilePath(filePath),
                FileSize = ValidateFileSize(fileSize),
                IsPrimary = isPrimary,
                UploadedAt = DateTime.UtcNow,
            };

            // Set value object through property
            image.EventId = eventId;

            return image;
        }

        // Factory method for restoring from database
        public static EventImage Restore(
            int id,
            int eventId,
            string fileName,
            string filePath,
            long fileSize,
            bool isPrimary,
            DateTime uploadedAt,
            DateTime createdAt)
        {
            var image = new EventImage
            {
                _eventId = eventId,
                FileName = fileName,
                FilePath = filePath,
                FileSize = fileSize,
                IsPrimary = isPrimary,
                UploadedAt = uploadedAt,
            };

            // Set base entity properties
            typeof(BaseEntity).GetProperty(nameof(Id))?.SetValue(image, id);
            image.SetCreatedAt(createdAt);

            return image;
        }

        // Business methods
        public void SetPrimary(bool isPrimary)
        {
            this.IsPrimary = isPrimary;
            this.MarkAsUpdated();
        }

        public void UpdateFilePath(string newFilePath)
        {
            this.FilePath = ValidateFilePath(newFilePath);
            this.MarkAsUpdated();
        }

        private static string ValidateFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));
            }

            return fileName.Trim();
        }

        private static string ValidateFilePath(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            }

            return filePath.Trim();
        }

        private static long ValidateFileSize(long fileSize)
        {
            if (fileSize <= 0)
            {
                throw new ArgumentException("File size must be greater than 0", nameof(fileSize));
            }

            const long maxFileSize = 10 * 1024 * 1024; // 10MB
            if (fileSize > maxFileSize)
            {
                throw new ArgumentException($"File size cannot exceed {maxFileSize} bytes", nameof(fileSize));
            }

            return fileSize;
        }
    }
}
