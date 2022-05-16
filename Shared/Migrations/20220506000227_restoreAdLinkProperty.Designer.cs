﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Entities;

#nullable disable

namespace Shared.Migrations
{
    [DbContext(typeof(OtomotoSearchAuctions))]
    [Migration("20220506000227_restoreAdLinkProperty")]
    partial class restoreAdLinkProperty
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("SearchLinkUser", b =>
                {
                    b.Property<Guid>("SearchLinksId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UsersId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("SearchLinksId", "UsersId");

                    b.HasIndex("UsersId");

                    b.ToTable("SearchLinkUser");
                });

            modelBuilder.Entity("Shared.Entities.AdLink", b =>
                {
                    b.Property<string>("Link")
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .ValueGeneratedOnUpdate()
                        .HasColumnType("datetime2");

                    b.Property<double>("Price")
                        .HasColumnType("float");

                    b.Property<Guid>("SearchLinkId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Link");

                    b.HasIndex("SearchLinkId");

                    b.ToTable("AdLinks");
                });

            modelBuilder.Entity("Shared.Entities.SearchLink", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .ValueGeneratedOnUpdate()
                        .HasColumnType("datetime2");

                    b.Property<string>("Link")
                        .IsRequired()
                        .HasMaxLength(2048)
                        .HasColumnType("nvarchar(2048)");

                    b.HasKey("Id");

                    b.ToTable("SearchLinks");
                });

            modelBuilder.Entity("Shared.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedDateTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("getutcdate()");

                    b.Property<DateTime?>("LastUpdateDateTime")
                        .ValueGeneratedOnUpdate()
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("Users");
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

            modelBuilder.Entity("Shared.Entities.AdLink", b =>
                {
                    b.HasOne("Shared.Entities.SearchLink", "SearchLink")
                        .WithMany("AdLinks")
                        .HasForeignKey("SearchLinkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("SearchLink");
                });

            modelBuilder.Entity("Shared.Entities.SearchLink", b =>
                {
                    b.Navigation("AdLinks");
                });
#pragma warning restore 612, 618
        }
    }
}
