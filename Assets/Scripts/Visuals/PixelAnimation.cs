using System;
using System.Collections.Generic;

namespace RhythmArmy.Visuals
{
    public struct PixelRect
    {
        public int X; public int Y; public int Width; public int Height;
        public PixelRect(int x,int y,int width,int height){X=x;Y=y;Width=width;Height=height;}
    }
    public class AnimationFrame
    {
        public PixelRect Rect; public float DurationSeconds; public string EventTag;
        public AnimationFrame(PixelRect rect,float durationSeconds,string eventTag=null)
        {Rect=rect;DurationSeconds=durationSeconds;EventTag=eventTag;}
    }
    public class AnimationStateDef
    {
        public string Name; public List<AnimationFrame> Frames; public bool Loop;
        public AnimationStateDef(string name,bool loop=true){Name=name;Frames=new List<AnimationFrame>();Loop=loop;}
        public void AddFrame(int x,int y,int width,int height,float duration,string eventTag=null)
        {Frames.Add(new AnimationFrame(new PixelRect(x,y,width,height),duration,eventTag));}
    }

    /// <summary>
    /// Animation contract for the nine-row production unit sheet:
    /// Idle, March, Attack, Defend, Fever, Charge, Jump, Hurt, Death.
    /// </summary>
    public class PixelAnimator
    {
        public string UnitId;
        public Dictionary<string,AnimationStateDef> States=new Dictionary<string,AnimationStateDef>();
        public string CurrentStateName{get;private set;}
        public int CurrentFrameIndex{get;private set;}
        public float ElapsedTime{get;private set;}
        public bool IsFinished{get;private set;}
        public event Action<string> OnAnimationEvent;

        public AnimationFrame CurrentFrame
        {
            get
            {
                AnimationStateDef state;
                if(string.IsNullOrEmpty(CurrentStateName)||!States.TryGetValue(CurrentStateName,out state)||state.Frames.Count==0)return null;
                return state.Frames[Math.Min(CurrentFrameIndex,state.Frames.Count-1)];
            }
        }
        public void Play(string stateName,bool forceRestart=false)
        {
            if(CurrentStateName==stateName&&!forceRestart&&!IsFinished)return;
            if(!States.ContainsKey(stateName))return;
            CurrentStateName=stateName;CurrentFrameIndex=0;ElapsedTime=0f;IsFinished=false;FireEvent(CurrentFrame);
        }
        public void Update(float deltaTime)
        {
            AnimationStateDef state;
            if(string.IsNullOrEmpty(CurrentStateName)||IsFinished||!States.TryGetValue(CurrentStateName,out state)||state.Frames.Count==0)return;
            ElapsedTime+=deltaTime;
            if(ElapsedTime<state.Frames[CurrentFrameIndex].DurationSeconds)return;
            ElapsedTime-=state.Frames[CurrentFrameIndex].DurationSeconds;
            CurrentFrameIndex++;
            if(CurrentFrameIndex>=state.Frames.Count)
            {
                if(state.Loop)CurrentFrameIndex=0;
                else{CurrentFrameIndex=state.Frames.Count-1;IsFinished=true;}
            }
            FireEvent(state.Frames[CurrentFrameIndex]);
        }
        private void FireEvent(AnimationFrame frame)
        {
            if(frame!=null&&!string.IsNullOrEmpty(frame.EventTag)&&OnAnimationEvent!=null)OnAnimationEvent(frame.EventTag);
        }

        public static PixelAnimator CreateStandardUnitAnimator(string unitId,int spriteSize=32)
        {
            var anim=new PixelAnimator{UnitId=unitId};
            Add(anim,"Idle",0,4,0.15f,true,spriteSize);
            Add(anim,"March",1,4,0.125f,true,spriteSize,new[]{"Step",null,"Step",null});
            Add(anim,"Attack",2,4,0.10f,false,spriteSize,new[]{"Windup","Release","Impact",null});
            Add(anim,"Defend",3,2,0.25f,true,spriteSize,new[]{"ShieldWall",null});
            Add(anim,"Fever",4,4,0.10f,true,spriteSize,new[]{"Chant",null,"Chant",null});
            Add(anim,"Charge",5,3,0.10f,false,spriteSize,new[]{"ChargeStart",null,"ChargeImpact"});
            Add(anim,"Jump",6,3,0.12f,false,spriteSize,new[]{"JumpStart",null,"Land"});
            Add(anim,"Hurt",7,2,0.10f,false,spriteSize);
            Add(anim,"Death",8,3,0.12f,false,spriteSize,new[]{"DeathStart",null,"SpiritRise"});
            Add(anim,"HeroAbility",2,4,0.08f,false,spriteSize,new[]{"HeroStart","HeroRelease","HeroImpact",null});
            anim.Play("Idle");
            return anim;
        }
        private static void Add(PixelAnimator anim,string name,int row,int count,float duration,bool loop,int spriteSize,string[] tags=null)
        {
            var state=new AnimationStateDef(name,loop);
            int size=spriteSize;
            for(int i=0;i<count;i++)state.AddFrame(i*size,row*size,size,size,duration,tags!=null&&i<tags.Length?tags[i]:null);
            anim.States[name]=state;
        }
    }
}
