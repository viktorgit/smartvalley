﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using SmartValley.Data.SQL.Core;
using SmartValley.Domain.Entities;
using System;

namespace SmartValley.Data.SQL.Migrations
{
    [DbContext(typeof(AppDBContext))]
    [Migration("20171122104600_AddScoringCounters")]
    partial class AddScoringCounters
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SmartValley.Domain.Entities.Application", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CryptoCurrency")
                        .HasMaxLength(20);

                    b.Property<string>("FinancialModelLink")
                        .HasMaxLength(100);

                    b.Property<decimal>("HardCap");

                    b.Property<bool>("InvestmentsAreAttracted");

                    b.Property<string>("MVPLink")
                        .HasMaxLength(100);

                    b.Property<long>("ProjectId");

                    b.Property<string>("ProjectStatus")
                        .HasMaxLength(30);

                    b.Property<decimal>("SoftCap");

                    b.Property<string>("WhitePaperLink")
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.ToTable("Applications");
                });

            modelBuilder.Entity("SmartValley.Domain.Entities.Project", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("AnalystEstimatesCount");

                    b.Property<string>("AuthorAddress")
                        .IsRequired()
                        .HasMaxLength(42);

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasMaxLength(30);

                    b.Property<Guid>("ExternalId");

                    b.Property<byte>("HrEstimatesCount");

                    b.Property<byte>("LawyerEstimatesCount");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("ProblemDesc")
                        .HasMaxLength(255);

                    b.Property<string>("ProjectAddress")
                        .IsRequired()
                        .HasMaxLength(42);

                    b.Property<string>("ProjectArea")
                        .IsRequired()
                        .HasMaxLength(20);

                    b.Property<double?>("Score");

                    b.Property<string>("SolutionDesc")
                        .HasMaxLength(255);

                    b.Property<byte>("TechnicalEstimatesCount");

                    b.HasKey("Id");

                    b.HasIndex("ExternalId")
                        .IsUnique();

                    b.ToTable("Projects");
                });

            modelBuilder.Entity("SmartValley.Domain.Entities.TeamMember", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ApplicationId");

                    b.Property<string>("FacebookLink")
                        .HasMaxLength(100);

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("LinkedInLink")
                        .HasMaxLength(100);

                    b.Property<int>("PersonType");

                    b.HasKey("Id");

                    b.ToTable("Persons");
                });
#pragma warning restore 612, 618
        }
    }
}
