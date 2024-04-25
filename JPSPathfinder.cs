using ConsoleApp1;
using Priority_Queue;


public class JPSPathfinder
{

    public static float EuclidianDistance(Node node1, Node node2)
    {
        return EuclidianDistance(node1.pos, node2.pos);
    }


    public static float EuclidianDistance(GridTile tile1, GridTile tile2)
    {
        return (float)Math.Sqrt(Math.Pow(tile2.x - tile1.x, 2) + Math.Pow(tile2.y - tile1.y, 2));
    }

    public static bool blocked(int cX, int cY, int dX, int dY, Region r)
    {
        GridTile g1 = new GridTile(cX + dX, cY + dY);
        if (!r.GridInRegion(g1))
            return true;
        if (dX != 0 && dY != 0)
        {
            GridTile hori_g = new GridTile(cX + dX, cY);
            GridTile vert_g = new GridTile(cX, cY + dY);
            if (!r.GridInRegion(hori_g) && !r.GridInRegion(vert_g))
                return true;
        }
        return false;
    }

    public static bool dblock(int cX, int cY, int dX, int dY, Region r)
    {
        GridTile hori_g = new GridTile(cX - dX, cY);
        GridTile vert_g = new GridTile(cX, cY - dY);
        if (!r.GridInRegion(hori_g) && !r.GridInRegion(vert_g))
            return true;
        else
            return false;
    }

    public static void direction(int cX, int cY, int pX, int pY, out int dX, out int dY)
    {
        dX = (int)Math.CopySign(1, cX - pX);
        dY = (int)Math.CopySign(1, cY - pY);
        if (cX - pX == 0)
            dX = 0;
        if (cY - pY == 0)
            dY = 0;
    }

    public static List<Node> nodeNeighbours(int cX, int cY, Node parent, Region r)
    {
        List<Node> neighbours = new List<Node>();
        if (parent == null)
        {
            GridTile g = new GridTile(cX, cY);
            Node n = r.AllNodes[g];
            foreach (Edge e in n.edges)
            {
                if (r.GridInRegion(e.end.pos))
                    neighbours.Add(r.AllNodes[e.end.pos]);
            }
            return neighbours;
        }
        int dX, dY;
        direction(cX, cY, parent.pos.x, parent.pos.y, out dX, out dY);
        if (dX != 0 && dY != 0)
        {
            if (!blocked(cX, cY, 0, dY, r))
                neighbours.Add(r.AllNodes[new GridTile(cX, cY + dY)]);
            if (!blocked(cX, cY, dX, 0, r))
                neighbours.Add(r.AllNodes[new GridTile(cX + dX, cY)]);
            if ((!blocked(cX, cY, 0, dY, r) || !blocked(cX, cY, dX, 0, r)) && !blocked(cX, cY, dX, dY, r))
                neighbours.Add(r.AllNodes[new GridTile(cX + dX, cY + dY)]);
            if (blocked(cX, cY, -dX, 0, r) && !blocked(cX, cY, 0, dY, r) && !blocked(cX, cY, -dX, dY, r))
                neighbours.Add(r.AllNodes[new GridTile(cX - dX, cY + dY)]);
            if (blocked(cX, cY, 0, -dY, r) && !blocked(cX, cY, dX, 0, r) && !blocked(cX, cY, dX, -dY, r))
                neighbours.Add(r.AllNodes[new GridTile(cX + dX, cY - dY)]);
        }

        else
        {
            if (dX == 0)
            {
                if (!blocked(cX, cY, 0, dY, r))
                    neighbours.Add(r.AllNodes[new GridTile(cX, cY + dY)]);
                if (blocked(cX, cY, 1, 0, r) && !blocked(cX, cY, 1, dY, r))
                    neighbours.Add(r.AllNodes[new GridTile(cX + 1, cY + dY)]);
                if (blocked(cX, cY, -1, 0, r) && !blocked(cX, cY, -1, dY, r))
                    neighbours.Add(r.AllNodes[new GridTile(cX - 1, cY + dY)]);
            }

            else
            {
                if (!blocked(cX, cY, dX, 0, r))
                    neighbours.Add(r.AllNodes[new GridTile(cX + dX, cY)]);
                if (blocked(cX, cY, 0, 1, r) && !blocked(cX, cY, dX, 1, r))
                    neighbours.Add(r.AllNodes[new GridTile(cX + dX, cY + 1)]);
                if (blocked(cX, cY, 0, -1, r) && !blocked(cX, cY, dX, -1, r))
                    neighbours.Add(r.AllNodes[new GridTile(cX + dX, cY - 1)]);
            }
        }
        return neighbours;
    }

