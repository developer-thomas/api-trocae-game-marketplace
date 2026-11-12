namespace Trocae.Api.Modules.Identity;

public sealed record RegisterResponse(Guid Id, string Email, string NextSteps);
