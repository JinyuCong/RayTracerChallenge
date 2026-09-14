using System.ComponentModel.Design.Serialization;
using System.Text;

namespace RayTracerChallenge.RayTracer;
using System;

public class ObjParser
{
    public int IgnoredLines { get; set; }
    public List<Tuple4> Vertices { get; set; } = new();  // 顶点
    public List<Tuple4> Normals { get; set; } = new();
    public Group RootGroup { get; } = new Group();
    private readonly Dictionary<string, Group> _groups = new Dictionary<string, Group>();
    private Group _current;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    public ObjParser(string filePath)
    {
        Vertices.Add(Tuple4.Point(0, 0, 0));
        Normals.Add(Tuple4.Point(0, 0, 0));
        _current = RootGroup;
        
        foreach (var line in File.ReadLines(filePath))
        {
            var parts = line.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                IgnoredLines++;
                continue;
            }
            
            switch (parts[0])
            {
                case "v": 
                    ParseVertex(parts);
                    break;
                case "vn":
                    ParseNormal(parts);
                    break;
                case "f":
                    ParseFace(parts);
                    break;
                case "g":
                    ParseGroup(parts);
                    break;
                default:
                    IgnoredLines++;
                    break;
            }
        }
    }

    private static double Num(string s)
    {
        return double.Parse(s, System.Globalization.CultureInfo.InvariantCulture);
    }

    private void ParseVertex(string[] parts)
    {
        Vertices.Add(Tuple4.Point(Num(parts[1]), Num(parts[2]), Num(parts[3])));
    }

    private void ParseNormal(string[] p)
    {
        Normals.Add(Tuple4.Vector(Num(p[1]), Num(p[2]), Num(p[3])));
    }
    
    private void ParseGroup(string[] parts)
    {
        _groups[parts[1]] = new Group();
        _current = _groups[parts[1]];
    }
    
    private void ParseFace(string[] p)
    {
        // 扇形三角化：把 n 边形拆成 n-2 个三角形，都以第一个顶点为轴
        for (int i = 2; i < p.Length - 1; i++)
        {
            var (v1, n1) = Ref(p[1]);
            var (v2, n2) = Ref(p[i]);
            var (v3, n3) = Ref(p[i + 1]);

            Shape tri = (n1 > 0 && n2 > 0 && n3 > 0)
                ? new SmoothTriangle(Vertices[v1], Vertices[v2], Vertices[v3], Normals[n1],  Normals[n2],  Normals[n3])
                : new Triangle(Vertices[v1], Vertices[v2], Vertices[v3]);

            _current.AddChild(tri);
        }
    }

    private static (int v, int n) Ref(string token)
    {
        var seg = token.Split('/');
        int v = int.Parse(seg[0]);
        int n = (seg.Length >= 3 && seg[2] != "") ? int.Parse(seg[2]) : 0;
        return (v, n);
    }
}