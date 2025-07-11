﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using UniversalBroker.Core.Database.Models.Configurations;
#nullable disable

namespace UniversalBroker.Core.Database.Models;

public partial class BrockerContext : DbContext
{
    public BrockerContext()
    {
    }

    public BrockerContext(DbContextOptions<BrockerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Attribute> Attributes { get; set; }

    public virtual DbSet<Chanel> Chanels { get; set; }

    public virtual DbSet<Communication> Communications { get; set; }

    public virtual DbSet<CommunicationAttribute> CommunicationAttributes { get; set; }

    public virtual DbSet<Connection> Connections { get; set; }

    public virtual DbSet<ConnectionAttribute> ConnectionAttributes { get; set; }

    public virtual DbSet<ExecutionLog> ExecutionLogs { get; set; }

    public virtual DbSet<Header> Headers { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Script> Scripts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new Configurations.AttributeConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ChanelConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.CommunicationConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.CommunicationAttributeConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ConnectionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ConnectionAttributeConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ExecutionLogConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.HeaderConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.MessageConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.ScriptConfiguration());

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
