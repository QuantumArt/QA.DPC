﻿﻿using Microsoft.EntityFrameworkCore;
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

        
        public static TaskRunnerEntities Get(IConnectionProvider provider)
        {
            TaskRunnerEntities result;
            var connectionString = provider.GetConnection(Service.Actions);
                if (provider.UsePostgres)
                {
                    var optionsBuilder = new DbContextOptionsBuilder<NpgSqlTaskRunnerEntities>();
                    optionsBuilder.UseNpgsql(connectionString);
                    result = new NpgSqlTaskRunnerEntities(optionsBuilder.Options);
                }
                else
                {
                    var optionsBuilder = new DbContextOptionsBuilder<SqlServerTaskRunnerEntities>();
                    optionsBuilder.UseSqlServer(connectionString);
                    result = new SqlServerTaskRunnerEntities(optionsBuilder.Options);
                }
            return result;

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
            
            modelBuilder.Entity<TaskState>().HasData(
                new TaskState {ID = 1, Name = "New"},
                new TaskState {ID = 2, Name = "Running"},
                new TaskState {ID = 3, Name = "Completed"},
                new TaskState {ID = 4, Name = "Error"},
                new TaskState {ID = 5, Name = "Cancelled"}
            );           
        }
    }
}