    public static Node jump(int cX, int cY, int dX, int dY, Region r, Node goal)
    {
        int nX = cX + dX;
        int nY = cY + dY;
        if (blocked(nX, nY, 0, 0, r))
            return null;
        if (nX == goal.pos.x && nY == goal.pos.y)
            return r.AllNodes[new GridTile(nX, nY)];
        int oX = nX;
        int oY = nY;
        if (dX != 0 && dY != 0)
        {
            while (true)
            {
                if ((!blocked(oX, oY, -dX, dY, r) && blocked(oX, oY, -dX, 0, r)) || (!blocked(oX, oY, dX, -dY, r) && blocked(oX, oY, 0, -dY, r)))
                    return r.AllNodes[new GridTile(oX, oY)];
                if (jump(oX, oY, dX, 0, r, goal) != null || jump(oX, oY, 0, dY, r, goal) != null)
                    return r.AllNodes[new GridTile(oX, oY)];
                oX += dX;
                oY += dY;
                if (blocked(oX, oY, 0, 0, r))
                    return null;
                if (dblock(oX, oY, dX, dY, r))
                    return null;
                if (oX == goal.pos.x && oY == goal.pos.y)
                    return r.AllNodes[new GridTile(oX, oY)];

            }
        }
        else
        {
            if (dX != 0)
            {
                while (true)
                {
                    if (!blocked(oX, nY, dX, 1, r) && blocked(oX, nY, 0, 1, r) || !blocked(oX, nY, dX, -1, r) && blocked(oX, nY, 0, -1, r))
                        return r.AllNodes[new GridTile(oX, nY)];
                    oX += dX;
                    if (blocked(oX, nY, 0, 0, r))
                        return null;
                    if (oX == goal.pos.x && nY == goal.pos.y)
                        return r.AllNodes[new GridTile(oX, nY)];
                }
            }
            else
            {
                while (true)
                {
                    if (!blocked(nX, oY, 1, dY, r) && blocked(nX, oY, 1, 0, r) || !blocked(nX, oY, -1, dY, r) && blocked(nX, oY, -1, 0, r))
                        return r.AllNodes[new GridTile(nX, oY)];
                    oY += dY;
                    if (blocked(nX, oY, 0, 0, r))
                        return null;
                    if (nX == goal.pos.x && oY == goal.pos.y)
                        return r.AllNodes[new GridTile(nX, oY)];
                }
            }
        }
    }

    public static List<Node> identifySuccessors(int cX, int cY, Dictionary<Node, Node> came_from, Region r, Node goal)
    {
        List<Node> successors = new List<Node>();
        GridTile g = new GridTile(cX, cY);
        Node current = r.AllNodes[g];
        Node parent = null;
        if (came_from.ContainsKey(current))
            parent = came_from[current];
        List<Node> neighbours = nodeNeighbours(cX, cY, parent, r);
        foreach (Node cell in neighbours)
        {

            int dX = cell.pos.x - cX;
            int dY = cell.pos.y - cY;
            Node jumpPoint = jump(cX, cY, dX, dY, r, goal);
            if (jumpPoint != null)
            {
                successors.Add(jumpPoint);
            }
        }
        return successors;
    }

    public static LinkedList<GridTile> straghtLine(int x, int y, int x2, int y2, Region r)
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
            if (r.GridInRegion(g))
            {
                list.AddLast(g);
            }
            else
            {
                return null;
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

    


    public static LinkedList<Edge> FindPath(Node start, Node dest, Region r, out float weight)
    {
        weight = 0;
        LinkedList<GridTile> line = straghtLine(start.pos.x, start.pos.y, dest.pos.x, dest.pos.y, r);
        if (line != null)
        {
            return BuildStraightPath(line, r, r.Type, out weight);
        }
        Dictionary<Node, Node> came_from = new Dictionary<Node, Node>();
        HashSet<GridTile> Visited = new HashSet<GridTile>(); //无顺序的非重复list，已访问节点列表，closed
        //Dictionary<GridTile, Edge> Parent = new Dictionary<GridTile, Edge>(); // parent：来的路
        Dictionary<GridTile, float> gScore = new Dictionary<GridTile, float>(); // gscroe：每个节点的score

        SimplePriorityQueue<Node, float> pq = new SimplePriorityQueue<Node, float>(); //open

        float temp_gCost, prev_gCost;

        gScore[start.pos] = 0;
        pq.Enqueue(start, EuclidianDistance(start, dest));
        Node current;        

        while (pq.Count > 0)
        {
            current = pq.Dequeue();
            if (current.pos.Equals(dest.pos))
                return RebuildPath(came_from, current, r.Type, out weight);
            Visited.Add(current.pos);
            List<Node> successors = identifySuccessors(current.pos.x, current.pos.y, came_from, r, dest);

            foreach (Node successor in successors)
            {
                Node jumpPoint = successor;
                if (Visited.Contains(jumpPoint.pos))
                    continue;
                float temp_g_score = gScore[current.pos] + EuclidianDistance(current, jumpPoint);
                if (gScore.TryGetValue(jumpPoint.pos, out prev_gCost) && temp_g_score >= prev_gCost)
                    continue;
                came_from[jumpPoint] = current;
                gScore[jumpPoint.pos] = temp_g_score; //当前edge终点的gScore是当前点到起点的cost+当前边的weight
                pq.Enqueue(jumpPoint, temp_g_score + EuclidianDistance(jumpPoint, dest)); // f=g+h，h是当前edge end到目标点的直线距离
            }
        }

        return new LinkedList<Edge>();//未找到路径
    }

    public static LinkedList<Edge> BuildStraightPath(LinkedList<GridTile> line, Region r, int avg_weight, out float weight)
    {
        //LinkedList<Edge> result = new LinkedList<Edge>();
        LinkedList<Edge> res = new LinkedList<Edge>();
        LinkedListNode<GridTile> g = line.First;
        //Edge e = null;
        weight = 0;
        while (g.Next!=null)
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

    //Rebuild edges
    public static LinkedList<Edge> RebuildPath(Dictionary<Node, Node> Parent, Node dest,int avg_weight, out float weight)
    {
        //LinkedList<Edge> result = new LinkedList<Edge>();
        LinkedList<Edge> res = new LinkedList<Edge>();
        Node current = dest;
        //Edge e = null;
        weight = 0;
        while (Parent.TryGetValue(current, out Node n))
        {
            Edge e = new Edge();
            e.end = current;
            Node n1 = null;
            if (Parent.TryGetValue(current, out n1))
                e.start = Parent[current];
            else
                e.start = current;
            res.AddFirst(e);
            weight += EuclidianDistance(e.start, e.end);
            current = Parent[current];
        }
        weight *= avg_weight;
        return res;
    }
}
