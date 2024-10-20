﻿// <auto-generated />
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using SpotifyApi.Entities;

#nullable disable

namespace SpotifyApi.Migrations
{
    [DbContext(typeof(SpotifyDbContext))]
    partial class SpotifyDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PlaylistUser", b =>
                {
                    b.Property<int>("CollaboratingPlaylistsId")
                        .HasColumnType("integer");

                    b.Property<int>("CollaboratorsId")
                        .HasColumnType("integer");

                    b.HasKey("CollaboratingPlaylistsId", "CollaboratorsId");

                    b.HasIndex("CollaboratorsId");

                    b.ToTable("PlaylistCoCreators", (string)null);
                });

            modelBuilder.Entity("PlaylistUser1", b =>
                {
                    b.Property<int>("FavoritePlaylistsId")
                        .HasColumnType("integer");

                    b.Property<int>("FavoritedByUsersId")
                        .HasColumnType("integer");

                    b.HasKey("FavoritePlaylistsId", "FavoritedByUsersId");

                    b.HasIndex("FavoritedByUsersId");

                    b.ToTable("UserFavoritePlaylists", (string)null);
                });

            modelBuilder.Entity("SpotifyApi.Entities.Playlist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("IsPublic")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("OwnerId")
                        .HasColumnType("integer");

                    b.Property<List<int>>("SongIds")
                        .IsRequired()
                        .HasColumnType("integer[]");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Playlists");
                });

            modelBuilder.Entity("SpotifyApi.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("DateOfBirth")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Gender")
                        .HasColumnType("integer");

                    b.Property<string>("Nickname")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Offers")
                        .HasColumnType("integer");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordResetToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("ShareInformation")
                        .HasColumnType("integer");

                    b.Property<int>("Terms")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Nickname")
                        .IsUnique();

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PlaylistUser", b =>
                {
                    b.HasOne("SpotifyApi.Entities.Playlist", null)
                        .WithMany()
                        .HasForeignKey("CollaboratingPlaylistsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SpotifyApi.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("CollaboratorsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("PlaylistUser1", b =>
                {
                    b.HasOne("SpotifyApi.Entities.Playlist", null)
                        .WithMany()
                        .HasForeignKey("FavoritePlaylistsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SpotifyApi.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("FavoritedByUsersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SpotifyApi.Entities.Playlist", b =>
                {
                    b.HasOne("SpotifyApi.Entities.User", "Owner")
                        .WithMany("CreatedPlaylists")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("SpotifyApi.Entities.User", b =>
                {
                    b.Navigation("CreatedPlaylists");
                });
#pragma warning restore 612, 618
        }
    }
}
