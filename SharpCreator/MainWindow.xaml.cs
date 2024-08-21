using SharpDX;
using SharpDX.Direct3D9;
using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using SharpDX.Mathematics.Interop;
using WpfPoint = System.Windows.Point;
using System.IO;
using System.Windows.Controls;
using System.Windows.Input;

namespace SharpCreator
{
    public partial class MainWindow : Window
    {
        private Direct3D _d3d;
        private Device _device;
        private D3DImage _d3dImage;
        private Surface _backBuffer;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private Camera _camera;
        private bool _isRightMouseButtonDown = false;
        private WpfPoint _lastMousePosition;
        private string currentDirectory;
        private bool isDragging = false;
        private System.Windows.Point clickPosition;
        private UIElement currentWindow;

        public MainWindow()
        {
            InitializeComponent();
            LoadDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            InitializeDirectX();
            _camera = new Camera(new Vector3(0, 5, -10), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            MouseDown += MainWindow_MouseDown;
            MouseUp += MainWindow_MouseUp;
            MouseMove += MainWindow_MouseMove;
        }

        private void LoadDirectory(string path)
        {
            try
            {
                currentDirectory = path;
                CurrentPath.Text = path;
                var directoryInfo = new DirectoryInfo(path);

                var items = directoryInfo.GetFileSystemInfos().Select(item => new FileItem
                {
                    Name = item.Name,
                    Type = item is DirectoryInfo ? "Папка" : "Файл",
                    Size = item is FileInfo fileInfo ? $"{fileInfo.Length / 1024} KB" : string.Empty,
                    FullPath = item.FullName
                }).ToList();

                FileList.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var parentDirectory = Directory.GetParent(currentDirectory);
            if (parentDirectory != null)
            {
                LoadDirectory(parentDirectory.FullName);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDirectory(currentDirectory);
        }

        private void FileList_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (FileList.SelectedItem is FileItem selectedItem)
            {
                if (Directory.Exists(selectedItem.FullPath))
                {
                    LoadDirectory(selectedItem.FullPath);
                }
                else if (File.Exists(selectedItem.FullPath))
                {
                    MessageBox.Show($"Выбран файл: {selectedItem.FullPath}");
                }
            }
        }

        private void MovableWindow_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                isDragging = true;
                currentWindow = sender as UIElement;
                clickPosition = e.GetPosition(this);
                currentWindow.CaptureMouse();
            }
        }

