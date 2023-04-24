using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Commentus_web.Models;

public partial class TestContext : DbContext
{
    public TestContext()
    {
    }

    public TestContext(DbContextOptions<TestContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomsMember> RoomsMembers { get; set; }

    public virtual DbSet<RoomsMessage> RoomsMessages { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<TasksSolver> TasksSolvers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySQL("server=localhost;uid=root;pwd=;database=test;Convert Zero Datetime=True");
        optionsBuilder.EnableSensitiveDataLogging(true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rooms");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<RoomsMember>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("rooms_members");

            entity.HasIndex(e => e.RoomId, "Room_id");

            entity.HasIndex(e => e.UserId, "idx_userid");

            entity.Property(e => e.RoomId)
                .HasColumnType("int(11)")
                .HasColumnName("Room_id");
            entity.Property(e => e.UserId)
                .ValueGeneratedOnAdd()
                .HasColumnType("int(11)")
                .HasColumnName("User_id");

            entity.HasOne(d => d.Room).WithMany()
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("rooms_members_ibfk_2");

            entity.HasOne(d => d.User).WithMany()
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("rooms_members_ibfk_1");
        });

        modelBuilder.Entity<RoomsMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("rooms_messages");

            entity.HasIndex(e => e.UserId, "User_id");

            entity.HasIndex(e => e.RoomId, "idx_roomid");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.RoomId)
                .HasColumnType("int(11)")
                .HasColumnName("Room_id");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("'current_timestamp()'")
                .HasColumnType("timestamp")
                .HasColumnName("timestamp");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("User_id");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomsMessages)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("rooms_messages_ibfk_2");

            entity.HasOne(d => d.User).WithMany(p => p.RoomsMessages)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("rooms_messages_ibfk_1");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tasks");

            entity.HasIndex(e => e.RoomsId, "Rooms_id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.DueDate).HasColumnType("date");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.RoomsId)
                .HasColumnType("int(11)")
                .HasColumnName("Rooms_id");
            entity.Property(e => e.Timestamp)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("'current_timestamp()'")
                .HasColumnType("timestamp")
                .HasColumnName("timestamp");

            entity.HasOne(d => d.Rooms).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.RoomsId)
                .HasConstraintName("tasks_ibfk_1");
        });

        modelBuilder.Entity<TasksSolver>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tasks_solvers");

            entity.HasIndex(e => e.TaskId, "Task_id");

            entity.HasIndex(e => e.UserId, "User_id");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.TaskId)
                .HasColumnType("int(11)")
                .HasColumnName("Task_id");
            entity.Property(e => e.UserId)
                .HasColumnType("int(11)")
                .HasColumnName("User_id");

            entity.HasOne(d => d.Task).WithMany(p => p.TasksSolvers)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("tasks_solvers_ibfk_1");

            entity.HasOne(d => d.User).WithMany(p => p.TasksSolvers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("tasks_solvers_ibfk_2");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Password).HasColumnType("blob");
            entity.Property(e => e.ProfilePicture).HasDefaultValueSql("'NULL'");
            entity.Property(e => e.Salt).HasColumnType("blob");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
