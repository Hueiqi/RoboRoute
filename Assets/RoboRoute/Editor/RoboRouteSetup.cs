using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RoboRoute.Editor
{
    public static class RoboRouteSetup
    {
        [MenuItem("RoboRoute/Create Demo Scene")]
        public static void CreateScene()
        {
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string path="Assets/RoboRoute/Scenes/Warehouse.unity";
            if(File.Exists(path) && !EditorUtility.DisplayDialog("Replace demo scene?","This replaces the existing Warehouse scene. Save your edits elsewhere first.","Replace","Cancel")) return;
            Directory.CreateDirectory("Assets/RoboRoute/Scenes");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            new GameObject("RoboRoute Simulation").AddComponent<ShiftController>();
            EditorSceneManager.SaveScene(scene,path);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("RoboRoute ready","Press Play. The warehouse is generated at runtime. Click the Game view, then use WASD and E.","Ready");
        }

        [MenuItem("RoboRoute/Run Domain Checks")]
        public static void Checks()
        {
            var s=new ShiftState();
            Require(s.IsValid(),"Initial state valid");
            Require(!s.Deliver(0),"Reject delivery without cargo");
            s.carrying=true;
            Require(!s.Deliver(1) && s.carrying && s.score==0,"Wrong bay preserves cargo and score");
            Require(s.Deliver(0) && s.score==100 && !s.carrying,"Valid delivery awards once");
            Require(!s.Deliver(0) && s.score==100,"Reject duplicate delivery");
            for(int i=1;i<6;i++) {s.carrying=true; Require(s.Deliver(i%3),"Mission rotation");}
            Require(s.Complete && s.score==600 && s.IsValid(),"Completed shift valid");
            s.carrying=true; Require(!s.Deliver(0) && !s.IsValid(),"Reject cargo after completion");
            s=new ShiftState(); s.battery=float.NaN; Require(!s.IsValid(),"Reject invalid battery");
            s=new ShiftState(); s.elapsed=-1; Require(!s.IsValid(),"Reject negative time");
            Debug.Log("RoboRoute: all domain checks passed.");
            EditorUtility.DisplayDialog("Domain checks","All checks passed. See Console. Movement and UI still require play testing.","OK");
        }
        static void Require(bool value,string label) { if(!value) throw new Exception("RoboRoute check failed: "+label); }
    }
}
