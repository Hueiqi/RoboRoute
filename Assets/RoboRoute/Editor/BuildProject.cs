using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace RoboRoute.Editor {
public static class BuildProject {
 public static void Setup(){
 Directory.CreateDirectory("Assets/RoboRoute/Resources"); if(AssetDatabase.LoadAssetAtPath<Material>("Assets/RoboRoute/Resources/WarehouseLit.mat")==null) AssetDatabase.CreateAsset(new Material(Shader.Find("Universal Render Pipeline/Lit")),"Assets/RoboRoute/Resources/WarehouseLit.mat"); PlayerSettings.companyName="RoboRoute";PlayerSettings.productName="RoboRoute";
 var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("RoboRoute Simulation").AddComponent<ShiftController>();Directory.CreateDirectory("Assets/RoboRoute/Scenes");EditorSceneManager.SaveScene(scene,"Assets/RoboRoute/Scenes/Warehouse.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/RoboRoute/Scenes/Warehouse.unity",true)};
 Check();AssetDatabase.SaveAssets();
 }
 public static void Check(){int count=0;Action<bool,string> test=(ok,label)=>{if(!ok)throw new Exception(label);count++;};
 var s=new ShiftState();test(s.IsValid(),"Fresh state");test(!s.Deliver(0),"No cargo");s.carrying=true;test(!s.Deliver(1),"Wrong station");s.Tick(5);test(s.Deliver(0)&&s.score==100,"Score");test(!s.Deliver(0),"Duplicate");s.Tick(75);test(s.failed==1&&!s.carrying&&s.Destination==2,"Deadline advances queue");test(s.IsValid(),"Valid timed state");var copy=JsonUtility.FromJson<ShiftState>(JsonUtility.ToJson(s));test(copy.IsValid()&&copy.failed==1,"JSON roundtrip");s.battery=float.NaN;test(!s.IsValid(),"Reject NaN");s=new ShiftState();for(int i=0;i<6;i++)s.Tick(75);test(s.Complete&&s.failed==6,"Failed shift ends");s.carrying=true;test(!s.IsValid(),"No cargo after end");
 s=new ShiftState { delivered=1, score=100, elapsed=100f, totalDeliveryTime=99.99f, missionTime=.01001f, carrying=true }; test(s.IsValid(),"Rounded timing input valid"); test(s.Deliver(1)&&s.IsValid()&&s.totalDeliveryTime<=s.elapsed,"Accumulated timing remains saveable after float rounding");
 foreach(var goal in WarehouseBuilder.Stations){var route=GridRouter.Find(WarehouseBuilder.Pickup,goal);test(route.Count>0,"Route exists");foreach(var p in route)test(GridRouter.Walkable(new Vector2Int(Mathf.RoundToInt(p.x),Mathf.RoundToInt(p.z))),"Obstacle clearance");}
 count+=AchievementChecks.Run();
 Directory.CreateDirectory("Evidence");File.WriteAllText("Evidence/domain-checks.txt",DateTime.UtcNow.ToString("O")+" Unity "+Application.unityVersion+"\nPASS: "+count+" assertions\n");Debug.Log("ROBOROUTE CHECKS PASSED: "+count);
 }
 public static void Windows(){Setup();var r=BuildPipeline.BuildPlayer(EditorBuildSettings.scenes,"Build/RoboRoute.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);if(r.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");}
}}


