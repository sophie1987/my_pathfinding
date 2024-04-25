using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Threading.Tasks;

public class Graph_New // 图，一层cluster的集合
{
    public static float SQRT2 = (float)Math.Sqrt(2f);
    public Dictionary<Int2, Cluster_New> C;
    public Dictionary<int, Region> R;

    public Dictionary<GridTile, Node> nodes;

    readonly int width; // 宽度
    readonly int height; // 高度

    List<Node> AddedNodes; // 新增节点列表

    public Graph_New(Map map, int clusterSize) // map:原始地图，maxlevel:分层数量，clustersize：cluster包含多少个gridtile（边）
    {
        AddedNodes = new List<Node>();
        nodes = CreateMapRepresentation(map); // 创建可通行节点列表。
        width = map.Width;
        height = map.Height;
        int ClusterWidth, ClusterHeight;

        ClusterHeight = (int)Math.Ceiling((float)map.Height / clusterSize);
        ClusterWidth = (int)Math.Ceiling((float)map.Width / clusterSize);

        C = BuildCluster(map, clusterSize, ClusterWidth, ClusterHeight);
        R = BuildRegion(ClusterWidth, ClusterHeight);
        AddAdjecentRegionNodes();
        deleteNeighbourNodes();
        foreach (Region r in R.Values)
        {
            GenerateIntraEdges(r);
        }

        /*foreach(Region r in R.Values)
        {
            Console.WriteLine("RegionID:" + r.Region_ID);
            Console.WriteLine("Boundaries:minx=" + r.minX + ",minY=" + r.minY + ",maxX=" + r.maxX + ",maxY=" + r.maxY);
            Console.WriteLine("Node Number:" + r.BoundaryNodes.Count);
            Console.WriteLine("Cluster Number:" + r.C.Count);
            foreach(Node n in r.BoundaryNodes.Values)
            {
                Console.WriteLine("Node Pos:" + n.getPos());
                Console.WriteLine(n.getEdges());
            }
            Console.WriteLine("\n");
        }*/
    }

    public void AddAdjecentRegionNodes()
    {
        foreach (Region r in R.Values)
        {
            foreach (Cluster_New c in r.C)
            {

                //Cluster_New right = c.right_cluster;
                Int2 current_index = c.index;
                Int2 right_index = new Int2(current_index.X + 1, current_index.Y);
                C.TryGetValue(right_index, out Cluster_New right);
                if (right != null && !r.C.Contains(right))
                {
                    int right_region_id = right.Region_ID;
                    Region right_region = R[right_region_id];
                    CreateConcreteBorderNodes(c, right, r, right_region, true);
                }
                //Cluster_New bottom = c.bottom_cluster;
                Int2 bottom_index = new Int2(current_index.X, current_index.Y + 1);
                C.TryGetValue(bottom_index, out Cluster_New bottom);
                if (bottom != null && !r.C.Contains(bottom))
                {
                    int bottom_region_id = bottom.Region_ID;
                    Region bottom_region = R[bottom_region_id];
                    CreateConcreteBorderNodes(c, bottom, r, bottom_region, false);
                }
            }
        }
    }

