﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using TrackableEntities.Core.Sample.XPlat.WebApi;

namespace TrackableEntities.Core.Sample.XPlat.WebApi.Migrations
{
    [DbContext(typeof(NorthwindSlimContext))]
    partial class NorthwindSlimContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("NetCoreSample.Entities.WebApi.Category", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CategoryName");

                    b.HasKey("CategoryId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("NetCoreSample.Entities.WebApi.Customer", b =>
                {
                    b.Property<string>("CustomerId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("City");

                    b.Property<string>("CompanyName");

                    b.Property<string>("ContactName");

                    b.Property<string>("Country");

                    b.HasKey("CustomerId");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("NetCoreSample.Entities.WebApi.Order", b =>
                {
                    b.Property<int>("OrderId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CustomerId");

                    b.Property<decimal?>("Freight");

                    b.Property<DateTime?>("OrderDate");

                    b.Property<int?>("ShipVia");

                    b.Property<DateTime?>("ShippedDate");

                    b.HasKey("OrderId");

                    b.HasIndex("CustomerId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("NetCoreSample.Entities.WebApi.OrderDetail", b =>
                {
                    b.Property<int>("OrderDetailId")
                        .ValueGeneratedOnAdd();

                    b.Property<float>("Discount");

                    b.Property<int>("OrderId");

                    b.Property<int>("ProductId");

                    b.Property<short>("Quantity");

                    b.Property<decimal>("UnitPrice");

                    b.HasKey("OrderDetailId");

                    b.HasIndex("OrderId");

                    b.HasIndex("ProductId");

                    b.ToTable("OrderDetails");
                });

            modelBuilder.Entity("NetCoreSample.Entities.WebApi.Product", b =>
                {
                    b.Property<int>("ProductId")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("CategoryId");

                    b.Property<bool>("Discontinued");

                    b.Property<string>("ProductName");

                    b.Property<byte[]>("RowVersion");

                    b.Property<decimal?>("UnitPrice");

                    b.HasKey("ProductId");

                    b.HasIndex("CategoryId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("NetCoreSample.Entities.WebApi.Order", b =>
                {
                    b.HasOne("NetCoreSample.Entities.WebApi.Customer", "Customer")
                        .WithMany("Orders")
                        .HasForeignKey("CustomerId");
                });

            modelBuilder.Entity("NetCoreSample.Entities.WebApi.OrderDetail", b =>
                {
                    b.HasOne("NetCoreSample.Entities.WebApi.Order", "Order")
                        .WithMany("OrderDetails")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("NetCoreSample.Entities.WebApi.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NetCoreSample.Entities.WebApi.Product", b =>
                {
                    b.HasOne("NetCoreSample.Entities.WebApi.Category", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId");
                });
#pragma warning restore 612, 618
        }
    }
}
