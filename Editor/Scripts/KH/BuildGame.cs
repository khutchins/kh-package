using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEditor.Build.Reporting;

namespace KH.Editor {
	public class BuildGame : EditorWindow {

		public class Platform {
			public readonly string Name;
			public readonly string FolderName;
			public readonly string Extension;
			public readonly BuildTarget BuildTarget;
			public readonly bool ShouldCopyFilesAndFolders;

			public Platform(string name, string folderName, string extension, BuildTarget buildTarget, bool shouldCopyFilesAndFolders) {
				Name = name;
				FolderName = folderName;
				Extension = extension;
				BuildTarget = buildTarget;
				ShouldCopyFilesAndFolders = shouldCopyFilesAndFolders;
			}
		}

		public static readonly Platform PLATFORM_WIN = new Platform("Windows", "win", ".exe", BuildTarget.StandaloneWindows64, true);
		public static readonly Platform PLATFORM_LINUX = new Platform("Linux", "lin", ".x64", BuildTarget.StandaloneLinux64, true);
		public static readonly Platform PLATFORM_MAC = new Platform("Mac", "mac", null, BuildTarget.StandaloneOSX, true);
		public static readonly Platform PLATFORM_WEBGL = new Platform("WebGL", "web", null, BuildTarget.WebGL, false);

		public static readonly Platform[] SupportedPlatforms = { PLATFORM_WIN, PLATFORM_LINUX, PLATFORM_MAC, PLATFORM_WEBGL };

		public static void BuildForPlatforms(Platform[] targets, string[] ddServices) {
			// If our current build target is in the list, build that first
			// to slightly increase build speed.
			int index = targets.Select(x => x.BuildTarget).ToList().IndexOf(EditorUserBuildSettings.activeBuildTarget);
			if (index >= 0) {
				List<Platform> newTargets = new List<Platform>();
				newTargets.Add(targets[index]);
				newTargets.AddRange(targets);
				targets = newTargets.Distinct().ToArray();
			}

			foreach (Platform target in targets) {
				foreach (string service in ddServices) {
					string path = PathForBuildTarget(target, service);
					Debug.Log("Building " + target.Name);
					BuildPlayerOptions options = new BuildPlayerOptions();
					options.scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray();
					options.locationPathName = Combine(path, PlayerSettings.productName);
					options.target = target.BuildTarget;
					options.options = BuildOptions.ShowBuiltPlayer;
					BuildResult result = BuildPipeline.BuildPlayer(options).summary.result;
					if (result != BuildResult.Succeeded) {
						Debug.LogErrorFormat("Build not successful for platform {0}. Result was {1}. No further builds will be attempted.", target.Name, result);
						return;
					}
					PlatformSpecificPostProcessing(target, path);

					if (target.ShouldCopyFilesAndFolders) {
						string[] foldersToPull = new string[2];
						foldersToPull[0] = target.FolderName;
						foldersToPull[1] = service;
						CopyFilesAndFolders(Combine(Application.dataPath, "Output"), path, foldersToPull);
					}
				}
			}
		}

		private static void PlatformSpecificPostProcessing(Platform target, string path) {
			if (target.Extension != null) {
				string appPath = Combine(path, PlayerSettings.productName);
				string pathWithExt = appPath + target.Extension;
				Debug.Log("Moving " + path + " -> " + pathWithExt);
				File.Move(appPath, pathWithExt);
			}
		}

		private static string PathForBuildTarget(Platform platform, string ddService) {
			string parent = Directory.GetParent(Application.dataPath).ToString();
			Debug.Log("Parent: " + parent);
			string platformPath = Combine(parent, "build", ddService, platform.FolderName, PlayerSettings.productName);

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

			foreach (string dir in Directory.GetDirectories(src, "*", SearchOption.AllDirectories)) {
				Debug.Log("Dir: " + dir);
				Directory.CreateDirectory(dir.Replace(src, dest));
			}

			// Copy all the files & Replaces any files with the same name
			foreach (string path in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories)) {
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
			if (paths.Length > 0) {
				str = paths[0];
				for (int i = 1; i < paths.Length; i++) {
					str += "/" + paths[i];
				}
			}
			return str;
		}
	}
}