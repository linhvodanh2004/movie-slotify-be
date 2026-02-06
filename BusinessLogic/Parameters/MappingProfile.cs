using AutoMapper;
using BusinessLogic.DTOs.requests;
using DataAccess.Entities;

namespace BusinessLogic.Parameters // Or BusinessLogic.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserRegistrationRequest, User>();
            CreateMap<User, BusinessLogic.DTOs.responses.UserResponse>();
        }
    }
}
