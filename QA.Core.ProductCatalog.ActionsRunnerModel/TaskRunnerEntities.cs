﻿using Microsoft.EntityFrameworkCore;
 using QA.Core.DPC.QP.Models;
 using QA.Core.DPC.QP.Services;

 namespace QA.Core.ProductCatalog.ActionsRunnerModel
{
    public class TaskRunnerEntities : DbContext
    {
        public TaskRunnerEntities()
        {
        }

        public TaskRunnerEntities(DbContextOptions options)
            : base(options)
        {
        }

        public static TaskRunnerEntities Create(IConnectionProvider provider)
        {
            DbContextOptionsBuilder<TaskRunnerEntities> builder;
            builder = new DbContextOptionsBuilder<TaskRunnerEntities>();
            builder.UseSqlServer(provider.GetConnection(Service.Actions));
            return new TaskRunnerEntities(builder.Options);
        }
        
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<TaskState> TaskStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Task>(entity =>
            {
                entity.HasOne(d => d.SheduledFromTask)
                    .WithMany(p => p.SpawnedTasks)
                    .HasForeignKey(d => d.ScheduledFromTaskID);

                entity.HasOne(d => d.TaskState)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.StateID);

                entity.HasOne(d => d.Schedule)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.ScheduleID);
            });
        }
    }
}
