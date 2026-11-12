using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Trocae.Infrastructure.Identity;
using Trocae.Infrastructure.Identity.CpfVerification;

namespace Trocae.Infrastructure.Persistence;

public sealed class TrocaeDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public TrocaeDbContext(DbContextOptions<TrocaeDbContext> options) : base(options)
    {
    }

    public DbSet<CpfLookupCache> CpfLookupCaches => Set<CpfLookupCache>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(user =>
        {
            user.Property(u => u.FullName).HasMaxLength(200).IsRequired();
            user.Property(u => u.CpfHash).HasMaxLength(64).IsRequired();

            // CPF único por conta — a unicidade é garantida pelo hash, nunca pelo CPF em texto puro.
            user.HasIndex(u => u.CpfHash).IsUnique();

            // Mesmo telefone não permanece verificado em duas contas; na ausência
            // de um módulo de verificação dedicado ainda, a unicidade já vale para
            // o campo informado no cadastro.
            user.HasIndex(u => u.PhoneNumber)
                .IsUnique()
                .HasFilter("\"PhoneNumber\" IS NOT NULL");
        });

        builder.Entity<CpfLookupCache>(cache =>
        {
            cache.ToTable("CpfLookupCaches");
            cache.HasKey(x => x.Id);
            cache.Property(x => x.CpfHash).HasMaxLength(64).IsRequired();
            cache.HasIndex(x => x.CpfHash).IsUnique();
        });
    }
}
