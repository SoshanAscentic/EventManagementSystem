// <copyright file="MappingProfile.cs" company="Ascentic">
// Copyright (c) Ascentic. All rights reserved.
// </copyright>

namespace EventManagementSystem.Application.Common.Mappings
{
    using AutoMapper;
    using EventManagementSystem.Application.DTOs;
    using EventManagementSystem.Domain.Entities;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Event mappings - FIXED
            this.CreateMap<Event, EventDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.StartDateTime, opt => opt.MapFrom(src => src.EventDateTime.StartDateTime))
                .ForMember(dest => dest.EndDateTime, opt => opt.MapFrom(src => src.EventDateTime.EndDateTime))
                .ForMember(dest => dest.Venue, opt => opt.MapFrom(src => src.Location.Venue))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Location.Address))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Location.City))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Location.Country))
                .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity.Value))
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType.Value))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : "Unknown"))
                .ForMember(dest => dest.CurrentRegistrations, opt => opt.MapFrom(src => src.CurrentRegistrations))
                .ForMember(dest => dest.RemainingCapacity, opt => opt.MapFrom(src => src.RemainingCapacity))
                .ForMember(dest => dest.IsRegistrationOpen, opt => opt.MapFrom(src => src.IsRegistrationOpen))
                .ForMember(dest => dest.PrimaryImageUrl, opt => opt.MapFrom(src => src.PrimaryImage != null ? src.PrimaryImage.FilePath : null))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));

            // User mappings - FIXED
            this.CreateMap<User, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Value : null))
                .ForMember(dest => dest.ActiveRegistrationsCount, opt => opt.MapFrom(src => src.ActiveRegistrationsCount))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));

            // EventRegistration mappings - FIXED
            this.CreateMap<EventRegistration, RegistrationDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EventId, opt => opt.MapFrom(src => src.EventId.Value))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.Value))
                .ForMember(dest => dest.EventTitle, opt => opt.MapFrom(src => src.Event != null ? src.Event.Title : "Unknown"))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Unknown"))
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email.Value : "Unknown"))
                .ForMember(dest => dest.RegisteredAt, opt => opt.MapFrom(src => src.RegisteredAt))
                .ForMember(dest => dest.CancelledAt, opt => opt.MapFrom(src => src.CancelledAt))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.Value))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.IsCancelled, opt => opt.MapFrom(src => src.IsCancelled));

            // EventCategory mappings - FIXED
            this.CreateMap<EventCategory, CategoryDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.EventCount, opt => opt.MapFrom(src => src.Events.Count));

            // EventImage mappings - FIXED
            this.CreateMap<EventImage, EventImageDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EventId, opt => opt.MapFrom(src => src.EventId.Value))
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
                .ForMember(dest => dest.FilePath, opt => opt.MapFrom(src => src.FilePath))
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.IsPrimary, opt => opt.MapFrom(src => src.IsPrimary))
                .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => src.UploadedAt));
        }
    }
}
