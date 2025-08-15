// <copyright file="DeleteEventImageCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.DeleteImage
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class DeleteEventImageCommandHandler : IRequestHandler<DeleteEventImageCommand, Result>
    {
        private readonly IEventRepository eventRepository;
        private readonly IFileStorageService fileStorageService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<DeleteEventImageCommandHandler> logger;

        public DeleteEventImageCommandHandler(
            IEventRepository eventRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork,
            ILogger<DeleteEventImageCommandHandler> logger)
        {
            this.eventRepository = eventRepository;
            this.fileStorageService = fileStorageService;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result> Handle(DeleteEventImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation(
                    "Deleting image {ImageId} from event {EventId}",
                    request.ImageId,
                    request.EventId);

                var eventEntity = await this.eventRepository.GetByIdWithImagesAsync(
                    Domain.ValueObjects.EventId.Create(request.EventId),
                    cancellationToken);

                if (eventEntity == null)
                {
                    this.logger.LogWarning("Event not found: {EventId}", request.EventId);
                    return DomainErrors.Event.NotFound(request.EventId);
                }

                var image = eventEntity.Images.FirstOrDefault(i => i.Id == request.ImageId);
                if (image == null)
                {
                    this.logger.LogWarning("Image not found: {ImageId}", request.ImageId);
                    return DomainErrors.Event.ImageNotFound(request.ImageId);
                }

                // Delete file from storage
                await this.fileStorageService.DeleteFileAsync(image.FilePath, cancellationToken);

                // Remove image from event
                eventEntity.RemoveImage(request.ImageId);

                this.eventRepository.Update(eventEntity);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation(
                    "Successfully deleted image {ImageId} from event {EventId}",
                    request.ImageId,
                    request.EventId);

                return Result.Success();
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error deleting image {ImageId} from event {EventId}",
                    request.ImageId,
                    request.EventId);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
