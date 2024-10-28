using FluentValidation;
using SpotifyApi.Responses;
using SpotifyApi.Requests;
using SpotifyApi.Utilities;

namespace SpotifyApi.Services
{
    public interface IAddCollaboratorService
    {
        Result<bool> AddCollaborator(int playlistId, AddCollaborator addCollaboratorDto, int userId);
    }

    public class AddCollaboratorService(
        IRequestValidatorService requestValidatorService,
        IValidator<AddCollaborator> addCollaboratorValidator,
        IPlaylistService playlistService
    ) : IAddCollaboratorService
    {
        private readonly IRequestValidatorService _requestValidatorService = requestValidatorService;
        private readonly IValidator<AddCollaborator> _addCollaboratorValidator = addCollaboratorValidator;
        private readonly IPlaylistService _playlistService = playlistService;

        public Result<AddCollaborator> ValidateCollaboratorAddition(AddCollaborator addCollaboratorDto)
        {
            var validationResult = _requestValidatorService.ValidateRequest(
                addCollaboratorDto,
                _addCollaboratorValidator
            );

            return validationResult.IsSuccess ? Result<AddCollaborator>.Success(addCollaboratorDto) :
                Result<AddCollaborator>.Failure(validationResult.Error);
        }

        public Result<bool> AddCollaborator(int playlistId, AddCollaborator addCollaboratorDto, int userId)
        {
            return ValidateCollaboratorAddition(addCollaboratorDto)
            .Bind(addCollaborator => _playlistService.AddCollaborator(playlistId, addCollaborator.CollaboratorId, userId));
        }
    }
}
