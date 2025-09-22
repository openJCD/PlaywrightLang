using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlaywrightLang.LanguageServices;
using PlaywrightLang.LanguageServices.AST;
using PlaywrightLang.LanguageServices.Object;
using PlaywrightLang.LanguageServices.Object.Primitive;
using PlaywrightLang.LanguageServices.Parse;

namespace PlaywrightLang.DebugEntry;

internal class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Tokeniser tokeniser;
    private PwState state;
    private ImGuiRenderer _imGuiRenderer;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        state = new PwState();
        /*
        // run tests.
        Stopwatch sw = new Stopwatch();
        PwAst f = state.ParseFile("tests.pw");
        sw.Start();
        state.ExecuteChunk(f);
        sw.Stop();
        Console.WriteLine("Evaluated tests in " + sw.ElapsedMilliseconds + " ms.");
        
        PwInstance actorTestInstance = new PwActor("ronnie").AsPwInstance();
        state.ExecuteFunction("test_external_calls", actorTestInstance);

        float fib_n = 20;
        sw.Restart();
        var place= state.ExecuteFunction("fib", fib_n);
        sw.Stop();
        Console.WriteLine($"Evaluated non-recusive fibonacci to {place} in {sw.ElapsedMilliseconds} ms.");

        sw.Restart();
        place = state.ExecuteFunction("fib_efficient", fib_n);
        sw.Stop();
        Console.WriteLine($"Evaluated efficient (non-array) fibonacci to {place} in {sw.ElapsedMilliseconds} ms.");
        */
        state.RegisterType<PwProp>("prop");
        PwAst s = state.ParseFile("script.pw");
        state.ExecuteChunk(s);
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        base.Draw(gameTime);
    }
    
}