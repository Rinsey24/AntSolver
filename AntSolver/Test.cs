using Microsoft.EntityFrameworkCore;
using AntSolver.Data;
using AntSolver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntSolver;

namespace AntSolver
{
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
                Console.WriteLine("Database created successfully.");
                
                context.PipeConnections.RemoveRange(context.PipeConnections);
                context.NodeRequirements.RemoveRange(context.NodeRequirements);
                await context.SaveChangesAsync();
                Console.WriteLine("Existing data cleared.");
                
                var nodes = new[]
                {
                    new NodeRequirement { Node = 1, LiquidNeeded = -1 }, // Root
                    new NodeRequirement { Node = 2, LiquidNeeded = -1 }, // Intermediate
                    new NodeRequirement { Node = 3, LiquidNeeded = 10 },
                    new NodeRequirement { Node = 4, LiquidNeeded = 20 },
                    new NodeRequirement { Node = 5, LiquidNeeded = 5 }
                };
                context.NodeRequirements.AddRange(nodes);
                
                var connections = new[]
                {
                    new PipeConnection { FromNode = 1, ToNode = 2, Percentage = 50.0, SuperPipe = false },
                    new PipeConnection { FromNode = 1, ToNode = 3, Percentage = 50.0, SuperPipe = true },
                    new PipeConnection { FromNode = 2, ToNode = 4, Percentage = 100.0, SuperPipe = false },
                    new PipeConnection { FromNode = 2, ToNode = 5, Percentage = 100.0, SuperPipe = false }
                };
                context.PipeConnections.AddRange(connections);

                await context.SaveChangesAsync();
                Console.WriteLine("Тестовые данные успешно добавлены.");

                var storedConnections = await context.PipeConnections.ToListAsync();
                var storedRequirements = await context.NodeRequirements.ToListAsync();

                Console.WriteLine("\nСохраненные соединения между трубами:");
                foreach (var conn in storedConnections)
                {
                    Console.WriteLine($"От: {conn.FromNode}, Кому: {conn.ToNode}, Процент: {conn.Percentage}, SuperPipe: {conn.SuperPipe}");
                }

                Console.WriteLine("\nStored Node Requirements:");
                foreach (var req in storedRequirements)
                {
                    Console.WriteLine($"Узел: {req.Node},Жидкости нужно: {req.LiquidNeeded}");
                }
                
                var n = storedRequirements.Count;
                var adjList = new List<PipeConnection>[n + 1];
                for (int i = 0; i <= n; i++)
                    adjList[i] = new List<PipeConnection>();
                foreach (var conn in storedConnections)
                {
                    adjList[conn.FromNode].Add(conn);
                }
                var liquidNeeded = storedRequirements.OrderBy(r => r.Node).Select(r => r.LiquidNeeded).ToArray();
                
                double result = Program.Solve(adjList, liquidNeeded);
                Console.WriteLine($"Минимально необходимый поток: {result:F3}");
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}