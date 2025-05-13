using System;
using System.Collections.Generic;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using PlaywrightLang.LanguageServices;
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
        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();
        
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
        _imGuiRenderer.BeforeLayout(gameTime);
        DebugGui();
        _imGuiRenderer.AfterLayout();
        base.Draw(gameTime);
    }

    private string testFileName = "";
    List<Token> currentTokens = new List<Token>();
    protected void DebugGui()
    {
        ImGui.Begin("Playwright Sandbox");
        ImGui.LabelText("Load file: ", "");
        ImGui.InputText("filepath", ref testFileName, 1024);
        ImGui.SameLine();
        if (ImGui.Button("Load Tokens"))
        {
            try
            {
                currentTokens = state.LoadFile(testFileName);
            }
            catch (Exception e)
            {
                Parser.Log($"Could not open the file {testFileName}: {e.Message}");
            }
        }

        if (ImGui.Button("Parse file"))
        {
            try
            {
                state.ParseFile(testFileName);
            }
            catch (Exception e)
            {
                Parser.Log($"Could not parse the file {testFileName}: {e.Message}");
            }
        }
        ImGui.End();

        ImGui.Begin("Playwright Log");
        ImGui.BeginTabBar("PwLogTabs");

        if (ImGui.BeginTabItem("Tokeniser Log"))
        {
            if (state.Tokeniser != null)
            {
                if (ImGui.Button("Clear##01"))
                {
                    state.Tokeniser.Log = "";
                }
                ImGui.Text(state.Tokeniser.Log);
            }
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Parser Log"))
        {
            if (ImGui.Button("Clear##02"))
            {
                Parser.TextLog = "";
            }
            ImGui.Text(Parser.TextLog);
            ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
        ImGui.End();
        ImGui.ShowDemoWindow();
    }
    
}