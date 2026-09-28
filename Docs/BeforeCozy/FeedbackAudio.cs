using UnityEngine;
namespace RoboRoute {
// Generated feedback has no external asset or licensing dependency.
[RequireComponent(typeof(AudioSource))]
public sealed class FeedbackAudio:MonoBehaviour {
 AudioSource source; AudioClip pickup, delivery, failure;
 void Awake(){source=GetComponent<AudioSource>();source.playOnAwake=false;pickup=Tone(660);delivery=Tone(880);failure=Tone(180);}
 AudioClip Tone(float hz){int count=6615;var samples=new float[count];for(int i=0;i<count;i++)samples[i]=Mathf.Sin(2*Mathf.PI*hz*i/44100)*.15f*(1f-i/(float)count);var clip=AudioClip.Create("Robot notification",count,1,44100,false);clip.SetData(samples,0);return clip;}
 public void Play(int kind){source.PlayOneShot(kind==0?pickup:kind==1?delivery:failure);}
 void OnDestroy(){Destroy(pickup);Destroy(delivery);Destroy(failure);}
}}
