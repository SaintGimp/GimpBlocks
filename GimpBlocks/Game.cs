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
        readonly GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D crosshairTexture;

        InputManager inputManager;
        ICamera camera;
        ICameraController cameraController;
        World world;
        BlockPicker blockPicker;

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
            inputManager.SetClientBounds(Window.ClientBounds);
        }

        protected override void OnActivated(object sender, EventArgs args)
        {
            inputManager.OnActivated();

            base.OnActivated(sender, args);
        }

        private void SetViewportDependentParameters()
        {
            var width = GraphicsDevice.Viewport.Width;
            var height = GraphicsDevice.Viewport.Height;

            float aspectRatio = width / (float)height;
            const float fieldOfView = MathHelper.Pi / 4;

            camera.SetProjectionParameters(fieldOfView, 1f, aspectRatio, .01f, 1000);
        }

        protected override void Initialize()
        {
            IsMouseVisible = false;
            Mouse.WindowHandle = Window.Handle;
            Mouse.SetPosition(Window.ClientBounds.Width / 2, Window.ClientBounds.Height / 2);

            inputManager = ObjectFactory.GetInstance<InputManager>();
            inputManager.SetClientBounds(Window.ClientBounds);
            camera = ObjectFactory.GetInstance<ICamera>();
            // TODO: Don't need this right now but we have to create the object in the container so it can receive messages
            cameraController = ObjectFactory.GetInstance<ICameraController>();

            var effect = Content.Load<Effect>("BlockEffect");
            var worldRenderer = new WorldRenderer(graphics.GraphicsDevice, effect);
            var boundingBoxRenderer = new BoundingBoxRenderer(graphics.GraphicsDevice);
            var chunkFactory = new ChunkFactory(new EnvironmentGenerator(),  () => new ChunkRenderer(graphics.GraphicsDevice, effect));
            world = new World(4, chunkFactory, worldRenderer, boundingBoxRenderer);
            blockPicker = new BlockPicker(world, camera);

            EventAggregator.Instance.AddListener(blockPicker);
            EventAggregator.Instance.AddListener(world);

            world.Generate();

            SetViewportDependentParameters();

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
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
    }
}
