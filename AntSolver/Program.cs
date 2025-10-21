using Microsoft.EntityFrameworkCore;
using AntSolver.Data;
using AntSolver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PipeNetwork;
namespace AntSolver;

  public class Program
    {
        
        public static bool CanFeed(int node, double liquid, List<PipeConnection>[] adjList, int[] liquidNeeded)
        {
            if (liquidNeeded[node - 1] != -1)
            {
                return liquid >= liquidNeeded[node - 1];
            }

            bool ok = true;
            foreach (var edge in adjList[node])
            {
                double baseFlow = liquid * edge.Percentage / 100.0;
                double flow = edge.SuperPipe
                    ? (baseFlow >= (liquidNeeded.ElementAtOrDefault(edge.ToNode - 1)) ? baseFlow : baseFlow * baseFlow)
                    : baseFlow;

                if (!CanFeed(edge.ToNode, flow, adjList, liquidNeeded))
                    ok = false;
            }
            return ok;
        }

        
        public static double Solve(List<PipeConnection>[] adjList, int[] liquidNeeded)
        {
            double low = 0.0;
            double high = 2e9;
            while (high - low > 0.00001)
            {
                double mid = (low + high) / 2;
                if (CanFeed(1, mid, adjList, liquidNeeded))
                    high = mid;
                else
                    low = mid;
            }
            return high;
        }

        public static async Task Main()
        {
            await Test.RunTest();
           
            var optionsBuilder = new DbContextOptionsBuilder<PipeNetworkContext>();
            optionsBuilder.UseSqlite("Data Source=pipenetwork.db");
            using var context = new PipeNetworkContext(optionsBuilder.Options);
            await context.Database.EnsureCreatedAsync();

            try
            {
              
                int n = int.Parse(Console.ReadLine() ?? throw new FormatException("Invalid input for number of nodes"));
                if (n <= 0)
                {
                    Console.WriteLine("Ошибка: Ты что пишешь, идиот? Лечись");
                    return;
                }

                var adjList = new List<PipeConnection>[n + 1];
                for (int i = 0; i <= n; i++)
                    adjList[i] = new List<PipeConnection>();

                var edges = new HashSet<(int, int)>();

              
                for (int i = 0; i < n - 1; i++)
                {
                    var input = Console.ReadLine()?.Split(' ').Select(x => int.TryParse(x, out var num) ? num : (int?)null).ToList();
                    if (input == null || input.Count != 4 || input.Any(x => x == null))
                    {
                        Console.WriteLine($"Ошибка: Некорректный формат данных ребра на строке {i + 2}");
                        return;
                    }

                    int from = input[0]!.Value;
                    int to = input[1]!.Value;
                    int p = input[2]!.Value;
                    int s = input[3]!.Value;

                    if (from < 1 || from > n || to < 1 || to > n)
                    {
                        Console.WriteLine($"Ошибка: Узлы {from} или {to} выходят за пределы 1..{n} на строке {i + 2}");
                        return;
                    }
                    if (p < 0 || p > 100)
                    {
                        Console.WriteLine($"Ошибка: Процент {p} должен быть в диапазоне 0..100 на строке {i + 2}");
                        return;
                    }
                    if (s != 0 && s != 1)
                    {
                        Console.WriteLine($"Ошибка: Флаг супертрубки {s} должен быть 0 или 1 на строке {i + 2}");
                        return;
                    }
                    if (edges.Contains((from, to)) || edges.Contains((to, from)))
                    {
                        Console.WriteLine($"Ошибка: Обнаружен цикл или дублирующее ребро между {from} и {to} на строке {i + 2}");
                        return;
                    }

                    edges.Add((from, to));
                    var connection = new PipeConnection { FromNode = from, ToNode = to, Percentage = p, SuperPipe = s == 1 };
                    adjList[from].Add(connection);
                    context.PipeConnections.Add(connection);
                }
                
                var liquidInput = Console.ReadLine()?.Split(' ').Select(x => int.TryParse(x, out var num) ? num : -1).ToList();
                if (liquidInput == null || liquidInput.Count != n)
                {
                    Console.WriteLine($"Ошибка: Количество требований к жидкости ({liquidInput?.Count}) не равно {n}");
                    return;
                }

                var liquidNeeded = liquidInput.ToArray();
                for (int i = 0; i < n; i++)
                {
                    context.NodeRequirements.Add(new NodeRequirement { Node = i + 1, LiquidNeeded = liquidNeeded[i] });
                }

                await context.SaveChangesAsync();

       
                double result = Solve(adjList, liquidNeeded);
                Console.WriteLine($"{result:F3}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
