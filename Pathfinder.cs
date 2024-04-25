using ConsoleApp1;
using Priority_Queue;
using System;
using System.Collections.Generic;

public class Pathfinder
{

    public static float EuclidianDistance(Node node1, Node node2)
    {
        return EuclidianDistance(node1.pos, node2.pos);
    }

    public static float EuclidianDistance(GridTile tile1, GridTile tile2)
    {
        return (float)Math.Sqrt(Math.Pow(tile2.x - tile1.x, 2) + Math.Pow(tile2.y - tile1.y, 2));
    }

    public static LinkedList<Edge> FindPath(Node start, Node dest, out float weight, Boundaries boundaries = null)
    {
        HashSet<GridTile> Visited = new HashSet<GridTile>(); //无顺序的非重复list，已访问节点列表，closed
        Dictionary<GridTile, Edge> Parent = new Dictionary<GridTile, Edge>(); // parent：来的路
        Dictionary<GridTile, float> gScore = new Dictionary<GridTile, float>(); // gscroe：每个节点的score

        SimplePriorityQueue<Node, float> pq = new SimplePriorityQueue<Node, float>(); //open

        float temp_gCost, prev_gCost;

        gScore[start.pos] = 0;
        pq.Enqueue(start, EuclidianDistance(start, dest));
        Node current;
        weight = 0;
        while (pq.Count > 0)
        {
            current = pq.Dequeue();
                if (current.pos.Equals(dest.pos))
                    //Rebuild path and return it
                    return RebuildPath(Parent, current, out weight); // 返回A*Path


                Visited.Add(current.pos);

            //Visit all neighbours through edges going out of node
            foreach (Edge e in current.edges) // edges包含周围八方向可走的路径及每条路径的weight，这里的weight是否可以在第一次创建的时候增加节点权值？
                                              // 上一层用的是下一层的edge。到了第二层，这里只有两个edge：(8,1)->(9,1);(8,1)->(7,2)
            {
                //If we defined boundaries, check if it crosses it
                if (boundaries != null && IsOutOfGrid(e.end.pos, boundaries))
                    continue;

                //Check if we visited the outer end of the edge
                if (Visited.Contains(e.end.pos))
                    continue;

                temp_gCost = gScore[current.pos] + e.weight; // temp_gCost=当前点到起点的cost+当前边的weight

                //If new value is not better then do nothing
                if (gScore.TryGetValue(e.end.pos, out prev_gCost) && temp_gCost >= prev_gCost) //gScore中有这个节点并且这个节点之前的weight小于本次temp_gCost，跳过
                    continue;

                //Otherwise store the new value and add the destination into the queue
                Parent[e.end.pos] = e; // 当前edge 终点的parent是当前edge
                gScore[e.end.pos] = temp_gCost; //当前edge终点的gScore是当前点到起点的cost+当前边的weight

                pq.Enqueue(e.end, temp_gCost + EuclidianDistance(e.end, dest)); // f=g+h，h是当前edge end到目标点的直线距离

            }
        }

        return new LinkedList<Edge>();//未找到路径
    }

    public static bool IsOutOfGrid(GridTile pos, Boundaries boundaries)
    {
        return (pos.x < boundaries.Min.x || pos.x > boundaries.Max.x) ||
               (pos.y < boundaries.Min.y || pos.y > boundaries.Max.y);
    }

    //Rebuild edges
    public static LinkedList<Edge> RebuildPath(Dictionary<GridTile, Edge> Parent, Node dest,out float weight)
    {
        LinkedList<Edge> res = new LinkedList<Edge>();
        GridTile current = dest.pos;
        Edge e = null;

        weight = 0;

        while (Parent.TryGetValue(current, out e))
        {
            res.AddFirst(e);
            current = e.start.pos;
            weight += e.weight;
        }

        return res;
    }

    public static LinkedList<GridTile> straghtLine(int x, int y, int x2, int y2, Region r, int region_flag,Boundaries boundaries=null)
    {
        LinkedList<GridTile> list = new LinkedList<GridTile>();
        int w = x2 - x;
        int h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Math.Abs(w);
        int shortest = Math.Abs(h);
        if (!(longest > shortest))
        {
            longest = Math.Abs(h);
            shortest = Math.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            GridTile g = new GridTile(x, y);
            //if (r.GridInRegion(g))
            if (region_flag == 0)
            {
                if (boundaries != null && IsOutOfGrid(g, boundaries))
                {
                    list.AddLast(g);
                }

                else
                {
                    return null;
                }
            }
            else
            {
                if (r.GridInRegion(g))
                    {
                    list.AddLast(g);
                }
                else
                {
                    return null;
                }
            }
            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }
        return list;
    }

    /*public static LinkedList<Edge> FindStaightPath(Node start, Node dest, Region r, out float weight, int region_flag, Boundaries boundaries = null)
    {
        weight = 0;
        LinkedList<GridTile> line = straghtLine(start.pos.x, start.pos.y, dest.pos.x, dest.pos.y,r, region_flag, boundaries);
        if (line != null)
        {
            return BuildStraightPath(line, r, r.Type, out weight);
        }
        return new LinkedList<Edge>();
    }*/
    public static Edge FindStaightPath(Node start, Node dest, Region r)
    {
        Edge e = new Edge();
        e.start = start;
        e.end = dest;
        e.weight = EuclidianDistance(start.pos, dest.pos) * r.Type;
        return e;
    }

    public static LinkedList<Edge> BuildStraightPath(LinkedList<GridTile> line, Region r, int avg_weight, out float weight)
    {
        //LinkedList<Edge> result = new LinkedList<Edge>();
        LinkedList<Edge> res = new LinkedList<Edge>();
        LinkedListNode<GridTile> g = line.First;
        //Edge e = null;
        weight = 0;
        while (g.Next != null)
        {
            Edge e = new Edge();
            e.start = r.AllNodes[g.Value];
            e.end = r.AllNodes[g.Next.Value];
            e.weight = EuclidianDistance(g.Value, g.Next.Value);
            weight += e.weight;
            res.AddLast(e);
            g = g.Next;
        }
        weight *= avg_weight;
        return res;
    }
}
