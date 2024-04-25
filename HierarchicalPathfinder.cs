using ConsoleApp1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public static class HierarchicalPathfinder
{

    //public static LinkedList<Edge> FindHierarchicalPath(Graph graph, GridTile start, GridTile dest, out float path_weight,out TimeSpan running_time)
    public static LinkedList<Edge> FindHierarchicalPath(Graph graph, GridTile start, GridTile dest, out float path_weight, out float running_time)
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        Node nStart, nDest;
        path_weight = 0;        

        //1. Insert nodes，分层找start和dest
        graph.InsertNodes(start, dest, out nStart, out nDest);
        if (nStart == null || nDest == null)
        {
            Console.WriteLine("Start or Dest is not accessible point");
            sw.Stop();
            running_time = 0;
            return null;
        }

        LinkedList<Edge> path;
        //2. search for path in the highest level
        path = Pathfinder.FindPath(nStart, nDest, out path_weight); //nStart，nDest是C[1][0]中的node和C[1][3]中的node


        //3. Remove all created nodes from the graph
        graph.RemoveAddedNodes();
        sw.Stop();
        //running_time = sw.Elapsed;
        //running_time = sw.ElapsedMilliseconds;
        if (sw.ElapsedMilliseconds > 0)
            running_time = sw.ElapsedMilliseconds;
        else
            running_time = (float)sw.Elapsed.Microseconds / 1000;
        return path;
    }

    //public static LinkedList<Edge> FindRegionPath(Graph_New graph, GridTile start, GridTile dest,out float path_weight,out TimeSpan running_time,out int same_region,out int same_region_type)
    public static LinkedList<Edge> FindRegionPath(Graph_New graph, GridTile start, GridTile dest, out float path_weight, out float running_time, out int same_region, out int same_region_type)

    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        Node nStart, nDest;
        path_weight = 0;
        
        Region rStart, rDest;
        Cluster_New cStart, cDest;
        bool existed = graph.GetNodeRegion(start, dest, out rStart, out rDest);
        if (!existed)
        {
            Console.WriteLine("Start or Dest is not accessible point");
            sw.Stop();
            running_time = 0;
            same_region = 0;
            same_region_type = 0;
            return null;
        }
        LinkedList<Edge> path=new LinkedList<Edge>();
        if (rStart.Region_ID == rDest.Region_ID)
        {
            same_region = 1;
            if (rStart.Type != -1)
            {
                same_region_type = 1;
                nStart = graph.nodes[start];
                nDest = graph.nodes[dest];
                Edge e = Pathfinder.FindStaightPath(nStart, nDest, rStart);
                path.AddLast(e);
                path_weight = e.weight;
            }
            else
            {
                same_region_type = 0;
                nStart = graph.nodes[start];
                nDest = graph.nodes[dest];
                Boundaries b = new Boundaries(rStart.minX, rStart.minY, rStart.maxX, rStart.maxY);
                path = Pathfinder.FindPath(nStart, nDest, out path_weight, b);
            }
        }
        else
        {
            same_region = 0;
            same_region_type = 0;
            Node newStart, newDest;
            graph.InsertNodesNew(start, dest, rStart, rDest, out newStart, out newDest);
            path = Pathfinder.FindPath(newStart, newDest, out path_weight);
            graph.RemoveAddedNodes();
        }
        sw.Stop();
        if (sw.ElapsedMilliseconds > 0)
            running_time = sw.ElapsedMilliseconds;
        else
            running_time = (float)sw.Elapsed.Microseconds / 1000;
        return path;
    }

    //public static LinkedList<Edge> FindLowlevelPath(Graph graph, GridTile start, GridTile dest, out float path_weight,out TimeSpan running_time)
    public static LinkedList<Edge> FindLowlevelPath(Graph graph, GridTile start, GridTile dest, out float path_weight,out float running_time)
    {
        Stopwatch sw = Stopwatch.StartNew();
        sw.Start();
        graph.nodes.TryGetValue(start, out Node nStart);
        graph.nodes.TryGetValue(dest, out Node nDest);
        path_weight = 0;
        if (nStart == null || nDest == null)
        {
            Console.WriteLine("Start or Dest is not accessible point");
            sw.Stop();
            running_time = 0;
            return null;
        }
        LinkedList<Edge> path = Pathfinder.FindPath(nStart, nDest, out path_weight);
        sw.Stop();
        if (sw.ElapsedMilliseconds > 0)
            running_time = sw.ElapsedMilliseconds;
        else
            running_time = (float)sw.Elapsed.Microseconds / 1000;
        return path;
    }
}