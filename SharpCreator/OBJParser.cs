using SharpCreator;
using SharpDX.Mathematics.Interop;
using System.Globalization;
using System.IO;
using SharpDX;

public class OBJParser
{
    public List<Vector3> Vertices { get; private set; }
    public List<int[]> Faces { get; private set; }

    public OBJParser()
    {
        Vertices = new List<Vector3>();
        Faces = new List<int[]>();
    }

    public void LoadModel(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Пропускаем комментарии
                if (line.StartsWith("#"))
                    continue;

                // Парсинг вершин
                if (line.StartsWith("v "))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                    Vertices.Add(new Vector3(x, y, z));
                }

                // Парсинг граней
                else if (line.StartsWith("f "))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    int[] face = new int[parts.Length - 1];

                    for (int i = 1; i < parts.Length; i++)
                    {
                        // Индексы вершин в OBJ начинаются с 1, а не с 0
                        face[i - 1] = int.Parse(parts[i].Split('/')[0]) - 1;
                    }

                    // Треугольник или многоугольник?
                    if (face.Length > 3)
                    {
                        // Если это многоугольник (например, квадрат), то разбиваем его на треугольники
                        for (int i = 1; i < face.Length - 1; i++)
                        {
                            Faces.Add(new int[] { face[0], face[i], face[i + 1] });
                        }
                    }
                    else
                    {
                        Faces.Add(face);
                    }
                }
            }
        }
    }

    public CustomVertex[] GetVertexBuffer()
    {
        // Преобразуем вершины в массив CustomVertex
        CustomVertex[] vertices = new CustomVertex[Vertices.Count];
        for (int i = 0; i < Vertices.Count; i++)
        {
            vertices[i] = new CustomVertex(Vertices[i], new RawColorBGRA(255, 255, 255, 255)); // Белый цвет по умолчанию
        }
        return vertices;
    }

    public short[] GetIndexBuffer()
    {
        // Преобразуем грани в массив индексов
        List<short> indices = new List<short>();
        foreach (var face in Faces)
        {
            indices.Add((short)face[0]);
            indices.Add((short)face[1]);
            indices.Add((short)face[2]);
        }
        return indices.ToArray();
    }
}
