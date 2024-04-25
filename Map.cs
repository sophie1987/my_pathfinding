using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

public class Map
{
    public int Width { get; set; } // 地图宽度
    public int Height { get; set; } // 地图高度

    public Boundaries Boundaries { get; set; } // 地图边界，boundaries包含两个节点，左上角和右下角

    public int FreeTiles { get; set; } // 可通行节点个数

    //Consider storing obstacles in a Hashset to save memory on large maps
    //Obstacles are stores with the y value in the first array and the x value in the second array
    public bool[][] Obstacles { get; set; } // 障碍节点

    //Original characters that forms the whole map
    //Tiles are stored with the y value in the first array and the x value in the second array
    //public char[][] Tiles { get; set; } // 地图的所有节点
    public int[][] Tile_weights { get; set; }

    public HashSet<char> tiles { get; set; }


    private Map() { }


    //Returns whether the tile is a valid free tile in the map or not，可通行节点，未超过边缘，非障碍节点
    public bool IsFreeTile(GridTile tile)
    {
        return tile.x >= 0 && tile.x < Width &&
            tile.y >= 0 && tile.y < Height &&
            !Obstacles[tile.y][tile.x];
    }


    public static List<FileInfo> GetMaps()
    {
        string BaseMapDirectory = GetBaseMapDirectory();
        DirectoryInfo d = new DirectoryInfo(BaseMapDirectory);
        return new List<FileInfo>(d.GetFiles("*.map"));
    }

    public static List<FileInfo> GetTestScens()
    {
        string BaseMapDirectory = GetBaseMapDirectory();
        DirectoryInfo d = new DirectoryInfo(BaseMapDirectory);
        return new List<FileInfo>(d.GetFiles("*.scen"));
    }

    /// <summary>
    /// Loads a map from the base map directory
    /// </summary>
    /// <param name="MapName">File from which to load the map</param>
    public static Map LoadMap(string MapName)
    {
        string BaseMapDirectory = GetBaseMapDirectory();
        FileInfo f = new FileInfo(Path.Combine(BaseMapDirectory, MapName));

        return ReadMap(f);
    }

    /// <summary>
    /// Gets the base map directory
    /// </summary>
    private static string GetBaseMapDirectory()
    {
        //return Path.Combine(Application.dataPath, "../Maps/map");
        return Path.GetFullPath("../../../Maps/map");
    }

    /// <summary>
    /// Reads map and returns a map object ，从地图文件中读取地图返回为一个map对象
    /// </summary>
    private static Map ReadMap(FileInfo file)
    {
        Map map = new Map();

        using (FileStream fs = file.OpenRead())
        using (StreamReader sr = new StreamReader(fs))
        {

            //Line 1 : type octile
            ReadLine(sr, "type octile");

            //Line 2 : height
            map.Height = ReadIntegerValue(sr, "height"); // 2048

            //Line 3 : width
            map.Width = ReadIntegerValue(sr, "width"); // 2048

            //Set boundaries according to width and height
            map.Boundaries = new Boundaries
            {
                Min = new GridTile(0, 0), //左上角
                Max = new GridTile(map.Width - 1, map.Height - 1) //右下角
            };

            //Line 4 to end : map
            ReadLine(sr, "map");

            map.Obstacles = new bool[map.Height][];
            map.FreeTiles = 0;

            //Store the array of characters that makes up the map
            //map.Tiles = new char[map.Height][];
            map.Tile_weights = new int[map.Height][];
            map.tiles = new HashSet<char> { };

            //Read tiles section
            map.ReadTiles(sr); // 读取tile和obstacles

            return map;
        }
    }

    /// <summary>
    /// Read a line and expect the line to be the value passed in arguments
    /// </summary>
    private static void ReadLine(StreamReader sr, string value)
    {
        string line = sr.ReadLine();
        if (line != value) throw new Exception(
                string.Format("Invalid format. Expected: {0}, Actual: {1}", value, line));
    }

    /// <summary>
    /// Returns an integer value from the streamreader that comes
    /// right after a key separated by a space.
    /// I.E. width 5
    /// </summary>
    private static int ReadIntegerValue(StreamReader sr, string key)
    {
        string[] block = sr.ReadLine().Split(null);
        if (block[0] != key) throw new Exception(
                string.Format("Invalid format. Expected: {0}, Actual: {1}", key, block[0]));

        return int.Parse(block[1]);
    }

    /// <summary>
    /// Read tiles from the map file, adding tiles and filling obstacles in the array
    /// </summary>
    private void ReadTiles(StreamReader sr)
    {
        char c;
        string line;

        for (int i = 0; i < Height; ++i) // 2048
        {
            line = sr.ReadLine(); // 读每行数据
            Obstacles[i] = new bool[Width]; // obstacles，bool[height][width]
            Tile_weights[i] = new int[Width];
            //Tiles[i] = new char[Width]; // Tiles为所有节点char[height][width]

            for (int j = 0; j < Width; ++j) // 读每格数据
            {
                c = line[j];
                //Tiles[i][j] = c;
                tiles.Add(c);

                switch (c)
                {
                    case 'D':
                    case 'P':
                    case 'O':
                    case '@':
                    case 'W':
                        Obstacles[i][j] = true; //不可通行节点
                        break;
                    case 'B':
                    case 'G':
                    case 'C':
                    case '.':
                    case 'I':
                        Obstacles[i][j] = false;
                        Tile_weights[i][j] = 1;
                        FreeTiles++;
                        break;
                    case 'T':
                    case 'F':
                        Obstacles[i][j] = false;
                        Tile_weights[i][j] = 2;
                        FreeTiles++;
                        break;
                    case 'A':
                        Obstacles[i][j] = false;
                        Tile_weights[i][j] = 3;
                        FreeTiles++;
                        break;
                    case 'K':
                    case 'N':
                        Obstacles[i][j] = false;
                        Tile_weights[i][j] = 4;
                        FreeTiles++;
                        break;
                    case 'E':
                    case 'M':
                        Obstacles[i][j] = false;
                        Tile_weights[i][j] = 5;
                        FreeTiles++;
                        break;
                    default:
                        Tile_weights[i][j] = 1;
                        Obstacles[i][j] = false; // 这些字母表示可通行区域，obstacles非空，可通行节点个数+1
                        FreeTiles++;
                        break;
                }
            }
        }
    }

}
