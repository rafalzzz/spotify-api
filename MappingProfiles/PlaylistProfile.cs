using AutoMapper;
using SpotifyApi.Entities;
using SpotifyApi.Responses;
using SpotifyApi.Requests;

namespace SpotifyApi.MappingProfiles
{
    public class PlaylistProfile : Profile
    {
        public PlaylistProfile()
        {
            CreateMap<CreatePlaylist, Playlist>()
                .ForMember(dest => dest.SongIds, opt => opt.Ignore())
                .ForMember(dest => dest.Collaborators, opt => opt.Ignore())
                .ForMember(dest => dest.FavoritedByUsers, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore());

            CreateMap<Playlist, PlaylistDto>()
                .ForMember(dest => dest.IsOwner, opt => opt.MapFrom((src, dest, destMember, context) =>
                    src.OwnerId == (int)context.Items["UserId"]))
                .ForMember(dest => dest.IsCollaborator, opt => opt.MapFrom((src, dest, destMember, context) =>
                    src.Collaborators.Any(c => c.Id == (int)context.Items["UserId"])))
                .ForMember(dest => dest.IsFavorite, opt => opt.MapFrom((src, dest, destMember, context) =>
                    src.FavoritedByUsers.Any(f => f.Id == (int)context.Items["UserId"])))
                .ForMember(dest => dest.SongIds, opt => opt.MapFrom(src => src.SongIds));
        }
    }
}
