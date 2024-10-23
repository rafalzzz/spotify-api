using AutoMapper;
using SpotifyApi.Entities;
using SpotifyApi.Requests;

namespace SpotifyApi.MappingProfiles
{
    public class PlaylistProfile : Profile
    {
        public PlaylistProfile()
        {
            // Map from CreatePlaylis to Playlist
            CreateMap<CreatePlaylist, Playlist>()
                .ForMember(dest => dest.SongIds, opt => opt.Ignore())
                .ForMember(dest => dest.Collaborators, opt => opt.Ignore())
                .ForMember(dest => dest.FavoritedByUsers, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore());

            // Map from EditPlaylist to Playlist
            CreateMap<EditPlaylist, Playlist>()
                .ForMember(dest => dest.SongIds, opt => opt.Ignore())
                .ForMember(dest => dest.Collaborators, opt => opt.Ignore())
                .ForMember(dest => dest.FavoritedByUsers, opt => opt.Ignore())
                .ForMember(dest => dest.OwnerId, opt => opt.Ignore())

                // Optional
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Map from Playlist to PlaylistDto
            /*  CreateMap<Playlist, PlaylistDto>()
                 .ForMember(dest => dest.OwnerNickname, opt => opt.MapFrom(src => src.Owner.Nickname))
                 .ForMember(dest => dest.CollaboratorNicknames, opt => opt.MapFrom(src => src.Collaborators.Select(c => c.Nickname).ToList()))
                 .ForMember(dest => dest.FavoriteCount, opt => opt.MapFrom(src => src.FavoritedByUsers.Count)); */

            CreateMap<AddCollaborator, Playlist>()
                .ForMember(dest => dest.Collaborators, opt => opt.Ignore());
        }
    }
}