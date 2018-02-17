namespace Harkenel.Gdax
{
    public interface IRequestAuthenticator
    {
        AuthenticationToken GetAuthenticationToken(ApiRequest request);
    }
}
