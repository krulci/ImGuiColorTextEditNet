using BepInEx;
using BepInEx.Unity.IL2CPP;
using ImGuiColorTextEditNet;
using ImGuiNET;
using System.Collections.Generic;

namespace TestPlugin;

[BepInDependency(DearImGuiInjection.Metadata.GUID)]
[BepInPlugin(Metadata.GUID, Metadata.Name, Metadata.Version)]
internal unsafe class TestPlugin : BasePlugin
{
    private static TextEditor editor;
    private static readonly string demoText = @"#include <stdio.h>

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
    public override void Load()
    {
        // Hit F4 To Trigger ImGui Cursor
        editor = new TextEditor
        {
            AllText = demoText,
            SyntaxHighlighter = new CStyleHighlighter(true)
        };

        DearImGuiInjection.DearImGuiInjection.Render += MyUI;
    }

    private static void MyUI()
    {
        var demoBreakpoints = new (int, object)[] { (10, ""), (14, "") };
        var demoErrors = new Dictionary<int, object> { { 16, "Syntax error etc" } };
        editor.Breakpoints.SetBreakpoints(demoBreakpoints);
        editor.ErrorMarkers.SetErrorMarkers(demoErrors);

        editor.SetColor(PaletteIndex.Custom, 0xff0000ff);
        editor.SetColor(PaletteIndex.Custom + 1, 0xff00ffff);
        editor.SetColor(PaletteIndex.Custom + 2, 0xffffffff);
        editor.SetColor(PaletteIndex.Custom + 3, 0xff808080);

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
    }
}