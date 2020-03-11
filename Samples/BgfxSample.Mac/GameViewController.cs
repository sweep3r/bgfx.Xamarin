using System;
using System.Runtime.InteropServices;
using System.Threading;

using AppKit;
using Foundation;
using Metal;
using MetalKit;
using ModelIO;
using OpenTK;

namespace BgfxSample.Mac
{
    public partial class GameViewController : NSViewController
    {
        // view
        MTKView view;
        private Renderer _renderer;

        public GameViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            InitRenderer();
        }

        private void InitRenderer()
        {
            _renderer = new Renderer();
            var device = MTLDevice.SystemDefault;
            if (device == null)
            {
                throw new NotSupportedException("Metal is not supported");
            }

            var mtkView = (MTKView)View;
            mtkView.Device = device;

            _renderer.InitRenderer(mtkView, device);
        }
    }
}
