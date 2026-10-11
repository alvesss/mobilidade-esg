using System.ComponentModel.DataAnnotations;

namespace MobilidadeESG.Models;

public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string SenhaHash { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Perfil { get; set; } = "Usuario"; // Admin, Gestor, Usuario

    public bool Ativo { get; set; } = true;

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
