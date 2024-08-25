using System;
using System.Windows;
using System.Windows.Interop;
using SharpDX;
using SharpDX.Direct3D9;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SharpCreator
{
    public class DirectXRenderer : IDisposable
    {
        private readonly Window _window;
        private Direct3D _d3d;
        private Device _device;
        private D3DImage _d3dImage;
        private Surface _backBuffer;
        private bool _disposed = false;

        public DirectXRenderer(Window window)
        {
            _window = window;
            InitializeDirectX();
            _window.SizeChanged += OnWindowSizeChanged;
        }

        private void InitializeDirectX()
        {
            try
            {
                _d3d = new Direct3D();
                CreateDevice();
                SetupD3DImage();
                LoadModel("C:\\Users\\maksi\\source\\repos\\SharpCreator\\SharpCreator\\models\\LowPolyChess.obj");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing DirectX: {ex.Message}");
                Dispose(true); // Dispose resources if initialization fails
            }
        }

        private void CreateDevice()
        {
            var presentParams = new PresentParameters
            {
                BackBufferWidth = (int)_window.ActualWidth,
                BackBufferHeight = (int)_window.ActualHeight,
                BackBufferFormat = Format.A8R8G8B8,
                SwapEffect = SwapEffect.Discard,
                DeviceWindowHandle = new WindowInteropHelper(_window).Handle,
                Windowed = true,
                PresentationInterval = PresentInterval.Immediate,
                EnableAutoDepthStencil = true,
                AutoDepthStencilFormat = Format.D16
            };

            _device = new Device(_d3d, 0, DeviceType.Hardware, presentParams.DeviceWindowHandle, CreateFlags.HardwareVertexProcessing, presentParams);
            _backBuffer = _device.GetBackBuffer(0, 0);
        }

        private void SetupD3DImage()
        {
            _d3dImage = new D3DImage();
            var imageControl = (Image)_window.FindName("DirectXImage");

            if (imageControl != null)
            {
                imageControl.Source = _d3dImage;
            }
            else
            {
                throw new InvalidOperationException("Image control with name 'DirectXImage' not found.");
            }
        }

        private void LoadModel(string path)
        {
            // Логика загрузки модели OBJ
            try
            {
                OBJParser parser = new OBJParser();
                parser.LoadModel(path);

                // Дополнительная логика для отображения модели
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading model: {ex.Message}");
            }
        }

        public void Render()
        {
            if (_device == null || _d3dImage == null) return;

            try
            {
                _device.Clear(ClearFlags.Target, new ColorBGRA(0, 0, 0, 255), 1.0f, 0);
                _device.BeginScene();

                // Ваша логика рендеринга

                _device.EndScene();
                _device.Present();

                _d3dImage.Lock();
                _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _backBuffer.NativePointer);
                _d3dImage.Unlock();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Rendering error: {ex.Message}");
            }
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_device != null)
            {
                _device.Reset(new PresentParameters
                {
                    BackBufferWidth = (int)_window.ActualWidth,
                    BackBufferHeight = (int)_window.ActualHeight,
                    BackBufferFormat = Format.A8R8G8B8,
                    SwapEffect = SwapEffect.Discard,
                    DeviceWindowHandle = new WindowInteropHelper(_window).Handle,
                    Windowed = true,
                    PresentationInterval = PresentInterval.Immediate,
                    EnableAutoDepthStencil = true,
                    AutoDepthStencilFormat = Format.D16
                });
                _backBuffer = _device.GetBackBuffer(0, 0);
            }
        }

        // IDisposable implementation
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _d3dImage?.Freeze(); // Ensure D3DImage is not in use
                _backBuffer?.Dispose();
                _device?.Dispose();
                _d3d?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
