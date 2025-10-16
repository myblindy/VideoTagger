using AutoMapper;

namespace VideoTagger.Models;

class MainModelMapperProfile : Profile
{
    public MainModelMapperProfile()
    {
        CreateMap<MainModelCategoryItemEnumValue, DbModelCategoryItemEnumValue>()
            .ReverseMap();
        CreateMap<MainModelCategoryItem, DbModelCategoryItem>()
            .ReverseMap();
        CreateMap<MainModelCategory, DbModelCategory>()
            .ReverseMap();
        CreateMap<MainModelVideoCacheEntry, DbModelVideoCacheEntry>()
            .ForMember(dest => dest.CoverImage, opt => opt.Ignore())
            .ReverseMap();
        CreateMap<MainModelVideoCacheTag, DbModelVideoCaccheEntryTag>()
            .ReverseMap();
        CreateMap<MainModelVideoCacheTagItem, DbModelVideoCacheEntryTagItem>()
            .ReverseMap();
        CreateMap<MainModelGroupMember, DbModelGroupMember>()
            .ReverseMap();
        CreateMap<MainModelGroup, DbModelGroup>()
            .ReverseMap();
        CreateMap<MainModelFolder, DbModelFolder>()
            .ReverseMap();
    }
}