    public void deleteNeighbourNodes() { 
        foreach(Region r in R.Values)
        {
            var nodes = new List<Node>(r.BoundaryNodes.Values);
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                Node n = nodes[i]; //nodes[0]，（8，1）
                int current_x = n.pos.x;
                int current_y = n.pos.y;
                List<Int2> directions = new List<Int2>();
                directions = [new Int2(-1, 0), new Int2(1, 0), new Int2(0, -1), new Int2(0, 1)]; // 上下左右四方向
                foreach(Int2 direction in directions)
                {
                    GridTile neighbour_grid = new GridTile(current_x + direction.X, current_y + direction.Y);
                    if (r.BoundaryNodes.ContainsKey(neighbour_grid)) // 隔壁节点也是边界node
                    {
                        Node neighbour_node = r.BoundaryNodes[neighbour_grid];
                        int current_node_inter_end_number = 0;
                        List<Node> current_node_inter_end_nodes = new List<Node>();
                        foreach(Edge e in n.edges)
                        {
                            if (e.type == EdgeType.INTER)
                            {
                                current_node_inter_end_number++;
                                current_node_inter_end_nodes.Add(e.end);
                            }                                
                        }
                        if (current_node_inter_end_number == 1)
                        {
                            int neightbour_node_inter_end_number = 0;
                            List<Node> neighbour_node_inter_end_nodes = new List<Node>();
                            foreach(Edge e in neighbour_node.edges)
                            {
                                if (e.type == EdgeType.INTER)
                                {
                                    neightbour_node_inter_end_number++;
                                    neighbour_node_inter_end_nodes.Add(e.end);
                                }
                            }
                            if (neightbour_node_inter_end_number == 1)
                            {
                                Node current_node_inter_end_node = current_node_inter_end_nodes[0];
                                Node neighbour_node_inter_end_node = neighbour_node_inter_end_nodes[0];
                                if (neighbour_node_inter_end_node.pos.y - current_node_inter_end_node.pos.y == direction.Y && neighbour_node_inter_end_node.pos.x - current_node_inter_end_node.pos.x == direction.X)
                                {
                                    Region current_node_inter_end_region, neighbour_node_inter_end_region;
                                    GetNodeRegion(current_node_inter_end_node.pos, neighbour_node_inter_end_node.pos, out current_node_inter_end_region, out neighbour_node_inter_end_region);
                                    if (current_node_inter_end_region.Region_ID == neighbour_node_inter_end_region.Region_ID)
                                    {
                                        r.BoundaryNodes.Remove(n.pos);
                                        current_node_inter_end_region.BoundaryNodes.Remove(current_node_inter_end_node.pos);
                                        break;
                                    }
                                    else
                                    {
                                        r.BoundaryNodes.Remove(n.pos);
                                        foreach (Edge e in n.edges)
                                            e.end.edges.RemoveAll((ee) => ee.end == n);
                                        Edge new_edge1 = new Edge();
                                        new_edge1.type = EdgeType.INTER;
                                        new_edge1.weight = SQRT2;
                                        new_edge1.start = current_node_inter_end_node;
                                        new_edge1.end = neighbour_node;
                                        current_node_inter_end_node.edges.Add(new_edge1);
                                        Edge new_edge2 = new Edge();
                                        new_edge2.type = EdgeType.INTER;
                                        new_edge2.weight = SQRT2;
                                        new_edge2.start = neighbour_node;
                                        new_edge2.end = current_node_inter_end_node;
                                        neighbour_node.edges.Add(new_edge2);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private Dictionary<int, Region> BuildRegion(int ClusterWidth, int ClusterHeight)
    {
        Dictionary<int, Region> regions = new Dictionary<int, Region>();

        int i = 0;
        foreach (var (int2, c) in C)
        {
            if (c.Region_ID >= 0)
                continue;
            if (c.Type == -1) // 该cluster未属于某个region，但是一个混合Type的cluster，单独新建一个region给它
            {
                Region r = new Region();
                r.Region_ID = i;
                c.Region_ID = i;
                r.AddCluster(c);
                regions.Add(i, r);
                r.minX = c.Boundaries.Min.x;
                r.minY = c.Boundaries.Min.y;
                r.maxX = c.Boundaries.Max.x;
                r.maxY = c.Boundaries.Max.y;
                i++;
            } // 该cluster未属于某个region，且Type为单一type的cluster，新建一个region，以这个cluster为始，向四周找相同type的cluster进行合并
            else
            {
                Region r = new Region();
                r.Region_ID = i;
                //c.Region_ID = i;
                r.Type = c.Type;
                //r.AddCluster(c);
                regions.Add(i, r);
                r.minX = c.Boundaries.Min.x;
                r.minY = c.Boundaries.Min.y;
                r.maxX = c.Boundaries.Max.x;
                r.maxY = c.Boundaries.Max.y;
                Int2 range = GetClusterMaxRange(c, r, ClusterWidth, ClusterHeight);
                AddClusterToRegion(c,r,range);
                i++;

            }
        }
        return regions;
    }

    public void UpdateRegionBoundary(Cluster_New c, Region r)
    {
        /*int x, y;
        for (x=c.Boundaries.Min.x;x<=c.Boundaries.Max.x;++x)
            for (y = c.Boundaries.Min.y; y <= c.Boundaries.Max.y; ++y)
            {
                GridTile g = new GridTile(x,y);
                if (nodes.ContainsKey(g))
                    r.AllNodes.Add(g, nodes[g]);
            }*/
        if (c.Boundaries.Min.x < r.minX)
            r.minX = c.Boundaries.Min.x;
        if (c.Boundaries.Min.y < r.minY)
            r.minY = c.Boundaries.Min.y;
        if (c.Boundaries.Max.x > r.maxX)
            r.maxX = c.Boundaries.Max.x;
        if (c.Boundaries.Max.y > r.maxY)
            r.maxY = c.Boundaries.Max.y;
    }

    public void AddClusterToRegion(Cluster_New c,Region r,Int2 range)
    {
        int minX, minY, maxX, maxY;
        minX = c.index.X;
        minY = c.index.Y;
        maxX = range.X;
        maxY = range.Y;
        for (int i = minX; i <= maxX; ++i)
        {
            for (int j = minY; j <= maxY; ++j)
            {
                C.TryGetValue(new Int2(i, j), out Cluster_New c_new);
                if(c_new != null&&c_new.Region_ID==-1)
                {
                    c_new.Region_ID = r.Region_ID;
                    r.AddCluster(c_new);
                    UpdateRegionBoundary(c_new, r);
                }
            }
        }
    }

    public Int2 GetClusterMaxRange(Cluster_New c, Region r, int ClusterWidth, int ClusterHeight)
    {
        int maxX = -1;
        int maxY = -1;
        int current_x = c.index.X;
        int current_y = c.index.Y;

        Dictionary<int, int> widths = new Dictionary<int, int>();

        int maxRectangle = 0;
        int height_index = 1;

        int maxWidth = ClusterWidth;

        int i, j;

        for (j = current_y; j <= ClusterHeight; ++j)
        {
            widths[height_index] = 0;
            for (i = current_x; i <= maxWidth; ++i)
            {
                Int2 index = new Int2(i,j); 
                C.TryGetValue(index, out Cluster_New c_new);

                if (c_new != null && c_new.Region_ID == -1 && c_new.Type == c.Type)
                {
                    //if (current_x == 0 && current_y == 1)
                    //    Console.WriteLine("c_new,indexx=" + c_new.index.X + ",indexy=" + c_new.index.Y + ",region=" + c_new.Region_ID + ",Type=" + c_new.Type);
                    widths[height_index]++;
                }
                else
                {
                    maxWidth = i-1;
                    break;
                }

            }
            height_index++;
            Int2 index2 = new Int2(current_x, j);
            C.TryGetValue(index2, out Cluster_New c_new2);
            if (c_new2 == null || c_new2.Region_ID != -1 && c_new2.Type != c.Type)
                break;
        }
        

        foreach(var(index,value) in widths)
        {
            if (value == 0)
                break;
            int num = index * value;
            if (maxRectangle < num)
            {
                maxRectangle = num;
                maxX = current_x + value - 1;
                maxY = current_y + index - 1;
            }
        }
        return new Int2(maxX, maxY);
    }

    private Dictionary<GridTile, Node> CreateMapRepresentation(Map map)
    {
        var mapnodes = new Dictionary<GridTile, Node>(map.FreeTiles);
        int i, j;
        GridTile gridTile;

        //1. Create all nodes necessary，将所有可通行点加入到mapnodes中，gridTile：Node(gridTile)
        for (i = 0; i < map.Width; ++i)
            for (j = 0; j < map.Height; ++j)
            {
                if (!map.Obstacles[j][i])
                {
                    //gridTile = new GridTile(i, j);
                    gridTile = new GridTile(i, j, map.Tile_weights[j][i]);
                    mapnodes.Add(gridTile, new Node(gridTile));
                }
            }

        //2. Create all possible edges
        foreach (Node n in mapnodes.Values) // n是mapnodes中的所有node，边？路径？为空：edges = new List<Edge>();
        {
            //Look for straight edges
            for (i = -1; i < 2; i += 2) // i=-1,i=1
                                        // *** 这里是否可以考虑用四叉树或者八叉树进行计算？
            {
                SearchMapEdge(map, mapnodes, n, n.pos.x + i, n.pos.y, false); //上下两方向(map,mapnodes,n,n左边和右边两节点,false)，一个节点往左右两个节点可以通行，则这个节点所在node添加两个edge，到左右两边

                SearchMapEdge(map, mapnodes, n, n.pos.x, n.pos.y + i, false); // 左右两方向(map,mapnodes,n,n上下两节点,false)，逻辑同上
            }

            //Look for diagonal edges
            for (i = -1; i < 2; i += 2)
                for (j = -1; j < 2; j += 2)
                {
                    SearchMapEdge(map, mapnodes, n, n.pos.x + i, n.pos.y + j, true); //斜方向
                }
        }
        return mapnodes;
    }

    private void SearchMapEdge(Map map, Dictionary<GridTile, Node> mapNodes, Node n, int x, int y, bool diagonal)
    {
        //var weight = diagonal ? SQRT2 : 1f; // 横向1，斜向根号2
        GridTile gridTile = new GridTile();

        //Don't let diagonal movement occur when an obstacle is crossing the edge，斜向移动时，如果移动边两边有障碍，则不能通行
        if (diagonal) // 斜向
        {
            gridTile.x = n.pos.x;
            gridTile.y = y;
            if (!map.IsFreeTile(gridTile)) return;

            gridTile.x = x;
            gridTile.y = n.pos.y;
            if (!map.IsFreeTile(gridTile)) return;
        }

        gridTile.x = x;
        gridTile.y = y;
        if (!map.IsFreeTile(gridTile)) return;

        float weight;
        if (diagonal)
        {
            weight = SQRT2 * map.Tile_weights[y][x];
        }
        else
        {
            weight = 1f * map.Tile_weights[y][x];
        }
        //Edge is valid, add it to the node
        n.edges.Add(new Edge()
        {
            start = n,
            end = mapNodes[gridTile],
            type = EdgeType.INTER,
            weight = weight
        });
    }

    private Dictionary<Int2, Cluster_New> BuildCluster(Map map, int ClusterSize, int ClusterWidth, int ClusterHeight) // 构造每一层的cluster集合。level为当前层数，从0开始
    {
        int cluster_number = ClusterWidth * ClusterHeight;
        Dictionary<Int2, Cluster_New> cluster_boundaries = new Dictionary<Int2, Cluster_New>(cluster_number);

        Cluster_New clst;

        int i, j;

        for (i = 0; i < ClusterHeight; ++i)
            for (j = 0; j < ClusterWidth; ++j)
            {
                clst = new Cluster_New(); // 
                clst.Boundaries.Min = new GridTile(j * ClusterSize, i * ClusterSize); // boundaries左上角坐标。这里的grid只有pos没有weight
                clst.Boundaries.Max = new GridTile(
                Math.Min(clst.Boundaries.Min.x + ClusterSize - 1, width - 1), // min(8,9)=8 width-1是保证这个cluster边界不超过整体地体边界
                    Math.Min(clst.Boundaries.Min.y + ClusterSize - 1, height - 1)); // min(8,9)=8

                //Adjust size of cluster based on boundaries
                clst.Width = clst.Boundaries.Max.x - clst.Boundaries.Min.x + 1; // 9
                clst.Height = clst.Boundaries.Max.y - clst.Boundaries.Min.y + 1; // 9

                int x, y;
                GridTile first_grid = new GridTile(clst.Boundaries.Min.x, clst.Boundaries.Min.y);
                if (!map.IsFreeTile(first_grid))
                {
                    clst.Type = -1;
                }
                else
                {
                    clst.Type = nodes[first_grid].pos.weight;
                    for (x = clst.Boundaries.Min.x; x < clst.Boundaries.Max.x; ++x)
                        for (y = clst.Boundaries.Min.y; y < clst.Boundaries.Max.y; ++y)
                        {
                            GridTile g = new GridTile(x, y);
                            if (!map.IsFreeTile(g))
                            {
                                clst.Type = -1;
                                break;
                            }
                            else
                            {
                                int new_weight = nodes[g].pos.weight;
                                if (new_weight != clst.Type)
                                {
                                    clst.Type = -1;
                                    break;
                                }
                            }
                        }
                }
                Int2 index = new Int2(j, i);
                clst.index = index;
                cluster_boundaries.Add(index, clst);
                
            }

        return cluster_boundaries;
    }

    private void CreateConcreteBorderNodes(Cluster_New c1, Cluster_New c2, Region r1, Region r2, bool x) //c1=((6,0),(8,2)),c2=((9,0),(9，2))，x=true
    {
        int i, iMin, iMax;
        if (x) // 水平相邻，c1与c2 min.y相同，c1=((0,0),(2,2)),c2=((3,0),(5，2))，连接线为竖直线
        {
            iMin = c1.Boundaries.Min.y; //iMin是c1 左上角y轴值，0
            iMax = iMin + c1.Height; // iMax是c1左上角y轴值+c1的高度，3
        }
        else // 垂直相邻，c1与c2 min.x相同，c1=((0,0),(2,2)),c2=((0,3),(2,5));连接线为水平线
        {
            iMin = c1.Boundaries.Min.x; // iMin是c1左上角x轴值，0
            iMax = iMin + c1.Width; // iMax是c1左上角x轴值+c1宽度，3
        }

        int lineSize = 0;
        for (i = iMin; i < iMax; ++i)
        {

            if (x && (nodes.ContainsKey(new GridTile(c1.Boundaries.Max.x, i)) && nodes.ContainsKey(new GridTile(c2.Boundaries.Min.x, i)))
                || !x && (nodes.ContainsKey(new GridTile(i, c1.Boundaries.Max.y)) && nodes.ContainsKey(new GridTile(i, c2.Boundaries.Min.y))))
            // x=true，水平相邻，（8，0）可通行，（9，0）可通行；（8，1）可通行，（9，1）可通行；（8，2）可通行，（9，2）可通行,lineSize=3
            {
                lineSize++;
            }
            else
            {
                CreateConcreteInterEdges(c1, c2, r1, r2, x, ref lineSize, i); // c1=((0,0),(2,2)),c2=((3,0),(5，2)),true,1,2
            }
        }

        //If line size > 0 after looping, then we have another line to fill in
        CreateConcreteInterEdges(c1, c2, r1, r2, x, ref lineSize, i);
    }

    //i is the index at which we stopped (either its an obstacle or the end of the cluster
    private void CreateConcreteInterEdges(Cluster_New c1, Cluster_New c2, Region r1, Region r2, bool x, ref int lineSize, int i) //c1=((0,0),(2,2)),c2=((3,0),(5，2)),true,1,2
    {
        if (lineSize > 0)
        {
            if (lineSize <= 5)
            {
                CreateConcreteInterEdge(c1, c2, r1, r2, x, i - (lineSize / 2 + 1));
            }
            else
            {
                //Create 2 inter edges
                CreateConcreteInterEdge(c1, c2, r1, r2, x, i - lineSize);
                CreateConcreteInterEdge(c1, c2, r1, r2, x, i - 1);
            }

            lineSize = 0; //这一步会将lineSize设置为0，表示从头开始计算
        }
    }

    //Inter edges are edges that crosses clusters
    private void CreateConcreteInterEdge(Cluster_New c1, Cluster_New c2, Region r1, Region r2, bool x, int i) // c1=((0,0),(2,2)),c2=((3,0),(5，2)),true,1
    {
        GridTile g1, g2;
        Node n1, n2;
        if (x)
        {
            g1 = new GridTile(c1.Boundaries.Max.x, i);
            g2 = new GridTile(c2.Boundaries.Min.x, i);
        }
        else
        {
            g1 = new GridTile(i, c1.Boundaries.Max.y);
            g2 = new GridTile(i, c2.Boundaries.Min.y);
        }

        if (!r1.BoundaryNodes.TryGetValue(g1, out n1)) // c1中不存在这个node
        {
            g1.weight = nodes[g1].pos.weight;
            n1 = new Node(g1);
            r1.BoundaryNodes.Add(g1, n1); //在cluster1中添加这个node坐标
            n1.child = nodes[g1]; // n1的child设置为nodes[g1].nodes[g1]返回的是底层map中这个节点对应的node
            //c1.Nodes.Add(g1, n1);
        }

        if (!r2.BoundaryNodes.TryGetValue(g2, out n2)) // c2中不存在这个node
        {
            g2.weight = nodes[g2].pos.weight;
            n2 = new Node(g2);
            r2.BoundaryNodes.Add(g2, n2);
            n2.child = nodes[g2];
            //c2.Nodes.Add(g2, n2);
        }

        n1.edges.Add(new Edge() { start = n1, end = n2, type = EdgeType.INTER, weight = n2.child.pos.weight }); //n1的edge加上n1到n2的inter边。相邻的cluster只能是垂直或者水平，不会是斜向的，因此weight都设置为1
        n2.edges.Add(new Edge() { start = n2, end = n1, type = EdgeType.INTER, weight = n1.child.pos.weight }); //n2的edge加上n2到n1的inter边
    }

    private void GenerateIntraEdges(Region r)
    {
        int i, j;
        Node n1, n2;
        var nodes = new List<Node>(r.BoundaryNodes.Values);

        for (i = 0; i < nodes.Count; ++i)
        {
            n1 = nodes[i]; //nodes[0]，（8，1）
            for (j = i + 1; j < nodes.Count; ++j)
            {
                n2 = nodes[j];

                if (r.Type == -1)
                {
                    ConnectNonStraightIntraNode(n1, n2, r);
                }
                else
                {
                    ConnectStraightNode(n1, n2, r);
                }
            }
        }
    }

    private delegate LinkedList<Edge> findPathMethod(Node start, Node dest, Region r,out float weight);

    private bool ConnectStraightNode(Node n1, Node n2, Region r)
    {
        Edge e1 = Pathfinder.FindStaightPath(n1, n2, r);
        Edge e2 = Pathfinder.FindStaightPath(n2, n1, r);
        n1.edges.Add(e1);
        n2.edges.Add(e2);
        return true;
    }

    private bool ConnectNonStraightIntraNode(Node n1, Node n2,Region r)
    {
        //LinkedList<Edge> path; //双向链表，存储路径
        LinkedListNode<Edge> iter; //双向链表中的节点
        Edge e1, e2;

        float weight = 0;
        Boundaries b = new Boundaries(r.minX, r.minY, r.maxX, r.maxY);
        LinkedList<Edge> path = Pathfinder.FindPath(n1.child, n2.child, out weight, b);

        if (path.Count > 0) // 找到路径
        {
            e1 = new Edge()
            {
                start = n1,
                end = n2,
                type = EdgeType.INTRA,
                UnderlyingPath = path
            }; // e1是node1到node2的路径，path已经计算出来的

            e2 = new Edge()
            {
                start = n2,
                end = n1,
                type = EdgeType.INTRA,
                UnderlyingPath = new LinkedList<Edge>()
            }; // e2是node2到node1的路径，path需要根据e1的path反转获得

            //Store inverse path in node n2
            //Sum weights of underlying edges at the same time
            iter = e1.UnderlyingPath.Last; // 获得e1 path的最后一条边
            while (iter != null)
            {
                // Find twin edge
                var val = iter.Value.end.edges.Find(
                    e => e.start == iter.Value.end && e.end == iter.Value.start); // 反向查找node2到node1的path


                e2.UnderlyingPath.AddLast(val);
                //weight += val.weight;

                iter = iter.Previous;
            }

            //Update weights
            e1.weight = weight; // 记录这个cluster中两个节点间的Inter edge的weight
            e2.weight = weight;

            n1.edges.Add(e1);
            n2.edges.Add(e2);

            return true;
        }
        else
        {
            //No path, return false
            return false;
        }
    }


    public void InsertNodesNew(GridTile start, GridTile dest, Region rStart, Region rDest, out Node nStart, out Node nDest)
    {
        AddedNodes.Clear();
        nStart = ConnectToBorder(start, rStart);
        nDest = ConnectToBorder(dest, rDest);
    }
    public bool GetNodeRegion(GridTile start, GridTile dest, out Region rStart,out Region rDest)
    {
        rStart = null;
        rDest = null;
        if (!nodes.ContainsKey(start) || !nodes.ContainsKey(dest))
        {
            return false;
        }
        foreach (Region r in R.Values)
        {
            if (r.GridInRegion(start))
            {
                rStart = r;
            }
            if (r.GridInRegion(dest))
            {
                rDest = r;
            }
            if (rStart != null && rDest != null)
                break;
        }
        if (rStart == null || rDest == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private Node ConnectToBorder(GridTile pos, Region r)
    {
        Node newNode;
        if (r.BoundaryNodes.TryGetValue(pos, out newNode))
            return newNode;

        newNode = new Node(pos) { child = nodes[pos] };
        
        foreach(Node n in r.BoundaryNodes.Values)
        {
            if (r.Type == -1)
            {
                ConnectNonStraightIntraNode(newNode, n, r);
            }
            else
            {
                ConnectStraightNode(newNode, n, r);
            }
        }
        AddedNodes.Add(newNode);

        return newNode;
    }

    public void RemoveAddedNodes()
    {
        foreach (Node n in AddedNodes)
            foreach (Edge e in n.edges)
                //Find an edge in current.end that points to this node
                e.end.edges.RemoveAll((ee) => ee.end == n);
    }
}