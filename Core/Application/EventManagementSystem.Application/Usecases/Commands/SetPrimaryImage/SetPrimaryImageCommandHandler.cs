// <copyright file="SetPrimaryImageCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.SetPrimaryImage
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Repositories;
    using EventManagementSystem.Domain.ValueObjects;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class SetPrimaryImageCommandHandler : IRequestHandler<SetPrimaryImageCommand, Result>
    {
        private readonly IEventRepository eventRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<SetPrimaryImageCommandHandler> logger;

        public SetPrimaryImageCommandHandler(
            IEventRepository eventRepository,
            IUnitOfWork unitOfWork,
            ILogger<SetPrimaryImageCommandHandler> logger)
        {
            this.eventRepository = eventRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result> Handle(SetPrimaryImageCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation(
                    "Setting image {ImageId} as primary for event {EventId}",
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

                eventEntity.SetPrimaryImage(request.ImageId);

                this.eventRepository.Update(eventEntity);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation(
                    "Successfully set image {ImageId} as primary for event {EventId}",
                    request.ImageId,
                    request.EventId);

                return Result.Success();
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
            {
                this.logger.LogWarning(ex, "Image not found: {ImageId}", request.ImageId);
                return DomainErrors.Event.ImageNotFound(request.ImageId);
            }
            catch (Exception ex)
            {
                this.logger.LogError(
                    ex,
                    "Error setting primary image {ImageId} for event {EventId}",
                    request.ImageId,
                    request.EventId);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