        private void MovableWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && currentWindow != null)
            {
                System.Windows.Point currentMousePosition = e.GetPosition(this);
                double offsetX = currentMousePosition.X - clickPosition.X;
                double offsetY = currentMousePosition.Y - clickPosition.Y;
                double newLeft = Canvas.GetLeft(currentWindow) + offsetX;
                double newTop = Canvas.GetTop(currentWindow) + offsetY;
                Canvas.SetLeft(currentWindow, newLeft);
                Canvas.SetTop(currentWindow, newTop);
                clickPosition = currentMousePosition;
            }
        }

        private void MovableWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                isDragging = false;
                currentWindow.ReleaseMouseCapture();
                currentWindow = null;
            }
        }

        private void InitializeDirectX()
        {
            _d3d = new Direct3D();

            var presentParams = new PresentParameters
            {
                BackBufferWidth = (int)Width,
                BackBufferHeight = (int)Height,
                BackBufferFormat = Format.A8R8G8B8,
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = new WindowInteropHelper(this).Handle,
                Windowed = true,
                PresentationInterval = PresentInterval.Immediate,
                EnableAutoDepthStencil = true,
                AutoDepthStencilFormat = Format.D16
            };

            _device = new Device(_d3d, 0, DeviceType.Hardware, presentParams.DeviceWindowHandle, CreateFlags.HardwareVertexProcessing, presentParams);

            _d3dImage = new D3DImage();
            DirectXImage.Source = _d3dImage;
            _backBuffer = _device.GetBackBuffer(0, 0);

            LoadModel("C:\\Users\\maksi\\source\\repos\\SharpCreator\\SharpCreator\\models\\LowPolyChess.obj");
        }

        private void LoadModel(string filePath)
        {
            var parser = new OBJParser();
            parser.LoadModel(filePath);

            var vertices = parser.GetVertexBuffer();
            var indices = parser.GetIndexBuffer();

            _vertexBuffer = new VertexBuffer(_device, CustomVertex.Stride * vertices.Length, Usage.WriteOnly, VertexFormat.Position | VertexFormat.Diffuse, Pool.Managed);
            DataStream stream = _vertexBuffer.Lock(0, 0, LockFlags.None);
            stream.WriteRange(vertices);
            _vertexBuffer.Unlock();

            _indexBuffer = new IndexBuffer(_device, sizeof(short) * indices.Length, Usage.WriteOnly, Pool.Managed, true);
            stream = _indexBuffer.Lock(0, 0, LockFlags.None);
            stream.WriteRange(indices);
            _indexBuffer.Unlock();
        }

        private void Render()
        {
            _device.Clear(ClearFlags.Target | ClearFlags.ZBuffer, new RawColorBGRA(0, 120, 215, 255), 1.0f, 0);
            _device.BeginScene();
            _device.SetTransform(TransformState.World, Matrix.Identity);
            _device.SetTransform(TransformState.View, _camera.ViewMatrix);
            _device.SetTransform(TransformState.Projection, Matrix.PerspectiveFovLH((float)Math.PI / 4, (float)Width / (float)Height, 0.1f, 100f));
            _device.SetStreamSource(0, _vertexBuffer, 0, CustomVertex.Stride);
            _device.Indices = _indexBuffer;
            _device.VertexFormat = VertexFormat.Position | VertexFormat.Diffuse;
            _device.SetRenderState(RenderState.FillMode, FillMode.Solid);
            _device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, 0, _vertexBuffer.Description.SizeInBytes / CustomVertex.Stride, 0, _indexBuffer.Description.Size / 2 / 3);
            _device.SetRenderState(RenderState.FillMode, FillMode.Wireframe);
            _device.SetRenderState(RenderState.CullMode, Cull.None);
            _device.SetRenderState(RenderState.Ambient, unchecked((int)0xFF00FF00));
            _device.DrawIndexedPrimitive(PrimitiveType.TriangleList, 0, 0, _vertexBuffer.Description.SizeInBytes / CustomVertex.Stride, 0, _indexBuffer.Description.Size / 2 / 3);
            _device.EndScene();
            _device.Present();
            _d3dImage.Lock();
            _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _backBuffer.NativePointer);
            _d3dImage.AddDirtyRect(new Int32Rect(0, 0, (int)Width, (int)Height));
            _d3dImage.Unlock();
        }

        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Render();
        }

        protected override void OnClosed(EventArgs e)
        {
            _backBuffer?.Dispose();
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
            _device?.Dispose();
            _d3d?.Dispose();
            base.OnClosed(e);
        }

        private void MainWindow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                _isRightMouseButtonDown = true;
                _lastMousePosition = e.GetPosition(this);
            }
        }

        private void MainWindow_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton == System.Windows.Input.MouseButtonState.Released)
            {
                _isRightMouseButtonDown = false;
            }
        }

        private void MainWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_isRightMouseButtonDown)
            {
                var currentPosition = e.GetPosition(this);
                var deltaX = (float)(currentPosition.X - _lastMousePosition.X);
                var deltaY = (float)(currentPosition.Y - _lastMousePosition.Y);
                _camera.Rotate(deltaX * 0.01f, deltaY * 0.01f);
                _lastMousePosition = currentPosition;
                Render();
            }
        }

    }

    public struct CustomVertex
    {
        public Vector3 Position;
        public RawColorBGRA Color;

        public CustomVertex(Vector3 position, RawColorBGRA color)
        {
            Position = position;
            Color = color;
        }

        public static int Stride => sizeof(float) * 3 + sizeof(int);
    }
}