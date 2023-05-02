﻿// <auto-generated />
using System;
using AKVN_Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AKVN_Backend.Migrations
{
    [DbContext(typeof(AKVNDBContext))]
    [Migration("20230502195610_Backgrounds")]
    partial class Backgrounds
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.5");

            modelBuilder.Entity("AKVN_Backend.Classes.Actor", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("SceneId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Sprite")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("SceneId");

                    b.ToTable("Actors");
                });

            modelBuilder.Entity("AKVN_Backend.Classes.Background", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Sprite")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Backgrounds");
                });

            modelBuilder.Entity("AKVN_Backend.Classes.Chapter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Chapters");
                });

            modelBuilder.Entity("AKVN_Backend.Classes.Scene", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ChapterId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ChapterId");

                    b.ToTable("Scenes");
                });

            modelBuilder.Entity("AKVN_Backend.Classes.Text", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Dialogue")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("OwnerId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("SceneId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.HasIndex("SceneId");

                    b.ToTable("Texts");
                });

            modelBuilder.Entity("AKVN_Backend.Classes.Actor", b =>
                {
                    b.HasOne("AKVN_Backend.Classes.Scene", null)
                        .WithMany("Actors")
                        .HasForeignKey("SceneId");
                });

            modelBuilder.Entity("AKVN_Backend.Classes.Scene", b =>
                {
                    b.HasOne("AKVN_Backend.Classes.Chapter", null)
                        .WithMany("Scenes")
                        .HasForeignKey("ChapterId");
                });

            modelBuilder.Entity("AKVN_Backend.Classes.Text", b =>
                {
                    b.HasOne("AKVN_Backend.Classes.Actor", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AKVN_Backend.Classes.Scene", null)
                        .WithMany("Texts")
                        .HasForeignKey("SceneId");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("AKVN_Backend.Classes.Chapter", b =>
                {
                    b.Navigation("Scenes");
                });

            modelBuilder.Entity("AKVN_Backend.Classes.Scene", b =>
                {
                    b.Navigation("Actors");

                    b.Navigation("Texts");
                });
#pragma warning restore 612, 618
        }
    }
}
