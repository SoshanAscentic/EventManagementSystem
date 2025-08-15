// <copyright file="UploadEventImageCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UploadEventImage
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class UploadEventImageCommandHandler : IRequestHandler<UploadEventImageCommand, Result<int>>
    {
        private readonly IEventRepository eventRepository;
        private readonly IFileStorageService fileStorageService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<UploadEventImageCommandHandler> logger;

        public UploadEventImageCommandHandler(
            IEventRepository eventRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork,
            ILogger<UploadEventImageCommandHandler> logger)
        {
            this.eventRepository = eventRepository;
            this.fileStorageService = fileStorageService;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result<int>> Handle(UploadEventImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Uploading image for event: {EventId}, FileName: {FileName}", request.EventId, request.FileName);

                var eventEntity = await this.eventRepository.GetByIdWithImagesAsync(
                    Domain.ValueObjects.EventId.Create(request.EventId),
                    cancellationToken);

                if (eventEntity == null)
                {
                    this.logger.LogWarning("Event not found: {EventId}", request.EventId);
                    return DomainErrors.Event.NotFound(request.EventId);
                }

                // Check if trying to set primary when one already exists
                if (request.IsPrimary && eventEntity.PrimaryImage != null)
                {
                    this.logger.LogWarning("Event already has a primary image: {EventId}", request.EventId);
                    return DomainErrors.Event.PrimaryImageAlreadyExists();
                }

                // Upload file to storage
                var filePath = await this.fileStorageService.SaveFileAsync(
                    request.FileStream,
                    request.FileName,
                    request.ContentType,
                    cancellationToken);

                // Add image to event
                var image = eventEntity.AddImage(
                    request.FileName,
                    filePath,
                    request.FileSize,
                    request.IsPrimary);

                this.eventRepository.Update(eventEntity);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation("Successfully uploaded image for event: {EventId}, ImageId: {ImageId}", request.EventId, image.Id);
                return image.Id;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("primary image"))
            {
                this.logger.LogWarning(ex, "Event already has primary image: {EventId}", request.EventId);
                return DomainErrors.Event.PrimaryImageAlreadyExists();
            }
            catch (ArgumentException ex) when (ex.Message.Contains("File name"))
            {
                this.logger.LogWarning(ex, "Invalid file name: {FileName}", request.FileName);
                return DomainErrors.File.InvalidFormat(request.FileName, new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" });
            }
            catch (ArgumentException ex) when (ex.Message.Contains("File size"))
            {
                this.logger.LogWarning(ex, "File size exceeded: {FileName}, Size: {Size}", request.FileName, request.FileSize);
                return DomainErrors.File.SizeExceeded(request.FileName, request.FileSize, 10 * 1024 * 1024);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("ID"))
            {
                this.logger.LogWarning(ex, "Invalid event ID provided: {EventId}", request.EventId);
                return DomainErrors.General.InvalidId("Event");
            }
            catch (Exception ex) when (ex.Message.Contains("upload") || ex.Message.Contains("storage"))
            {
                this.logger.LogError(ex, "File upload failed: {FileName}", request.FileName);
                return DomainErrors.File.UploadFailed(request.FileName, ex.Message);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error uploading image for event: {EventId}", request.EventId);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
