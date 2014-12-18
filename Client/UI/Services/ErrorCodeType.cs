namespace Services
{
    public enum ErrorCodeType
    {
        Unclassified,

        InvalidRequest,

        FeatureUnavailable,

        InvalidApiCredentials,
        OAuthFailure,
        SenderAuthenticationFailure,

        ResourceNotFound,
        ResourceNotModified,
        ResourceAlreadyExists,

        ServiceInternalError,
        ServiceUnexpectedError,
        ServiceTimeout,
        ServiceBadResponse,
        ServiceUnavailable,
        SessionTimeout
    }
}