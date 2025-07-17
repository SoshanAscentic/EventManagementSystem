// <copyright file="UploadEventImageCommandValidator.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UploadEventImage
{
    using FluentValidation;

    public class UploadEventImageCommandValidator : AbstractValidator<UploadEventImageCommand>
    {
        private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] allowedContentTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };

        public UploadEventImageCommandValidator()
        {
            this.RuleFor(x => x.EventId)
                .GreaterThan(0);

            this.RuleFor(x => x.FileName)
                .NotEmpty()
                .Must(this.HaveValidExtension)
                .WithMessage($"File must have one of the following extensions: {string.Join(", ", this.allowedExtensions)}");

            this.RuleFor(x => x.ContentType)
                .NotEmpty()
                .Must(this.HaveValidContentType)
                .WithMessage($"File must be one of the following types: {string.Join(", ", this.allowedContentTypes)}");

            this.RuleFor(x => x.FileSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(MaxFileSize)
                .WithMessage($"File size must be less than {MaxFileSize / (1024 * 1024)}MB");

            this.RuleFor(x => x.FileStream)
                .NotNull();
        }

        private bool HaveValidExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return this.allowedExtensions.Contains(extension);
        }

        private bool HaveValidContentType(string contentType)
        {
            return this.allowedContentTypes.Contains(contentType.ToLowerInvariant());
        }
    }
}
