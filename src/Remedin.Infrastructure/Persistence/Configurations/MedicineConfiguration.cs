using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeia o agregado sem contaminar o domínio: nenhuma classe de
/// <c>Remedin.Domain</c> conhece EF Core ou atributo de persistência.
///
/// Tabelas e colunas em minúsculo porque o PostgreSQL exige aspas em
/// identificador com maiúscula, e isso atrapalha quem consulta na mão.
/// </summary>
public sealed class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
{
    public void Configure(EntityTypeBuilder<Medicine> builder)
    {
        builder.ToTable("medicines");

        builder.HasKey(medicine => medicine.RegistrationNumber);

        builder.Property(medicine => medicine.RegistrationNumber)
            .HasColumnName("registration_number")
            .HasMaxLength(RegistrationNumber.Length)
            .IsFixedLength()
            .HasConversion(
                number => number.Value,
                value => RegistrationNumber.Parse(value));

        builder.Property(medicine => medicine.Name)
            .HasColumnName("name")
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(medicine => medicine.ActiveIngredient)
            .HasColumnName("active_ingredient")
            .HasMaxLength(1000);

        builder.Property(medicine => medicine.Manufacturer)
            .HasColumnName("manufacturer")
            .HasMaxLength(300);

        builder.Property(medicine => medicine.TherapeuticClassCode)
            .HasColumnName("therapeutic_class_code")
            .HasMaxLength(20);

        builder.Property(medicine => medicine.TherapeuticClassName)
            .HasColumnName("therapeutic_class_name")
            .HasMaxLength(300);

        builder.Property(medicine => medicine.PrescriptionBand)
            .HasColumnName("prescription_band")
            .HasMaxLength(50);

        builder.Property(medicine => medicine.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // A apresentação não tem vida fora do medicamento, então é mapeada
        // como parte do agregado: não dá para consultá-la sozinha.
        builder.OwnsMany(medicine => medicine.Presentations, presentation =>
        {
            presentation.ToTable("presentations");

            presentation.WithOwner().HasForeignKey("registration_number");

            presentation.Property(p => p.GgremCode)
                .HasColumnName("ggrem_code")
                .HasMaxLength(20)
                .IsRequired();

            presentation.HasKey("registration_number", nameof(Presentation.GgremCode));

            presentation.Property(p => p.Description)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired();

            presentation.Property(p => p.HospitalOnly)
                .HasColumnName("hospital_only")
                .IsRequired();

            presentation.Property(p => p.SoldRecently)
                .HasColumnName("sold_recently")
                .IsRequired();

            // Um preço por alíquota de ICMS: o teto legal muda conforme o
            // estado onde o medicamento é vendido (ADR 0006).
            presentation.OwnsMany(p => p.Prices, price =>
            {
                price.ToTable("prices");

                price.WithOwner().HasForeignKey("registration_number", "ggrem_code");

                price.Property(p => p.Kind)
                    .HasColumnName("kind")
                    .HasConversion<string>()
                    .HasMaxLength(10)
                    .IsRequired();

                // Nulo na coluna sem impostos, e por isso fora da chave.
                price.Property(p => p.IcmsRate)
                    .HasColumnName("icms_rate")
                    .HasPrecision(4, 2);

                price.Property(p => p.FreeTradeZone)
                    .HasColumnName("free_trade_zone")
                    .IsRequired();

                // numeric preserva o centavo. Preço em ponto flutuante erra na
                // soma e na comparação, e aqui ele é informação legal.
                price.Property(p => p.Amount)
                    .HasColumnName("amount")
                    .HasPrecision(12, 2)
                    .IsRequired();

                // Os dois primeiros são as chaves estrangeiras sombra criadas
                // acima; os dois últimos são propriedades de Price.
                price.HasIndex(
                        "registration_number",
                        "ggrem_code",
                        nameof(Price.Kind),
                        nameof(Price.IcmsRate))
                    .HasDatabaseName("ix_prices_presentation_kind_rate");
            });

            presentation.Navigation(p => p.Prices)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Navigation(medicine => medicine.Presentations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
