using UnityEngine;
namespace RoboRoute
{
    // Screen-space presentation is separate from mission state and rules.
    public static class CozyHud
    {
        public static readonly Color Ink=WarehouseBuilder.Hex("294B50"), Paper=WarehouseBuilder.Hex("FFF9EC"), Muted=WarehouseBuilder.Hex("728983"), Teal=WarehouseBuilder.Teal;
        static Texture2D circle;
        static Font font;
        static GUIStyle style;
        static void Initialize()
        {
            if(circle!=null)return;
            circle=new Texture2D(32,32,TextureFormat.RGBA32,false);circle.filterMode=FilterMode.Bilinear;
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)circle.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(16-Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(16,16)))));
            circle.Apply();font=Font.CreateDynamicFontFromOSFont("Segoe UI",20);style=new GUIStyle{font=font,wordWrap=true};
        }
        public static void Begin()
        {
            Initialize();float scale=Mathf.Min(Screen.width/1600f,Screen.height/1000f);
            GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1600*scale)/2,(Screen.height-1000*scale)/2),Quaternion.identity,Vector3.one*scale);
        }
        public static void Panel(Rect r,Color color,float radius=14)
        {
            Initialize();GUI.color=color;
            GUI.DrawTexture(new Rect(r.x+radius,r.y,r.width-radius*2,r.height),Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(r.x,r.y+radius,r.width,r.height-radius*2),Texture2D.whiteTexture);
            GUI.DrawTextureWithTexCoords(new Rect(r.x,r.y,radius,radius),circle,new Rect(0,.5f,.5f,.5f));
            GUI.DrawTextureWithTexCoords(new Rect(r.xMax-radius,r.y,radius,radius),circle,new Rect(.5f,.5f,.5f,.5f));
            GUI.DrawTextureWithTexCoords(new Rect(r.x,r.yMax-radius,radius,radius),circle,new Rect(0,0,.5f,.5f));
            GUI.DrawTextureWithTexCoords(new Rect(r.xMax-radius,r.yMax-radius,radius,radius),circle,new Rect(.5f,0,.5f,.5f));GUI.color=Color.white;
        }
        public static void Card(Rect r,Color color)
        {Panel(new Rect(r.x,r.y+5,r.width,r.height),new Color(.19f,.32f,.3f,.12f));Panel(r,color);}
        public static void Text(Rect r,string text,int size,Color color,bool bold=false,TextAnchor align=TextAnchor.UpperLeft)
        {Initialize();style.fontSize=size;style.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;style.normal.textColor=color;style.alignment=align;GUI.Label(r,text,style);}
        public static bool Button(Rect r,string label,bool primary=false)
        {
            bool hover=r.Contains(Event.current.mousePosition)&&GUI.enabled;
            Panel(r,primary?(hover?WarehouseBuilder.Hex("269B8D"):Teal):(hover?WarehouseBuilder.Hex("E3EBDE"):WarehouseBuilder.Hex("EEF0E4")),10);
            Text(r,label,18,GUI.enabled?(primary?Color.white:Ink):Muted,true,TextAnchor.MiddleCenter);
            return GUI.Button(r,GUIContent.none,GUIStyle.none);
        }
        public static void Metric(float x,string label,string value,Color color)
        {
            Card(new Rect(x,28,204,78),Paper);Panel(new Rect(x+14,46,6,40),color,3);
            Text(new Rect(x+32,40,165,22),label,12,Muted,true);Text(new Rect(x+32,60,165,36),value,27,Ink,true);
        }
    }
}
