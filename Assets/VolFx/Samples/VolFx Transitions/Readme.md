◆ Concept

Move acts as a central controller that manages when a transition starts, pauses, and completes.

It relies on two complementary component types:

Move_In — locks the transition (prevents playback from continuing) while active.

Move_Out — releases the transition (allows playback to resume) when active.

These components can exist anywhere in the scene hierarchy and operate independently.

◇ Structure
▶ Move

The main controller responsible for coordinating transition playback.

Fields

▪ Wait In Locks — pause playback until all active Move_In components release their locks.

▪ Wait Out Release — pause playback until at least one active Move_Out is present.

▪ Wait Signal — optional Timeline signal that defines when the transition pauses to wait for conditions.

Events

▸ _onStart — invoked when transition playback begins.

▸ _onWait — invoked when the transition is paused (waiting).

▸ _onComplete — invoked when the transition finishes playback.

▶ Move_In

Marks an entry point for the transition — holds playback until released.

Fields

▪ _activeLock — if true, keeps the transition locked while this component is active.

▸ _onStart — invoked when the transition starts.

▸ _onWait — invoked when the transition is paused and this lock is still active.

Use this when a transition should not progress until certain parts of the scene (like effects or UI) are ready.

▶ Move_Out

Marks an exit point — allows the transition to continue when active.

Fields

▪ _activeRelease — if true, counts as an active release that lets the transition resume.

▸ _onWait — invoked when the transition is paused waiting for releases.

▸ _onComplete — invoked after the transition has fully completed.

Use this for fade-out, scene switch, or any outro effect synchronization.

► How It Works

A Move component plays a Timeline via Move.Play().

When the defined Wait Signal is reached, playback pauses.

The system waits until:

all active Move_In locks are cleared, and

(optionally) at least one Move_Out is active.

Once the conditions are met, the timeline continues to the end.

When playback finishes, _onComplete is invoked.

▣ Example Flow
Scene A (Move_In active)
     |
     |  blocks transition
     v
[ Timeline Pause ]  <--- waiting for release
     |
     |  Scene B loads with Move_Out active
     v
Transition resumes
     |
     v
[ Timeline Complete ]

▪ Tips

→ Attach Move to an object with a PlayableDirector.

→ Use a SignalEmitter in Timeline to mark the pause point.

→ Move_In and Move_Out can exist across multiple scenes.

→ For testing, TempSceneInjector can temporarily add all scenes to Build Settings during Play Mode.

▣ Example Code
using VolFx;

public class TransitionTrigger : MonoBehaviour
{
    public Move transition;

    public void StartTransition()
    {
        transition.Play();
    }
}

✓ Compatibility

Unity 2021.3 or newer

Works in both Editor and Runtime

Integrates with Timeline, SignalEmitter, and UnityEvents