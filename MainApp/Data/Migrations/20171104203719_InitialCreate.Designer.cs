﻿// <auto-generated />
using JanuszMail.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using System;

namespace JanuszMail.Data.Migrations
{
    [DbContext(typeof(JanuszMailDbContext))]
    [Migration("20171104203719_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("JanuszMail.Models.ProviderParams", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("EmailAdress")
                        .IsRequired();

                    b.Property<int>("ImapPortNumber");

                    b.Property<string>("ImapServerName")
                        .IsRequired();

                    b.Property<string>("Password")
                        .IsRequired();

                    b.Property<int>("SmtpPortNumber");

                    b.Property<string>("SmtpServerName")
                        .IsRequired();

                    b.Property<string>("UserId");

                    b.HasKey("ID");

                    b.ToTable("ProviderParams");
                });
#pragma warning restore 612, 618
        }
    }
}
