using System.Numerics;
using ImGuiNET;
using LSDView.controller;
using LSDView.Controllers;
using LSDView.GUI.Components;
using LSDView.Util;

namespace LSDView.GUI.GUIComponents
{
    public class MainMenuBar : ImGuiComponent
    {
        private readonly FileDialog _openDialog;
        private readonly FileDialog _openVramDialog;

        private bool _openFileOpenDialog = false;
        private bool _openVramOpenDialog = false;

        private readonly FileOpenController _fileOpenController;
        private readonly VRAMController _vramController;
        private readonly ConfigController _configController;
        private readonly CameraController _cameraController;

        private string _streamingAssetsPathFieldValue;

        public MainMenuBar(FileOpenController fileOpenController,
            VRAMController vramController,
            ConfigController configController,
            CameraController cameraController)
        {
            _configController = configController;
            _streamingAssetsPathFieldValue = _configController.Config.StreamingAssetsPath;
            _openDialog = new FileDialog(_configController.Config.StreamingAssetsPath, FileDialog.DialogType.Open);
            _openVramDialog = new FileDialog(_configController.Config.StreamingAssetsPath, FileDialog.DialogType.Open);
            _fileOpenController = fileOpenController;
            _vramController = vramController;
            _cameraController = cameraController;
        }

        protected override void renderSelf()
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    renderFileMenu();
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("VRAM"))
                {
                    renderVramMenu();
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Help"))
                {
                    renderHelpMenu();
                    ImGui.EndMenu();
                }
            }

            ImGui.EndMainMenuBar();

            if (_openFileOpenDialog)
            {
                _openDialog.Show(path => _fileOpenController.OpenFile(path), ".lbd|.tix|.mom|.tmd|.tim");
                _openFileOpenDialog = false;
            }

            if (_openVramOpenDialog)
            {
                _openVramDialog.Show(path => _vramController.LoadTIXIntoVRAM(path), ".tix");
                _openVramOpenDialog = false;
            }

            _openDialog.Render();
            _openVramDialog.Render();
        }

        private void renderFileMenu()
        {
            if (ImGui.MenuItem("Open"))
            {
                _openFileOpenDialog = true;
            }

            if (ImGui.BeginMenu("Open Recent"))
            {
                if (_configController.Config.RecentFiles.Count > 0)
                {
                    string fileToOpen = null;
                    foreach (var recentFile in _configController.Config.RecentFiles)
                    {
                        var relPath = PathUtil.MakeRelative(recentFile, _configController.Config.StreamingAssetsPath);
                        if (ImGui.MenuItem(relPath))
                        {
                            fileToOpen = recentFile;
                            break;
                        }
                    }

                    if (fileToOpen != null) _fileOpenController.OpenFile(fileToOpen);
                }
                else
                {
                    ImGui.MenuItem("No recent files!");
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Set StreamingAssets path"))
            {
                createModal("Set StreamingAssets path...", new GenericDialog(setStreamingAssetsPathDialog),
                    new Vector2(500, 85));
            }
        }

        private void setStreamingAssetsPathDialog()
        {
            ImGui.PushItemWidth(-1);
            ImGui.InputText("##streamingassets", ref _streamingAssetsPathFieldValue, 1024);
            ImGui.PopItemWidth();
            ImGui.Spacing();
            ImGui.SameLine(ImGui.GetWindowWidth() - 30);
            if (ImGui.Button("Ok"))
            {
                _configController.Config.StreamingAssetsPath = _streamingAssetsPathFieldValue;
                _configController.Save();
            }
        }

        private void renderVramMenu()
        {
            if (ImGui.MenuItem("Load VRAM"))
            {
                _openVramOpenDialog = true;
            }
        }

        private void renderHelpMenu()
        {
            if (ImGui.MenuItem("About LSDView"))
            {
                createModal("Test modal", new InfoDialog(InfoDialog.DialogType.Info, "Test message"),
                    new Vector2(200, 200));
            }

            if (ImGui.MenuItem("Recenter view"))
            {
                _cameraController.RecenterView();
            }
        }
    }
}
