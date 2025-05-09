using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using DentalNUB.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

#nullable disable

namespace DentalNUB.Entities.Models
{
    public partial class DBContext : IdentityDbContext<AdminUser>
    {

        private IConfiguration Configuration;

        public DBContext(IConfiguration _configuration)
        {
            Configuration = _configuration;
        }

        public DBContext(DbContextOptions<DbContext> options)
        : base(options)
        {
        }

        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<PatientCase> PatientCases { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatNUB> ChatNUBs { get; set; }
        public DbSet<Clinic> Clinics { get; set; }
        public DbSet<ClinicSection> ClinicSections { get; set; }
        public DbSet<Consultant> Consultants { get; set; }
        public DbSet<Diagnose> Diagnoses { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<DoctorSectionRanking> DoctorSectionRankings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connString = this.Configuration.GetConnectionString("DBConnection");
                
                optionsBuilder.UseSqlServer(connString);
                optionsBuilder.EnableSensitiveDataLogging();
            }
        }
    }
}
