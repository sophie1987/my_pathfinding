﻿using ConsoleApp1;

public class JPSPlusMapBaker
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int[,] BlockLUT;
    public JPSPlusMapBakerBlock[] Blocks;

    #region Public
    public JPSPlusMapBaker()
    {

    }

    public JPSPlusMapBaker(Map map)
    {
        Init(map);
    }
    public void Init(Map map)
    {
        Width = map.Width;
        Height = map.Height;
        BlockLUT = new int[Height, Width];

        List<JPSPlusMapBakerBlock> blocks = new List<JPSPlusMapBakerBlock>();
        int index = 0;
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                if (map.Obstacles[x][y])
                {
                    BlockLUT[y, x] = -1;
                }
                else
                {
                    BlockLUT[y, x] = index;
                    blocks.Add(new JPSPlusMapBakerBlock(new Int2(x, y)));
                    index++;
                }
            }
        }
        Blocks = blocks.ToArray();
    }

    public JPSPlusMapBaker(Region r)
    {
        Init(r);
    }
    public void Init(Region r)
    {
        Width = r.maxX - r.minX;
        Height = r.maxY - r.minY;
        BlockLUT = new int[Height, Width];

        List<JPSPlusMapBakerBlock> blocks = new List<JPSPlusMapBakerBlock>();
        int index = 0;
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                GridTile g = new GridTile(x, y);
                if (!r.GridInRegion(g))
                {
                    BlockLUT[y, x] = -1;
                }
                else
                {
                    BlockLUT[y, x] = index;
                    blocks.Add(new JPSPlusMapBakerBlock(new Int2(x, y)));
                    index++;
                }
            }
        }
        Blocks = blocks.ToArray();
    }
    public JPSPlusMapBaker(bool[,] walls)
    {
        Init(walls);
    }

    public void Init(bool[,] walls)
    {
        Width = walls.GetLength(1);
        Height = walls.GetLength(0);
        BlockLUT = new int[Height, Width];

        List<JPSPlusMapBakerBlock> blocks = new List<JPSPlusMapBakerBlock>();
        int index = 0;
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                if (walls[y, x])
                {
                    BlockLUT[y, x] = -1;
                }
                else
                {
                    BlockLUT[y, x] = index;
                    blocks.Add(new JPSPlusMapBakerBlock(new Int2(x, y)));
                    index++;
                }
            }
        }
        Blocks = blocks.ToArray();
    }

    public JPSPlusBakedMap Bake()
    {
        MarkPrimary();
        MarkStraight();
        MarkDiagonal();
        return new JPSPlusBakedMap(
            BlockLUT,
            Blocks.Select(x =>
            {
                return new JPSPlusBakedMap.JPSPlusBakedMapBlock(x.Pos, x.JumpDistances);
            }).ToArray());
    }
    #endregion Public

    private void MarkPrimary()
    {
        for (int y = 0; y < Height; ++y)
        {
            for (int x = 0; x < Width; ++x)
            {
                Int2 p = new Int2(x, y);

                if (IsWalkable(p))
                {
                    continue;
                }

                for (int d = 0b10000000; d > 0b00001111; d >>= 1)
                {
                    EDirFlags dir = (EDirFlags)d;
                    Int2 primaryP = p.Foward(dir);
                    JPSPlusMapBakerBlock primaryB = GetBlockOrNull(primaryP);
                    if (primaryB == null)
                    {
                        continue;
                    }

                    switch (dir)
                    {
                        case EDirFlags.NORTHEAST:
                            {
                                Int2 p1 = p.Foward(EDirFlags.NORTH);
                                Int2 p2 = p.Foward(EDirFlags.EAST);
                                if (IsWalkable(p1) && IsWalkable(p2))
                                {
                                    primaryB.JumpDirFlags |= EDirFlags.SOUTH | EDirFlags.WEST;
                                }
                                break;
                            }
                        case EDirFlags.SOUTHEAST:
                            {
                                Int2 p1 = p.Foward(EDirFlags.SOUTH);
                                Int2 p2 = p.Foward(EDirFlags.EAST);
                                if (IsWalkable(p1) && IsWalkable(p2))
                                {
                                    primaryB.JumpDirFlags |= EDirFlags.NORTH | EDirFlags.WEST;
                                }
                                break;
                            }
                        case EDirFlags.NORTHWEST:
                            {
                                Int2 p1 = p.Foward(EDirFlags.NORTH);
                                Int2 p2 = p.Foward(EDirFlags.WEST);
                                if (IsWalkable(p1) && IsWalkable(p2))
                                {
                                    primaryB.JumpDirFlags |= EDirFlags.SOUTH | EDirFlags.EAST;
                                }
                                break;
                            }
                        case EDirFlags.SOUTHWEST:
                            {
                                Int2 p1 = p.Foward(EDirFlags.SOUTH);
                                Int2 p2 = p.Foward(EDirFlags.WEST);
                                if (IsWalkable(p1) && IsWalkable(p2))
                                {
                                    primaryB.JumpDirFlags |= EDirFlags.NORTH | EDirFlags.EAST;
                                }
                                break;
                            }
                        default:
                            throw new ArgumentException();
                    }
                }
            }
        }
    }

    private void MarkStraight()
    {
        // . . .
        // W . .
        // . . .
        for (int y = 0; y < Height; ++y)
        { // WEST
            bool isJumpPointLastSeen = false;
            int distance = -1;
            for (int x = 0; x < Width; ++x)
            {
                Int2 p = new Int2(x, y);
                JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                if (block == null)
                {
                    distance = -1;
                    isJumpPointLastSeen = false;
                    continue;
                }

                distance++;
                if (isJumpPointLastSeen)
                {
                    block.SetDistance(EDirFlags.WEST, distance); // Straight Distance
                }
                else
                {
                    block.SetDistance(EDirFlags.WEST, -distance); // Straight-Wall Distance
                }

                if (block.IsJumpable(EDirFlags.EAST))
                {
                    distance = 0;
                    isJumpPointLastSeen = true;
                }
            }
        } // WEST

        // . . .
        // . . E
        // . . .
        for (int y = 0; y < Height; ++y)
        { // EAST
            bool isJumpPointLastSeen = false;
            int distance = -1;
            for (int x = Width - 1; x >= 0; --x)
            {
                Int2 p = new Int2(x, y);
                JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                if (block == null)
                {
                    distance = -1;
                    isJumpPointLastSeen = false;
                    continue;
                }

                distance++;
                if (isJumpPointLastSeen)
                {
                    block.SetDistance(EDirFlags.EAST, distance); // Straight Distance
                }
                else
                {
                    block.SetDistance(EDirFlags.EAST, -distance); // Straight-Wall Distance
                }

                if (block.IsJumpable(EDirFlags.WEST))
                {
                    distance = 0;
                    isJumpPointLastSeen = true;
                }
            }
        } // EAST

        // . . .
        // . . .
        // . S .
        for (int x = 0; x < Width; ++x)
        { // SOUTH
            bool isJumpPointLastSeen = false;
            int distance = -1;
            for (int y = Height - 1; y >= 0; --y)
            {
                Int2 p = new Int2(x, y);
                JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                if (block == null)
                {
                    distance = -1;
                    isJumpPointLastSeen = false;
                    continue;
                }

                distance++;
                if (isJumpPointLastSeen)
                {
                    block.SetDistance(EDirFlags.SOUTH, distance); // Straight Distance
                }
                else
                {
                    block.SetDistance(EDirFlags.SOUTH, -distance); // Straight-Wall Distance
                }

                if (block.IsJumpable(EDirFlags.NORTH))
                {
                    distance = 0;
                    isJumpPointLastSeen = true;
                }
            }
        } // SOUTH

        // . N .
        // . . .
        // . . .
        for (int x = 0; x < Width; ++x)
        { // NORTH
            bool isJumpPointLastSeen = false;
            int distance = -1;

            for (int y = 0; y < Height; ++y)
            {
                Int2 p = new Int2(x, y);
                JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                if (block == null)
                {
                    distance = -1;
                    isJumpPointLastSeen = false;
                    continue;
                }

                distance++;
                if (isJumpPointLastSeen)
                {
                    block.SetDistance(EDirFlags.NORTH, distance); // Straight Distance
                }
                else
                {
                    block.SetDistance(EDirFlags.NORTH, -distance); // Straight-Wall Distance
                }

                if (block.IsJumpable(EDirFlags.SOUTH))
                {
                    distance = 0;
                    isJumpPointLastSeen = true;
                }
            }
        } // NORTH
    }

    private void MarkDiagonal()
    {
        // * N .
        // W . .
        // . . .
        for (int y = 0; y < Height; ++y)
        { // NORTH & WEST
            for (int x = 0; x < Width; ++x)
            {
                Int2 p = new Int2(x, y);
                JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                if (block == null)
                {
                    continue;
                }

                if (x == 0 || y == 0)
                {
                    block.SetDistance(EDirFlags.NORTHWEST, 0); // Diagonal-Wall Distance
                    continue;
                }

                Int2 p1 = p.Foward(EDirFlags.NORTH);
                Int2 p2 = p.Foward(EDirFlags.NORTHWEST);
                Int2 p3 = p.Foward(EDirFlags.WEST);
                bool p1Walkable = IsWalkable(p1);
                bool p3Walkable = IsWalkable(p3);

                if (!p1Walkable || !IsWalkable(p2) || !p3Walkable)
                {
                    block.SetDistance(EDirFlags.NORTHWEST, 0); // Diagonal-Wall Distance
                    continue;
                }

                JPSPlusMapBakerBlock prevBlock = GetBlockOrNull(p2);
                if (p1Walkable && p3Walkable
                    && (prevBlock.GetDistance(EDirFlags.NORTH) > 0 || prevBlock.GetDistance(EDirFlags.WEST) > 0))
                {
                    block.SetDistance(EDirFlags.NORTHWEST, 1); // Initial Diagonal Distance
                    continue;
                }

                int distanceFromPrev = prevBlock.GetDistance(EDirFlags.NORTHWEST);
                if (distanceFromPrev > 0)
                {
                    block.SetDistance(EDirFlags.NORTHWEST, distanceFromPrev + 1); // Diagonal Distance
                }
                else
                {
                    block.SetDistance(EDirFlags.NORTHWEST, distanceFromPrev - 1); // Diagonal-Wall Distance
                }
            }
        } // NORTH & WEST

        // . N *
        // . . E
        // . . .
        for (int y = 0; y < Height; ++y)
        { // NORTH & EAST
            for (int x = Width - 1; x >= 0; --x)
            {
                Int2 p = new Int2(x, y);
                JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                if (block == null)
                {
                    continue;
                }

                if (x == Width - 1 || y == 0)
                {
                    block.SetDistance(EDirFlags.NORTHEAST, 0); // Diagonal-Wall Distance
                    continue;
                }

                Int2 p1 = p.Foward(EDirFlags.NORTH);
                Int2 p2 = p.Foward(EDirFlags.NORTHEAST);
                Int2 p3 = p.Foward(EDirFlags.EAST);
                bool p1Walkable = IsWalkable(p1);
                bool p3Walkable = IsWalkable(p3);

                if (!p1Walkable || !IsWalkable(p2) || !p3Walkable)
                {
                    block.SetDistance(EDirFlags.NORTHEAST, 0); // Diagonal-Wall Distance
                    continue;
                }

                JPSPlusMapBakerBlock prevBlock = GetBlockOrNull(p2);
                if (p1Walkable && p3Walkable
                    && (prevBlock.GetDistance(EDirFlags.NORTH) > 0 || prevBlock.GetDistance(EDirFlags.EAST) > 0))
                {
                    block.SetDistance(EDirFlags.NORTHEAST, 1); // Initial Diagonal Distance
                    continue;
                }

                int distanceFromPrev = prevBlock.GetDistance(EDirFlags.NORTHEAST);
                if (distanceFromPrev > 0)
                {
                    block.SetDistance(EDirFlags.NORTHEAST, distanceFromPrev + 1); // Diagonal Distance
                }
                else
                {
                    block.SetDistance(EDirFlags.NORTHEAST, distanceFromPrev - 1); // Diagonal-Wall Distance
                }
            }
        } // NORTH & EAST

        // . . .
        // W . .
        // * S .
        for (int y = Height - 1; y >= 0; --y)
        { // SOUTH & WEST
            for (int x = 0; x < Width; ++x)
            {
                Int2 p = new Int2(x, y);
                JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                if (block == null)
                {
                    continue;
                }

                if (x == 0 || y == Height - 1)
                {
                    block.SetDistance(EDirFlags.SOUTHWEST, 0); // Diagonal-Wall Distance
                    continue;
                }

                Int2 p1 = p.Foward(EDirFlags.SOUTH);
                Int2 p2 = p.Foward(EDirFlags.SOUTHWEST);
                Int2 p3 = p.Foward(EDirFlags.WEST);
                bool p1Walkable = IsWalkable(p1);
                bool p3Walkable = IsWalkable(p3);

                if (!p1Walkable || !IsWalkable(p2) || !p3Walkable)
                {
                    block.SetDistance(EDirFlags.SOUTHWEST, 0); // Diagonal-Wall Distance
                    continue;
                }

                JPSPlusMapBakerBlock prevBlock = GetBlockOrNull(p2);
                if (p1Walkable && p3Walkable
                    && (prevBlock.GetDistance(EDirFlags.SOUTH) > 0 || prevBlock.GetDistance(EDirFlags.WEST) > 0))
                {
                    block.SetDistance(EDirFlags.SOUTHWEST, 1); // Initial Diagonal Distance
                    continue;
                }

                int distanceFromPrev = prevBlock.GetDistance(EDirFlags.SOUTHWEST);
                if (distanceFromPrev > 0)
                {
                    block.SetDistance(EDirFlags.SOUTHWEST, distanceFromPrev + 1); // Diagonal Distance
                }
                else
                {
                    block.SetDistance(EDirFlags.SOUTHWEST, distanceFromPrev - 1); // Diagonal-Wall Distance
                }
            }
        } // SOUTH & WEST

        // . . .
        // . . E
        // . S *
        for (int y = Height - 1; y >= 0; --y)
        { // SOUTH & EAST
            for (int x = Width - 1; x >= 0; --x)
            {
                Int2 p = new Int2(x, y);
                JPSPlusMapBakerBlock block = GetBlockOrNull(p);
                if (block == null)
                {
                    continue;
                }

                if (x == Width - 1 || y == Height - 1)
                {
                    block.SetDistance(EDirFlags.SOUTHEAST, 0); // Diagonal-Wall Distance
                    continue;
                }

                Int2 p1 = p.Foward(EDirFlags.SOUTH);
                Int2 p2 = p.Foward(EDirFlags.SOUTHEAST);
                Int2 p3 = p.Foward(EDirFlags.EAST);
                bool p1Walkable = IsWalkable(p1);
                bool p3Walkable = IsWalkable(p3);

                if (!p1Walkable || !IsWalkable(p2) || !p3Walkable)
                {
                    block.SetDistance(EDirFlags.SOUTHEAST, 0); // Diagonal-Wall Distance
                    continue;
                }

                JPSPlusMapBakerBlock prevBlock = GetBlockOrNull(p2);
                if (p1Walkable && p3Walkable
                    && (prevBlock.GetDistance(EDirFlags.SOUTH) > 0 || prevBlock.GetDistance(EDirFlags.EAST) > 0))
                {
                    block.SetDistance(EDirFlags.SOUTHEAST, 1); // Initial Diagonal Distance
                    continue;
                }

                int distanceFromPrev = prevBlock.GetDistance(EDirFlags.SOUTHEAST);
                if (distanceFromPrev > 0)
                {
                    block.SetDistance(EDirFlags.SOUTHEAST, distanceFromPrev + 1); // Diagonal Distance
                }
                else
                {
                    block.SetDistance(EDirFlags.SOUTHEAST, distanceFromPrev - 1); // Diagonal-Wall Distance
                }
            }
        } // SOUTH & EAST
    }

    private bool IsWalkable(in Int2 p)
    {
        if (!IsInBoundary(p))
        {
            return false;
        }
        return BlockLUT[p.Y, p.X] >= 0;
    }

    private bool IsInBoundary(in Int2 p)
    {
        return 0 <= p.X && p.X < Width && 0 <= p.Y && p.Y < Height;
    }

    private JPSPlusMapBakerBlock GetBlockOrNull(in Int2 p)
    {
        if (!IsInBoundary(p))
        {
            return null;
        }

        int index = BlockLUT[p.Y, p.X];
        if (index < 0)
        {
            return null;
        }
        return Blocks[index];
    }
}
