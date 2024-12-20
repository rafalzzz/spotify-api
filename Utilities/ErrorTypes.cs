using System.ComponentModel;

namespace SpotifyApi.Utilities
{
    public enum ErrorType
    {
        [Description("Validation")]
        Validation = 0,
        [Description("Database")]
        Database = 1,
        [Description("Internal")]
        Internal = 2,
        [Description("Failure")]
        Failure = 3,
        [Description("UserAlreadyExist")]
        UserAlreadyExist = 4,
        [Description("PasswordHashing")]
        PasswordHashing = 5,
        [Description("WrongLogin")]
        WrongLogin = 6,
        [Description("WrongPassword")]
        WrongPassword = 7,
        [Description("WrongEmail")]
        WrongEmail = 8,
        [Description("GeneratePasswordResetTokenError")]
        GeneratePasswordResetTokenError = 9,
        [Description("TokenExpired")]
        TokenExpired = 10,
        [Description("InvalidToken")]
        InvalidToken = 11,
        [Description("ApiItunes")]
        ApiItunes = 12,
        [Description("ConfigurationError")]
        ConfigurationError = 13,
        [Description("WrongUserId")]
        WrongUserId = 14,
        [Description("UserNotFound")]
        WrongPlaylistId = 15,
        [Description("Unathorized")]
        Unauthorized = 16,
        [Description("UserIsAlreadyAdded")]
        UserIsAlreadyAdded = 17,
        [Description("UserIsNotAdded")]
        UserIsNotAdded = 18,
        [Description("SongIsAlreadyAdded")]
        SongIsAlreadyAdded = 19,
        [Description("SongIsNotAdded")]
        SongIsNotAdded = 20,
        [Description("PlaylistIsNotPublic")]
        PlaylistIsNotPublic = 21,
        [Description("PlaylistIsAlreadyAddedToFavorites")]
        PlaylistIsAlreadyAddedToFavorites = 22,
        [Description("PlaylistIsNotAddedToFavorites")]
        PlaylistIsNotAddedToFavorites = 23,
        [Description("UserIsOwnerOrCollaborator")]
        UserIsOwnerOrCollaborator = 24
    }
}