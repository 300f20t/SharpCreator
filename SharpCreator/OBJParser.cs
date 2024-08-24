using SharpDX;
using SharpDX.Mathematics.Interop;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace SharpCreator
{
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
            try
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
                            if (parts.Length >= 4)
                            {
                                float x = float.Parse(parts[1], CultureInfo.InvariantCulture);
                                float y = float.Parse(parts[2], CultureInfo.InvariantCulture);
                                float z = float.Parse(parts[3], CultureInfo.InvariantCulture);
                                Vertices.Add(new Vector3(x, y, z));
                            }
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

                            // Если это многоугольник (например, квадрат), то разбиваем его на треугольники
                            if (face.Length > 3)
                            {
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
            catch (IOException ex)
            {
                // Логирование ошибки чтения файла
                System.Diagnostics.Debug.WriteLine($"Ошибка чтения файла: {ex.Message}");
            }
            catch (FormatException ex)
            {
                // Логирование ошибки формата данных
                System.Diagnostics.Debug.WriteLine($"Ошибка формата данных: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Логирование других непредвиденных ошибок
                System.Diagnostics.Debug.WriteLine($"Неизвестная ошибка: {ex.Message}");
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
                if (face.Length == 3) // Убедимся, что грань является треугольником
                {
                    indices.Add((short)face[0]);
                    indices.Add((short)face[1]);
                    indices.Add((short)face[2]);
                }
            }
            return indices.ToArray();
        }
    }
}
