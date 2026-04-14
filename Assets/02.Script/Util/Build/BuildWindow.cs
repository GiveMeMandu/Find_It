#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

// 프로젝트별로 EditorPrefs 키를 네임스페이스화하고, 키 정의를 한 곳에서 관리하기 위한 유틸리티
internal static class BuildPrefs
{
    // 최종 키 형태: {ProjectName}.Build.{Name}
    private static string ProjectPrefix
    {
        get
        {
            string name = Application.productName;
            if (string.IsNullOrWhiteSpace(name))
            {
                // productName이 비어있을 경우 프로젝트 폴더명 사용
                var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                name = Path.GetFileName(projectRoot);
            }
            // 키 안전 문자만 남기고 나머지는 '_'
            name = Regex.Replace(name, "[^A-Za-z0-9_.-]+", "_");
            return name + ".";
        }
    }

    private static string Prefix => ProjectPrefix + "Build.";
    private static string Key(string name) => Prefix + name;

    // 공용 키
    internal static string SdkPathKey => Key("SteamSdkPath");
    internal static string SteamAccountKey => Key("SteamAccount");
    internal static string SteamPasswordKey => Key("SteamPassword");
    internal static string HeadKey => Key("Head");
    internal static string BuildTypeKey => Key("BuildType");
    internal static string CleanTargetKey => Key("CleanTarget");
    internal static string PlatformKey => Key("Platform");
}

// 빌드 타입 선택용 열거형
public enum BuildType { Normal, Demo }

// 플랫폼/유통 방식 선택: 스크립팅 정의(DISABLESTEAMWORKS) 토글에 사용
public enum PlatformType { Standalone, Steam }

// Unity 에디터 메뉴에서 실행할 빌드 & Steam 업로드 자동화 스크립트
public static class BuildScript
{
    internal const string DefaultVdfProjectRelative = "Tools\\app_build.vdf";   // 프로젝트 기준 기본 VDF 경로

    internal static string GetVdfRelative(BuildType buildType)
        => buildType == BuildType.Demo ? "Tools\\app_build_demo.vdf" : DefaultVdfProjectRelative;

