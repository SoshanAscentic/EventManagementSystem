// <copyright file="CreateCategoryCommandHandler.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Usecases.Commands.CreateCategory
{
    using EventManagementSystem.Application.Common.Models;
    using EventManagementSystem.Domain.Entities;
    using EventManagementSystem.Domain.Repositories;
    using MediatR;

    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<int>>
    {
        private readonly IEventCategoryRepository categoryRepository;
        private readonly IUnitOfWork unitOfWork;

        public CreateCategoryCommandHandler(IEventCategoryRepository categoryRepository, IUnitOfWork unitOfWork)
        {
            this.categoryRepository = categoryRepository;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            // Check if category with same name exists
            if (await this.categoryRepository.ExistsByNameAsync(request.Name, cancellationToken))
            {
                return Result.Failure<int>("Category with this name already exists");
            }

            try
            {
                var category = EventCategory.Create(request.Name, request.Description);

                await this.categoryRepository.AddAsync(category, cancellationToken);
                await this.unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success(category.Id);
            }
            catch (Exception ex)
            {
                return Result.Failure<int>($"Failed to create category: {ex.Message}");
            }
        }
    }
}
