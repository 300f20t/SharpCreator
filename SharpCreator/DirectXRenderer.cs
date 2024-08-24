using System;
using System.Windows;
using System.Windows.Interop;
using SharpDX;
using SharpDX.Direct3D9;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace SharpCreator
{
    public class DirectXRenderer
    {
        private readonly Window _window;
        private Direct3D _d3d;
        private Device _device;
        private D3DImage _d3dImage;
        private Surface _backBuffer;

        public DirectXRenderer(Window window)
        {
            _window = window;
            InitializeDirectX();
        }

        private void InitializeDirectX()
        {
            _d3d = new Direct3D();

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

            _d3dImage = new D3DImage();
            var imageControl = (Image)_window.FindName("DirectXImage"); // Получаем элемент XAML по имени

            if (imageControl != null)
            {
                imageControl.Source = _d3dImage; // Устанавливаем Direct3D изображение как источник
            }

            _backBuffer = _device.GetBackBuffer(0, 0);

            LoadModel("C:\\Users\\maksi\\source\\repos\\SharpCreator\\SharpCreator\\models\\LowPolyChess.obj");
        }

        private void LoadModel(string path)
        {
            // Логика загрузки модели OBJ
            OBJParser parser = new OBJParser();
            parser.LoadModel(path);

            // Дополнительная логика для отображения модели
        }

        public void Render()
        {
            if (_device == null || _d3dImage == null) return;

            _device.Clear(ClearFlags.Target, new ColorBGRA(0, 0, 0, 255), 1.0f, 0);
            _device.BeginScene();

            // Ваша логика рендеринга

            _device.EndScene();
            _device.Present();

            _d3dImage.Lock();
            _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _backBuffer.NativePointer);
            _d3dImage.Unlock();
        }
    }
}
