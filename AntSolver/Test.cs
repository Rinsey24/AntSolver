using Microsoft.EntityFrameworkCore;
using AntSolver.Data;
using AntSolver.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AntSolver;

 public class Test
    {
        public static async Task RunTest()
        {
            try
            {
              
                var optionsBuilder = new DbContextOptionsBuilder<PipeNetworkContext>();
                optionsBuilder.UseSqlite("Data Source=pipenetwork.db");
                using var context = new PipeNetworkContext(optionsBuilder.Options);

            
                await context.Database.EnsureCreatedAsync();
                Console.WriteLine("База данных создана успешно.");

                
                context.PipeConnections.RemoveRange(context.PipeConnections);
                context.NodeRequirements.RemoveRange(context.NodeRequirements);
                await context.SaveChangesAsync();
                Console.WriteLine("Существующая инфа очищена.");

              
                var connection = new PipeConnection
                {
                    FromNode = 1,
                    ToNode = 2,
                    Percentage = 50.0,
                    SuperPipe = true
                };
                context.PipeConnections.Add(connection);


                var requirement = new NodeRequirement
                {
                    Node = 2,
                    LiquidNeeded = 100
                };
                context.NodeRequirements.Add(requirement);

                await context.SaveChangesAsync();
                Console.WriteLine("Тестовые данные добавлены");
                
                var connections = await context.PipeConnections.ToListAsync();
                var requirements = await context.NodeRequirements.ToListAsync();

                Console.WriteLine("\nStored Pipe Connections:");
                foreach (var conn in connections)
                {
                    Console.WriteLine($"From: {conn.FromNode}, To: {conn.ToNode}, Percentage: {conn.Percentage}, SuperPipe: {conn.SuperPipe}");
                }

                Console.WriteLine("\nStored Node Requirements:");
                foreach (var req in requirements)
                {
                    Console.WriteLine($"Node: {req.Node}, Liquid Needed: {req.LiquidNeeded}");
                }

                Console.WriteLine("\nEF Core with SQLite is working correctly!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
