using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace KH.Editor {
	public class BuildGame : EditorWindow {

		private static string PLATFORM_WIN = "win";
		private static string PLATFORM_LINUX = "lin";
		private static string PLATFORM_MAC = "mac";
		private static string PLATFORM_WEBGL = "web";
		private static string DD_STEAM = "steam";
		private static string DD_ITCHIO = "itchio";

		private static string[] SupportedDDServices = { DD_ITCHIO };

		[MenuItem("Build/Build Supported Platforms")]
		static void BuildForSupportedPlatforms() {
			BuildTarget[] supportedTargets = { BuildTarget.StandaloneWindows64, BuildTarget.StandaloneOSX, BuildTarget.StandaloneLinux64, BuildTarget.WebGL };
			BuildForPlatforms(supportedTargets, SupportedDDServices);
		}

		[MenuItem("Build/Build Desktop Platforms")]
		static void BuildForDesktopPlatforms() {
			BuildTarget[] supportedTargets = { BuildTarget.StandaloneWindows64, BuildTarget.StandaloneOSX, BuildTarget.StandaloneLinux64 };
			BuildForPlatforms(supportedTargets, SupportedDDServices);
		}

		[MenuItem("Build/Build for Windows")]
		static void BuildForWindows() {
			BuildForPlatform(BuildTarget.StandaloneWindows64);
		}

		[MenuItem("Build/Build for Mac")]
		static void BuildForMac() {
			BuildForPlatform(BuildTarget.StandaloneOSX);
		}

		[MenuItem("Build/Build for Linux")]
		static void BuildForLinux() {
			BuildForPlatform(BuildTarget.StandaloneLinux64);
		}

		[MenuItem("Build/Build for WebGL")]
		static void BuildForWebGL() {
			BuildForPlatform(BuildTarget.WebGL);
		}

		static void BuildForPlatform(BuildTarget target) {
			BuildTarget[] targets = { target };
			BuildForPlatforms(targets, SupportedDDServices);
		}

		static void BuildForPlatforms(BuildTarget[] targets, string[] ddServices) {
			// If our current build target is in the list, build that first
			// to slightly increase build speed.
			if (targets.Contains(EditorUserBuildSettings.activeBuildTarget)) {
				List<BuildTarget> newTargets = new List<BuildTarget>();
				newTargets.Add(EditorUserBuildSettings.activeBuildTarget);
				newTargets.AddRange(targets);
				targets = newTargets.Distinct().ToArray();
			}

			foreach(BuildTarget target in targets) {
				foreach (string service in ddServices) {
					string path = PathForBuildTarget(target, service);
					Debug.Log("Building " + target);
					BuildPlayerOptions options = new BuildPlayerOptions();
					options.scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray();
					options.locationPathName = Combine(path, PlayerSettings.productName);
					options.target = target;
					options.options = BuildOptions.ShowBuiltPlayer;
					BuildPipeline.BuildPlayer(options);
					PlatformSpecificPostProcessing(target, path);

					if (ShouldCopyFilesAndFolders(target)) {
						string[] foldersToPull = new string[2];
						foldersToPull[0] = NameForBuildTarget(target);
						foldersToPull[1] = service;
						CopyFilesAndFolders(Combine(Application.dataPath, "Output"), path, foldersToPull);
					}
				}
			}
		}

		private static string FolderExtForBuildTarget(BuildTarget target) {
			switch(target) {
				case BuildTarget.StandaloneOSX:
					return ".app";
				default:
					return "";
			}
		}

		private static string ExtForBuildTarget(BuildTarget target) {
			switch(target) {
				case BuildTarget.StandaloneOSX:
					return ".app";
				case BuildTarget.StandaloneLinux64: 
					return ".x64";
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return ".exe";
				case BuildTarget.WebGL:
					return "";
				default:
					return ".UNSUPPORTED";
			}
		}

		/// <summary>
		/// Whether or not supplemental files (like licenses and READMEs)
		/// should be copied to the built folder. Will be true if the user
		/// can explore the file structure, false otherwise.
		/// </summary>
		/// <param name="target">The build target</param>
		/// <returns></returns>
		private static bool ShouldCopyFilesAndFolders(BuildTarget target) {
			switch(target) {
				case BuildTarget.StandaloneOSX:
				case BuildTarget.StandaloneLinux64: 
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return true;
				case BuildTarget.WebGL:
				default:
					return false;
			}
		}

		private static void PlatformSpecificPostProcessing(BuildTarget target, string path) {
			if(target == BuildTarget.StandaloneWindows 
				|| target == BuildTarget.StandaloneWindows64
				|| target == BuildTarget.StandaloneLinux64) {
				string appPath = Combine(path, PlayerSettings.productName);
				string pathWithExt = appPath + ExtForBuildTarget(target);
				Debug.Log("Moving " + path + " -> " + pathWithExt);
                File.Move(appPath, appPath + ExtForBuildTarget(target));
			}
		}

		private static string NameForBuildTarget(BuildTarget target) {
			switch (target) {
				case BuildTarget.StandaloneOSX:
					return PLATFORM_MAC;
				case BuildTarget.StandaloneLinux64:
					return PLATFORM_LINUX;
				case BuildTarget.StandaloneWindows:
				case BuildTarget.StandaloneWindows64:
					return PLATFORM_WIN;
				case BuildTarget.WebGL:
					return PLATFORM_WEBGL;
			}
			return "UNSUPPORTED";
		}

		private static string PathForBuildTarget(BuildTarget target, string ddService) {
			string platform = NameForBuildTarget(target);

			string parent = Directory.GetParent(Application.dataPath).ToString();
			Debug.Log("Parent: " + parent);
			string platformPath = Combine(parent, "build", ddService, platform, PlayerSettings.productName);

			if (Directory.Exists(platformPath)) {
				FileUtil.DeleteFileOrDirectory(platformPath);
			}

			return platformPath;
		}

		static void CopyFilesAndFolders(string src, string dest, string[] foldersToPull) {
			if (!Directory.Exists(src)) {
				Debug.Log("Not outputting extra files. Source does not exist. Source: " + src);
				return;
			}
			foreach (string dir in Directory.GetDirectories(src, "*", SearchOption.TopDirectoryOnly)) {
				// If something is marked as all, it should be present on all platforms
				string dirName = Path.GetFileName(dir);
				if (dirName == "all") {
					CopyFilesAndFolders(dir, dest);
				}
				if (foldersToPull.Contains(dirName)) {
					CopyFilesAndFolders(dir, dest, foldersToPull);
				}
			}
		}

		static void CopyFilesAndFolders(string src, string dest) {
			Debug.Log("CopyFilesAndFolders: " + src + "->" + dest);

			foreach(string dir in Directory.GetDirectories(src, "*", SearchOption.AllDirectories)) {
				Debug.Log("Dir: " + dir);
                Directory.CreateDirectory(dir.Replace(src, dest));
			}

			// Copy all the files & Replaces any files with the same name
			foreach(string path in Directory.GetFiles(src, "*.*", SearchOption.TopDirectoryOnly)) {
				Debug.Log("Path: " + path);
				File.Copy(path, path.Replace(src, dest), true);
			}

			DeleteFilesOfType(dest, "meta");
			DeleteFilesOfType(dest, "pdb");
		}

		static void DeleteFilesOfType(string folder, string type) {
			string[] files = Directory.GetFiles(folder, "*." + type, SearchOption.AllDirectories);
			foreach (string file in files) {
				FileUtil.DeleteFileOrDirectory(file);
			}
		}

		static string Combine(params string[] paths) {
			string str = "";
			if(paths.Length > 0) {
				str = paths[0];
				for(int i = 1; i < paths.Length; i++) {
					str += "/" + paths[i];
				}
			}
			return str;
		}
	}
}