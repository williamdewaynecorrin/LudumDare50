using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Timer
{
    public int frametime = 30;
    [HideInInspector]
    public int framereset = 30;

    public void Init()
    {
        framereset = frametime;
    }

    public int Elapsed()
    {
        return framereset - frametime;
    }

    public bool TimerReached()
    {
        return frametime <= 0;
    }

    public bool TimerMax()
    {
        return frametime == framereset;
    }

    public void Reset()
    {
        frametime = framereset;
    }

    public void Finish()
    {
        frametime = 0;
    }

    public void Increment()
    {
        ++frametime;
    }

    public void Decrement()
    {
        --frametime;
    }
}


[System.Serializable]
public class TimerRT
{
    const int kPrevCheckFailsForError = 10;

    public float time = 30;
    [HideInInspector]
    public float timereset = 30;

    private int secondspassed = 0;
    private bool secondchange = false;
    private float previous = float.MinValue;
    private int prevcheckfails = 0;

    public void Init()
    {
        timereset = time;
        previous = time;
    }

    public float Elapsed()
    {
        return timereset - time;
    }

    public float CurrentTime()
    {
        return time;
    }

    public bool TimerReached()
    {
        return time <= 0;
    }

    public bool TimerMax()
    {
        return Mathf.Approximately(time, timereset);
    }

    public void Reset()
    {
        time = timereset;
        previous = time;
        secondspassed = 0;
        secondchange = false;
        previous = float.MinValue;
        prevcheckfails = 0;
    }

    public void Finish()
    {
        time = 0;
    }

    public void Tick(float delta)
    {
        secondchange = false;
        float prevtime = time;
        time -= delta;

        // -- check for new second
        if((int)prevtime != (int)time)
        {
            ++secondspassed;
            secondchange = true;
        }
    }

    public bool SecondChange()
    {
        return secondchange;
    }

    public int SecondsPassed()
    {
        return secondspassed;
    }

    public float Milliseconds()
    {
        return time * 1000f;
    }

    public float CurrentSecondFraction()
    {
        int rounded = (int)time;

        return ((float)(time - rounded));
    }

    public float GetLerpValue()
    {
        float normalizedtime = time / timereset;
        float invnormtime = Mathf.Clamp01(1f - normalizedtime);

        return invnormtime;
    }

    public float GetNormazliedTime()
    {
        float normalizedtime = time / timereset;

        return normalizedtime;
    }

    public void RecordPrevious()
    {
        previous = time;
    }

    public bool JustPassed(float passedtime)
    {
        // -- check for users that might not be recording previous values
        if(previous == float.MinValue)
        {
            ++prevcheckfails;
            if (prevcheckfails >= kPrevCheckFailsForError)
                Debug.LogError("Detecting Timer functionality desiring usage of previous states, but RecordPrevious() is never being called. JustPassed() will not work.");

            return false;
        }

        return previous > passedtime && time <= passedtime;
    }
}
