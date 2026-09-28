using System;
namespace RoboRoute
{
    // Serializable shift rules. New optional fields keep existing version-2 saves readable.
    [Serializable]
    public sealed class ShiftState
    {
        public int version=2, delivered, failed, score;
        public bool carrying;
        public float battery=100, elapsed, missionTime, totalDeliveryTime;
        public int currentStreak, bestStreak, wrongBayAttempts, recoveries, rechargeCount, visitedBaysMask;
        public float fastestDelivery;
        public bool lastSecondDelivery;
        public const int Goal=6;
        public const float Deadline=75;
        public bool Complete => delivered+failed>=Goal;
        public int Destination => (delivered+failed)%3;
        public float SuccessRate => delivered+failed==0?0:100f*delivered/(delivered+failed);
        public int Stars => delivered>=6?3:delivered>=4?2:delivered>=1?1:0;
        public string Rank => Stars==3?"Village superstar":Stars==2?"Brilliant courier":Stars==1?"Promising parcel pal":"Keep growing, little courier";
        public bool TryPickup(){if(Complete||carrying)return false;carrying=true;return true;}
        public void Tick(float dt)
        {
            if(Complete||dt<0||!Finite(dt))return;
            elapsed+=dt;missionTime+=dt;
            if(missionTime>=Deadline){failed++;carrying=false;missionTime=0;currentStreak=0;}
        }
        public bool Deliver(int station)
        {
            if(Complete||!carrying)return false;
            if(station!=Destination){wrongBayAttempts++;return false;}
            carrying=false;delivered++;score+=100;currentStreak++;bestStreak=Math.Max(bestStreak,currentStreak);
            visitedBaysMask|=1<<station;
            if(missionTime>0&&(fastestDelivery==0||missionTime<fastestDelivery))fastestDelivery=missionTime;
            if(missionTime>=Deadline-10)lastSecondDelivery=true;
            totalDeliveryTime=Math.Min(elapsed,totalDeliveryTime+missionTime);missionTime=0;return true;
        }
        public bool Recover(){if(Complete)return false;recoveries++;Tick(30);return true;}
        public void RecordCharge(){if(!Complete)rechargeCount++;}
        public bool IsValid()
        {
            return version==2&&delivered>=0&&failed>=0&&delivered+failed<=Goal&&score==delivered*100&&
                Finite(battery)&&battery>=0&&battery<=100&&Finite(elapsed)&&elapsed>=0&&
                Finite(missionTime)&&missionTime>=0&&missionTime<Deadline&&Finite(totalDeliveryTime)&&totalDeliveryTime>=0&&totalDeliveryTime<=elapsed&&!(Complete&&carrying)&&
                currentStreak>=0&&currentStreak<=bestStreak&&bestStreak<=delivered&&wrongBayAttempts>=0&&recoveries>=0&&rechargeCount>=0&&visitedBaysMask>=0&&visitedBaysMask<=7&&
                Finite(fastestDelivery)&&fastestDelivery>=0&&fastestDelivery<Deadline&&(delivered>0||fastestDelivery==0)&&(!lastSecondDelivery||delivered>0);
        }
        static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
    }
}
