// <copyright file="FileExceptions.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Domain.Exceptions
{
    using EventManagementSystem.Domain.Common;

    public sealed class FileOperationException : DomainException
    {
        public string FileName { get; }

        public string Operation { get; }

        public FileOperationException(string fileName, string operation, string reason)
            : base("File.OperationFailed", $"File operation '{operation}' failed for '{fileName}': {reason}")
        {
            this.FileName = fileName;
            this.Operation = operation;
        }

        public FileOperationException(string fileName, string operation, string reason, Exception innerException)
            : base("File.OperationFailed", $"File operation '{operation}' failed for '{fileName}': {reason}", innerException)
        {
            this.FileName = fileName;
            this.Operation = operation;
        }
    }

    public sealed class InvalidFileException : DomainException
    {
        public string FileName { get; }

        public long FileSize { get; }

        public InvalidFileException(string fileName, long fileSize, string reason)
            : base("File.Invalid", $"File '{fileName}' ({fileSize} bytes) is invalid: {reason}")
        {
            this.FileName = fileName;
            this.FileSize = fileSize;
        }
    }

    public sealed class FileSizeExceededException : DomainException
    {
        public string FileName { get; }

        public long FileSize { get; }

        public long MaxAllowedSize { get; }

        public FileSizeExceededException(string fileName, long fileSize, long maxAllowedSize)
            : base(
                "File.SizeExceeded",
                $"File '{fileName}' size ({fileSize} bytes) exceeds maximum allowed size ({maxAllowedSize} bytes).")
        {
            this.FileName = fileName;
            this.FileSize = fileSize;
            this.MaxAllowedSize = maxAllowedSize;
        }
    }

    public sealed class UnsupportedFileTypeException : DomainException
    {
        public string FileName { get; }

        public string FileExtension { get; }

        public string[] SupportedExtensions { get; }

        public UnsupportedFileTypeException(string fileName, string fileExtension, string[] supportedExtensions)
            : base(
                "File.UnsupportedType",
                $"File '{fileName}' has unsupported extension '{fileExtension}'. Supported extensions: {string.Join(", ", supportedExtensions)}")
        {
            this.FileName = fileName;
            this.FileExtension = fileExtension;
            this.SupportedExtensions = supportedExtensions;
        }
    }
}
