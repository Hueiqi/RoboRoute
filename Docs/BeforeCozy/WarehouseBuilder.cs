using UnityEngine;

namespace RoboRoute
{
    public static class WarehouseBuilder
    {
        public static readonly Vector3 Dock = new Vector3(-10, 0, -7);
        public static readonly Vector3 Pickup = new Vector3(-10, 0, 6);
        public static readonly Vector3[] Stations = { new Vector3(10,0,6), new Vector3(10,0,0), new Vector3(10,0,-6) };
        public static readonly Color Teal = new Color(.13f,.87f,.75f);
        public static readonly Color[] Colors = { new Color(.24f,.65f,1), new Color(1,.65f,.18f), new Color(.77f,.43f,1) };

        static Material Material(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var template = Resources.Load<Material>("WarehouseLit");
            var mat = template != null ? new Material(template) : new Material(shader);
            mat.color = color;
            return mat;
        }

        public static GameObject Box(string name, Vector3 pos, Vector3 scale, Color color, Transform parent, bool solid = true)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = pos;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().sharedMaterial = Material(color);
            if (!solid) obj.GetComponent<Collider>().enabled = false;
            return obj;
        }

        static void Label(string text, Vector3 pos, Transform parent, Color color)
        {
            var obj = new GameObject(text);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = pos;
            obj.transform.rotation = Quaternion.Euler(65, 0, 0);
            var label = obj.AddComponent<TextMesh>();
            label.text = text;
            label.fontSize = 48;
            label.characterSize = .11f;
            label.anchor = TextAnchor.MiddleCenter;
            label.color = color;
        }

        public static RobotMotor Build(Transform root, ShiftController shift, out GameObject cargo)
        {
            Color floor = new Color(.10f,.14f,.19f);
            Box("Warehouse floor", new Vector3(0,-.25f,0), new Vector3(28,.5f,22), floor, root);
            for (int x=-12;x<=12;x+=2)
                Box("Floor line",new Vector3(x,.012f,0),new Vector3(.025f,.015f,20),new Color(.18f,.24f,.29f),root,false);
            for (int z=-10;z<=10;z+=2)
                Box("Floor line",new Vector3(0,.013f,z),new Vector3(26,.015f,.025f),new Color(.18f,.24f,.29f),root,false);
            Box("North wall",new Vector3(0,.6f,11),new Vector3(28,1.2f,.4f),floor,root);
            Box("South wall",new Vector3(0,.6f,-11),new Vector3(28,1.2f,.4f),floor,root);
            Box("West wall",new Vector3(-14,.6f,0),new Vector3(.4f,1.2f,22),floor,root);
            Box("East wall",new Vector3(14,.6f,0),new Vector3(.4f,1.2f,22),floor,root);
            for (int x=-4;x<=4;x+=8)
                for (int z=-3;z<=3;z+=6)
                {
                    Box("Storage rack",new Vector3(x,1.1f,z),new Vector3(3,2.2f,2.4f),new Color(.22f,.29f,.36f),root);
                    Box("Rack trim",new Vector3(x,2.22f,z),new Vector3(3.1f,.08f,2.5f),Teal,root,false);
                    for (int i=-1;i<=1;i++) Box("Stored box",new Vector3(x+i,2.6f,z),new Vector3(.8f,.7f,1),new Color(.55f,.39f,.23f),root,false);
                }
            Box("Charging zone",Dock+Vector3.up*.025f,new Vector3(3,.05f,3),Teal,root,false);
            Label("CHARGE",Dock+new Vector3(0,.12f,-1.9f),root,Teal);
            Box("Pickup zone",Pickup+Vector3.up*.025f,new Vector3(3,.05f,3),new Color(.9f,.9f,.92f),root,false);
            Label("PICKUP",Pickup+new Vector3(0,.12f,2),root,Color.white);
            for(int i=0;i<3;i++)
            {
                Box("Station "+(char)('A'+i),Stations[i]+Vector3.up*.025f,new Vector3(3,.05f,3),Colors[i],root,false);
                Label("BAY "+(char)('A'+i),Stations[i]+new Vector3(0,.12f,2),root,Colors[i]);
            }
            var robot = new GameObject("RR-01 Player Robot");
            robot.transform.SetParent(root,false);
            robot.transform.position = Dock+Vector3.up*.1f;
            var cc=robot.AddComponent<CharacterController>();
            cc.center=new Vector3(0,.65f,0); cc.height=1.3f; cc.radius=.6f; cc.stepOffset=.15f;
            Box("Body",new Vector3(0,.55f,0),new Vector3(1.15f,.6f,1.45f),Teal,robot.transform,false);
            Box("Sensor",new Vector3(0,1,.25f),new Vector3(.8f,.3f,.5f),floor,robot.transform,false);
            Box("Front light",new Vector3(0,.6f,.74f),new Vector3(.8f,.12f,.03f),Color.white,robot.transform,false);
            foreach(float x in new[]{-.62f,.62f})
                Box("Wheel",new Vector3(x,.27f,0),new Vector3(.22f,.42f,1.2f),new Color(.04f,.06f,.08f),robot.transform,false);
            cargo=Box("Carried package",new Vector3(0,1.3f,-.25f),new Vector3(.7f,.65f,.7f),Colors[0],robot.transform,false);
            cargo.SetActive(false);
            var motor=robot.AddComponent<RobotMotor>(); motor.shift=shift;
            var cameraObj=new GameObject("Warehouse Camera"); cameraObj.transform.SetParent(root,false);
            var cam=cameraObj.AddComponent<Camera>(); cameraObj.tag="MainCamera";
            cam.transform.position=new Vector3(0,26,-19); cam.transform.LookAt(Vector3.zero);
            cam.rect=new Rect(.27f,.1f,.73f,.9f);
            cam.orthographic=true; cam.orthographicSize=16; cam.clearFlags=CameraClearFlags.SolidColor;
            cam.backgroundColor=new Color(.035f,.055f,.08f); cam.farClipPlane=100;
            cameraObj.AddComponent<AudioListener>();
            var sun=new GameObject("Warehouse light"); sun.transform.SetParent(root,false);
            sun.transform.rotation=Quaternion.Euler(55,-30,0);
            var light=sun.AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.5f;
            RenderSettings.ambientLight=new Color(.6f,.65f,.72f);
            return motor;
        }
    }
}

