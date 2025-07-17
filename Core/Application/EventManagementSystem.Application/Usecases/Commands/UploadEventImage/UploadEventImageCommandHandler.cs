// <copyright file="UploadEventImageCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.UploadEventImage
{
    using EventManagementSystem.Application.Common.Interfaces;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;

    public class UploadEventImageCommandHandler : IRequestHandler<UploadEventImageCommand, Result<int>>
    {
        private readonly IEventRepository eventRepository;
        private readonly IFileStorageService fileStorageService;
        private readonly IUnitOfWork unitOfWork;

        public UploadEventImageCommandHandler(
            IEventRepository eventRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork)
        {
            this.eventRepository = eventRepository;
            this.fileStorageService = fileStorageService;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(UploadEventImageCommand request, CancellationToken cancellationToken)
        {
            var eventEntity = await eventRepository.GetByIdWithImagesAsync(
                EventId.Create(request.EventId),
                cancellationToken);

            if (eventEntity == null)
            {
                return Result.Failure<int>("Event not found");
            }

            try
            {
                // Upload file to storage
                var filePath = await fileStorageService.SaveFileAsync(
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

                eventRepository.Update(eventEntity);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success(image.Id);
            }
            catch (Exception ex)
            {
                return Result.Failure<int>($"Failed to upload image: {ex.Message}");
            }
        }
    }
}
