using System;
using System.Diagnostics;
using OpenGL;
using Tao.FreeGlut;

namespace OpenGLTutorial
{
    internal class Program
    {
        private static int _width = 1280;
        private static int _height = 720;
        private static ShaderProgram _program;
        private static VBO<Vector3> _cube, _cubeNormals;
        private static VBO<Vector2> _cubeUv;
        private static VBO<int> _cubeQuads;
        private static Texture _crateTexture;
        private static Stopwatch _watch;
        private static float _xangle, _yangle;
        private static bool _autoRotate, _lighting = true, _fullscreen;
        private static bool _left, _right, _up, _down;

        private static void Main()
        {
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            Glut.glutInitWindowSize(_width, _height);
            Glut.glutCreateWindow("OpenGL Tutorial");

            Glut.glutIdleFunc(OnRenderFrame);
            Glut.glutDisplayFunc(OnDisplay);

            Glut.glutKeyboardFunc(OnKeyboardDown);
            Glut.glutKeyboardUpFunc(OnKeyboardUp);

            Glut.glutCloseFunc(OnClose);
            Glut.glutReshapeFunc(OnReshape);

            Gl.Enable(EnableCap.DepthTest);

            _program = new ShaderProgram(VertexShader, FragmentShader);

            _program.Use();
            _program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)_width / _height, 0.1f, 1000f));
            _program["view_matrix"].SetValue(Matrix4.LookAt(new Vector3(0, 0, 10), Vector3.Zero, new Vector3(0, 1, 0)));

            _program["light_direction"].SetValue(new Vector3(0, 0, 1));
            _program["enable_lighting"].SetValue(_lighting);

            _crateTexture = new Texture("crate.jpg");

            _cube = new VBO<Vector3>(new[] {
                new Vector3(1, 1, -1), new Vector3(-1, 1, -1), new Vector3(-1, 1, 1), new Vector3(1, 1, 1),         // top
                new Vector3(1, -1, 1), new Vector3(-1, -1, 1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1),     // bottom
                new Vector3(1, 1, 1), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1), new Vector3(1, -1, 1),         // front face
                new Vector3(1, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1),     // back face
                new Vector3(-1, 1, 1), new Vector3(-1, 1, -1), new Vector3(-1, -1, -1), new Vector3(-1, -1, 1),     // left
                new Vector3(1, 1, -1), new Vector3(1, 1, 1), new Vector3(1, -1, 1), new Vector3(1, -1, -1) });      // right
            _cubeNormals = new VBO<Vector3>(new[] {
                new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0),
                new Vector3(0, -1, 0), new Vector3(0, -1, 0), new Vector3(0, -1, 0), new Vector3(0, -1, 0),
                new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 1),
                new Vector3(0, 0, -1), new Vector3(0, 0, -1), new Vector3(0, 0, -1), new Vector3(0, 0, -1),
                new Vector3(-1, 0, 0), new Vector3(-1, 0, 0), new Vector3(-1, 0, 0), new Vector3(-1, 0, 0),
                new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 0, 0) });
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
            _cube.Dispose();
            _cubeNormals.Dispose();
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
            _watch.Stop();
            float deltaTime = (float)_watch.ElapsedTicks / Stopwatch.Frequency;
            _watch.Restart();

            // perform rotation of the cube depending on the keyboard state
            if (_autoRotate)
            {
                _xangle += deltaTime / 2;
                _yangle += deltaTime;
            }
            if (_right) _yangle += deltaTime;
            if (_left) _yangle -= deltaTime;
            if (_up) _xangle -= deltaTime;
            if (_down) _xangle += deltaTime;

            // set up the viewport and clear the previous depth and color buffers
            Gl.Viewport(0, 0, _width, _height);
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // make sure the shader program and texture are being used
            Gl.UseProgram(_program);
            Gl.BindTexture(_crateTexture);

            // set up the model matrix and draw the cube
            _program["model_matrix"].SetValue(Matrix4.CreateRotationY(_yangle) * Matrix4.CreateRotationX(_xangle));
            _program["enable_lighting"].SetValue(_lighting);

            Gl.BindBufferToShaderAttribute(_cube, _program, "vertexPosition");
            Gl.BindBufferToShaderAttribute(_cubeNormals, _program, "vertexNormal");
            Gl.BindBufferToShaderAttribute(_cubeUv, _program, "vertexUV");
            Gl.BindBuffer(_cubeQuads);

            Gl.DrawElements(BeginMode.Quads, _cubeQuads.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);

            Glut.glutSwapBuffers();
        }

        private static void OnReshape(int width, int height)
        {
            _width = width;
            _height = height;

            _program.Use();
            _program["projection_matrix"].SetValue(Matrix4.CreatePerspectiveFieldOfView(0.45f, (float)width / height, 0.1f, 1000f));
        }

        private static void OnKeyboardDown(byte key, int x, int y)
        {
            if (key == 'w') _up = true;
            else if (key == 's') _down = true;
            else if (key == 'd') _right = true;
            else if (key == 'a') _left = true;
            else if (key == 27) Glut.glutLeaveMainLoop();
        }

        private static void OnKeyboardUp(byte key, int x, int y)
        {
            if (key == 'w') _up = false;
            else if (key == 's') _down = false;
            else if (key == 'd') _right = false;
            else if (key == 'a') _left = false;
            else if (key == ' ') _autoRotate = !_autoRotate;
            else if (key == 'l') _lighting = !_lighting;
            else if (key == 'f')
            {
                _fullscreen = !_fullscreen;
                if (_fullscreen) Glut.glutFullScreen();
                else
                {
                    Glut.glutPositionWindow(0, 0);
                    Glut.glutReshapeWindow(1280, 720);
                }
            }
        }

        public static string VertexShader = @"
#version 130
in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 vertexUV;
out vec3 normal;
out vec2 uv;
uniform mat4 projection_matrix;
uniform mat4 view_matrix;
uniform mat4 model_matrix;
void main(void)
{
    normal = normalize((model_matrix * vec4(floor(vertexNormal), 0)).xyz);
    uv = vertexUV;
    gl_Position = projection_matrix * view_matrix * model_matrix * vec4(vertexPosition, 1);
}
";

        public static string FragmentShader = @"
#version 130
uniform sampler2D texture;
uniform vec3 light_direction;
uniform bool enable_lighting;
in vec3 normal;
in vec2 uv;
out vec4 fragment;
void main(void)
{
    float diffuse = max(dot(normal, light_direction), 0);
    float ambient = 0.3;
    float lighting = (enable_lighting ? max(diffuse, ambient) : 1);
    fragment = lighting * texture2D(texture, uv);
}
";
    }
}
