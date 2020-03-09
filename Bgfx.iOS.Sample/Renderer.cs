#define BGFX
using Foundation;

using CoreGraphics;
using Metal;
using MetalKit;
using System.IO;
using Ara3D;
using System;
#if BGFX
using Bgfx;
#endif
namespace BgfxSample.iOS
{
    public static class Data
    {

        public struct PosColorVertex
        {


            float X;
            float Y;
            float Z;
            uint Abgr;

            public PosColorVertex(float x, float y, float z, uint agbr) : this()
            {
                X = x;
                Y = y;
                Z = z;
                Abgr = agbr;
            }
        };

        public static PosColorVertex[] cubeVertices = new[]
        {
            new PosColorVertex(-1.0f,  1.0f,  1.0f, 0xff000000),
            new PosColorVertex(1.0f,  1.0f,  1.0f, 0xff0000ff ),
            new PosColorVertex(-1.0f, -1.0f,  1.0f, 0xff00ff00 ),
            new PosColorVertex( 1.0f, -1.0f,  1.0f, 0xff00ffff ),
            new PosColorVertex(-1.0f,  1.0f, -1.0f, 0xffff0000 ),
            new PosColorVertex( 1.0f,  1.0f, -1.0f, 0xffff00ff ),
            new PosColorVertex(-1.0f, -1.0f, -1.0f, 0xffffff00 ),
            new PosColorVertex( 1.0f, -1.0f, -1.0f, 0xffffffff ),
        };

        public static ushort[] cubeTriList =
        {
            0, 1, 2,
            1, 3, 2,
            4, 6, 5,
            5, 6, 7,
            0, 2, 4,
            4, 2, 6,
            1, 5, 3,
            5, 7, 3,
            0, 4, 1,
            4, 5, 1,
            2, 3, 6,
            6, 3, 7,
        };
    }

    public class Renderer : MTKViewDelegate
    {
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private Program _program;
        private int _frame;

        public override void DrawableSizeWillChange(MTKView view, CGSize size)
        {

#if BGFX
            Bgfx.Bgfx.Reset((int)view.Bounds.Size.Width, (int)view.Bounds.Size.Height, 0, 0);
#endif
        }

        public override void Draw(MTKView view)
        {
#if BGFX
            Bgfx.Bgfx.Reset((int)view.Bounds.Size.Width, (int)view.Bounds.Size.Height, 0, 0);
            Bgfx.Bgfx.SetViewClear(0, (ClearTargets.Color | ClearTargets.Depth), 0x443355FF, 1.0f, 0);
            Bgfx.Bgfx.SetViewRect(0, 0, 0, (ushort)view.Bounds.Size.Width, (ushort)view.Bounds.Size.Height);

            var target = Vector3.Zero;
            var camera = new Vector3(0, 0, -5);
            var lookAtView = CreateLookAt(camera, target, Vector3.UnitY);


            var fov = MathOps.ToDegrees(60f);
            var projectionMatrix = CreatePerspectiveFieldOfView(fov, (float)(view.Bounds.Size.Width / view.Bounds.Size.Height), 0.1f, 100f);
            
            Bgfx.Bgfx.SetViewTransform(0, lookAtView.ToFloats(), projectionMatrix.ToFloats());

            float offset = (float)_frame;
            var rotation = Matrix4x4.CreateFromYawPitchRoll(offset * 0.01f, offset * 0.01f, offset * 0.01f);

            Bgfx.Bgfx.SetTransform(rotation.ToFloats());

            Bgfx.Bgfx.SetVertexBuffer(0, _vertexBuffer);
            Bgfx.Bgfx.SetIndexBuffer(_indexBuffer);
            Bgfx.Bgfx.Submit(0, _program);

            _frame = Bgfx.Bgfx.Frame();
            System.Diagnostics.Debug.WriteLine($"Frame {_frame}");
#endif
        }