    public static void BuildWindows64Only()
    {
        var paths = PreparePaths();
        EnsureScenesConfigured();
        EnsureSteamFolder(paths.steamRoot);
    // 선택된 플랫폼에 맞춰 스크립팅 정의 적용
    var platform = (PlatformType)EditorPrefs.GetInt(BuildPrefs.PlatformKey, (int)PlatformType.Standalone);
    ApplyPlatformDefines(platform);
        // HeadVer 계산 및 Unity AppVersion 갱신
    var head = EditorPrefs.GetString(BuildPrefs.HeadKey, string.Empty);
        if (string.IsNullOrWhiteSpace(head))
            throw new Exception("Head 값이 비어있습니다. Build Window에서 'Head'를 먼저 설정하세요.");
        var headVer = HeadVerUtil.Compute(paths.projectRoot, head);
        UpdateUnityAppVersion(headVer);
        var report = BuildWindows64(paths.buildExePath, cleanTargetFolder: true);
        LogBuildSummary(report);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception("Unity 빌드 실패");
        }
        Process.Start("explorer.exe", paths.buildFolder);
    }
    public static void BuildWindows64AndUploadToSteam()
    {
        var paths = PreparePaths();
        EnsureScenesConfigured();
        EnsureSteamFolder(paths.steamRoot);
    // 선택된 플랫폼에 맞춰 스크립팅 정의 적용
    var platform = (PlatformType)EditorPrefs.GetInt(BuildPrefs.PlatformKey, (int)PlatformType.Standalone);
    ApplyPlatformDefines(platform);
        // HeadVer 계산 및 Unity AppVersion 갱신
    var head = EditorPrefs.GetString(BuildPrefs.HeadKey, string.Empty);
        if (string.IsNullOrWhiteSpace(head))
            throw new Exception("Head 값이 비어있습니다. Build Window에서 'Head'를 먼저 설정하세요.");
        var headVer = HeadVerUtil.Compute(paths.projectRoot, head);
        UpdateUnityAppVersion(headVer);
        var report = BuildWindows64(paths.buildExePath, cleanTargetFolder: true);
        LogBuildSummary(report);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception("Unity 빌드 실패");
        }
        

        // 플랫폼이 Steam일 때만 업로드 진행
        var platformForUpload = (PlatformType)EditorPrefs.GetInt(BuildPrefs.PlatformKey, (int)PlatformType.Standalone);
        if (platformForUpload != PlatformType.Steam)
        {
            Debug.Log("플랫폼이 Standalone이므로 Steam 업로드를 건너뜁니다. (빌드만 완료)");
            EditorUtility.DisplayDialog("Steam 업로드 건너뜀", "플랫폼이 Steam일 때만 업로드가 진행됩니다. 현재: Standalone", "확인");
            Process.Start("explorer.exe", paths.buildFolder);
            return;
        }

        // 업로드 실행: 계정/비밀번호는 EditorPrefs에서 읽고, VDF는 고정 경로 사용
    var account = EditorPrefs.GetString(BuildPrefs.SteamAccountKey, string.Empty);
        if (string.IsNullOrWhiteSpace(account))
        {
            throw new Exception("Steam 계정이 설정되지 않았습니다. 'Build & Steam Upload' 창에서 계정 정보를 먼저 저장하세요.");
        }
        // 빌드 타입은 EditorPrefs에서 읽어 사용
    var buildType = (BuildType)EditorPrefs.GetInt(BuildPrefs.BuildTypeKey, (int)BuildType.Normal);
        RunSteamUpload(paths.steamRoot, paths.projectRoot, head, account, buildType);
    }

    // 모든 PlayerPrefs 초기화 (런타임 저장 데이터)
    [MenuItem("Build/Clear: PlayerPrefs (All)")]
    public static void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("모든 PlayerPrefs가 삭제되었습니다.");
        EditorUtility.DisplayDialog("PlayerPrefs 초기화", "모든 PlayerPrefs가 삭제되었습니다.", "확인");
    }

    // Build Window 관련 EditorPrefs 초기화(옵션)
    [MenuItem("Build/Clear: Build Window Settings (EditorPrefs)")]
    public static void ClearBuildWindowEditorPrefs()
    {
        // 새 네임스페이스 키 삭제
        var newKeys = new[]
        {
            BuildPrefs.SdkPathKey,
            BuildPrefs.CleanTargetKey,
            BuildPrefs.SteamAccountKey,
            BuildPrefs.SteamPasswordKey,
            BuildPrefs.HeadKey,
            BuildPrefs.BuildTypeKey,
            BuildPrefs.PlatformKey,
        };
        foreach (var k in newKeys)
        {
            if (!string.IsNullOrEmpty(k) && EditorPrefs.HasKey(k)) EditorPrefs.DeleteKey(k);
        }
        Debug.Log("Build Window 관련 EditorPrefs가 초기화되었습니다.");
        EditorUtility.DisplayDialog("설정 초기화", "Build Window 관련 EditorPrefs가 초기화되었습니다.", "확인");
        EditorWindow.GetWindow<BuildWindow>()?.RefreshKeys();
    }

    // PreparePaths: ContentBuilder 루트를 steamRoot로 사용한다 (sdk/tools/ContentBuilder)
    internal static (string projectRoot, string steamRoot, string contentRoot, string buildFolder, string buildExePath) PreparePaths()
    {
    var sdkRoot = EditorPrefs.GetString(BuildPrefs.SdkPathKey, string.Empty);
        if (string.IsNullOrWhiteSpace(sdkRoot))
        {
            throw new Exception("Steamworks SDK 경로가 설정되지 않았습니다. Build 창에서 SDK의 'sdk' 폴더 경로를 먼저 지정하세요.");
        }
        return PreparePaths(sdkRoot);
    }

    internal static (string projectRoot, string steamRoot, string contentRoot, string buildFolder, string buildExePath) PreparePaths(string sdkRoot)
    {
        // 프로젝트 루트(…/YourProject)
        var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        // sdk/tools/ContentBuilder (업로드 실행 위치)
        var contentBuilderRoot = Path.Combine(sdkRoot, "tools", "ContentBuilder");
        // 프로젝트 내 콘텐츠 루트 (Tools/app_build.vdf와 일치)
        var contentRoot = Path.Combine(projectRoot, "Builds", "content");
        // 빌드 산출물을 프로젝트의 content 루트에 직접 생성한다.
        var buildFolder = contentRoot;
        var exePath = Path.Combine(buildFolder, Application.productName + ".exe");
        return (projectRoot, contentBuilderRoot, contentRoot, buildFolder, exePath);
    }

    internal static void EnsureScenesConfigured()
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).ToArray();
        if (scenes.Length == 0)
        {
            throw new Exception("Build Settings에 활성화된 Scene이 없습니다. (File > Build Settings… 에서 Scenes In Build 설정)");
        }
    }

    internal static void EnsureSteamFolder(string contentBuilderRoot)
    {
        if (!Directory.Exists(contentBuilderRoot))
        {
            throw new DirectoryNotFoundException($"ContentBuilder 폴더를 찾을 수 없습니다: {contentBuilderRoot}\n(Steamworks SDK의 sdk/tools/ContentBuilder)");
        }
        var runBat = Path.Combine(contentBuilderRoot, "run_build.bat");
        if (!File.Exists(runBat))
        {
            throw new FileNotFoundException($"run_build.bat이 없습니다: {runBat}");
        }
    }

    internal static BuildReport BuildWindows64(string exePath, bool cleanTargetFolder)
    {
        var target = BuildTarget.StandaloneWindows64;
        var group = BuildTargetGroup.Standalone;

        // 타겟 전환(필요 시)
        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(group, target);
        }

        var targetDir = Path.GetDirectoryName(exePath)!;
        if (cleanTargetFolder && Directory.Exists(targetDir))
        {
            FileUtil.DeleteFileOrDirectory(targetDir);
        }
        Directory.CreateDirectory(targetDir);

        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        var options = BuildOptions.None;
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = exePath,
            target = target,
            options = options,
        };

        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        return report;
    }

    // 플랫폼 선택에 따라 DISABLESTEAMWORKS 정의 토글
    internal static void ApplyPlatformDefines(PlatformType platform)
    {
        try
        {
            var nbt = NamedBuildTarget.Standalone;
            string defines = PlayerSettings.GetScriptingDefineSymbols(nbt) ?? string.Empty;
            // 기존 순서를 보존하기 위해 List 사용, 공백/빈 토큰 제거
            var list = new List<string>();
            foreach (var token in defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var t = token.Trim();
                if (!string.IsNullOrEmpty(t)) list.Add(t);
            }

            // 새 정책: Steam일 때는 DISABLESTEAMWORKS 제거, Standalone일 때는 DISABLESTEAMWORKS 추가
            bool isStandalone = platform == PlatformType.Standalone;
            const string disableDefine = "DISABLESTEAMWORKS";
            bool hasDisable = list.Any(s => s == disableDefine);

            if (isStandalone)
            {
                if (!hasDisable)
                {
                    list.Add(disableDefine);
                }
            }
            else
            {
                if (hasDisable)
                {
                    list.RemoveAll(s => s == disableDefine);
                }
            }

            // 정리: 기존 프로젝트에 남아있을 수 있는 STEAMWORKS_NET는 제거
            if (list.Any(s => s == "STEAMWORKS_NET"))
            {
                list.RemoveAll(s => s == "STEAMWORKS_NET");
            }

            string joined = string.Join(";", list);
            if (!string.Equals(joined, defines, StringComparison.Ordinal))
            {
                PlayerSettings.SetScriptingDefineSymbols(nbt, joined);
                UnityEngine.Debug.Log($"플랫폼 정의 적용: {platform} -> DISABLESTEAMWORKS {(isStandalone ? "ON" : "OFF")} (Standalone)");
            }
            else
            {
                UnityEngine.Debug.Log($"플랫폼 정의 변경 없음: {platform} (Standalone)");
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"스크립팅 정의 적용 실패: {ex.Message}");
        }
    }

    internal static void LogBuildSummary(BuildReport report)
    {
        var s = report.summary;
        UnityEngine.Debug.Log($"[Build] 결과: {s.result}, 플랫폼: {s.platform}, 출력: {s.outputPath}");
        UnityEngine.Debug.Log($"[Build] 파일 크기: {EditorUtility.FormatBytes((long)s.totalSize)}, 시간: {s.totalTime}");
        if (s.result != BuildResult.Succeeded)
        {
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        UnityEngine.Debug.LogError($"[BuildError] {msg.content}");
                }
            }
        }
    }

    internal static void CopySteamAppIdIfExists(string projectRoot, string buildFolder)
    {
        var appIdSrc = Path.Combine(projectRoot, "steam_appid.txt");
        if (File.Exists(appIdSrc))
        {
            var appIdDst = Path.Combine(buildFolder, "steam_appid.txt");
            File.Copy(appIdSrc, appIdDst, overwrite: true);
            UnityEngine.Debug.Log($"steam_appid.txt 복사: {appIdDst}");
        }
    }

    // 하위 호환: 기존 시그니처는 Pref에서 타입을 읽어 사용
    internal static void RunSteamUpload(string steamRoot, string projectRoot, string head, string account)
    {
    var bt = (BuildType)EditorPrefs.GetInt(BuildPrefs.BuildTypeKey, (int)BuildType.Normal);
        RunSteamUpload(steamRoot, projectRoot, head, account, bt);
    }

    // 선택된 BuildType에 따라 VDF를 바꿔 업로드
    internal static void RunSteamUpload(string steamRoot, string projectRoot, string head, string account, BuildType buildType)
    {
        var steamCmdPath = Path.Combine(steamRoot, "builder", "steamcmd.exe");
        if (!File.Exists(steamCmdPath))
        {
            throw new FileNotFoundException($"steamcmd.exe를 찾을 수 없습니다: {steamCmdPath}");
        }

        var vdfRelative = GetVdfRelative(buildType); // Normal/Demo에 따라 VDF 선택
        var vdfFull = Path.Combine(projectRoot, vdfRelative);
        if (!File.Exists(vdfFull))
        {
            throw new FileNotFoundException($"VDF 스크립트를 찾을 수 없습니다: {vdfFull}");
        }

        // 업로드 전에 VDF desc를 현재 HeadVer({head}.{yearweek}.{githash})로 갱신
        var headVer = HeadVerUtil.Compute(projectRoot, head);
        try
        {
            UpdateVdfDesc(vdfFull, headVer.FullVersion);
            UnityEngine.Debug.Log($"VDF desc를 '{headVer.FullVersion}'로 갱신했습니다.");
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"VDF desc 갱신 실패(무시하고 계속 진행): {ex.Message}");
        }

        // appid를 VDF에서 읽어, 성공 시 빌드 페이지를 자동으로 연다.
        string openBuildsOnSuccess = string.Empty;
        if (TryReadAppIdFromVdf(vdfFull, out var appId))
        {
            var url = $"https://partner.steamgames.com/apps/builds/{appId}";
            // 성공(ExitCode 0)시에만 실행되도록 && 체인에 포함
            openBuildsOnSuccess = $" && start \"\" \"{url}\"";
        }

        // 가시적인 터미널(cmd.exe)을 띄워 진행 상황을 보여준다. (/k: 창 유지)
        // 전체 명령을 이중 인용으로 감싸 파싱 문제를 방지하고, VDF는 절대 경로 사용
    var cmdArgs = $"/k \"\"{steamCmdPath}\" +login \"{account}\" +run_app_build \"{vdfFull}\" +quit{openBuildsOnSuccess} && echo. && echo ===== 작업 완료 ({buildType}) ===== && pause\"";
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = cmdArgs,
            // VDF 내 ContentRoot의 상대 경로(..\\Builds\\content)를 올바르게 해석하도록 VDF가 위치한 폴더를 작업 디렉터리로 설정
            WorkingDirectory = Path.GetDirectoryName(vdfFull),
            UseShellExecute = true,     // 콘솔 창 표시
            CreateNoWindow = false,
        };

        var started = Process.Start(psi);
        if (started == null)
        {
            throw new Exception("Steam 업로드용 터미널을 시작할 수 없습니다.");
        }
    UnityEngine.Debug.Log($"Steam 업로드 콘솔을 열었습니다. 선택된 VDF: {vdfRelative} (타입: {buildType}). 터미널 창에서 진행 상황을 확인하세요.");
    }

    // steamcmd로 계정 로그인만 테스트 (별도 콘솔에서 +login 후 +quit)
    internal static void RunSteamLoginTest(string steamRoot, string account)
    {
        var steamCmdPath = Path.Combine(steamRoot, "builder", "steamcmd.exe");
        if (!File.Exists(steamCmdPath))
        {
            throw new FileNotFoundException($"steamcmd.exe를 찾을 수 없습니다: {steamCmdPath}");
        }

        // 로그인만 시도하고 종료. 콘솔을 유지해 사용자에게 결과를 보여준다.
        var cmdArgs = $"/k \"\"{steamCmdPath}\" +login \"{account}\" +quit && echo. && echo ===== 로그인 시도 완료 ===== && pause\"";
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = cmdArgs,
            WorkingDirectory = Path.GetDirectoryName(steamCmdPath),
            UseShellExecute = true,
            CreateNoWindow = false,
        };

        var started = Process.Start(psi);
        if (started == null)
        {
            throw new Exception("Steam 로그인 테스트 콘솔을 시작할 수 없습니다.");
        }
        UnityEngine.Debug.Log("Steam 로그인 테스트 콘솔을 열었습니다. 필요 시 Steam Guard 코드를 입력하세요.");
    }

    // Unity AppVersion을 HeadVer로 갱신 (SemVer+gitHash 형태)
    internal static void UpdateUnityAppVersion(HeadVerResult headVer)
    {
        var appVersion = headVer.FullVersion; // e.g. 10.202535.abc1234
        PlayerSettings.bundleVersion = appVersion;
        UnityEngine.Debug.Log($"Unity AppVersion 갱신: {appVersion}");
    }

    // VDF의 desc 값을 현재 버전으로 갱신한다. 없으면 AppID 바로 다음 줄에 추가를 시도한다.
    internal static void UpdateVdfDesc(string vdfPath, string descValue)
    {
        if (string.IsNullOrEmpty(vdfPath) || !File.Exists(vdfPath))
            throw new FileNotFoundException($"VDF 파일을 찾을 수 없습니다: {vdfPath}");

        string text = File.ReadAllText(vdfPath);
        string escaped = EscapeVdf(descValue);
        var descRegex = new Regex("(?im)^\\s*\"(?i:desc)\"\\s*\"[^\"]*\"");
        if (descRegex.IsMatch(text))
        {
            text = descRegex.Replace(text, $"\"desc\" \"{escaped}\"", 1);
        }
        else
        {
            // AppID 라인 뒤에 삽입 시도
            var appIdMatch = Regex.Match(text, "(?im)^(?<indent>\\s*)\"AppID\"\\s*\"\\d+\"\\s*$");
            if (appIdMatch.Success)
            {
                int insertIndex = appIdMatch.Index + appIdMatch.Length;
                string indent = appIdMatch.Groups["indent"].Value;
                text = text.Insert(insertIndex, $"\n{indent}\"desc\" \"{escaped}\"");
            }
            else
            {
                // 첫번째 여는 중괄호 다음에 삽입 (마지막 수단)
                int braceIndex = text.IndexOf('{');
                if (braceIndex >= 0)
                {
                    int lineEnd = text.IndexOf('\n', braceIndex);
                    if (lineEnd < 0) lineEnd = braceIndex + 1;
                    text = text.Insert(lineEnd, $"\n    \"desc\" \"{escaped}\"");
                }
            }
        }

        File.WriteAllText(vdfPath, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    private static string EscapeVdf(string value)
    {
        if (value == null) return string.Empty;
        return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    // VDF(app_build.vdf)에서 AppID 읽기 (존재하지 않거나 잘못된 경우 false)
    internal static bool TryReadAppIdFromVdf(string vdfPath, out string appId)
    {
        try
        {
            if (!File.Exists(vdfPath))
            {
                appId = null;
                return false;
            }
            var text = File.ReadAllText(vdfPath);
            // 줄 시작의 "AppID" "<digits>" 패턴 추출
            var m = Regex.Match(text, @"(?im)^\s*""AppID""\s*""(?<id>\d+)""", RegexOptions.Compiled);
            if (!m.Success)
            {
                appId = null;
                return false;
            }
            appId = m.Groups["id"].Value;
            return true;
        }
        catch
        {
            appId = null;
            return false;
        }
    }
}

// 간단한 빌드/업로드용 Editor Window
public class BuildWindow : EditorWindow
{
    private string _sdkPath;
    private bool _cleanTarget = false;
    private string _steamAccount;
    private string _head;
    private BuildType _buildType = BuildType.Normal;
    private PlatformType _platform = PlatformType.Standalone;

    [MenuItem("Build/Open Build Window")]
    public static void OpenWindow()
    {
        var win = GetWindow<BuildWindow>(utility: false, title: "Build & Steam Upload");
        win.minSize = new Vector2(420, 300);
        win.Show();
    }

    public void RefreshKeys()
    {
    _sdkPath = EditorPrefs.GetString(BuildPrefs.SdkPathKey, string.Empty);
    _cleanTarget = EditorPrefs.GetBool(BuildPrefs.CleanTargetKey, true);
    _steamAccount = EditorPrefs.GetString(BuildPrefs.SteamAccountKey, string.Empty);
    _head = EditorPrefs.GetString(BuildPrefs.HeadKey, "0");
    _buildType = (BuildType)EditorPrefs.GetInt(BuildPrefs.BuildTypeKey, (int)BuildType.Normal);
    _platform = (PlatformType)EditorPrefs.GetInt(BuildPrefs.PlatformKey, (int)PlatformType.Standalone);
    }

    private void OnEnable()
    {
        RefreshKeys();
    }

    private void OnDisable()
    {
        SavePrefs();
    }

    private void SavePrefs()
    {
    EditorPrefs.SetString(BuildPrefs.SdkPathKey, _sdkPath ?? string.Empty);
    EditorPrefs.SetBool(BuildPrefs.CleanTargetKey, _cleanTarget);
    EditorPrefs.SetString(BuildPrefs.SteamAccountKey, _steamAccount ?? string.Empty);
    EditorPrefs.SetString(BuildPrefs.HeadKey, _head ?? "0");
    EditorPrefs.SetInt(BuildPrefs.BuildTypeKey, (int)_buildType);
    EditorPrefs.SetInt(BuildPrefs.PlatformKey, (int)_platform);
    }

    private void OnGUI()
    {
        // SDK 경로가 유효하면 해당 SDK 기준으로 경로 구성
        var hasValidSdk = IsValidSdkPath(_sdkPath, out var contentBuilderRoot);
        var paths = hasValidSdk ? BuildScript.PreparePaths(_sdkPath) : (projectRoot: string.Empty, steamRoot: contentBuilderRoot, contentRoot: string.Empty, buildFolder: string.Empty, buildExePath: string.Empty);
    var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
    var selectedVdfRelative = BuildScript.GetVdfRelative(_buildType);
    var selectedVdfFull = Path.Combine(projectRoot, selectedVdfRelative);

        EditorGUILayout.LabelField("경로", EditorStyles.boldLabel);
        using (new EditorGUI.IndentLevelScope())
        {
            EditorGUILayout.LabelField("SDK Root (sdk)", string.IsNullOrEmpty(_sdkPath) ? "(미설정)" : _sdkPath);
            EditorGUILayout.LabelField("ContentBuilder Root", contentBuilderRoot ?? "(유효한 SDK 필요)");
            if (hasValidSdk)
            {
                EditorGUILayout.LabelField("Content Root", paths.contentRoot);
                EditorGUILayout.LabelField("Build Folder", paths.buildFolder);
                EditorGUILayout.LabelField("Exe Path", paths.buildExePath);
            }
        }

        // SDK 경로 입력/선택
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Steamworks SDK", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            _sdkPath = EditorGUILayout.TextField("SDK의 'sdk' 폴더", _sdkPath ?? string.Empty);
            if (GUILayout.Button("…", GUILayout.Width(30)))
            {
                var picked = EditorUtility.OpenFolderPanel("Select Steamworks SDK 'sdk' folder", string.IsNullOrEmpty(_sdkPath) ? Application.dataPath : _sdkPath, "");
                if (!string.IsNullOrEmpty(picked))
                {
                    _sdkPath = picked;
                }
            }
        }

        if (!hasValidSdk)
        {
            EditorGUILayout.HelpBox(
                "Steamworks SDK를 먼저 설치한 뒤, SDK의 'sdk' 폴더 경로를 지정하세요.\n다운로드: https://partner.steamgames.com/downloads/list",
                MessageType.Error);
            if (GUILayout.Button("다운로드 페이지 열기"))
            {
                Application.OpenURL("https://partner.steamgames.com/downloads/list");
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("옵션", EditorStyles.boldLabel);
        _cleanTarget = EditorGUILayout.ToggleLeft("빌드 전에 대상 폴더 정리", _cleanTarget);
        _head = EditorGUILayout.TextField("HeadVer", _head ?? "0");
    _buildType = (BuildType)EditorGUILayout.EnumPopup("빌드 타입", _buildType);

        // 플랫폼 선택 및 현재 정의 상태 표시
        _platform = (PlatformType)EditorGUILayout.EnumPopup("플랫폼", _platform);
        using (new EditorGUILayout.HorizontalScope())
        {
            // 현재 Standalone 그룹의 DISABLESTEAMWORKS 상태 표시
            var currentDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone) ?? string.Empty;
            bool disabled = currentDefines.Contains("DISABLESTEAMWORKS");
            EditorGUILayout.LabelField("STEAMWORKS", !disabled ? "ON" : "OFF");
            if (GUILayout.Button("플랫폼 정의 적용", GUILayout.Width(120)))
            {
                BuildScript.ApplyPlatformDefines(_platform);
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Steam 업로드", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "빌드 타입에 따라 VDF가 선택됩니다.\nNormal: Tools/app_build.vdf\nDemo: Tools/app_build_demo.vdf",
            MessageType.Info);
        using (new EditorGUILayout.VerticalScope("box"))
        {
            EditorGUILayout.LabelField("Steam 로그인", EditorStyles.miniBoldLabel);
            _steamAccount = EditorGUILayout.TextField("Account", _steamAccount ?? string.Empty);
            if (GUILayout.Button("로그인 테스트"))
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(_steamAccount))
                        throw new Exception("Steam 계정을 입력하세요.");
                    var pathsForTest = BuildScript.PreparePaths(_sdkPath);
                    BuildScript.RunSteamLoginTest(pathsForTest.steamRoot, _steamAccount);
                    EditorUtility.DisplayDialog("Steam 로그인 테스트", "로그인 콘솔을 실행했습니다. 필요 시 Steam Guard 코드를 입력하세요.", "확인");
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    EditorUtility.DisplayDialog("로그인 테스트 실패", ex.Message, "확인");
                }
            }
        }

        // 상태 표시
        EditorGUILayout.Space();
    DrawStatus(_sdkPath, contentBuilderRoot, selectedVdfFull);

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            // Build Only는 SDK만 유효하면 가능
            EditorGUI.BeginDisabledGroup(!hasValidSdk);
            if (GUILayout.Button("Build Only", GUILayout.Height(32)))
            {
                RunBuild(BuildScript.PreparePaths(_sdkPath));
            }
            EditorGUI.EndDisabledGroup();

            // Build + Upload는 Steam 플랫폼에서만 가능
            EditorGUI.BeginDisabledGroup(!hasValidSdk || _platform != PlatformType.Steam);
            if (GUILayout.Button("Build + Upload", GUILayout.Height(32)))
            {
                RunBuild(BuildScript.PreparePaths(_sdkPath), andUpload: true);
            }
            EditorGUI.EndDisabledGroup();

            // Upload Only 역시 Steam 플랫폼에서만 가능
            EditorGUI.BeginDisabledGroup(!hasValidSdk || _platform != PlatformType.Steam);
            if (GUILayout.Button("Upload Only", GUILayout.Height(32)))
            {
                RunUpload(BuildScript.PreparePaths(_sdkPath));
            }
            EditorGUI.EndDisabledGroup();
        }

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUI.BeginDisabledGroup(!hasValidSdk);
            if (GUILayout.Button("ContentBuilder 폴더 열기"))
            {
                Process.Start("explorer.exe", paths.steamRoot);
            }
            if (GUILayout.Button("빌드 폴더 열기"))
            {
                Process.Start("explorer.exe", paths.buildFolder);
            }
            EditorGUI.EndDisabledGroup();
        }

        // 유틸리티: 저장 데이터/설정 초기화
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("유틸리티", EditorStyles.boldLabel);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("PlayerPrefs 전체 초기화"))
            {
                if (EditorUtility.DisplayDialog("확인", "모든 PlayerPrefs를 삭제할까요?\n(되돌릴 수 없습니다)", "예", "아니오"))
                {
                    BuildScript.ClearAllPlayerPrefs();
                }
            }
            if (GUILayout.Button("Build Window 설정 초기화"))
            {
                if (EditorUtility.DisplayDialog("확인", "Build Window 관련 EditorPrefs를 삭제할까요?", "예", "아니오"))
                {
                    BuildScript.ClearBuildWindowEditorPrefs();
                }
            }
        }
    }

    private bool IsValidSdkPath(string sdkRoot, out string contentBuilderRoot)
    {
        contentBuilderRoot = null;
        if (string.IsNullOrWhiteSpace(sdkRoot)) return false;
        try
        {
            contentBuilderRoot = Path.Combine(sdkRoot, "tools", "ContentBuilder");
            var runBat = Path.Combine(contentBuilderRoot, "run_build.bat");
            return Directory.Exists(contentBuilderRoot) && File.Exists(runBat);
        }
        catch
        {
            return false;
        }
    }

    private void DrawStatus(string sdkRoot, string contentBuilderRoot, string vdfPath)
    {
        void StatusLine(string label, bool ok, string path)
        {
            var color = ok ? Color.green : Color.red;
            using (new EditorGUILayout.HorizontalScope())
            {
                var prev = GUI.color;
                GUI.color = color;
                GUILayout.Label(ok ? "OK" : "NG", GUILayout.Width(28));
                GUI.color = prev;
                EditorGUILayout.LabelField(label, path);
            }
        }

        bool sdkOk = !string.IsNullOrEmpty(sdkRoot) && Directory.Exists(sdkRoot);
        bool cbOk = !string.IsNullOrEmpty(contentBuilderRoot) && Directory.Exists(contentBuilderRoot) && File.Exists(Path.Combine(contentBuilderRoot, "run_build.bat"));
        bool vdfOk = !string.IsNullOrEmpty(vdfPath) && File.Exists(vdfPath);
        StatusLine("SDK Root (sdk)", sdkOk, string.IsNullOrEmpty(sdkRoot) ? "(미설정)" : sdkRoot);
        StatusLine("ContentBuilder", cbOk, contentBuilderRoot ?? "(미설정)");
        StatusLine("VDF", vdfOk, vdfPath ?? "(미설정)");
    }

    private void RunBuild((string projectRoot, string steamRoot, string contentRoot, string buildFolder, string buildExePath) paths, bool andUpload = false)
    {
        SavePrefs();
        try
        {
            BuildScript.EnsureScenesConfigured();
            BuildScript.EnsureSteamFolder(paths.steamRoot);
            // 플랫폼 정의 먼저 적용
            BuildScript.ApplyPlatformDefines(_platform);
            // HeadVer 계산 및 Unity AppVersion 갱신
            var head = EditorPrefs.GetString(BuildPrefs.HeadKey, string.Empty);
            if (string.IsNullOrWhiteSpace(head))
                throw new Exception("Head 값이 비어있습니다. Build Window에서 'Head'를 먼저 설정하세요.");
            var headVer = HeadVerUtil.Compute(paths.projectRoot, head);
            BuildScript.UpdateUnityAppVersion(headVer);
            var report = BuildScript.BuildWindows64(paths.buildExePath, _cleanTarget);
            BuildScript.LogBuildSummary(report);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new Exception("Unity 빌드 실패");
            }
            if (andUpload)
            {
                if (_platform == PlatformType.Steam)
                {
                    RunUpload(paths);
                }
                else
                {
                    Debug.Log("플랫폼이 Standalone이므로 업로드를 건너뜁니다. (빌드만 완료)");
                    EditorUtility.DisplayDialog("업로드 건너뜀", "플랫폼이 Steam일 때만 업로드됩니다.", "확인");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            EditorUtility.DisplayDialog("Build 실패", ex.Message, "확인");
        }
    }

    private void RunUpload((string projectRoot, string steamRoot, string contentRoot, string buildFolder, string buildExePath) paths)
    {
        SavePrefs();
        try
        {
            if (_platform != PlatformType.Steam)
            {
                EditorUtility.DisplayDialog("업로드 불가", "플랫폼이 Steam일 때만 업로드할 수 있습니다.", "확인");
                return;
            }
            BuildScript.EnsureSteamFolder(paths.steamRoot);
            if (string.IsNullOrWhiteSpace(_steamAccount))
            {
                throw new Exception("Steam 계정을 입력하세요.");
            }
            var head = EditorPrefs.GetString(BuildPrefs.HeadKey, "0");
            if (string.IsNullOrWhiteSpace(head))
                throw new Exception("Head 값이 비어있습니다. Build Window에서 'Head'를 먼저 설정하세요.");
            BuildScript.RunSteamUpload(paths.steamRoot, paths.projectRoot, head, _steamAccount, _buildType);
            EditorUtility.DisplayDialog("Steam 업로드", "업로드 콘솔을 실행했습니다. 터미널에서 진행 상황을 확인하세요.", "확인");
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            EditorUtility.DisplayDialog("업로드 실패", ex.Message, "확인");
        }
    }
}

