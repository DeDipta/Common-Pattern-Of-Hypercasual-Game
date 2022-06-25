using System.IO;
using UnityEditor.Callbacks;
using UnityEditor;
using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace Balaso
{
    /// <summary>
    /// PostProcessor script to automatically fill all required dependencies
    /// for App Tracking Transparency
    /// </summary>
    public class AppTrackingTransparencyPostProcessor
    {
#if UNITY_IOS
        [MenuItem("Tools/AppTrackingTransparencyPostProcessor/Run manually")]
        public static void RunManual()
        {
            var path = EditorPrefs.GetString("AppTrackingTransparencyPostProcessorPath", Application.dataPath);
            path = EditorUtility.OpenFolderPanel("Select build folder", path, string.Empty);
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return;
            EditorPrefs.SetString("AppTrackingTransparencyPostProcessorPath", path);
            OnPostprocessBuild(BuildTarget.iOS, path);
        }

        [MenuItem("Tools/AppTrackingTransparencyPostProcessor/verify")]
        public static void RunVerify()
        {
            var path = EditorPrefs.GetString("AppTrackingTransparencyPostProcessorPath", Application.dataPath);
            path = EditorUtility.OpenFolderPanel("Select build folder", path, string.Empty);
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return;
            EditorPrefs.SetString("AppTrackingTransparencyPostProcessorPath", path);
            VerifySkAdNetworks(path);
        }


        private static void VerifySkAdNetworks(string pathToBuiltProject)
        {


            string plistPath = Path.Combine(pathToBuiltProject, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            var rootDict = plist.root;

            PlistElementArray SKAdNetworkItems = null;
            if (rootDict.values.ContainsKey("SKAdNetworkItems"))
            {
                try
                {
                    SKAdNetworkItems = rootDict.values["SKAdNetworkItems"] as PlistElementArray;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(string.Format("Could not obtain SKAdNetworkItems PlistElementArray: {0}", e.Message));
                }
            }
            else
            {
                Debug.LogError("SKAdNetworkItems not found in plist");
                return;
            }

            // If not exists, create it
            if (SKAdNetworkItems == null)
            {
                Debug.LogError("SKAdNetworkItems not found in plist");
                return;
            }



            if (SettingsInspector.Settings.SkAdNetworkIds == null)
                SettingsInspector.Settings.SkAdNetworkIds = new List<MediationInfo>();
            var list = new List<MediationInfo>(SettingsInspector.Settings.SkAdNetworkIds);
            var networkIdsWithoutDuplicates = list.Where(x => x.enable).SelectMany(y => y.skAdNetworkIds).Distinct().ToList();

            string plistContent = File.ReadAllText(pathToBuiltProject + "/Info.plist");
            for (int i = 0; i < networkIdsWithoutDuplicates.Count; i++)
            {
                if (plistContent.Contains(networkIdsWithoutDuplicates[i]))
                {
                    Debug.Log("SKAdNetworkIdentifier Verified  =>  " + networkIdsWithoutDuplicates[i]);
                }
                else
                {
                    Debug.LogError("SKAdNetworkIdentifier not found  =>  " + networkIdsWithoutDuplicates[i]);
                }
            }

            //var capabilities = rootDict["UIRequiredDeviceCapabilities"].AsArray();
            //capabilities.values.RemoveAll(item => item.AsString() == "arm64");
            //File.WriteAllText(plistPath, plist.WriteToString());

        }



        [PostProcessBuild]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                /*
                 * PBXProject
                 */
                PBXProject project = new PBXProject();
                string projectPath = PBXProject.GetPBXProjectPath(buildPath);
                project.ReadFromFile(projectPath);

                // If loaded, add `AppTrackingTransparency` Framework
                if (project != null)
                {
                    string targetId;
#if UNITY_2019_3_OR_NEWER
                    targetId = project.GetUnityFrameworkTargetGuid();
#else
                    targetId = project.TargetGuidByName("Unity-iPhone");
#endif

                    project.AddFrameworkToProject(targetId, "AppTrackingTransparency.framework", true);
                    project.AddFrameworkToProject(targetId, "AdSupport.framework", false);
                    project.AddFrameworkToProject(targetId, "StoreKit.framework", false);

                    project.WriteToFile(PBXProject.GetPBXProjectPath(buildPath));
                }

                /*
                 * PList
                 */
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(buildPath + "/Info.plist"));
                if (plist != null)
                {
                    // Get root
                    PlistElementDict rootDict = plist.root;

                    // Add NSUserTrackingUsageDescription
                    rootDict.SetString("NSUserTrackingUsageDescription", SettingsInspector.Settings.PopupMessage);

                    // Check if SKAdNetworkItems already exists
                    PlistElementArray SKAdNetworkItems = null;
                    if (rootDict.values.ContainsKey("SKAdNetworkItems"))
                    {
                        try
                        {
                            SKAdNetworkItems = rootDict.values["SKAdNetworkItems"] as PlistElementArray;
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning(string.Format("Could not obtain SKAdNetworkItems PlistElementArray: {0}", e.Message));
                        }
                    }

                    // If not exists, create it
                    if (SKAdNetworkItems == null)
                    {
                        SKAdNetworkItems = rootDict.CreateArray("SKAdNetworkItems");
                    }

                    if (SettingsInspector.Settings.SkAdNetworkIds == null)
                        SettingsInspector.Settings.SkAdNetworkIds = new List<MediationInfo>();

                    //List<string> networkIdsWithoutDuplicates = SettingsInspector.Settings.SkAdNetworkIds.Distinct().ToList();
                    var list = new List<MediationInfo>(SettingsInspector.Settings.SkAdNetworkIds);
                    var networkIdsWithoutDuplicates = list.Where(x => x.enable).SelectMany(y => y.skAdNetworkIds).Distinct().ToList();

                    string plistContent = File.ReadAllText(buildPath + "/Info.plist");
                    for (int i = 0; i < networkIdsWithoutDuplicates.Count; i++)
                    {
                        if (!plistContent.Contains(networkIdsWithoutDuplicates[i]))
                        {
                            PlistElementDict SKAdNetworkIdentifierDict = SKAdNetworkItems.AddDict();
                            SKAdNetworkIdentifierDict.SetString("SKAdNetworkIdentifier", networkIdsWithoutDuplicates[i]);
                            Debug.Log("Added SKAdNetworkIdentifier  =>  " + networkIdsWithoutDuplicates[i]);
                        }
                        else
                        {
                            Debug.Log("Already Added SKAdNetworkIdentifier  =>  " + networkIdsWithoutDuplicates[i]);
                        }
                    }
                    File.WriteAllText(buildPath + "/Info.plist", plist.WriteToString());
                }
            }
        }
#endif
    }
}