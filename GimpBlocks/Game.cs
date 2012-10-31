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

namespace GimpBlocks
{
    public class Game : Microsoft.Xna.Framework.Game,
        IListener<EnabledMouseLookMode>,
        IListener<DisabledMouseLookMode>
    {
        readonly GraphicsDeviceManager graphics;
        
        // Game systems
        InputManager inputManager;
        ICamera camera;
        CameraController cameraController;
        World world;
        BlockPicker blockPicker;

        // Content
        SpriteBatch spriteBatch;
        Texture2D crosshairTexture;
        Effect blockEffect;

        public Game()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            IsFixedTimeStep = false;
            graphics.SynchronizeWithVerticalRetrace = true;

            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24;
            graphics.PreferMultiSampling = false;

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnWindowClientSizeChanged;
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            inputManager.OnActivated();

            base.OnActivated(sender, args);
        }

        void OnWindowClientSizeChanged(object sender, EventArgs e)
        {
            OnWindowClientSizeChanged();
        }

        void OnWindowClientSizeChanged()
        {
            SetViewportDependentParameters();
            inputManager.SetClientBounds(Window.ClientBounds);
        }

        void SetViewportDependentParameters()
        {
            var width = GraphicsDevice.Viewport.Width;
            var height = GraphicsDevice.Viewport.Height;

            float aspectRatio = width / (float)height;
            const float fieldOfView = MathHelper.Pi / 4;

            camera.SetProjectionParameters(fieldOfView, 1f, aspectRatio, .01f, 1000);
        }

        protected override void Initialize()
        {
            // This needs to be called first because it calls LoadContent
            // which sets up stuff we need.
            base.Initialize();

            InitializeMouse();

            InitializeWorld();

            OnWindowClientSizeChanged();
        }

        void InitializeMouse()
        {
            IsMouseVisible = false;
            Mouse.WindowHandle = Window.Handle;
            Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);
        }

        void InitializeWorld()
        {
            var boundingBoxRenderer = new BoundingBoxRenderer(graphics.GraphicsDevice);
            var chunkFactory = new ChunkFactory(new EnvironmentGenerator(), () => new ChunkRenderer(graphics.GraphicsDevice, blockEffect));
            world = new World(4, chunkFactory, boundingBoxRenderer);
            EventAggregator.Instance.AddListener(world);

            inputManager = new InputManager(new XnaInputState());
            EventAggregator.Instance.AddListener(inputManager);

            camera = new Camera();
            cameraController = new CameraController(camera);
            EventAggregator.Instance.AddListener(cameraController);

            blockPicker = new BlockPicker(world, camera);
            EventAggregator.Instance.AddListener(blockPicker);

            world.Generate();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            blockEffect = Content.Load<Effect>("BlockEffect");
            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            crosshairTexture = Content.Load<Texture2D>("crosshair");
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
                inputManager.HandleInput(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            world.Draw(camera.Location, camera.OriginBasedViewTransformation, camera.ProjectionTransformation);
            
            spriteBatch.Begin();
            var crossHairPosition = new Vector2(Window.ClientBounds.Width / 2 - crosshairTexture.Width / 2, Window.ClientBounds.Height / 2 - crosshairTexture.Height / 2);
            spriteBatch.Draw(crosshairTexture, crossHairPosition, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }

        public void Handle(EnabledMouseLookMode message)
        {
            IsMouseVisible = false;
        }

        public void Handle(DisabledMouseLookMode message)
        {
            IsMouseVisible = true;
        }
    }
}
