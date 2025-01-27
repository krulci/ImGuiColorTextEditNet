﻿using System.Numerics;
using ImGuiColorTextEditNet;
using ImGuiNET;
using Veldrid;
using Veldrid.StartupUtilities;

namespace TextEdit.Demo;

public static class Program
{
    public static void Main()
    {
        var windowInfo = new WindowCreateInfo
        {
            X = 100,
            Y = 100,
            WindowWidth = 480,
            WindowHeight = 640,
            WindowInitialState = WindowState.Normal,
            WindowTitle = "TextEdit.Test"
        };

        var gdOptions = new GraphicsDeviceOptions(
            true,
            PixelFormat.D24_UNorm_S8_UInt,
            true,
            ResourceBindingModel.Improved,
            true,
            true,
            false);

        var window = VeldridStartup.CreateWindow(ref windowInfo);
        var gd = VeldridStartup.CreateGraphicsDevice(window, gdOptions, GraphicsBackend.Direct3D11);

        var imguiRenderer = new ImGuiRenderer(
            gd,
            gd.MainSwapchain.Framebuffer.OutputDescription,
            (int)gd.MainSwapchain.Framebuffer.Width,
            (int)gd.MainSwapchain.Framebuffer.Height);

        var cl = gd.ResourceFactory.CreateCommandList();
        window.Resized += () =>
        {
            gd.ResizeMainWindow((uint)window.Width, (uint)window.Height);
            imguiRenderer.WindowResized(window.Width, window.Height);
        };

        var demoText = @"#include <stdio.h>

void main(int argc, char **argv) {
	printf(""Hello world!\n"");
	/* A multi-line
	comment which continues on
	to here */

	for (int i = 0; i < 10; i++)
		printf(""%d\n"", i); // Breakpoint here

	// A single line comment
	int a = 123456;
	int b = 0x123456; // and here
	int c = 0b110101;
    errors on this line!
}
";

        var editor = new TextEditor
        {
            AllText = demoText,
            SyntaxHighlighter = new CStyleHighlighter(true)
        };

        var demoBreakpoints = new (int, object)[] { (10, ""), (14, "") };
        var demoErrors = new Dictionary<int, object> { { 16, "Syntax error etc" } };
        editor.Breakpoints.SetBreakpoints(demoBreakpoints);
        editor.ErrorMarkers.SetErrorMarkers(demoErrors);

        editor.SetColor(PaletteIndex.Custom, 0xff0000ff);
        editor.SetColor(PaletteIndex.Custom + 1, 0xff00ffff);
        editor.SetColor(PaletteIndex.Custom + 2, 0xffffffff);
        editor.SetColor(PaletteIndex.Custom + 3, 0xff808080);

        DateTime lastFrame = DateTime.Now;
        while (window.Exists)
        {
            var input = window.PumpEvents();
            if (!window.Exists)
                break;

            var thisFrame = DateTime.Now;
            imguiRenderer.Update((float)(thisFrame - lastFrame).TotalSeconds, input);
            lastFrame = thisFrame;

            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(window.Width, window.Height));
            ImGui.Begin("Demo");

            if (ImGui.Button("Reset"))
            {
                editor.AllText = demoText;
                editor.Breakpoints.SetBreakpoints(demoBreakpoints);
                editor.ErrorMarkers.SetErrorMarkers(demoErrors);
            }

            ImGui.SameLine(); if (ImGui.Button("err line")) editor.AppendLine("Some error text", PaletteIndex.Custom);
            ImGui.SameLine(); if (ImGui.Button("warn line")) editor.AppendLine("Some warning text", PaletteIndex.Custom + 1);
            ImGui.SameLine(); if (ImGui.Button("info line")) editor.AppendLine("Some info text", PaletteIndex.Custom + 2);
            ImGui.SameLine(); if (ImGui.Button("verbose line")) editor.AppendLine("Some debug text", PaletteIndex.Custom + 3);

            ImGui.Text($"Cur:{editor.CursorPosition} SEL: {editor.Selection.Start} - {editor.Selection.End}");
            editor.Render("EditWindow");

            ImGui.End();

            cl.Begin();
            cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
            cl.ClearColorTarget(0, RgbaFloat.Black);
            imguiRenderer.Render(gd, cl);
            cl.End();
            gd.SubmitCommands(cl);
            gd.SwapBuffers(gd.MainSwapchain);
        }
    }
}
