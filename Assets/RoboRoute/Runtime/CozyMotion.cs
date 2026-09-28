using UnityEngine;
namespace RoboRoute
{
    // Lightweight procedural animations share the simulation pause state.
    public sealed class CozyMotion : MonoBehaviour
    {
        public enum Kind { Robot, Blink, Conveyor, Charge, Confetti }
        public Kind kind;
        public ShiftController shift;
        public float phase;
        Vector3 origin, scale, velocity;
        float time, reaction;
        void Start(){origin=transform.localPosition;scale=transform.localScale;if(kind==Kind.Confetti)velocity=new Vector3(Mathf.Sin(phase)*2,3+phase%2,Mathf.Cos(phase)*2);}
        public void Celebrate(){reaction=1;}
        void Update()
        {
            if(shift!=null && shift.Paused)return;
            float dt=Time.deltaTime;time+=dt;
            switch(kind)
            {
                case Kind.Robot:
                    reaction=Mathf.Max(0,reaction-dt);
                    transform.localPosition=origin+Vector3.up*(Mathf.Sin(time*3)*.035f+Mathf.Sin(reaction*Mathf.PI)*.3f);
                    transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(time*5)*reaction*12);break;
                case Kind.Blink:
                    float blink=Mathf.Repeat(time+phase,4)>3.82f?.12f:1;
                    transform.localScale=new Vector3(scale.x,scale.y*blink,scale.z);break;
                case Kind.Conveyor:
                    transform.localPosition=origin+Vector3.forward*(Mathf.Repeat(time*.5f+phase*2,2)-1);break;
                case Kind.Charge:
                    float pulse=1+Mathf.Sin(time*2)*.035f;transform.localScale=new Vector3(scale.x*pulse,scale.y,scale.z*pulse);break;
                case Kind.Confetti:
                    velocity+=Vector3.down*4*dt;transform.position+=velocity*dt;transform.Rotate(new Vector3(90,60,120)*dt);
                    transform.localScale=scale*Mathf.Clamp01(2-time);if(time>2)Destroy(gameObject);break;
            }
        }
        public static void Burst(Vector3 position,Transform root,ShiftController shift,Color color)
        {
            for(int i=0;i<18;i++)
            {
                var bit=WarehouseBuilder.Box("Delivery celebration",position+Vector3.up*.8f,new Vector3(.11f,.06f,.17f),i%2==0?color:WarehouseBuilder.Hex("FFF0CE"),root,false);
                var animation=bit.AddComponent<CozyMotion>();animation.shift=shift;animation.kind=Kind.Confetti;animation.phase=i*2.4f;
            }
        }
    }
}
