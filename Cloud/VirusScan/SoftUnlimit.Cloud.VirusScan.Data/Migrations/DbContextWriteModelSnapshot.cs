﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SoftUnlimit.Cloud.VirusScan.Data;

namespace SoftUnlimit.Cloud.VirusScan.Data.Migrations
{
    [DbContext(typeof(DbContextWrite))]
    partial class DbContextWriteModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("viruscan")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SoftUnlimit.CQRS.EventSourcing.Json.JsonVersionedEventPayload", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CorrelationId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("EventName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsPubliched")
                        .HasColumnType("bit");

                    b.Property<string>("Payload")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SourceId")
                        .IsRequired()
                        .HasMaxLength(36)
                        .HasColumnType("nvarchar(36)");

                    b.Property<long>("Version")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.ToTable("VersionedEvent");
                });

            modelBuilder.Entity("SoftUnlimit.Cloud.VirusScan.Data.Model.Complete", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BlobUri")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasComment("Unique BlobUri identifier of the file");

                    b.Property<string>("CorrelationId")
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)")
                        .HasComment("CorrelationId asociate to the process. Unique value to identifier the source of the operation");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2")
                        .HasComment("Date where request is created");

                    b.Property<Guid?>("CustomerId")
                        .HasColumnType("uniqueidentifier")
                        .HasComment("User owner of the file. Null if no user asociate");

                    b.Property<bool>("HasVirus")
                        .HasColumnType("bit")
                        .HasComment("Indicate if the file has virus or not");

                    b.Property<Guid?>("RequestId")
                        .HasColumnType("uniqueidentifier")
                        .HasComment("Identifier of the request");

                    b.Property<int>("Retry")
                        .HasColumnType("int")
                        .HasComment("Number of retry attemp for the file");

                    b.Property<DateTime>("Scanned")
                        .HasColumnType("datetime2")
                        .HasComment("Date when the file was scanned");

                    b.Property<long>("Version")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("CorrelationId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("RequestId");

                    b.ToTable("Complete");
                });

            modelBuilder.Entity("SoftUnlimit.Cloud.VirusScan.Data.Model.Customer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("FirstVirusDetected")
                        .HasColumnType("datetime2")
                        .HasComment("Date when some request has mark with virus for first time");

                    b.Property<int>("HistoryVirusDetected")
                        .HasColumnType("int")
                        .HasComment("Amount of virus detected for this user in the entirely history");

                    b.Property<long>("Version")
                        .HasColumnType("bigint");

                    b.Property<int>("VirusDetected")
                        .HasColumnType("int")
                        .HasComment("Amount of request with virus detected from the FirstVirusDetected date");

                    b.HasKey("Id");

                    b.ToTable("Customer");
                });

            modelBuilder.Entity("SoftUnlimit.Cloud.VirusScan.Data.Model.Pending", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BlobUri")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasComment("Unique BlobUri identifier of the file");

                    b.Property<string>("CorrelationId")
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)")
                        .HasComment("CorrelationId asociate to the process. Unique value to identifier the source of the operation");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2")
                        .HasComment("Date where request is created");

                    b.Property<Guid?>("CustomerId")
                        .HasColumnType("uniqueidentifier")
                        .HasComment("User owner of the file. Null if no user asociate");

                    b.Property<string>("Metadata")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Metadata asociate to the file, serialize in json");

                    b.Property<Guid?>("RequestId")
                        .HasColumnType("uniqueidentifier")
                        .HasComment("Identifier of the request");

                    b.Property<int>("Retry")
                        .HasColumnType("int")
                        .HasComment("Number of retry attemp for the file");

                    b.Property<DateTime?>("Scanned")
                        .HasColumnType("datetime2")
                        .HasComment("Date when the file was scanned");

                    b.Property<DateTime>("Scheduler")
                        .HasColumnType("datetime2")
                        .HasComment("Date where the file will be scanned");

                    b.Property<int>("Status")
                        .HasColumnType("int")
                        .HasComment("Status of the request. (1 - Pending, 2 - Approved, 3 - Error)");

                    b.Property<long>("Version")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("CorrelationId");

                    b.HasIndex("CustomerId");

                    b.HasIndex("RequestId");

                    b.ToTable("Pending");
                });

            modelBuilder.Entity("SoftUnlimit.Cloud.VirusScan.Data.Model.Complete", b =>
                {
                    b.HasOne("SoftUnlimit.Cloud.VirusScan.Data.Model.Customer", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("SoftUnlimit.Cloud.VirusScan.Data.Model.Pending", b =>
                {
                    b.HasOne("SoftUnlimit.Cloud.VirusScan.Data.Model.Customer", "Customer")
                        .WithMany()
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Customer");
                });
#pragma warning restore 612, 618
        }
    }
}
