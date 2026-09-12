using System.Text;

namespace RayTracerChallenge.RayTracer;
using System;

public class ObjParser
{
    public int IgnoredLines { get; set; }
    public List<Tuple4> Vertices { get; set; } = new List<Tuple4>();  // 顶点
    public List<Tuple4> Normals { get; set; } = new List<Tuple4>();  // 法向量
    public Group DefaultGroup { get; } = new Group();

    private readonly Dictionary<string, Group> _groups = new();
    private Group _current;

    public ObjParser(string filePath)
    {
        Vertices.Add(Tuple4.Point(0, 0, 0));
        Normals.Add(Tuple4.Vector(0, 0, 0));
        _current = DefaultGroup;
        
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
                    ParseFace(parts);
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

    private void ParseNormal(string[] parts)
    {
        Normals.Add(Tuple4.Vector(Num(parts[1]), Num(parts[2]), Num(parts[3])));
    }

    private void ParseFace(string[] parts)
    {
        for (int i = 2; i < parts.Length; i++)
        {
            var tri = new Triangle(
                Vertices[int.Parse(parts[1])],
                Vertices[int.Parse(parts[i])],
                Vertices[int.Parse(parts[i + 1])]);
            _current.AddChild(tri);
        }
    }
}