﻿// <auto-generated />
using System;
using App.Manual.Tests.CQRS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace App.Manual.Tests.Migrations
{
    [DbContext(typeof(DbContextWrite))]
    partial class DbContextWriteModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("App.Manual.Tests.CQRS.Data.Dummy", b =>
                {
                    b.Property<Guid>("ID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(60)")
                        .HasMaxLength(60);

                    b.Property<long>("Version")
                        .HasColumnType("bigint");

                    b.HasKey("ID");

                    b.ToTable("Dummy");
                });

            modelBuilder.Entity("SoftUnlimit.CQRS.EventSourcing.Binary.BinaryVersionedEventPayload", b =>
                {
                    b.Property<string>("SourceID")
                        .HasColumnType("nvarchar(36)")
                        .HasMaxLength(36);

                    b.Property<long>("Version")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("CreatorID")
                        .IsRequired()
                        .HasColumnType("nvarchar(36)")
                        .HasMaxLength(36);

                    b.Property<string>("CreatorName")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<string>("EventName")
                        .IsRequired()
                        .HasColumnType("nvarchar(255)")
                        .HasMaxLength(255);

                    b.Property<bool>("IsDomain")
                        .HasColumnType("bit");

                    b.Property<bool>("IsFinalAction")
                        .HasColumnType("bit");

                    b.Property<bool>("IsPubliched")
                        .HasColumnType("bit");

                    b.Property<bool>("IsStartAction")
                        .HasColumnType("bit");

                    b.Property<byte[]>("RawData")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<long>("ServiceID")
                        .HasColumnType("bigint");

                    b.Property<int>("WorkerID")
                        .HasColumnType("int");

                    b.HasKey("SourceID", "Version");

                    b.HasIndex("CreatorName");

                    b.HasIndex("EventName");

                    b.HasIndex("SourceID");

                    b.ToTable("BinaryVersionedEventPayload");
                });
#pragma warning restore 612, 618
        }
    }
}