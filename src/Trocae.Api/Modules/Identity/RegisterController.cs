using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Trocae.Domain.Identity;
using Trocae.Infrastructure.Identity;
using Trocae.Infrastructure.Identity.CpfVerification;

namespace Trocae.Api.Modules.Identity;

[ApiController]
[Route("api/auth")]
public sealed class RegisterController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ICpfVerificationService _cpfVerification;
    private readonly IConfiguration _configuration;

    public RegisterController(
        UserManager<ApplicationUser> userManager,
        ICpfVerificationService cpfVerification,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _cpfVerification = cpfVerification;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        // 1) Validação local do CPF — formato e dígitos verificadores, sem rede.
        if (!Cpf.TryParse(request.Cpf, out var cpf, out var cpfError))
        {
            return Problem(cpfError, statusCode: StatusCodes.Status400BadRequest, title: "CPF inválido");
        }

        // 2) Maioridade.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!AgeRule.IsAdult(request.BirthDate, today))
        {
            return Problem(
                "A conta precisa ter 18 anos completos.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Idade mínima não atingida");
        }

        // 3) Hash do CPF e checagem de unicidade local (CPF, e-mail, telefone).
        var pepper = _configuration["Cpf:Pepper"];
        if (string.IsNullOrEmpty(pepper))
        {
            return Problem(
                "Configuração de servidor incompleta.",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        var cpfHash = CpfHasher.Hash(cpf!.Digits, pepper);

        var cpfAlreadyUsed = await _userManager.Users.AnyAsync(u => u.CpfHash == cpfHash, cancellationToken);
        if (cpfAlreadyUsed)
        {
            return Problem(
                "Este CPF já possui uma conta.",
                statusCode: StatusCodes.Status409Conflict,
                title: "CPF já cadastrado");
        }

        var phoneAlreadyUsed = await _userManager.Users
            .AnyAsync(u => u.PhoneNumber == request.PhoneNumber, cancellationToken);
        if (phoneAlreadyUsed)
        {
            return Problem(
                "Este telefone já está associado a outra conta.",
                statusCode: StatusCodes.Status409Conflict,
                title: "Telefone já cadastrado");
        }

        // 4) Consulta Simples do CPFHub (ou mock em dev) — só depois de tudo local.
        CpfVerificationOutcome outcome;
        try
        {
            outcome = await _cpfVerification.VerifyAsync(
                cpf.Digits,
                cpfHash,
                request.FullName,
                request.BirthDate,
                cancellationToken);
        }
        catch (CpfProviderUnavailableException)
        {
            return Problem(
                "Serviço de verificação de CPF indisponível. Tente novamente em alguns minutos.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        switch (outcome)
        {
            case CpfVerificationOutcome.NotFound:
                return Problem(
                    "CPF não encontrado na base oficial.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "CPF não encontrado");
            case CpfVerificationOutcome.Mismatch:
                return Problem(
                    "Nome ou data de nascimento não conferem com o CPF informado.",
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Dados não conferem");
        }

        // 5) Cria a conta. O CPF em texto puro nunca é persistido — só o hash.
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            BirthDate = request.BirthDate,
            CpfHash = cpfHash,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            foreach (var identityError in createResult.Errors)
            {
                ModelState.AddModelError(identityError.Code, identityError.Description);
            }

            return ValidationProblem(ModelState);
        }

        var response = new RegisterResponse(
            user.Id,
            user.Email!,
            "Verifique seu e-mail e confirme seu telefone por SMS antes de negociar.");

        return CreatedAtAction(nameof(Register), new { }, response);
    }
}
