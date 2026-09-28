using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace RoboRoute
{
    public sealed class CozyPreview : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-cozy-preview")>=0)
                new GameObject("Cozy preview capture").AddComponent<CozyPreview>();
        }
        IEnumerator Start()
        {
            Application.runInBackground=true;Application.targetFrameRate=60;
            yield return null;
            var shift=FindFirstObjectByType<ShiftController>();shift.BeginDemo();
            yield return new WaitForSeconds(3);
            yield return new WaitForEndOfFrame();
            var folder=Path.Combine(Application.dataPath,"..","Evidence");Directory.CreateDirectory(folder);
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,"cozy-preview.png"));
            File.WriteAllText(Path.Combine(folder,"cozy-scene-check.txt"),
                "Unity "+Application.unityVersion+"\nPlayer robots: "+FindObjectsByType<RobotMotor>(FindObjectsSortMode.None).Length+
                "\nAutonomous robots: "+FindObjectsByType<AutonomousRobot>(FindObjectsSortMode.None).Length+
                "\nRenderers: "+FindObjectsByType<Renderer>(FindObjectsSortMode.None).Length+
                "\nShift valid: "+shift.State.IsValid());
            yield return new WaitForSeconds(2);Application.Quit();
        }
    }
}
