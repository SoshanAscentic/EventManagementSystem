// <copyright file="CreateCategoryCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CreateCategory
{
    using EventManagementSystem.Application.Common.Constants;
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.Repositories;
    using MediatR;
    using Microsoft.Extensions.Logging;

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<int>>
    {
        private readonly IEventCategoryRepository categoryRepository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<CreateCategoryCommandHandler> logger;

        public CreateCategoryCommandHandler(
            IEventCategoryRepository categoryRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateCategoryCommandHandler> logger)
        {
            this.categoryRepository = categoryRepository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Result<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                this.logger.LogInformation("Creating category: {Name}", request.Name);

                // Check if category with same name exists
                if (await this.categoryRepository.ExistsByNameAsync(request.Name, cancellationToken))
                {
                    this.logger.LogWarning("Category with name already exists: {Name}", request.Name);
                    return DomainErrors.Category.NameAlreadyExists(request.Name);
                }

                var category = EventCategory.Create(request.Name, request.Description);

                await this.categoryRepository.AddAsync(category, cancellationToken);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                this.logger.LogInformation("Successfully created category with ID: {CategoryId}", category.Id);
                return category.Id;
            }
            catch (ArgumentException ex) when (ex.Message.Contains("name") && ex.Message.Contains("empty"))
            {
                this.logger.LogWarning(ex, "Category name cannot be empty");
                return DomainErrors.Category.NameEmpty();
            }
            catch (ArgumentException ex) when (ex.Message.Contains("name") && ex.Message.Contains("characters"))
            {
                this.logger.LogWarning(ex, "Category name too long: {Name}", request.Name);
                return DomainErrors.Category.NameTooLong(50);
            }
            catch (ArgumentException ex) when (ex.Message.Contains("description") && ex.Message.Contains("empty"))
            {
                this.logger.LogWarning(ex, "Category description cannot be empty");
                return DomainErrors.Category.DescriptionEmpty();
            }
            catch (ArgumentException ex) when (ex.Message.Contains("description") && ex.Message.Contains("characters"))
            {
                this.logger.LogWarning(ex, "Category description too long");
                return DomainErrors.Category.DescriptionTooLong(500);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error creating category: {Name}", request.Name);
                return DomainErrors.General.UnexpectedError();
            }
        }
    }
}
