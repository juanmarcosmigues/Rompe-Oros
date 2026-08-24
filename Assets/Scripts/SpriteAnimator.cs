using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    const int LAYERS_AMOUNT = 3;

    [SerializeField] SpriteRenderer target;
    [SerializeField] int framerate = 12;
    [SerializeField] SpriteAnimation[] animations;

    Dictionary<string, SpriteAnimation> dictionaryAnimations;

    [System.Serializable]
    class PlayState
    {
        public void Start (SpriteAnimation animation) 
        {
            currentPlaying = animation;
            currentTime = 0;
        }

        SpriteAnimation currentPlaying;
        int currentTime;
        public bool IsPlaying => currentPlaying != null;
        public SpriteAnimation.Frame Update ()
        {
            if (!currentPlaying)
                return null;

            var currentFrame = currentPlaying.GetFrameAtTime(currentTime);

            if (currentTime < currentPlaying.TotalFrames)
                currentTime++;
            else
            {
                if (currentPlaying.Mode == SpriteAnimation.PlayMode.Single)
                {
                    currentPlaying = null;
                }
                else if (currentPlaying.Mode == SpriteAnimation.PlayMode.Clamp) 
                {
                    currentTime = currentPlaying.TotalFrames;
                }
                else if (currentPlaying.Mode == SpriteAnimation.PlayMode.Loop)
                {
                    currentTime = 0;
                }                  
            }

            return currentFrame;
        }
    }

    PlayState[] layers = new PlayState[LAYERS_AMOUNT];

    private void Awake()
    {
        dictionaryAnimations = new Dictionary<string, SpriteAnimation>();
        foreach (var animation in animations)
        {
            animation.SetUpClip();
            dictionaryAnimations.Add(animation.name, animation);
        }
    }
    public void Play(string name, int layer = 0)
    {
        if (!dictionaryAnimations.ContainsKey(name))
            return;
        if (layer >= LAYERS_AMOUNT)
            return;

        if (layers[layer] == null)
            layers[layer] = new PlayState();

        layers[layer].Start(dictionaryAnimations[name]);
    }

    float tickTime = 0f;
    private void Update()
    {
        tickTime += Time.deltaTime * framerate;
        if (tickTime > 1f)
        {
            tickTime -= 1f;
            UpdateAnimator();
        }
    }


    private void UpdateAnimator ()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i] == null || !layers[i].IsPlaying) continue;

            var frame = layers[i].Update();

            target.sprite = frame.sprite;
            target.transform.localPosition = frame.localPosition;
        }    
    }
}
