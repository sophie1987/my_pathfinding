public class Graph // 图，由map抽象而来
{
    public static float SQRT2 = (float)Math.Sqrt(2f);

    public int depth;
    //List of clusters for every level of abstraction，cluster集合，从细节往上有多层
    public List<Cluster>[] C;

    //Keep a representation of the map by low level nodes，由node组成的map
    public Dictionary<GridTile, Node> nodes;

    readonly int width; // 宽度
    readonly int height; // 高度

    //We keep track of added nodes to remove them afterwards
    List<Node> AddedNodes; // 新增节点列表

    //public Map map;


    /// <summary>
    /// Construct a graph from the map
    /// </summary>
    public Graph(Map map, int MaxLevel, int clusterSize) // map:原始地图，maxlevel:分层数量，clustersize：cluster包含多少个gridtile（边）
    {
        //map = input_map;
        depth = MaxLevel;
        AddedNodes = new List<Node>(); // node是相邻两个cluster之间可通行的节点

        nodes = CreateMapRepresentation(map); // 返回的mapnodes是一个key/value列表，包含了所有非障碍节点：非障碍节点对应的node（包含该节点的x，y，该节点与周边八方向可联通edge(inter)列表）
                                              // *** 八叉树实现？
        width = map.Width; // 10
        height = map.Height; // 10

        int ClusterWidth, ClusterHeight;

        C = new List<Cluster>[MaxLevel]; // C为cluster的集合，从下往上构造，这里构造两层，每3个格子进行合并，MaxLevel=2

        for (int i = 0; i < MaxLevel; ++i) // i=0时为第一层，i=1时为第二层
        {
            if (i != 0) // i>0，即往上合并时，加到clusterSize，每次*3。即第一层为3，第二层为9
                //Increment cluster size for higher levels
                clusterSize = clusterSize * 3;    //Scaling factor 3 is arbitrary

            //Set number of clusters in horizontal and vertical direction
            ClusterHeight = (int)Math.Ceiling((float)map.Height / clusterSize);// i=1时，clusterheight=10/9 =2
            ClusterWidth = (int)Math.Ceiling((float)map.Width / clusterSize); // width =2

            if (ClusterWidth <= 1 && ClusterHeight <= 1)
            {

                depth = i;
                break;
            }

            C[i] = BuildClusters(i, clusterSize, ClusterWidth, ClusterHeight); // 1,9,2,2，C[0]=在原始map基础上每3*3组成的cluster List，共16个cluster

        }
    }


    /// <summary>
    /// Create the node-based representation of the map
    /// </summary>
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

    /// <summary>
    /// Add the edge to the node if it's a valid map edge
    /// </summary>
    private void SearchMapEdge(Map map, Dictionary<GridTile, Node> mapNodes, Node n, int x, int y, bool diagonal)
    {
        //var weight = diagonal ? SQRT2 : 1f; // 横向1，斜向根号2
        GridTile gridTile = new GridTile();
        float weight;

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

    /// <summary>
    /// Insert start and dest nodes in graph in all layers
    /// </summary>
    public void InsertNodes(GridTile start, GridTile dest, out Node nStart, out Node nDest)
    {
        
        if (!nodes.ContainsKey(start) || !nodes.ContainsKey(dest))
        {
            nStart = null;
            nDest = null;
            return;
        }

        Cluster cStart, cDest;
        Node newStart, newDest;
        nStart = nodes[start];
        nDest = nodes[dest];
        bool isConnected;
        AddedNodes.Clear();

        for (int i = 0; i < depth; ++i) //depth=2,i=1
        {
            cStart = null;
            cDest = null;
            isConnected = false;

            foreach (Cluster c in C[i]) //c[1],4个cluster
            {
                if (c.Contains(start)) //C[1][0]
                    cStart = c;

                if (c.Contains(dest)) //C[1][4]
                    cDest = c;

                if (cStart != null && cDest != null) 
                    break;
            }

            //This is the right cluster
            if (cStart == cDest) // 在这一层找到了start和dest
            {
                newStart = new Node(start) { child = nStart }; //该层node，child是下一层的node，包含edge
                newDest = new Node(dest) { child = nDest };

                isConnected = ConnectNodes(newStart, newDest, cStart);//newStart,newDest都是C[1][0]层的node，cStart=C[1][0]

                if (isConnected)
                {
                    //If they are reachable then we set them as the nodes
                    //Otherwise we might be able to reach them from an upper layer
                    nStart = newStart;//可连接，返回的nStart和nDest就是C[1][0]中的这两个node
                    nDest = newDest;
                }
            }

            if (!isConnected)
            {
                nStart = ConnectToBorder(start, cStart, nStart); // cStart = c[1][0], nStart=c[0][2].nodes[start]，如果在nStart中，cStart是node，则返回这个node，否则新建这个node到该cluster的所有node的edge
                nDest = ConnectToBorder(dest, cDest, nDest); // cDest = c[1][4], nDest=c[0][8].nodes[dest]
            }
        }
    }

    /// <summary>
    /// Remove nodes from the graph, including all underlying edges
    /// </summary>
    public void RemoveAddedNodes()
    {
        foreach (Node n in AddedNodes)
            foreach (Edge e in n.edges)
                //Find an edge in current.end that points to this node
                e.end.edges.RemoveAll((ee) => ee.end == n);
    }

    /// <summary>
    /// Connect the grid tile to borders by creating a new node
    /// </summary>
    /// <returns>The node created</returns>
    private Node ConnectToBorder(GridTile pos, Cluster c, Node child) // (8,8),C[0][8],null
    {
        Node newNode;

        //If the position is an actual border node, then return it
        if (c.Nodes.TryGetValue(pos, out newNode)) //C[0][2]中的node(8,1)
            return newNode;

        //Otherwise create a node and pathfind through border nodes
        newNode = new Node(pos) { child = child }; //内部grid，新建node，child为null

        foreach (KeyValuePair<GridTile, Node> n in c.Nodes)
        {
            ConnectNodes(newNode, n.Value, c); //内部grid，将该node与该cluster内的所有相连接
        }

        //Since this node is not part of the graph, we keep track of it to remove it later
        AddedNodes.Add(newNode); // 将这个node加入AddedNodes列表中以便后续删除

        return newNode;
    }

    /// <summary>
    /// Connect two nodes by pathfinding between them. 
    /// </summary>
    /// <remarks>We assume they are different nodes. If the path returned is 0, then there is no path that connects them.</remarks>
    private bool ConnectNodes(Node n1, Node n2, Cluster c) //(8,1),(8,4),p1
    {
        LinkedList<Edge> path; //双向链表，存储路径
        LinkedListNode<Edge> iter; //双向链表中的节点
        Edge e1, e2;

        float weight = 0;
        path = Pathfinder.FindPath(n1.child, n2.child,out weight,c.Boundaries); // n1.child，c1当中的(8,1),c5当中的（8，4）

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
                //float_weight += val.weight; // e1的path是根据A*查出来的路径，并没有路径距离。这里从起点开始，整条路径为clild每个edge的weight之和
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


    private delegate void CreateBorderNodes(Cluster c1, Cluster c2, bool x);

    /// <summary>
    /// Build Clusters of a certain level, given the size of a cluster
    /// ClusterWidth is the number of clusters in the horizontal direction.
    /// ClusterHeight is the number of clusters in the vertical direction.
    /// </summary>
    private List<Cluster> BuildClusters(int level, int ClusterSize, int ClusterWidth, int ClusterHeight) // 构造每一层的cluster集合。level为当前层数，从0开始
    {
        List<Cluster> clusters = new List<Cluster>();

        Cluster clst;

        int i, j;

        //Create clusters of this level，第0层，16个cluster；第1层，4个cluster
        // *** 这里可否用四叉树或者八叉树生成cluster？
        for (i = 0; i < ClusterHeight; ++i)
            for (j = 0; j < ClusterWidth; ++j)
            {
                clst = new Cluster(); // 
                clst.Boundaries.Min = new GridTile(j * ClusterSize, i * ClusterSize); // boundaries左上角坐标。这里的grid只有pos没有weight
                clst.Boundaries.Max = new GridTile(
                Math.Min(clst.Boundaries.Min.x + ClusterSize - 1, width - 1), // min(8,9)=8 width-1是保证这个cluster边界不超过整体地体边界
                    Math.Min(clst.Boundaries.Min.y + ClusterSize - 1, height - 1)); // min(8,9)=8

                //Adjust size of cluster based on boundaries
                clst.Width = clst.Boundaries.Max.x - clst.Boundaries.Min.x + 1; // 9
                clst.Height = clst.Boundaries.Max.y - clst.Boundaries.Min.y + 1; // 9

                if (level > 0)
                {
                    //Since we're abstract, we will have lower level clusters
                    clst.Clusters = new List<Cluster>(); // clst.Clusters是该层cluster下一层对应的cluster的列表集合

                    //Add lower level clusters in newly created clusters
                    foreach (Cluster c in C[level - 1]) // C[level-1]表示下一层cluster，这里是C[0]
                        if (clst.Contains(c)) // 如果当前cluster范围包含下一层cluster范围，则当前cluster的子cluster添加下一层cluster
                            clst.Clusters.Add(c); // C[1][0].Clusters=[C[0][0]--C[0][8]]
                }

                clusters.Add(clst); //这里只初始化了boundaries，width，height，未初始化cluster中的node
            }

        // 创建这一层中每两个相邻的cluster之间的nodes
        if (level == 0) //第一层，增加的是具体的node
        {
            //Add border nodes for every adjacent pair of clusters，双重遍历，CreateConcreteBorderNodes(inter)
            // 这里是否是考虑用四叉树可以快速获得相邻cluster的地方？
            for (i = 0; i < clusters.Count; ++i) // 0
                for (j = i + 1; j < clusters.Count; ++j) //1-15
                    DetectAdjacentClusters(clusters[i], clusters[j], CreateConcreteBorderNodes);

        }

        else //以上各层，增加的是抽象的node
        {
            //Add border nodes for every adjacent pair of clusters
            for (i = 0; i < clusters.Count; ++i) // 0,1,2,3
                for (j = i + 1; j < clusters.Count; ++j)
                    DetectAdjacentClusters(clusters[i], clusters[j], CreateAbstractBorderNodes);
        }

        //创建内部nodes
        for (i = 0; i < clusters.Count; ++i)
            GenerateIntraEdges(clusters[i]);

        return clusters;
    }

    private void DetectAdjacentClusters(Cluster c1, Cluster c2, CreateBorderNodes CreateBorderNodes) //c1=((0,0),(8,8)),c2=((9,0),(9,8))
    {
        //Check if both clusters are adjacent
        if (c1.Boundaries.Min.x == c2.Boundaries.Min.x) //垂直相邻，c1=((0,0),(2,2)),c2=((0,3),(2,5));c2=((0,6),(2,8))
        {
            if (c1.Boundaries.Max.y + 1 == c2.Boundaries.Min.y) // max.y+1=min.y，则表示上下相邻
                CreateBorderNodes(c1, c2, false);
            else if (c2.Boundaries.Max.y + 1 == c1.Boundaries.Min.y) 
                CreateBorderNodes(c2, c1, false);

        }
        else if (c1.Boundaries.Min.y == c2.Boundaries.Min.y) //水平相邻，c1=((0,0),(8,8)),c2=((9,0),(9,8))
        {
            if (c1.Boundaries.Max.x + 1 == c2.Boundaries.Min.x)
                CreateBorderNodes(c1, c2, true);
            else if (c2.Boundaries.Max.x + 1 == c1.Boundaries.Min.x)
                CreateBorderNodes(c2, c1, true);
        }
    }

    /// <summary>
    /// Create border nodes and attach them together.
    /// We always pass the lower cluster first (in c1).
    /// Adjacent index : if x == true, then c1.BottomRight.x else c1.BottomRight.y
    /// </summary>
    private void CreateConcreteBorderNodes(Cluster c1, Cluster c2, bool x) //c1=((6,0),(8,2)),c2=((9,0),(9，2))，x=true
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
                CreateConcreteInterEdges(c1, c2, x, ref lineSize, i); // c1=((0,0),(2,2)),c2=((3,0),(5，2)),true,1,2
            }
        }

        //If line size > 0 after looping, then we have another line to fill in
        CreateConcreteInterEdges(c1, c2, x, ref lineSize, i);//c1=((6,0),(8,2)),c2=((9,0),(9，2))，x=true,lineSize=3
    }

    //i is the index at which we stopped (either its an obstacle or the end of the cluster
    private void CreateConcreteInterEdges(Cluster c1, Cluster c2, bool x, ref int lineSize, int i) //c1=((0,0),(2,2)),c2=((3,0),(5，2)),true,1,2
    {
        if (lineSize > 0)
        {
            if (lineSize <= 5)
            {
                //Line is too small, create 1 inter edges
                CreateConcreteInterEdge(c1, c2, x, i - (lineSize / 2 + 1));//c1=((0,0),(2,2)),c2=((3,0),(5，2)),true,1
            }
            else
            {
                //Create 2 inter edges
                CreateConcreteInterEdge(c1, c2, x, i - lineSize);
                CreateConcreteInterEdge(c1, c2, x, i - 1);
            }

            lineSize = 0; //这一步会将lineSize设置为0，表示从头开始计算
        }
    }

    //Inter edges are edges that crosses clusters
    private void CreateConcreteInterEdge(Cluster c1, Cluster c2, bool x, int i) // c1=((0,0),(2,2)),c2=((3,0),(5，2)),true,1
    {
        GridTile g1, g2;
        Node n1, n2;
        if (x)
        {
            g1 = new GridTile(c1.Boundaries.Max.x, i); //(2,0)
            g2 = new GridTile(c2.Boundaries.Min.x, i); //(3,0)
        }
        else
        {
            g1 = new GridTile(i, c1.Boundaries.Max.y); //(0,2)
            g2 = new GridTile(i, c2.Boundaries.Min.y); //(0,3)
        }

        if (!c1.Nodes.TryGetValue(g1, out n1)) // c1中不存在这个node
        {
            g1.weight = nodes[g1].pos.weight;
            n1 = new Node(g1); 
            c1.Nodes.Add(g1, n1); //在cluster1中添加这个node坐标
            n1.child = nodes[g1]; // n1的child设置为nodes[g1].nodes[g1]返回的是底层map中这个节点对应的node
        }

        if (!c2.Nodes.TryGetValue(g2, out n2)) // c2中不存在这个node
        {
            g2.weight = nodes[g2].pos.weight;
            n2 = new Node(g2);
            c2.Nodes.Add(g2, n2);
            n2.child = nodes[g2];
        }

        n1.edges.Add(new Edge() { start = n1, end = n2, type = EdgeType.INTER, weight = n2.child.pos.weight }); //n1的edge加上n1到n2的inter边。相邻的cluster只能是垂直或者水平，不会是斜向的，因此weight都设置为1
        n2.edges.Add(new Edge() { start = n2, end = n1, type = EdgeType.INTER, weight = n1.child.pos.weight }); //n2的edge加上n2到n1的inter边
    }


    private void CreateAbstractBorderNodes(Cluster p1, Cluster p2, bool x) //第1层及以上层创建连接点，找他们的下一层的cluster集合。垂直相邻x=false，水平相邻x=true
                                                                           //p1=((0,0),(8,8)),p2=((9,0),(9,8))
    {
        foreach (Cluster c1 in p1.Clusters) // p1.Clusters有9个子cluster
            foreach (Cluster c2 in p2.Clusters) // p2.Clusters有3个子cluster
            { //c1和c2都是下一层的cluster
                if ((x && c1.Boundaries.Min.y == c2.Boundaries.Min.y && c1.Boundaries.Max.x + 1 == c2.Boundaries.Min.x) || //子cluster相邻
                    (!x && c1.Boundaries.Min.x == c2.Boundaries.Min.x && c1.Boundaries.Max.y + 1 == c2.Boundaries.Min.y))
                {
                    CreateAbstractInterEdges(p1, p2, c1, c2); // p1里的c1和p2里的c2相邻,p1=((0,0),(8,8)),p2=((9,0),(9,8)),c1=((6,0),(8,2)),c2=((9,0),(9,2))
                }
            }
    }
    private void CreateAbstractInterEdges(Cluster p1, Cluster p2, Cluster c1, Cluster c2) //p1=((0,0),(8,8)),p2=((9,0),(9,8)),c1=((6,0),(8,2)),c2=((9,0),(9,2))
    {
        List<Edge> edges1 = new List<Edge>(),
            edges2 = new List<Edge>();
        Node n1, n2;

        //Add edges that connects them from c1
        foreach (KeyValuePair<GridTile, Node> n in c1.Nodes) // c1内可通行的node，（8，1），（7，2）
            foreach (Edge e in n.Value.edges) //(8,1)的edges和(7,2)的edges
            {
                if (e.type == EdgeType.INTER && c2.Contains(e.end.pos)) // INTER edge，并且终点在c2内，添加该node到edges1内，（8，1）->（9，1）,(9,1)在c2内
                    edges1.Add(e); //（8，1）->（9，1）
            }

        foreach (KeyValuePair<GridTile, Node> n in c2.Nodes) // c2内可通行的node，（9，1），（9，2）
            foreach (Edge e in n.Value.edges)
            {
                if (e.type == EdgeType.INTER && c1.Contains(e.end.pos)) //(9,1) -> (8,1)在c1内
                    edges2.Add(e); //(9,1) -> (8,1)
            }

        //Find every pair of twin edges and insert them in their respective parents
        foreach (Edge e1 in edges1)//（8，1）->（9，1）
            foreach (Edge e2 in edges2)//(9,1) -> (8,1)
            {
                if (e1.end == e2.start)
                {
                    if (!p1.Nodes.TryGetValue(e1.start.pos, out n1)) //p1的node中没有e1的start
                    {
                        n1 = new Node(e1.start.pos) { child = e1.start }; // (8,1),child=e1.start(8,1)。这里的child是下一层的node
                        p1.Nodes.Add(n1.pos, n1); // p1的node中添加这个node
                    }

                    if (!p2.Nodes.TryGetValue(e2.start.pos, out n2))
                    {
                        n2 = new Node(e2.start.pos) { child = e2.start };
                        p2.Nodes.Add(n2.pos, n2);
                    }

                    n1.edges.Add(new Edge() { start = n1, end = n2, type = EdgeType.INTER, weight = n2.child.pos.weight });//n1，第二层的（8，1）,添加edge，终点是第二层的（9，1）
                    n2.edges.Add(new Edge() { start = n2, end = n1, type = EdgeType.INTER, weight = n1.child.pos.weight });

                    break;  //Break the second loop since we've found a pair
                }
            }
    }

    //Intra edges are edges that lives inside clusters，在一个cluster内寻找所有的node之间的intra edge
    private void GenerateIntraEdges(Cluster c) // p1内有6个node，计算每两个node之间的距离
    {
        int i, j;
        Node n1, n2;

        /* We do this so that we can iterate through pairs once, 
         * by keeping the second index always higher than the first */
        var nodes = new List<Node>(c.Nodes.Values);

        for (i = 0; i < nodes.Count; ++i) // nodes.count=6
        {
            n1 = nodes[i]; //nodes[0]，（8，1）
            for (j = i + 1; j < nodes.Count; ++j) // nodes[1]，（8，4）
            {
                n2 = nodes[j];

                ConnectNodes(n1, n2, c);
            }
        }
    }
}