        public void InitRenderer(MTKView view, IMTLDevice device)
        {
#if BGFX
            unsafe
            {
                var platformData = new PlatformData();
                platformData.WindowHandle = view.Handle;
                platformData.Context = device.Handle;

                Bgfx.Bgfx.SetPlatformData(platformData);

                var settings = new InitSettings();
                settings.Backend = RendererBackend.Metal;
                settings.Width = (int)view.Bounds.Width;
                settings.Height = (int)view.Bounds.Height;
                settings.ResetFlags = ResetFlags.Vsync;
                settings.PlatformData = platformData;
                //settings.limits.maxEncoders = 128;

                Bgfx.Bgfx.ManuallyRenderFrame();
                Bgfx.Bgfx.Init(settings);

                var vertexLayout = new VertexLayout();
                vertexLayout.Begin(RendererBackend.Metal);
                vertexLayout.Add(VertexAttributeUsage.Position, 3, VertexAttributeType.Float);
                vertexLayout.Add(VertexAttributeUsage.Color0, 4, VertexAttributeType.UInt8, true);
                vertexLayout.End();

                var vertexBuffer = new VertexBuffer(MemoryBlock.FromArray(Data.cubeVertices), vertexLayout);
                var indexBuffer = new IndexBuffer(MemoryBlock.FromArray(Data.cubeTriList));

                var vertexShader = LoadShader("vs_cubes.bin");
                var fragmentShader = LoadShader("fs_cubes.bin");
                var program = new Program(vertexShader, fragmentShader);


                _vertexBuffer = vertexBuffer;
                _indexBuffer = indexBuffer;
                _program = program;

            }

            Bgfx.Bgfx.Touch(0);
#endif
            view.Delegate = this;

        }

        Shader LoadShader(string fileName)
        {
            var filePath = System.IO.Path.Combine(NSBundle.MainBundle.ResourcePath, "shaders", "metal", fileName);
            var file = File.OpenRead(filePath);
            var buffer = new byte[file.Length];
            file.Read(buffer, 0, (int)file.Length);
            return new Shader(MemoryBlock.FromArray(buffer));
        }
        
        public static Matrix4x4 CreateLookAt(
            Vector3 cameraPosition,
            Vector3 cameraTarget,
            Vector3 cameraUpVector)
        {
            var view = (cameraTarget - cameraPosition).Normalize();
            var uxv = cameraUpVector.Cross(view);
            var right = uxv.Normalize();
            var up = view.Cross(right);

            Matrix4x4 matrix4x4 = new Matrix4x4();
            matrix4x4.M11 = right.X;
            matrix4x4.M12 = up.X;
            matrix4x4.M13 = view.X;

            matrix4x4.M21 = right.Y;
            matrix4x4.M22 = up.Y;
            matrix4x4.M23 = view.Y;

            matrix4x4.M31 = right.Z;
            matrix4x4.M32 = up.Z;
            matrix4x4.M33 = view.Z;

            matrix4x4.M41 = -Vector3.Dot(right, cameraPosition);
            matrix4x4.M42 = -Vector3.Dot(up, cameraPosition);
            matrix4x4.M43 = -Vector3.Dot(view, cameraPosition);
            matrix4x4.M44 = 1.0f;
           
            return matrix4x4;
        }
        
        public static Matrix4x4 CreatePerspectiveFieldOfView(
            float fieldOfView,
            float aspectRatio,
            float nearPlaneDistance,
            float farPlaneDistance)
        {
            if (fieldOfView <= 0.0 || fieldOfView >= 3.14159274101257)
                throw new ArgumentOutOfRangeException(nameof (fieldOfView));
            if (nearPlaneDistance <= 0.0)
                throw new ArgumentOutOfRangeException(nameof (nearPlaneDistance));
            if (farPlaneDistance <= 0.0)
                throw new ArgumentOutOfRangeException(nameof (farPlaneDistance));
            if (nearPlaneDistance >= (double) farPlaneDistance)
                throw new ArgumentOutOfRangeException(nameof (nearPlaneDistance));
            float height = 1f / (fieldOfView * 0.5f).Tan();
            float width = height * 1.0f / aspectRatio;
            
            Matrix4x4 matrix4x4 = new Matrix4x4();
            var diff = farPlaneDistance - nearPlaneDistance;
            var aa = (farPlaneDistance + nearPlaneDistance) / diff;
            var bb = (nearPlaneDistance * aa);

            matrix4x4.M11 = width;
            matrix4x4.M22 = height;
            matrix4x4.M33 = aa;
            matrix4x4.M34 = 1.0f;
            matrix4x4.M43 = -bb;
            
            // matrix4x4.M11 = width;
            // matrix4x4.M12 = matrix4x4.M13 = matrix4x4.M14 = 0.0f;
            // matrix4x4.M22 = height;
            // matrix4x4.M21 = matrix4x4.M23 = matrix4x4.M24 = 0.0f;
            // matrix4x4.M31 = matrix4x4.M32 = 0.0f;
            // float num3 = float.IsPositiveInfinity(farPlaneDistance) ? -1f : farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            // matrix4x4.M33 = num3;
            // matrix4x4.M34 = -1f;
            // matrix4x4.M41 = matrix4x4.M42 = matrix4x4.M44 = 0.0f;
            // matrix4x4.M43 = nearPlaneDistance * num3;
            return matrix4x4;
        }
    }
}