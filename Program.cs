using ConsoleApp1;
using System.Collections.Generic;
using System.Diagnostics;

public class HPA
{
    public static void Main()
    {
        HPARun();
        //NewGraphTest();
    }

    private static void HPARun()
    {
        string path = "D:\\temp\\result.txt";
        using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
        using (StreamWriter sw = new StreamWriter(fs))
        {
            List<FileInfo> maps = Map.GetMaps();
            FileInfo filename = maps[1];
            List<FileInfo> tests = Map.GetTestScens();
            FileInfo test_filename = tests[1];
            //List<TestCase> test_cases = Benchmark.ReadNewTestCases(test_filename);
            List<TestCase> test_cases = Benchmark.ReadTestCases(test_filename);
            string FullName = filename.FullName;
            Console.WriteLine(FullName);
            Map map = Map.LoadMap(FullName);
            float deltaT;
            int LayerDepth = 1;
            int ClusterSize = 10;

            Graph graph1 = RunGenerateGraph(map, LayerDepth, ClusterSize, out deltaT);
            sw.WriteLine("HPA Map loaded,time is:" + deltaT);
            int hpa_node_number = 0;
            int hpa_edge_number = 0;
            foreach(Cluster c in graph1.C[0])
            {
                hpa_node_number += c.Nodes.Count;
                foreach(Node n in c.Nodes.Values)
                {
                    hpa_edge_number += n.edges.Count;
                }
            }
            sw.WriteLine("HPA Map nodes number:" + hpa_node_number);
            sw.WriteLine("HPA Map edges number:" + hpa_edge_number);

            Graph_New graph2 = RunGenerateNewGraph(map, ClusterSize, out deltaT);
            sw.WriteLine("New Map loaded,time is:" + deltaT);

            int new_node_number = 0;
            int new_edge_number = 0;
            foreach (Region r in graph2.R.Values)
            {
                new_node_number += r.BoundaryNodes.Count;
                foreach (Node n in r.BoundaryNodes.Values)
                {
                    new_edge_number += n.edges.Count;
                }
            }
            sw.WriteLine("New Map nodes number:" + new_node_number);
            sw.WriteLine("New Map edges number:" + new_edge_number);

            foreach(TestCase tc in test_cases)
            {
                GridTile start = tc.Start;
                GridTile dest = tc.destination;
                TestResult result = RunPathfind(graph1, start, dest);
                int same_region;
                int same_region_type;
                TestResult result2 = RunRegionPathfind(graph2, start, dest,out same_region,out same_region_type);
                sw.WriteLine("(" + start.x + "," + start.y + ");(" + dest.x + "," + dest.y + ");" + result.HPAStarResult.RunningTime + ";" + result.HPAStarResult.PathLength + ";" + result.AStarResult.RunningTime + ";" + result.AStarResult.PathLength + ";" + result2.RegionResult.RunningTime + ";" + result2.RegionResult.PathLength + ";" + same_region + ";" + same_region_type);

            }
        }
        Console.ReadLine();
    }

    private static void NewGraphTest()
    {
        List<FileInfo> maps = Map.GetMaps();
        FileInfo filename = maps[2];
        string FullName = filename.FullName;
        Console.WriteLine(FullName);
        Map map = Map.LoadMap(FullName);
        Graph_New graph = new Graph_New(map, 10);
        int new_node_number = 0;
        int new_edge_number = 0;
        foreach (Region r in graph.R.Values)
        {
            new_node_number += r.BoundaryNodes.Count;
            foreach (Node n in r.BoundaryNodes.Values)
            {
                new_edge_number += n.edges.Count;
            }
        }
        Console.WriteLine("New Map nodes number:" + new_node_number);
        Console.WriteLine("New Map edges number:" + new_edge_number);
    }


    private static Graph RunGenerateGraph(Map map, int LayerDepth, int ClusterSize, out float deltatime)
    {
        Stopwatch sw = Stopwatch.StartNew();
        Graph graph = new Graph(map, LayerDepth, ClusterSize);
        sw.Stop();
        deltatime = sw.ElapsedMilliseconds;
        return graph;
    }

    private static Graph_New RunGenerateNewGraph(Map map, int ClusterSize, out float deltatime)
    {
        Stopwatch sw = Stopwatch.StartNew();
        Graph_New graph = new Graph_New(map, ClusterSize);
        sw.Stop();
        deltatime = sw.ElapsedMilliseconds;
        return graph;
    }

    private static TestResult RunPathfind(Graph graph,GridTile start, GridTile dest)
    {
        TestResult result = new TestResult();

        PathfindResult res = new PathfindResult();
        float hpa_weight;
        //TimeSpan running_time;
        float running_time;
        res.Path = HierarchicalPathfinder.FindHierarchicalPath(graph, start, dest, out hpa_weight, out running_time);

        //res.RunningTime = (float)running_time.Microseconds / 1000;
        res.RunningTime = running_time;
        res.PathLength = hpa_weight;
        
        result.HPAStarResult = res;

        res = new PathfindResult();
        float a_weight;
        //TimeSpan a_running_time;
        float a_running_time;
        res.Path = HierarchicalPathfinder.FindLowlevelPath(graph, start, dest,out a_weight, out a_running_time);

        //res.RunningTime = (float)a_running_time.Microseconds / 1000;
        res.RunningTime = a_running_time;
        res.PathLength = a_weight;
        result.AStarResult = res;

        return result;
    }

    private static TestResult RunRegionPathfind(Graph_New graph, GridTile start, GridTile dest, out int same_region, out int same_region_type)
    {
        TestResult result = new TestResult();

        PathfindResult res = new PathfindResult();

        float region_weight;
        //TimeSpan running_time;
        float running_time;
        res.Path = HierarchicalPathfinder.FindRegionPath(graph, start, dest,out region_weight,out running_time,out same_region,out same_region_type);


        //res.RunningTime = (float)running_time.Microseconds / 1000;
        res.RunningTime = running_time;
        res.PathLength = region_weight;

        result.RegionResult = res;

        return result;
    }
}