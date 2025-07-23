// MappingProfile.cs - Simplified version using extension methods
using AutoMapper;
using EventManagementSystem.Application.DTOs;
using EventManagementSystem.Domain.Entities;

namespace EventManagementSystem.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Simple mapping configurations for when AutoMapper is still needed
            // Most mapping should use the extension methods for better performance

            this.CreateMap<EventCategory, CategoryDto>()
                .ForMember(dest => dest.EventCount, opt => opt.MapFrom(src => src.Events.Count));

            // Note: For Event, User, EventRegistration, and EventImage mappings,
            // use the extension methods in EventExtensions.cs for better performance
            // and to avoid EF.Property issues in AutoMapper
        }
    }
}