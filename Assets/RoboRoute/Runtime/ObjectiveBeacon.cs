using UnityEngine;
namespace RoboRoute
{
    public sealed class ObjectiveBeacon : MonoBehaviour
    {
        ShiftController shift; Renderer[] dots;
        public void Initialize(ShiftController owner)
        {
            shift=owner;
            for(int i=0;i<24;i++)
            {
                float angle=i*Mathf.PI*2/24;
                WarehouseBuilder.Shape("Follow these dots",PrimitiveType.Sphere,new Vector3(Mathf.Cos(angle)*1.7f,.18f,Mathf.Sin(angle)*1.7f),Vector3.one*.12f,WarehouseBuilder.Hex("FFF9EC"),transform);
            }
            dots=GetComponentsInChildren<Renderer>();
        }
        void Update()
        {
            if(shift==null)return;
            foreach(var r in dots)r.enabled=!shift.State.Complete;
            if(shift.State.Complete||shift.Paused)return;
            transform.position=shift.State.battery<25?WarehouseBuilder.Dock:shift.State.carrying?WarehouseBuilder.Stations[shift.State.Destination]:WarehouseBuilder.Pickup;
            transform.Rotate(Vector3.up*18*Time.deltaTime);
        }
    }
}
