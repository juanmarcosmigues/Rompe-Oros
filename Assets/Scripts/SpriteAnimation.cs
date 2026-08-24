using UnityEngine;

[CreateAssetMenu(fileName = "New Animation", menuName = "Animation/Sprite Animation")]
public class SpriteAnimation : ScriptableObject
{
    [System.Serializable]
    public class Frame
    {
        public Sprite sprite;
        public Vector3 localPosition;
        public int hold;
        public string message;
    }
    public enum PlayMode { Single, Loop, Clamp }

    [SerializeField] Frame[] frames;
    [SerializeField] PlayMode playMode;

    int totalFrames;
    (int time, Frame frame)[] timeline;

    public void SetUpClip ()
    {
        timeline = new (int, Frame)[frames.Length];
        totalFrames = 0;
        for (int i = 0; i < frames.Length; i++)
        {         
            timeline[i].time = totalFrames;
            timeline[i].frame = frames[i];
            totalFrames += frames[i].hold + 1;
        }
    }

    public Frame GetFrame (int index) => frames[index];
    public Frame GetFrameAtTime (int frame)
    {
        for (int i = timeline.Length - 1; i >= 0 ; i--)
        {
            if (timeline[i].time <= frame)
                return timeline[i].frame;
        }

        Debug.Log("Couldnt find frame for" +  frame);   
        return default;
    }
    public int TotalFrames => totalFrames;
    public PlayMode Mode => playMode;
}