// HeadVer 계산 결과 컨테이너
internal sealed class HeadVerResult
{
    // 최종 버전 포맷: {head}.{yearweek}.{githash}
    public string FullVersion { get; }
    public string Head { get; }
    public string YearWeek { get; }
    public string GitHash { get; }

    public HeadVerResult(string head, string yearWeek, string gitHash)
    {
        Head = head;
        YearWeek = yearWeek;
        GitHash = gitHash;
        FullVersion = $"{Head}.{YearWeek}.{GitHash}";
    }
}

// 간이 HeadVer 구현: 태그/커밋 메타로 SemVer 생성, build metadata에 짧은 git hash
internal static class HeadVerUtil
{
    public static HeadVerResult Compute(string repoRoot, string head)
    {
        if (string.IsNullOrWhiteSpace(head)) head = "0";
        string yearWeek = GetIsoYearWeek(DateTime.UtcNow);
        string hash = GitUtil.TryGetShortHash(repoRoot) ?? "unknown";
        return new HeadVerResult(head, yearWeek, hash);
    }

    private static string GetIsoYearWeek(DateTime utcNow)
    {
        // ISO-8601 주차 계산: ISO 주는 월~일, 한 해의 첫 주는 그 해의 첫 목요일을 포함
        // .NET의 CalendarWeekRule/FirstFourDayWeek + Monday 기준으로 구현
        var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
        var dfi = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat;
        var week = cal.GetWeekOfYear(utcNow, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        int isoWeek = week;
        int isoYear = utcNow.Year;

        // 연초/연말 경계 보정: ISO 규칙에 따라 1주가 전년도로, 52/53주가 다음 해로 넘어가는 경우 처리
        // 날짜가 1월이고 주가 52 이상이면 ISO 연도는 전년
        if (utcNow.Month == 1 && isoWeek >= 52)
        {
            isoYear = utcNow.Year - 1;
        }
        // 날짜가 12월이고 주가 1이면 ISO 연도는 다음 해
        else if (utcNow.Month == 12 && isoWeek == 1)
        {
            isoYear = utcNow.Year + 1;
        }

        return $"{isoYear % 100:00}{isoWeek:00}"; // YYWW
    }
}

internal static class GitUtil
{
    public static string TryGetShortHash(string repoRoot)
    {
        var (ok, output) = TryRun(repoRoot, "rev-parse --short HEAD");
        return ok ? output?.Trim() : null;
    }

    public static (bool ok, string output) TryRun(string repoRoot, string args)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                WorkingDirectory = string.IsNullOrEmpty(repoRoot) ? null : repoRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };
            using (var p = Process.Start(psi))
            {
                string stdout = p.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();
                p.WaitForExit(5000);
                if (p.ExitCode == 0)
                {
                    return (true, stdout);
                }
                UnityEngine.Debug.LogWarning($"git {args} 실패: {stderr}");
                return (false, null);
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogWarning($"git 실행 실패: {ex.Message}");
            return (false, null);
        }
    }
}
#endif
