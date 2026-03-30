using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameBootstrap))]
public class GameBootstrapServerControls : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Server Controls", EditorStyles.boldLabel);

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("Node Path", EditorStyles.boldLabel);
            string currentPath = ServerProcessManager.GetNodePathPref();
            EditorGUILayout.BeginHorizontal();
            string newPath = EditorGUILayout.TextField(currentPath);
            if (GUILayout.Button("Browse", GUILayout.Width(80)))
            {
                string picked = EditorUtility.OpenFilePanel("Select node executable", "/usr/local/bin", "");
                if (!string.IsNullOrEmpty(picked))
                {
                    newPath = picked;
                }
            }
            if (GUILayout.Button("Auto", GUILayout.Width(60)))
            {
                string auto = ServerProcessManager.GetNodePath();
                newPath = auto;
            }
            EditorGUILayout.EndHorizontal();
            if (newPath != currentPath)
            {
                ServerProcessManager.SetNodePath(newPath);
            }

            string resolved = ServerProcessManager.GetNodePath();
            EditorGUILayout.LabelField(string.IsNullOrEmpty(resolved) ? "Resolved: not found" : $"Resolved: {resolved}");
        }

        var status = ServerProcessManager.GetStatus();
        DrawStatus(status);

        using (new EditorGUILayout.HorizontalScope())
        {
            GUI.enabled = status.State != ServerProcessState.Running;
            if (GUILayout.Button("Start Server"))
            {
                ServerProcessManager.Start();
            }

            GUI.enabled = status.State == ServerProcessState.Running;
            if (GUILayout.Button("Stop Server"))
            {
                ServerProcessManager.Stop();
            }

            GUI.enabled = true;
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Open server/"))
            {
                EditorUtility.RevealInFinder(ServerProcessManager.ServerRootPath);
            }

            if (GUILayout.Button("Open playerData/"))
            {
                EditorUtility.RevealInFinder(ServerProcessManager.PlayerDataPath);
            }
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Player Tools", EditorStyles.boldLabel);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            var bootstrap = target as GameBootstrap;
            string rawId = bootstrap != null ? bootstrap.remotePlayerId : "player";
            string safeId = SafePlayerId(rawId);
            EditorGUILayout.LabelField($"Current Player Id: {safeId}");

            if (GUILayout.Button("Reset Current Player"))
            {
                bool ok = EditorUtility.DisplayDialog(
                    "Reset Player Data",
                    $"Reset player data for '{safeId}'? This will delete the server playerData file.",
                    "Reset",
                    "Cancel");

                if (ok)
                {
                    ResetPlayerData(safeId);
                }
            }
        }
    }

    private static void ResetPlayerData(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            UnityEngine.Debug.LogWarning("[ServerTools] Player id is empty. Reset aborted.");
            return;
        }

        string filePath = Path.Combine(ServerProcessManager.PlayerDataPath, $"{playerId}.json");
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                UnityEngine.Debug.Log($"[ServerTools] Player reset: {playerId} ({filePath})");
            }
            else
            {
                UnityEngine.Debug.LogWarning($"[ServerTools] Player file not found: {filePath}");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[ServerTools] Failed to reset player '{playerId}': {ex.Message}");
        }
    }

    private static string SafePlayerId(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "player";
        }

        char[] buffer = new char[value.Length];
        int idx = 0;
        foreach (char c in value)
        {
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
            {
                buffer[idx++] = c;
            }
        }

        return idx > 0 ? new string(buffer, 0, idx) : "player";
    }

    private static void DrawStatus(ServerProcessStatus status)
    {
        Color prev = GUI.color;
        GUI.color = status.State switch
        {
            ServerProcessState.Running => new Color(0.6f, 1f, 0.6f),
            ServerProcessState.Stopped => new Color(1f, 0.7f, 0.7f),
            _ => new Color(1f, 0.9f, 0.6f)
        };

        EditorGUILayout.HelpBox(status.Message, MessageType.Info);
        GUI.color = prev;
    }
}

internal enum ServerProcessState
{
    Unknown,
    Running,
    Stopped
}

internal readonly struct ServerProcessStatus
{
    public readonly ServerProcessState State;
    public readonly string Message;

    public ServerProcessStatus(ServerProcessState state, string message)
    {
        State = state;
        Message = message;
    }
}

internal static class ServerProcessManager
{
    private const string PrefPid = "GraphToolkit.ServerProcessPid";
    private static Process process;

    public static string ServerRootPath => Path.Combine(ProjectRoot, "server");
    public static string PlayerDataPath => Path.Combine(ServerRootPath, "playerData");
    public static string ServerScriptPath => Path.Combine(ServerRootPath, "index.js");

