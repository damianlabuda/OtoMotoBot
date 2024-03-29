﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Shared.Entities;

#nullable disable

namespace Shared.Migrations
{
    [DbContext(typeof(OtoMotoContext))]
    partial class OtoMotoContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AdLinkSearchLink", b =>
                {
                    b.Property<long>("AdLinksId")
                        .HasColumnType("bigint");

                    b.Property<Guid>("SearchLinksId")
                        .HasColumnType("uuid");

                    b.HasKey("AdLinksId", "SearchLinksId");

                    b.HasIndex("SearchLinksId");

                    b.ToTable("AdLinkSearchLink");
                });

            modelBuilder.Entity("SearchLinkUser", b =>
                {
                    b.Property<Guid>("SearchLinksId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UsersId")
                        .HasColumnType("uuid");

                    b.HasKey("SearchLinksId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("SearchLinkUser");
                });

            modelBuilder.Entity("Shared.Entities.AdLink", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("integer");

                    b.Property<string>("City")
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<int>("EngineCapacity")
                        .HasColumnType("integer");

                    b.Property<string>("FuelType")
                        .HasColumnType("text");

                    b.Property<string>("Gearbox")
                        .HasColumnType("text");

                    b.Property<int>("HowManyTimesHasNotInSearch")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Make")
                        .HasColumnType("text");

                    b.Property<int>("Mileage")
                        .HasColumnType("integer");

                    b.Property<string>("Model")
                        .HasColumnType("text");

                    b.Property<string>("Region")
                        .HasColumnType("text");

                    b.Property<string>("Thumbnail")
                        .HasColumnType("text");

                    b.Property<int>("Year")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("AdLinks");
                });

            modelBuilder.Entity("Shared.Entities.AdPrice", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<long>("AdLinkId")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<double>("Price")
                        .HasColumnType("double precision");

                    b.HasKey("Id");

                    b.HasIndex("AdLinkId");

                    b.ToTable("AdPrices");
                });

            modelBuilder.Entity("Shared.Entities.SearchLink", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AdLinksCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("character varying(2048)");

                    b.Property<int>("SearchCount")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(0);

                    b.HasKey("Id");

                    b.ToTable("SearchLinks");
                });

            modelBuilder.Entity("Shared.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("now()");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long?>("TelegramChatId")
                        .HasColumnType("bigint");

                    b.Property<bool>("TelegramChatNotFound")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("TelegramName")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("AdLinkSearchLink", b =>
                {
                    b.HasOne("Shared.Entities.AdLink", null)
                        .WithMany()
                        .HasForeignKey("AdLinksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Shared.Entities.SearchLink", null)
                        .WithMany()
                        .HasForeignKey("SearchLinksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SearchLinkUser", b =>
                {
                    b.HasOne("Shared.Entities.SearchLink", null)
                        .WithMany()
                        .HasForeignKey("SearchLinksId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Shared.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Shared.Entities.AdPrice", b =>
                {
                    b.HasOne("Shared.Entities.AdLink", "AdLink")
                        .WithMany("Prices")
                        .HasForeignKey("AdLinkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AdLink");
                });

            modelBuilder.Entity("Shared.Entities.AdLink", b =>
                {
                    b.Navigation("Prices");
                });
#pragma warning restore 612, 618
        }
    }
}
