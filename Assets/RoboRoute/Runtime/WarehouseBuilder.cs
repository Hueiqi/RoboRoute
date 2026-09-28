using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RoboRoute
{
    // All scenery is built from original geometry. Collision footprints match GridRouter.
    public static class WarehouseBuilder
    {
        public static readonly Vector3 Dock = new Vector3(-10, 0, -7);
        public static readonly Vector3 Pickup = new Vector3(-10, 0, 6);
        public static readonly Vector3[] Stations = {new Vector3(10,0,6),new Vector3(10,0,0),new Vector3(10,0,-6)};
        public static readonly Color Teal = Hex("36BBAA");
        public static readonly Color[] Colors = {Hex("36BBAA"),Hex("EF8875"),Hex("F4C65F")};
        static readonly Color Cream=Hex("FFF0CE"), Wood=Hex("966445"), Ink=Hex("253C48"), Grass=Hex("91B76E"), Stone=Hex("D9BD93");
        static readonly Dictionary<Color,Material> materials=new Dictionary<Color,Material>();
        public static Color Hex(string value) {ColorUtility.TryParseHtmlString("#"+value,out var c);return c;}
        static Material Mat(Color color)
        {
            if(materials.TryGetValue(color,out var material) && material!=null) return material;
            var template=Resources.Load<Material>("WarehouseLit");
            material=template!=null?new Material(template):new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color=color; material.SetFloat("_Smoothness",.22f); materials[color]=material;return material;
        }
        public static GameObject Shape(string name,PrimitiveType type,Vector3 position,Vector3 scale,Color color,Transform parent,bool solid=false)
        {
            var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=position;o.transform.localScale=scale;
            o.GetComponent<Renderer>().sharedMaterial=Mat(color);o.GetComponent<Collider>().enabled=solid;return o;
        }
        public static GameObject Box(string name,Vector3 pos,Vector3 scale,Color color,Transform parent,bool solid=true) => Shape(name,PrimitiveType.Cube,pos,scale,color,parent,solid);
        static GameObject Ball(string name,Vector3 pos,Vector3 size,Color color,Transform parent) => Shape(name,PrimitiveType.Sphere,pos,size,color,parent);
        static GameObject Post(string name,Vector3 pos,float radius,float height,Color color,Transform parent) => Shape(name,PrimitiveType.Cylinder,pos,new Vector3(radius,height/2,radius),color,parent);
        static Transform Group(string name,Vector3 position,Transform parent)
        {var o=new GameObject(name);o.transform.SetParent(parent,false);o.transform.localPosition=position;return o.transform;}
        public static void Label(string text,Vector3 pos,Transform parent,Color color,float size=.16f)
        {
            var o=new GameObject(text);o.transform.SetParent(parent,false);o.transform.localPosition=pos;
            var t=o.AddComponent<TextMesh>();t.text=text;t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.color=color;
            o.transform.rotation=Quaternion.Euler(45,-35,0);
        }
        static void Fence(Vector3 start,Vector3 end,Transform parent)
        {
            int n=Mathf.CeilToInt(Vector3.Distance(start,end)/1.7f);
            for(int i=0;i<=n;i++){var p=Vector3.Lerp(start,end,i/(float)n);Post("Oak fence post",p+Vector3.up*.55f,.18f,1.1f,Wood,parent);Ball("Post cap",p+Vector3.up*1.12f,Vector3.one*.25f,Cream,parent);}
            var center=(start+end)/2;var delta=end-start;
            for(int i=0;i<2;i++){var rail=Box("Fence rail",center+Vector3.up*(.35f+i*.4f),new Vector3(.12f,.13f,delta.magnitude),Wood,parent,false);rail.transform.rotation=Quaternion.LookRotation(delta);}
        }
        static void Plant(Vector3 p,float size,Transform root,int seed)
        {
            var r=new System.Random(seed);Post("Terracotta planter",p+Vector3.up*.18f,.65f*size,.35f,Hex("CD9271"),root);
            for(int j=0;j<4;j++){var q=p+new Vector3((float)r.NextDouble()*.5f-.25f,.4f+(float)r.NextDouble()*.3f,(float)r.NextDouble()*.5f-.25f)*size;Ball("Soft foliage",q,Vector3.one*size*.65f,j%2==0?Hex("548C64"):Hex("A6C976"),root);}
            for(int j=0;j<3;j++){var q=p+new Vector3((j-1)*.2f,.78f,.12f)*size;Ball("Daisy",q,Vector3.one*.16f*size,Cream,root);Ball("Pollen",q+Vector3.up*.06f,Vector3.one*.07f*size,Colors[2],root);}
        }
        static void Tree(Vector3 p,float size,Transform root)
        {
            Post("Tree trunk",p+Vector3.up*size,.35f,size*2,Wood,root);
            Ball("Tree crown",p+Vector3.up*size*2.3f,new Vector3(2.1f,2.2f,2)*size,Hex("60996A"),root);
            Ball("Tree crown sunlit",p+new Vector3(.65f,2.55f,-.15f)*size,Vector3.one*1.6f*size,Hex("87B875"),root);
            Ball("Tree crown soft",p+new Vector3(-.65f,2.25f,.35f)*size,Vector3.one*1.55f*size,Hex("ACC978"),root);
        }
        static void Lantern(Vector3 p,Transform root)
        {
            Post("Lamp post",p+Vector3.up*.8f,.13f,1.6f,Wood,root);
            Box("Warm lantern",p+Vector3.up*1.8f,new Vector3(.32f,.43f,.32f),Hex("FFE6A1"),root,false);
            Box("Lantern cap",p+Vector3.up*2.06f,new Vector3(.43f,.12f,.43f),Wood,root,false);
        }
        static void House(Vector3 p,Color roof,Transform root,string title)
        {
            var h=Group(title,p,root);
            Box("Stone foundation",new Vector3(0,.15f,0),new Vector3(3.2f,.3f,2.6f),Stone,h,false);
            Box("Cream plaster",new Vector3(0,1.2f,0),new Vector3(3,2.1f,2.4f),Cream,h,true);
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Box("Timber corner",new Vector3(x*1.43f,1.3f,z*1.16f),new Vector3(.18f,2.3f,.18f),Wood,h,false);
            for(int z=-1;z<=1;z+=2)Box("Timber lintel",new Vector3(0,2.1f,z*1.22f),new Vector3(3.15f,.17f,.14f),Wood,h,false);
            // Individual roof tiles make the silhouette read at the gameplay camera distance.
            for(int side=-1;side<=1;side+=2)for(int row=0;row<5;row++)for(int col=0;col<7;col++)
            {
                float x=side*(.16f+row*.33f);float y=3.2f-row*.18f;
                var tile=Box("Glazed roof tile",new Vector3(x,y,-1.45f+col*.48f),new Vector3(.47f,.16f,.54f),Color.Lerp(roof,Cream,((row+col)%3)*.07f),h,false);
                tile.transform.localRotation=Quaternion.Euler(0,0,-side*29);
            }
            Box("Roof ridge",new Vector3(0,3.28f,0),new Vector3(.3f,.22f,3.5f),roof,h,false);
            Box("Door timber frame",new Vector3(0,.83f,-1.24f),new Vector3(.94f,1.56f,.13f),Wood,h,false);
            Box("Arched teal door",new Vector3(0,.78f,-1.33f),new Vector3(.72f,1.33f,.12f),roof,h,false);
            Ball("Brass door knob",new Vector3(.22f,.73f,-1.42f),Vector3.one*.11f,Colors[2],h);
            foreach(float x in new[]{-1f,1f})
            {
                Ball("Round window frame",new Vector3(x,1.35f,-1.26f),new Vector3(.64f,.73f,.16f),Wood,h);
                Ball("Golden window",new Vector3(x,1.35f,-1.35f),new Vector3(.47f,.54f,.09f),Hex("FFD986"),h);
                Box("Window mullion",new Vector3(x,1.35f,-1.42f),new Vector3(.055f,.53f,.05f),Wood,h,false);
                Plant(new Vector3(x,.08f,-1.55f),.6f,h,(int)(x*7)+29);
            }
            Box("Chimney",new Vector3(.85f,3.2f,.7f),new Vector3(.45f,1.2f,.5f),Stone,h,false);
            Post("Chimney cap",new Vector3(.85f,3.85f,.7f),.64f,.15f,Wood,h);
            Box("Hanging sign",new Vector3(0,2.15f,-1.55f),new Vector3(1.7f,.42f,.12f),Wood,h,false);
            Box("Parcel sign emblem",new Vector3(0,2.15f,-1.64f),new Vector3(.25f,.25f,.08f),Cream,h,false);
            Plant(new Vector3(1.48f,.1f,.7f),.75f,h,15);
        }
        static void Conveyor(Vector3 p,Transform root,ShiftController shift)
        {
            var h=Group("Animated parcel conveyor",p,root);
            Box("Conveyor frame",new Vector3(0,.47f,0),new Vector3(1.15f,.26f,2.2f),Wood,h,false);
            Box("Belt",new Vector3(0,.63f,0),new Vector3(.92f,.1f,2.15f),Ink,h,false);
            for(int i=0;i<8;i++)Box("Belt tread",new Vector3(0,.7f,-.94f+i*.27f),new Vector3(.9f,.06f,.1f),Stone,h,false);
            for(int i=0;i<3;i++){var box=Parcel(h,new Vector3(0,.98f,-.7f+i*.7f),.4f);var a=box.AddComponent<CozyMotion>();a.shift=shift;a.kind=CozyMotion.Kind.Conveyor;a.phase=i/3f;}
        }
        public static GameObject Parcel(Transform root,Vector3 pos,float size)
        {
            var box=Box("Parcel",pos,Vector3.one*size,Hex("E7B268"),root,false);
            // Each parcel owns its tint: gameplay can recolor cargo without affecting the village.
            box.GetComponent<Renderer>().sharedMaterial=new Material(box.GetComponent<Renderer>().sharedMaterial);
            Box("Parcel ribbon",new Vector3(0,.51f,0),new Vector3(.17f,.02f,1.01f),Cream,box.transform,false);
            Box("Parcel label",new Vector3(0,0,-.51f),new Vector3(.46f,.48f,.025f),Cream,box.transform,false);return box;
        }
        public static GameObject BuildRobot(Transform root,Color accent,ShiftController shift)
        {
            var visual=Group("Courier shell",Vector3.zero,root);
            Ball("Cream round body",new Vector3(0,.75f,0),new Vector3(1.2f,1.2f,1.08f),Cream,visual);
            Ball("Accent helmet",new Vector3(0,1.13f,0),new Vector3(1.05f,.54f,1),accent,visual);
            Ball("LED visor",new Vector3(0,.93f,.46f),new Vector3(.92f,.62f,.3f),Ink,visual);
            foreach(float x in new[]{-.23f,.23f})
            {
                var eye=Ball("LED eye",new Vector3(x,.99f,.61f),new Vector3(.14f,.2f,.06f),Teal,visual);
                var blink=eye.AddComponent<CozyMotion>();blink.kind=CozyMotion.Kind.Blink;blink.shift=shift;blink.phase=x;
            }
            foreach(float x in new[]{-.59f,.59f})
            {
                var wheel=Post("Rubber wheel",new Vector3(x,.3f,-.07f),.53f,.25f,Ink,visual);wheel.transform.localRotation=Quaternion.Euler(0,0,90);
                var hub=Ball("Wheel hub",new Vector3(x*1.19f,.3f,-.07f),new Vector3(.08f,.28f,.28f),accent,visual);
                Ball("Courier hand",new Vector3(x,.6f,.46f),Vector3.one*.29f,accent,visual);
            }
            Box("Backpack",new Vector3(0,.8f,-.58f),new Vector3(.75f,.62f,.28f),accent,visual,false);
            Post("Antenna",new Vector3(0,1.5f,-.12f),.07f,.42f,Ink,visual);
            Ball("Antenna light",new Vector3(0,1.73f,-.12f),Vector3.one*.2f,accent,visual);
            var reaction=visual.gameObject.AddComponent<CozyMotion>();reaction.shift=shift;reaction.kind=CozyMotion.Kind.Robot;
            var cargo=Parcel(root,new Vector3(0,.55f,.85f),.49f);cargo.SetActive(false);return cargo;
        }
        static void Road(Vector3 a,Vector3 b,float width,Color color,Transform root,float height=.025f)
        {var d=b-a;var o=Box("Village path",(a+b)/2+Vector3.up*height,new Vector3(width,.035f,d.magnitude+width),color,root,false);o.transform.rotation=Quaternion.LookRotation(d);}
        public static RobotMotor Build(Transform root,ShiftController shift,out GameObject cargo)
        {
            Box("Floating island rock",new Vector3(0,-1.5f,0),new Vector3(28,2.5f,22),Hex("A69A84"),root,false);
            Box("Grassy island",new Vector3(0,-.28f,0),new Vector3(28,.55f,22),Grass,root,true);
            for(int i=0;i<22;i++)
            {float x=-13+i*1.23f;Ball("Cliff stone",new Vector3(x,-1.5f,-10.4f),new Vector3(1.5f,2.6f,1.8f),i%2==0?Stone:Hex("A89C87"),root);}
            foreach(float x in new[]{-10f,0f,10f})Road(new Vector3(x,0,-9),new Vector3(x,0,8),2.5f,Stone,root);
            foreach(float z in new[]{-7f,0f,6f})Road(new Vector3(-11,0,z),new Vector3(11,0,z),2.5f,Stone,root);
            // Three striped lanes indicate bay identity without introducing new collision geometry.
            for(int bay=0;bay<3;bay++)
            {var route=GridRouter.Find(Pickup,Stations[bay]);var previous=Pickup;foreach(var p in route){var direction=(p-previous).normalized;var offset=new Vector3(direction.z,0,-direction.x)*(bay-1)*.34f;Road(previous+offset,p+offset,.28f,Colors[bay],root,.065f);previous=p;}}
            for(int x=-4;x<=4;x+=8)for(int z=-3;z<=3;z+=6)House(new Vector3(x,0,z),x<0?Hex("4D9FB1"):Hex("719E8B"),root,z>0?"PARCEL POST":"WORKSHOP");
            for(int i=0;i<3;i++)
            {
                var p=Stations[i];Post("Delivery landing",p+Vector3.up*.09f,2.75f,.16f,Colors[i],root);
                Label("BAY "+(char)('A'+i),p+new Vector3(0,.2f,-1.9f),root,Ink,.095f);
                var booth=Group("Bay kiosk",p+new Vector3(2.35f,0,.3f),root);
                Box("Kiosk cabinet",new Vector3(0,.75f,0),new Vector3(1.1f,1.5f,1.25f),Cream,booth,false);
                Box("Kiosk canopy",new Vector3(0,1.65f,0),new Vector3(1.5f,.25f,1.6f),Colors[i],booth,false);
                Label(((char)('A'+i)).ToString(),new Vector3(0,2.05f,0),booth,Ink,.17f);
                Parcel(booth,new Vector3(-.28f,.35f,-1),.6f);Plant(new Vector3(.3f,.1f,1.05f),.65f,booth,27+i);
            }
            Post("Pickup landing",Pickup+Vector3.up*.09f,2.75f,.16f,Cream,root);
            Label("PICKUP",Pickup+new Vector3(0,.2f,-1.8f),root,Ink,.10f);
            Conveyor(Pickup+new Vector3(-2.2f,0,0),root,shift);
            Post("Charging plinth",Dock+Vector3.up*.08f,2.9f,.14f,Cream,root);
            var pulse=Post("Charging ring",Dock+Vector3.up*.17f,2.5f,.07f,Teal,root).AddComponent<CozyMotion>();pulse.kind=CozyMotion.Kind.Charge;pulse.shift=shift;
            var charger=Group("Botanical charging station",Dock+new Vector3(-2.2f,0,0),root);
            Post("Charger pedestal",Vector3.up*.8f,.72f,1.6f,Cream,charger);Box("Charger screen",new Vector3(0,.92f,-.37f),new Vector3(.5f,.95f,.07f),Teal,charger,false);
            Label("+",new Vector3(0,1.85f,0),charger,Cream,.4f);Label("RECHARGE",Dock+new Vector3(0,.2f,-1.8f),root,Ink,.085f);
            for(int i=0;i<10;i++){Plant(new Vector3(-12.8f+i*2.8f,0,-9.9f),.7f,root,i+80);if(i%2==0)Lantern(new Vector3(-12.8f+i*2.8f,0,-9.2f),root);}
            for(int i=0;i<8;i++){Tree(new Vector3(-12+i*3.5f,0,9.8f),.7f+(i%3)*.14f,root);Plant(new Vector3(-11.7f+i*3.4f,0,8.8f),.75f,root,i+10);}
            Fence(new Vector3(-13,0,-10.5f),new Vector3(-3,0,-10.5f),root);Fence(new Vector3(3,0,-10.5f),new Vector3(13,0,-10.5f),root);
            foreach(float x in new[]{-14f,14f}){var wall=Box("Boundary collision",new Vector3(x,1,0),new Vector3(.3f,2,22),Grass,root);wall.GetComponent<Renderer>().enabled=false;}
            foreach(float z in new[]{-11f,11f}){var wall=Box("Boundary collision",new Vector3(0,1,z),new Vector3(28,2,.3f),Grass,root);wall.GetComponent<Renderer>().enabled=false;}
            // Entry bridge: decorative, beyond the closed simulation boundary.
            Box("Bridge deck",new Vector3(0,-.12f,-12.8f),new Vector3(3,.3f,4),Wood,root,false);
            for(int i=0;i<10;i++)Box("Bridge plank",new Vector3(0,.06f,-11.1f-i*.4f),new Vector3(3.1f,.1f,.32f),Stone,root,false);
            Fence(new Vector3(-1.6f,0,-11),new Vector3(-1.6f,0,-14.5f),root);Fence(new Vector3(1.6f,0,-11),new Vector3(1.6f,0,-14.5f),root);
            var robot=new GameObject("RR-01 Player Robot");robot.transform.SetParent(root,false);robot.transform.position=Dock+Vector3.up*.1f;robot.transform.rotation=Quaternion.Euler(0,145,0);
            var cc=robot.AddComponent<CharacterController>();cc.center=new Vector3(0,.7f,0);cc.height=1.4f;cc.radius=.6f;cc.stepOffset=.15f;
            cargo=BuildRobot(robot.transform,Teal,shift);var motor=robot.AddComponent<RobotMotor>();motor.shift=shift;
            var backdrop=new GameObject("Pastel backdrop");backdrop.transform.SetParent(root,false);var background=backdrop.AddComponent<Camera>();background.depth=-10;background.cullingMask=0;background.clearFlags=CameraClearFlags.SolidColor;background.backgroundColor=Hex("D4E5DF");
            var cameraObj=new GameObject("Village Camera");cameraObj.transform.SetParent(root,false);var cam=cameraObj.AddComponent<Camera>();cameraObj.tag="MainCamera";
            cam.transform.position=new Vector3(23,30,-30);cam.transform.LookAt(new Vector3(0,0,-.5f));cam.orthographic=true;cam.orthographicSize=15f;
            cam.rect=new Rect(.20f,.09f,.80f,.80f);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Hex("D4E5DF");cam.farClipPlane=150;cameraObj.AddComponent<AudioListener>();
            var sun=new GameObject("Golden afternoon");sun.transform.SetParent(root,false);sun.transform.rotation=Quaternion.Euler(48,-35,0);var light=sun.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;light.color=Hex("FFF0D0");light.shadows=LightShadows.Soft;light.shadowStrength=.65f;
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.36f,.43f,.46f);QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowDistance=100;
            return motor;
        }
    }
}




