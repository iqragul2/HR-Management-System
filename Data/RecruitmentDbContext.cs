using System;
using System.Collections.Generic;
using HRGroup.Models;
using Microsoft.EntityFrameworkCore;

namespace HRGroup.Data;

public partial class RecruitmentDbContext : DbContext
{
    public RecruitmentDbContext()
    {
    }

    public RecruitmentDbContext(DbContextOptions<RecruitmentDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AboutU> AboutUs { get; set; }

    public virtual DbSet<Applicant> Applicants { get; set; }

    public virtual DbSet<ApplicantVacancy> ApplicantVacancies { get; set; }

    public virtual DbSet<ContactU> ContactUs { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Interview> Interviews { get; set; }

    public virtual DbSet<RecruiterRequest> RecruiterRequests { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vacancy> Vacancies { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AboutU>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AboutUs__3214EC0715BC81C6");

            entity.Property(e => e.FacebookUrl).HasMaxLength(200);
            entity.Property(e => e.ImagePath).HasMaxLength(200);
            entity.Property(e => e.InstagramUrl).HasMaxLength(200);
            entity.Property(e => e.LinkedInUrl).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Position).HasMaxLength(100);
            entity.Property(e => e.TwitterUrl).HasMaxLength(200);
        });

        modelBuilder.Entity<Applicant>(entity =>
        {
            entity.HasKey(e => e.ApplicantId).HasName("PK__Applican__39AE91A8E562F4AD");

            entity.HasIndex(e => e.Email, "UQ__Applican__A9D10534DD84390B").IsUnique();

            entity.Property(e => e.ApplicantId).HasMaxLength(20);
            entity.Property(e => e.AppliedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Not In Process");
        });

        modelBuilder.Entity<ApplicantVacancy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Applican__3214EC07447C67D0");

            entity.ToTable("ApplicantVacancy");

            entity.Property(e => e.ApplicantId).HasMaxLength(20);
            entity.Property(e => e.AttachedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.InterviewScheduledDate).HasColumnType("datetime");
            entity.Property(e => e.IsInterviewScheduled).HasDefaultValue(false);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Interview Scheduled");
            entity.Property(e => e.VacancyId).HasMaxLength(20);

            entity.HasOne(d => d.Applicant).WithMany(p => p.ApplicantVacancies)
                .HasForeignKey(d => d.ApplicantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicant__Appli__6E01572D");

            entity.HasOne(d => d.Interviewer).WithMany(p => p.ApplicantVacancies)
                .HasForeignKey(d => d.InterviewerId)
                .HasConstraintName("FK__Applicant__Inter__6FE99F9F");

            entity.HasOne(d => d.Vacancy).WithMany(p => p.ApplicantVacancies)
                .HasForeignKey(d => d.VacancyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicant__Vacan__6EF57B66");
        });

        modelBuilder.Entity<ContactU>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ContactU__3214EC07FFD3B95E");

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.From)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Subject)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK__Departme__B2079BED2AB68C95");

            entity.HasIndex(e => e.Name, "UQ__Departme__737584F6743C7A83").IsUnique();

            entity.Property(e => e.DepartmentId).HasMaxLength(10);
            entity.Property(e => e.ImageUrl).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04F11D1ABA727");

            entity.HasIndex(e => e.UserId, "UQ__Employee__1788CC4D6D980E24").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.DepartmentId).HasMaxLength(10);
            entity.Property(e => e.Designation).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(20);
            entity.Property(e => e.JoinedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.ProfileCompleted).HasDefaultValue(false);

            entity.HasOne(d => d.Department).WithMany(p => p.Employees)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__Employees__Depar__5812160E");

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employees__UserI__571DF1D5");
        });

        modelBuilder.Entity<Interview>(entity =>
        {
            entity.HasKey(e => e.InterviewId).HasName("PK__Intervie__C97C585244C81B55");

            entity.Property(e => e.Result)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.ScheduledDate).HasColumnType("datetime");

            entity.HasOne(d => d.ApplicantVacancy).WithMany(p => p.Interviews)
                .HasForeignKey(d => d.ApplicantVacancyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Interview__Appli__73BA3083");

            entity.HasOne(d => d.Interviewer).WithMany(p => p.Interviews)
                .HasForeignKey(d => d.InterviewerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Interview__Inter__74AE54BC");
        });

        modelBuilder.Entity<RecruiterRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__Recruite__33A8517A51B7EC86");

            entity.Property(e => e.ContactInfo).HasMaxLength(150);
            entity.Property(e => e.DepartmentId).HasMaxLength(10);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.RequestDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Department).WithMany(p => p.RecruiterRequests)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK__Recruiter__Depar__5DCAEF64");

            entity.HasOne(d => d.User).WithMany(p => p.RecruiterRequests)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Recruiter__UserI__5CD6CB2B");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A4E533AA4");

            entity.HasIndex(e => e.RoleName, "UQ__Roles__8A2B6160F74402D7").IsUnique();

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CF3B83519");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053478E3EB50").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(150);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__RoleId__4F7CD00D");
        });

        modelBuilder.Entity<Vacancy>(entity =>
        {
            entity.HasKey(e => e.VacancyId).HasName("PK__Vacancie__6456763F671B82A8");

            entity.Property(e => e.VacancyId).HasMaxLength(20);
            entity.Property(e => e.ClosingDate).HasColumnType("datetime");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepartmentId).HasMaxLength(10);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Open");
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Vacancies)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Vacancies__Creat__68487DD7");

            entity.HasOne(d => d.Department).WithMany(p => p.Vacancies)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Vacancies__Depar__6754599E");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
