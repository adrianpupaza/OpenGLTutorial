using System;
using System.Diagnostics;
using OpenGL;
using Tao.FreeGlut;

namespace OpenGLTutorial
{
    internal class Program
    {
        private const int Width = 1280;
        private const int Height = 720;
        private static ShaderProgram _program;
        private static VBO<Vector3> _cube;
        private static VBO<Vector2> _cubeUv;
        private static VBO<int> _cubeQuads;
        private static Texture _crateTexture;
        private static Stopwatch _watch;
        private static float _angle;

        private static void Main()
        {
            // create an OpenGL window
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(Width, Height);
            Glut.glutCreateWindow("OpenGL Tutorial");

            // provide the Glut callbacks that are necessary for running this tutorial
            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);
            Glut.glutCloseFunc(OnClose);

            // enable depth testing to ensure correct z-ordering of our fragments
            Gl.Enable(EnableCap.DepthTest);

            // compile the shader program
            _program = new ShaderProgram(VertexShader, FragmentShader);

            // set the view and projection matrix, which are static throughout this tutorial
            _program.Use();
            _program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)Width / Height, 0.1f, 1000f));
            _program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, new Vector3(0, 1, 0)));

            // load a crate texture
            _crateTexture = new Texture("crate.jpg");

            // create a crate with vertices and UV coordinates
            _cube = new VBO<Vector3>(new[] {
                new Vector3(1, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1),
                new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1),
                new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1),
                new Vector3(1, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1),
                new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1),
                new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, -1) });
            _cubeUv = new VBO<Vector2>(new[] {
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) });

            _cubeQuads = new VBO<int>(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, BufferTarget.ElementArrayBuffer);

            _watch = Stopwatch.StartNew();

            Glut.glutMainLoop();
        }

        private static void OnClose()
        {
            // dispose of all of the resources that were created
            _cube.Dispose();
            _cubeUv.Dispose();
            _cubeQuads.Dispose();
            _crateTexture.Dispose();
            _program.DisposeChildren = true;
            _program.Dispose();
        }

        private static void OnDisplay()
        {

        }

        private static void OnRenderFrame()
        {
            // calculate how much time has elapsed since the last frame
            _watch.Stop();
            float deltaTime = (float)_watch.ElapsedTicks / Stopwatch.Frequency;
            _watch.Restart();

            // use the deltaTime to adjust the angle of the cube
            _angle += deltaTime;

            // set up the OpenGL viewport and clear both the color and depth bits
            Gl.Viewport(0, 0, Width, Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // use our shader program and bind the crate texture
            Gl.UseProgram(_program);
            Gl.BindTexture(_crateTexture);

            // set the transformation of the cube
            _program["model_matrix"].SetValue(Matrix4.CreateRotationY(_angle / 2) * Matrix4.CreateRotationX(_angle));

            // bind the vertex positions, UV coordinates and element array
            Gl.BindBufferToShaderAttribute(_cube, _program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(_cubeUv, _program, "vertexUV");
            Gl.BindBuffer(_cubeQuads);

            // draw the textured cube
            Gl.DrawElements(BeginMode.Quads, _cubeQuads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            Glut.glutSwapBuffers();
        }

        public static string VertexShader = @"
#version 130
in vec3 vertexPosition;
in vec2 vertexUV;
out vec2 uv;
uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;
void main(void)
{
    uv = vertexUV;
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
";

        public static string FragmentShader = @"
#version 130
uniform sampler2D texture;
in vec2 uv;
out vec4 fragment;
void main(void)
{
    fragment = texture2D(texture, uv);
}
";
    }
}
