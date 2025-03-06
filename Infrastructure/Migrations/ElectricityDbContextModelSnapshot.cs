﻿// <auto-generated />
using System;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

//comment -- github being stupid

namespace Infrastructure.Migrations
{
    [DbContext(typeof(ElectricityDbContext))]
    partial class ElectricityDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Domain.Entities.ElectricityPriceData", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal>("Price")
                        .HasPrecision(18, 2)
                        .HasColumnType("decimal(18,2)")
                        .HasAnnotation("Relational:JsonPropertyName", "value");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime(6)")
                        .HasAnnotation("Relational:JsonPropertyName", "date");

                    b.Property<DateTime>("UpdatedAt")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("EndDate")
                        .HasDatabaseName("IX_ElectricityPriceData_EndDate");

                    b.HasIndex("StartDate")
                        .HasDatabaseName("IX_ElectricityPriceData_StartDate");

                    b.HasIndex("StartDate", "EndDate")
                        .HasDatabaseName("IX_ElectricityPriceData_StartEndDate");

                    b.ToTable("ElectricityPriceDatas");
                });
#pragma warning restore 612, 618
        }
    }
}
