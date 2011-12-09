using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using StructureMap;

namespace GimpBlocks
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        IInputState _inputState;
        IInputMapper _inputMapper;
        ICamera _camera;
        ICameraController _cameraController;
        World _world;
        IWorldRenderer _worldRenderer;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = true;

            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
            graphics.PreferMultiSampling = false;

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            SetViewportDependentParameters();
        }

        private void SetViewportDependentParameters()
        {
            var width = GraphicsDevice.Viewport.Width;
            var height = GraphicsDevice.Viewport.Height;

            float aspectRatio = width / (float)height;
            const float fieldOfView = MathHelper.Pi / 4;

            _camera.SetProjectionParameters(fieldOfView, 1f, aspectRatio, .01f, 1000);
        }

        protected override void Initialize()
        {
            IsMouseVisible = true;

            _inputState = ObjectFactory.GetInstance<IInputState>();
            _inputMapper = ObjectFactory.GetInstance<IInputMapper>();
            _camera = ObjectFactory.GetInstance<ICamera>();
            // Don't need this right now but we have to create the object in the container so it can receive messages
            _cameraController = ObjectFactory.GetInstance<ICameraController>();

            SetInputBindings();

            var effect = Content.Load<Effect>("BlockEffect");
            _worldRenderer = new WorldRenderer(graphics.GraphicsDevice, effect);
            _world = new World(_worldRenderer);

            var stopWatch = new Stopwatch();
            stopWatch.Measure(() => _world.Generate());
            Debug.WriteLine("Generation time: " + stopWatch.ElapsedMilliseconds + " ms");

            SetViewportDependentParameters();

            base.Initialize();
        }

        void SetInputBindings()
        {
            _inputMapper.AddKeyDownMessage<MoveForward>(Keys.W);
            _inputMapper.AddKeyDownMessage<MoveBackward>(Keys.S);
            _inputMapper.AddKeyDownMessage<MoveLeft>(Keys.A);
            _inputMapper.AddKeyDownMessage<MoveRight>(Keys.D);
            _inputMapper.AddKeyDownMessage<MoveUp>(Keys.E);
            _inputMapper.AddKeyDownMessage<MoveDown>(Keys.C);
            _inputMapper.AddKeyPressMessage<IncreaseCameraSpeed>(Keys.OemPlus);
            _inputMapper.AddKeyPressMessage<DecreaseCameraSpeed>(Keys.OemMinus);
            _inputMapper.AddKeyPressMessage<ZoomIn>(Keys.OemPeriod);
            _inputMapper.AddKeyPressMessage<ZoomOut>(Keys.OemComma);
            _inputMapper.AddKeyDownMessage<GoToGround>(Keys.Z);

            _inputMapper.AddKeyPressMessage<ToggleDrawWireframeSetting>(Keys.F);
            _inputMapper.AddKeyPressMessage<ToggleUpdateSetting>(Keys.U);
            _inputMapper.AddKeyPressMessage<ToggleSingleStepSetting>(Keys.P);

            _inputMapper.AddKeyPressMessage<GarbageCollect>(Keys.G);

            _inputMapper.AddKeyPressMessage<ExitApplication>(Keys.Escape);

            // TODO: we don't specify which mouse button must be down (hardcoded to right button ATM),
            // this can be extended when we need to.
            _inputMapper.AddMouseMoveMessage<MouseLook>();
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        protected override void Update(GameTime gameTime)
        {
            _inputState.Update(gameTime.ElapsedGameTime, Keyboard.GetState(), Mouse.GetState());
            _inputMapper.HandleInput(_inputState);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _worldRenderer.Draw(Vector3.Zero, _camera.Location, _camera.OriginBasedViewTransformation,
                                _camera.ProjectionTransformation);
            
            base.Draw(gameTime);
        }
    }
}
