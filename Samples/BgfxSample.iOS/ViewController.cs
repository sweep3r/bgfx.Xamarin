#define BGFX
using System;
using UIKit;
using Metal;
using MetalKit;

namespace BgfxSample.iOS
{

    public partial class ViewController : UIViewController
    {
        private Renderer _renderer;
        
        public ViewController(IntPtr handle) : base(handle)
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
            MTKView mtkView = new MTKView(View.Frame, device)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            View.AddSubview(mtkView);

            var constraints = new[]
            {
                mtkView.LeftAnchor.ConstraintEqualTo(View.LeftAnchor),
                mtkView.RightAnchor.ConstraintEqualTo(View.RightAnchor),
                mtkView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                mtkView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor, 0.5f),
            };

            NSLayoutConstraint.ActivateConstraints(constraints);

            _renderer.InitRenderer(mtkView, device);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}