using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JTacticalSim.API.Game;
using JTacticalSim.GUI.Render;
using XnaGame = Microsoft.Xna.Framework.Game;

namespace JTacticalSim.GUI;

public class MonoGameHost : XnaGame
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private SpriteFont _mapFont;

    public MonoGameHost(GameContext ctx)
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 900;
    }

    protected override void Initialize()
    {
        Window.Title = "JTacticalSim";
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        try { _font    = Content.Load<SpriteFont>("Fonts/DefaultFont"); } catch { }
        try { _mapFont = Content.Load<SpriteFont>("Fonts/MapFont");     } catch { }

        var pixel = new Texture2D(GraphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        var renderer = (Renderer)Game().Renderer;
        renderer.SpriteBatch = _spriteBatch;
        renderer.Font        = _font;
        renderer.MapFont     = _mapFont;
        renderer.GraphicsDevice = GraphicsDevice;
        renderer.Pixel = pixel;
        // renderer.LoadContent() is called by the engine via Game.Start() after a game is loaded
    }

    protected override void Update(GameTime gameTime)
    {
        Game().StateSystem.Update(gameTime.ElapsedGameTime.TotalMilliseconds);
        ((Renderer)Game().Renderer).MainScreenRenderer.UpdateAnimation(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin();
        var renderer = (Renderer)Game().Renderer;
        renderer.IsInDrawPhase = true;
        Game().StateSystem.Render();
        // Modal overlay and dev CLI always draw on top of whatever state is rendering
        renderer.Overlay.Draw(_spriteBatch, _font, renderer.Pixel,
                              GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height,
                              renderer.MainScreenRenderer.MapBounds);
        renderer.DevCli.Draw(_spriteBatch, _font, renderer.Pixel);
        renderer.IsInDrawPhase = false;
        _spriteBatch.End();
        base.Draw(gameTime);
    }

    private static IGame Game() => JTacticalSim.Game.Instance;
}
