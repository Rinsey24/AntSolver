namespace AntSolver.Models;

public class PipeConnection
{
    public int Id { get; set; }
    public int FromNode { get; set; }
    public int ToNode { get; set; }
    public double Percentage { get; set; }
    public bool SuperPipe { get; set; }
}
