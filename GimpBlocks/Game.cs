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
        readonly GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;
        Texture2D _crosshairTexture;

        IInputState _inputState;
        InputManager _inputManager;
        ICamera _camera;
        ICameraController _cameraController;
        World _world;
        BlockPicker _blockPicker;

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsFixedTimeStep = false;
            _graphics.SynchronizeWithVerticalRetrace = true;

            _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
            _graphics.PreferMultiSampling = false;

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            SetViewportDependentParameters();
            _inputManager.SetClientBounds(Window.ClientBounds);
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            _inputManager.OnActivated();

            base.OnActivated(sender, args);
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
            IsMouseVisible = false;
            Mouse.WindowHandle = Window.Handle;
            Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);

            _inputManager = ObjectFactory.GetInstance<InputManager>();
            _inputManager.SetClientBounds(Window.ClientBounds);
            _camera = ObjectFactory.GetInstance<ICamera>();
            // Don't need this right now but we have to create the object in the container so it can receive messages
            _cameraController = ObjectFactory.GetInstance<ICameraController>();

            var effect = Content.Load<Effect>("BlockEffect");
            var worldRenderer = new WorldRenderer(_graphics.GraphicsDevice, effect);
            var boundingBoxRenderer = new BoundingBoxRenderer(_graphics.GraphicsDevice);
            var prototypeMap = new BlockPrototypeMap();
            Func<World, int, int, Chunk> chunkFactory = (world, chunkX, chunkZ) =>
            {
                var chunkRenderer = new ChunkRenderer(_graphics.GraphicsDevice, effect);
                return new Chunk(chunkX, chunkZ, world, chunkRenderer, prototypeMap);
            };
            _world = new World(worldRenderer, chunkFactory, boundingBoxRenderer);
            _blockPicker = new BlockPicker(_world, _camera);

            EventAggregator.Instance.AddListener(_blockPicker);
            EventAggregator.Instance.AddListener(_world);

            _world.Generate();

            SetViewportDependentParameters();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
            _crosshairTexture = Content.Load<Texture2D>("crosshair");
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
            if (IsActive)
            {
                _inputManager.HandleInput(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            _world.Draw(_camera.Location, _camera.OriginBasedViewTransformation, _camera.ProjectionTransformation);
            
            _spriteBatch.Begin();
            var crossHairPosition = new Vector2(Window.ClientBounds.Width / 2 - _crosshairTexture.Width / 2, Window.ClientBounds.Height / 2 - _crosshairTexture.Height / 2);
            _spriteBatch.Draw(_crosshairTexture, crossHairPosition, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
