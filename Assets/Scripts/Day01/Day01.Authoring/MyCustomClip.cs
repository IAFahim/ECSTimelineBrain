using UnityEngine;
using UnityEngine.Playables;

// 1. THE CLIP (exposed variable appears in Inspector)
public class MyCustomClip : PlayableAsset
{
    public ExposedReference<GameObject> myTargetObject; // Creates a slot in the Inspector

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<MyCustomBehavior>.Create(graph);
        
        // Resolve the reference and pass it to the behavior
        var behavior = playable.GetBehaviour();
        behavior.resolvedObject = myTargetObject.Resolve(graph.GetResolver());
        
        return playable;
    }
}

// 2. THE LOGIC
public class MyCustomBehavior : PlayableBehaviour
{
    public GameObject resolvedObject;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (resolvedObject != null)
        {
            Debug.Log("Connected to: " + resolvedObject.name);
        }
    }
}