    private static string ProjectRoot => Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

    public static ServerProcessStatus GetStatus()
    {
        if (IsRunning())
        {
            return new ServerProcessStatus(ServerProcessState.Running, $"Server: RUNNING (pid {process?.Id})");
        }

        return new ServerProcessStatus(ServerProcessState.Stopped, "Server: STOPPED");
    }

    public static void Start()
    {
        if (IsRunning())
        {
            UnityEngine.Debug.Log("[ServerLauncher] Server already running.");
            return;
        }

        TryKillProcessOnPort(3000);

        if (!File.Exists(ServerScriptPath))
        {
            UnityEngine.Debug.LogError($"[ServerLauncher] Server script not found: {ServerScriptPath}");
            return;
        }

        string nodePath = GetNodePath();
        if (string.IsNullOrEmpty(nodePath))
        {
            UnityEngine.Debug.LogError("[ServerLauncher] Node.js not found. Set Node Path in Server Controls.");
            return;
        }

        var psi = new ProcessStartInfo
        {
            FileName = nodePath,
            Arguments = $"\"{ServerScriptPath}\"",
            WorkingDirectory = ProjectRoot,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.Log($"[server] {e.Data}");
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.LogError($"[server] {e.Data}");
            }
        };
        process.Exited += (_, __) =>
        {
            UnityEngine.Debug.Log("[ServerLauncher] Server process exited.");
            ClearProcess();
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            EditorPrefs.SetInt(PrefPid, process.Id);
            UnityEngine.Debug.Log($"[ServerLauncher] Server started (pid {process.Id}).");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[ServerLauncher] Failed to start server: {ex.Message}");
            ClearProcess();
        }
    }

    public static void Stop()
    {
        if (!IsRunning())
        {
            UnityEngine.Debug.Log("[ServerLauncher] Server is not running.");
            return;
        }

        try
        {
            process.Kill();
            process.WaitForExit(2000);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[ServerLauncher] Failed to stop server: {ex.Message}");
        }
        finally
        {
            ClearProcess();
            UnityEngine.Debug.Log("[ServerLauncher] Server stopped.");
        }
    }

    private static bool IsRunning()
    {
        if (process != null)
        {
            if (!process.HasExited)
            {
                return true;
            }

            ClearProcess();
        }

        if (EditorPrefs.HasKey(PrefPid))
        {
            int pid = EditorPrefs.GetInt(PrefPid);
            try
            {
                var existing = Process.GetProcessById(pid);
                if (!existing.HasExited)
                {
                    process = existing;
                    return true;
                }
            }
            catch
            {
                ClearProcess();
            }
        }

        return false;
    }

    private static void ClearProcess()
    {
        process = null;
        if (EditorPrefs.HasKey(PrefPid))
        {
            EditorPrefs.DeleteKey(PrefPid);
        }
    }

    private static void TryKillProcessOnPort(int port)
    {
        try
        {
            if (Application.platform != RuntimePlatform.OSXEditor)
            {
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-lc \"lsof -ti :{port}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var lsof = Process.Start(psi);
            if (lsof == null)
            {
                return;
            }

            string output = lsof.StandardOutput.ReadToEnd();
            lsof.WaitForExit(1500);

            if (string.IsNullOrWhiteSpace(output))
            {
                return;
            }

            string[] pids = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string pid in pids)
            {
                if (int.TryParse(pid.Trim(), out int id))
                {
                    try
                    {
                        var existing = Process.GetProcessById(id);
                        existing.Kill();
                        existing.WaitForExit(1000);
                        UnityEngine.Debug.Log($"[ServerLauncher] Killed process on port {port} (pid {id}).");
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarning($"[ServerLauncher] Failed to kill pid {id} on port {port}: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"[ServerLauncher] Port cleanup failed: {ex.Message}");
        }
    }

    public static string GetNodePath()
    {
        string custom = EditorPrefs.GetString(NodePathPref, string.Empty);
        if (!string.IsNullOrEmpty(custom) && File.Exists(custom))
        {
            return custom;
        }

        string[] candidates =
        {
            "/opt/homebrew/bin/node",
            "/usr/local/bin/node",
            "/usr/bin/node"
        };

        foreach (string path in candidates)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        return string.Empty;
    }

    public static void SetNodePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            EditorPrefs.DeleteKey(NodePathPref);
            return;
        }

        EditorPrefs.SetString(NodePathPref, path);
    }

    public static string GetNodePathPref()
    {
        return EditorPrefs.GetString(NodePathPref, string.Empty);
    }

    private const string NodePathPref = "GraphToolkit.NodePath";
}
