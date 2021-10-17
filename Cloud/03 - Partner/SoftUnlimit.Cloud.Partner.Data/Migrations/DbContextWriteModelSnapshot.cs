﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SoftUnlimit.Cloud.Partner.Data;

namespace SoftUnlimit.Cloud.Partner.Data.Migrations
{
    [DbContext(typeof(DbContextWrite))]
    partial class DbContextWriteModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasDefaultSchema("partner")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("SoftUnlimit.Cloud.Partner.Data.Model.JnRewardComplete", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Body")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Body of the event serialized as json.");

                    b.Property<DateTime>("Completed")
                        .HasColumnType("datetime2")
                        .HasComment("Date where the event was process complete.");

                    b.Property<string>("CorrelationId")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasComment("Correlation of the event, indicate the trace were the event was generate.");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2")
                        .HasComment("Event create date.");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uniqueidentifier")
                        .HasComment("Identifier of the event");

                    b.Property<Guid?>("IdentityId")
                        .HasColumnType("uniqueidentifier")
                        .HasComment("Identity owner of the event.");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasComment("Name of the event.");

                    b.Property<int?>("PartnerId")
                        .HasColumnType("int")
                        .HasComment("Partner identifier where the event comming from (if null is internal system).");

                    b.Property<int>("Retry")
                        .HasColumnType("int")
                        .HasComment("Retry attempt for this event.");

                    b.Property<int>("ServiceId")
                        .HasColumnType("int")
                        .HasComment("Service identifier.");

                    b.Property<string>("SourceId")
                        .HasColumnType("nvarchar(450)")
                        .HasComment("Primary key for the entity is unique for all the system.");

                    b.Property<long>("Version")
                        .HasColumnType("bigint")
                        .HasComment("Event version number alwais is incremental for the same SourceId.");

                    b.Property<string>("WorkerId")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasComment("Worker identifier where the event was generate.");

                    b.HasKey("Id");

                    b.HasIndex("CorrelationId");

                    b.HasIndex("Created");

                    b.HasIndex("EventId");

                    b.HasIndex("Name");

                    b.HasIndex("SourceId");

                    b.ToTable("JnRewardComplete");
                });

            modelBuilder.Entity("SoftUnlimit.Cloud.Partner.Data.Model.JnRewardPending", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Body")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Body of the event serialized as json.");

                    b.Property<string>("CorrelationId")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasComment("Correlation of the event, indicate the trace were the event was generate.");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2")
                        .HasComment("Event create date.");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uniqueidentifier")
                        .HasComment("Identifier of the event");

                    b.Property<Guid?>("IdentityId")
                        .HasColumnType("uniqueidentifier")
                        .HasComment("Identity owner of the event.");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasComment("Name of the event.");

                    b.Property<int?>("PartnerId")
                        .HasColumnType("int")
                        .HasComment("Partner identifier where the event comming from (if null is internal system).");

                    b.Property<int>("Retry")
                        .HasColumnType("int")
                        .HasComment("Retry attempt for this event.");

                    b.Property<DateTime>("Scheduler")
                        .HasColumnType("datetime2")
                        .HasComment("Scheduler time popone this event.");

                    b.Property<int>("ServiceId")
                        .HasColumnType("int")
                        .HasComment("Service identifier.");

                    b.Property<string>("SourceId")
                        .HasColumnType("nvarchar(450)")
                        .HasComment("Primary key for the entity is unique for all the system.");

                    b.Property<long>("Version")
                        .HasColumnType("bigint")
                        .HasComment("Event version number alwais is incremental for the same SourceId.");

                    b.Property<string>("WorkerId")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasComment("Worker identifier where the event was generate.");

                    b.HasKey("Id");

                    b.HasIndex("CorrelationId");

                    b.HasIndex("Created");

                    b.HasIndex("EventId");

                    b.HasIndex("Name");

                    b.HasIndex("SourceId");

                    b.ToTable("JnRewardPending");
                });

            modelBuilder.Entity("SoftUnlimit.Cloud.Partner.Data.Model.SaleforceComplete", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Body")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Body of the event serialized as json.");

                    b.Property<DateTime>("Completed")
                        .HasColumnType("datetime2")
                        .HasComment("Date where the event was process complete.");

                    b.Property<string>("CorrelationId")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasComment("Correlation of the event, indicate the trace were the event was generate.");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2")
                        .HasComment("Event create date.");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uniqueidentifier")
                        .HasComment("Identifier of the event");

                    b.Property<Guid?>("IdentityId")
                        .HasColumnType("uniqueidentifier")
                        .HasComment("Identity owner of the event.");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasComment("Name of the event.");

                    b.Property<int?>("PartnerId")
                        .HasColumnType("int")
                        .HasComment("Partner identifier where the event comming from (if null is internal system).");

                    b.Property<int>("Retry")
                        .HasColumnType("int")
                        .HasComment("Retry attempt for this event.");

                    b.Property<int>("ServiceId")
                        .HasColumnType("int")
                        .HasComment("Service identifier.");

                    b.Property<string>("SourceId")
                        .HasColumnType("nvarchar(450)")
                        .HasComment("Primary key for the entity is unique for all the system.");

                    b.Property<long>("Version")
                        .HasColumnType("bigint")
                        .HasComment("Event version number alwais is incremental for the same SourceId.");

                    b.Property<string>("WorkerId")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasComment("Worker identifier where the event was generate.");

                    b.HasKey("Id");

                    b.HasIndex("CorrelationId");

                    b.HasIndex("Created");

                    b.HasIndex("EventId");

                    b.HasIndex("Name");

                    b.HasIndex("SourceId");

                    b.ToTable("SaleforceComplete");
                });

            modelBuilder.Entity("SoftUnlimit.Cloud.Partner.Data.Model.SaleforcePending", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Body")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Body of the event serialized as json.");

                    b.Property<string>("CorrelationId")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasComment("Correlation of the event, indicate the trace were the event was generate.");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2")
                        .HasComment("Event create date.");

                    b.Property<Guid>("EventId")
                        .HasColumnType("uniqueidentifier")
                        .HasComment("Identifier of the event");

                    b.Property<Guid?>("IdentityId")
                        .HasColumnType("uniqueidentifier")
                        .HasComment("Identity owner of the event.");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasComment("Name of the event.");

                    b.Property<int?>("PartnerId")
                        .HasColumnType("int")
                        .HasComment("Partner identifier where the event comming from (if null is internal system).");

                    b.Property<int>("Retry")
                        .HasColumnType("int")
                        .HasComment("Retry attempt for this event.");

                    b.Property<DateTime>("Scheduler")
                        .HasColumnType("datetime2")
                        .HasComment("Scheduler time popone this event.");

                    b.Property<int>("ServiceId")
                        .HasColumnType("int")
                        .HasComment("Service identifier.");

                    b.Property<string>("SourceId")
                        .HasColumnType("nvarchar(450)")
                        .HasComment("Primary key for the entity is unique for all the system.");

                    b.Property<long>("Version")
                        .HasColumnType("bigint")
                        .HasComment("Event version number alwais is incremental for the same SourceId.");

                    b.Property<string>("WorkerId")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasComment("Worker identifier where the event was generate.");

                    b.HasKey("Id");

                    b.HasIndex("CorrelationId");

                    b.HasIndex("Created");

                    b.HasIndex("EventId");

                    b.HasIndex("Name");

                    b.HasIndex("SourceId");

                    b.ToTable("SaleforcePending");
                });
#pragma warning restore 612, 618
        }
    }
}
