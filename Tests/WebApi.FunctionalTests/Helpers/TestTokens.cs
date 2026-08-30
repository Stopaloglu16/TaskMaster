namespace WebApi.FunctionalTests.Helpers
{
    /// <summary>
    /// These tests authenticate through <see cref="TestAuthHandler"/>, registered as the default
    /// "IntegrationTest" scheme, which ignores the Authorization header entirely. The header is still
    /// set so requests look like the real thing; the value is never parsed or validated.
    /// <para>
    /// This replaces the old HS256 <c>JwtTokenHelper</c>, which minted tokens for a shared-secret
    /// scheme WebApi no longer has — it validates RS256 tokens against the Keycloak realm's JWKS.
    /// </para>
    /// </summary>
    public static class TestTokens
    {
        public const string Bearer = "integration-test-token";
    }
}
