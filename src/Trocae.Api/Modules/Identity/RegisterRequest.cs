using System.ComponentModel.DataAnnotations;

namespace Trocae.Api.Modules.Identity;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "Nome completo é obrigatório.")]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Data de nascimento é obrigatória.")]
    public DateOnly BirthDate { get; set; }

    [Required(ErrorMessage = "E-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "E-mail inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefone é obrigatório.")]
    [Phone(ErrorMessage = "Telefone inválido.")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>Com ou sem máscara — a validação local aceita ambos os formatos.</summary>
    [Required(ErrorMessage = "CPF é obrigatório.")]
    public string Cpf { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória.")]
    [MinLength(8, ErrorMessage = "Senha deve ter no mínimo 8 caracteres.")]
    public string Password { get; set; } = string.Empty;
}
