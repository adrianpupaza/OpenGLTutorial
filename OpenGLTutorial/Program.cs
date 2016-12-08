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
        private static VBO<Vector3> _triangle, _square;
        private static VBO<int> _triangleElements, _squareElements;
        private static VBO<Vector3> _triangleColor, _squareColor;
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

            // compile the shader program
            _program = new ShaderProgram(VertexShader, FragmentShader);

            // set the view and projection matrix, which are static throughout this tutorial
            _program.Use();
            _program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)Width / Height, 0.1f, 1000f));
            _program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, new Vector3(0, 1, 0)));

            // create a triangle
            _triangle = new VBO<Vector3>(new[] { new Vector3(0, 1, 0), new Vector3(-1, -1, 0), new Vector3(1, -1, 0) });
            _triangleColor = new VBO<Vector3>(new[] { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1), });
            _triangleElements = new VBO<int>(new[] { 0, 1, 2 }, BufferTarget.ElementArrayBuffer);

            // create a square
            _square = new VBO<Vector3>(new[] { new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, -1, 0), new Vector3(-1, -1, 0) });
            _squareColor = new VBO<Vector3>(new[] {new Vector3(0.5f,0.5f,1), new Vector3(0.5f, 0.5f, 1), new Vector3(0.5f, 0.5f, 1), new Vector3(0.5f, 0.5f, 1) });
            _squareElements = new VBO<int>(new[] { 0, 1, 2, 3 }, BufferTarget.ElementArrayBuffer);

            _watch = new Stopwatch();
            _watch.Start();

            Glut.glutMainLoop();
        }

        private static void OnClose()
        {
            // dispose of all of the resources that were created
            _triangle.Dispose();
            _triangleElements.Dispose();
            _triangleColor.Dispose();
            _square.Dispose();
            _squareColor.Dispose();
            _squareElements.Dispose();
            _program.DisposeChildren = true;
            _program.Dispose();
        }

        private static void OnDisplay()
        {

        }

        private static void OnRenderFrame()
        {
            _watch.Stop();
            float deltaTime = (float)_watch.ElapsedTicks/ Stopwatch.Frequency;
            _watch.Restart();

            _angle += deltaTime;

            // set up the OpenGL viewport and clear both the color and depth bits
            Gl.Viewport(0, 0, Width, Height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // use our shader program
            Gl.UseProgram(_program);

            // transform the triangle
            _program["model_matrix"].SetValue(Matrix4.CreateRotationY(_angle) * Matrix4.CreateTranslation(new Vector3(-1.5f, 0, 0)));

            // bind the vertex attribute arrays for the triangle (the hard way)
            uint vertexPositionIndex = (uint)Gl.GetAttribLocation(_program.ProgramID, "vertexPosition");
            Gl.EnableVertexAttribArray(vertexPositionIndex);
            Gl.BindBuffer(_triangle);
            Gl.VertexAttribPointer(vertexPositionIndex, _triangle.Size, _triangle.PointerType, true, 12, IntPtr.Zero);
            Gl.BindBufferToShaderAttribute(_triangleColor, _program, "vertexColor");
            Gl.BindBuffer(_triangleElements);

            // draw the triangle
            Gl.DrawElements(BeginMode.Triangles, _triangleElements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            // transform the square
            _program["model_matrix"].SetValue(Matrix4.CreateRotationX(_angle) * Matrix4.CreateTranslation(new Vector3(1.5f, 0, 0)));

            // bind the vertex attribute arrays for the square (the easy way)
            Gl.BindBufferToShaderAttribute(_square, _program, "vertexPosition");
            Gl.BindBuffer(_squareElements);
            Gl.BindBufferToShaderAttribute(_squareColor, _program, "vertexColor");

            // draw the square
            Gl.DrawElements(BeginMode.Quads, _squareElements.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            Glut.glutSwapBuffers();
        }

        public static string VertexShader = @"
#version 130
in vec3 vertexPosition;
in vec3 vertexColor;
out vec3 color;
uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;
void main(void)
{
    color = vertexColor;
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
";

        public static string FragmentShader = @"
#version 130
in vec3 color;
out vec4 fragment;
void main(void)
{
    fragment = vec4(color, 1);
}
";
    }
}
