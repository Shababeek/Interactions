# Throwable — Physics-Based Throwing

> **Quick Reference**
> **Menu Path:** Component > Shababeek > Interactions > Interactables > Throwable
> **Use For:** Adding realistic throwing physics to grabbable objects
> **Requires:** Grabable component, Rigidbody component

---

## What It Does

The **Throwable** component enhances Grabable objects with realistic throw physics. It calculates release velocity based on hand movement, applies it to the Rigidbody, and can optionally add visual effects like trails.

**Without Throwable:** Objects simply drop when released.
**With Throwable:** Objects fly based on your hand's motion when you let go.

**Perfect for:**
- ✅ Balls (sports, toys)
- ✅ Grenades and projectiles
- ✅ Darts and throwing knives
- ✅ Any object that should fly when thrown

---

## Quick Example

> **Goal:** Create a ball you can throw

![Throwable Arc](../Images/throwable-arc.gif)

1. Create a sphere with Rigidbody
2. Add Grabable component
3. Add Throwable component
4. Throw!

---

## Inspector Reference

![Throwable Inspector](../Images/throwable-inspector.png)

### Physics Settings

#### Velocity Multiplier
Scales the calculated release velocity.

| Value | Effect |
|-------|--------|
| **0.5** | Throws feel weak |
| **1.0** | Normal (default) |
| **1.5** | Throws feel powerful |
| **2.0+** | Superhuman throws |

**Default:** 1.0

Adjust based on game feel. Arcade games often use higher values.

---

#### Angular Velocity Multiplier
Scales the rotation applied to thrown objects.

| Value | Effect |
|-------|--------|
| **0** | No spin on throws |
| **0.5** | Reduced spin |
| **1.0** | Natural spin (default) |
| **2.0** | Exaggerated spin |

**Default:** 1.0

---

#### Velocity Samples
Number of frames to average for velocity calculation.

| Value | Effect |
|-------|--------|
| **3** | Very responsive, potentially jittery |
| **5** | Balanced (default) |
| **10** | Smooth but may feel laggy |

**Default:** 5

More samples = smoother but less responsive throws.

---

### Release Settings

#### Release Delay
Time (seconds) after release before physics takes over.

**Default:** 0

Use small values (0.05-0.1) if objects seem to "stick" to hand before flying.

---

#### Gravity Scale On Release
Multiplier for gravity after throwing.

| Value | Effect |
|-------|--------|
| **0.5** | Floaty, moon-like throws |
| **1.0** | Normal gravity (default) |
| **2.0** | Heavy, drops quickly |

**Default:** 1.0

---

### Visual Effects

#### Use Trail
Enables a trail renderer while object is moving fast.

**Default:** Disabled

When enabled, shows a visual trail during flight.

#### Trail Renderer
Reference to a TrailRenderer component for the effect.

Only visible when Use Trail is enabled.

---

## Adding to Your Scene

### Step 1: Start with a Grabbable Object

1. Create or select your object (e.g., Sphere)
2. Ensure it has:
   - A **Collider**
   - A **Rigidbody**
   - A **Grabable** component

### Step 2: Add Throwable

1. Select the object
2. **Add Component > Throwable**
3. Configure settings as needed

### Step 3: Configure Physics (Optional)

For best throwing feel, adjust the Rigidbody:

| Setting | Recommendation |
|---------|----------------|
| **Mass** | 0.5 - 2.0 (lighter = farther throws) |
| **Drag** | 0.1 - 0.5 (higher = slows in air) |
| **Angular Drag** | 0.5 - 2.0 (controls spin decay) |

### Step 4: Test and Tune

1. Enter Play mode
2. Grab and throw the object
3. Adjust Velocity Multiplier if throws feel wrong
4. Adjust Rigidbody Mass/Drag for trajectory

---

## Common Workflows

### How To: Create a Baseball

> **Goal:** A ball that throws realistically
> **Time:** ~3 minutes

#### Setup
1. Create Sphere, scale to (0.07, 0.07, 0.07)
2. Add Rigidbody: Mass=0.15, Drag=0.1
3. Add Grabable
4. Add Throwable

#### Throwable Settings
```
Velocity Multiplier: 1.2
Angular Velocity Multiplier: 0.8
Velocity Samples: 5
```

The ball should feel natural to throw with realistic arc.

---

### How To: Create a Grenade

> **Goal:** Throwable with explosion on impact
> **Time:** ~5 minutes

#### Setup
1. Create grenade model
2. Add Rigidbody, Grabable, Throwable
3. Create explosion prefab

#### Add Impact Detection
```csharp
public class Grenade : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float fuseTime = 3f;
    private bool isArmed = false;

    public void Arm()
    {
        isArmed = true;
        Invoke(nameof(Explode), fuseTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isArmed && collision.relativeVelocity.magnitude > 2f)
        {
            Explode();
        }
    }

    void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
```

Wire Grabable's **On Deselected** to Grenade.Arm()

---

### How To: Add a Throw Trail

> **Goal:** Visual trail during flight
> **Time:** ~2 minutes

1. Add **Trail Renderer** component to object
2. Configure trail appearance (width, material, time)
3. Set **Emitting** to false (starts disabled)
4. On Throwable, check **Use Trail**
5. Assign the Trail Renderer to the field

The trail will appear when thrown and moving fast.

![Throwable Trail](../Images/throwable-trail.png)

---

### How To: Make a Dart

> **Goal:** Object that sticks where it lands
> **Time:** ~5 minutes

#### Setup
1. Create dart model (cylinder + cone)
2. Add Rigidbody, Grabable, Throwable

#### Settings
```
Velocity Multiplier: 1.5 (darts throw fast)
Angular Velocity Multiplier: 0 (no spin, flies straight)
```

#### Add Stick Behavior
```csharp
public class Dart : MonoBehaviour
{
    [SerializeField] private float stickThreshold = 5f;
    private Rigidbody rb;
    private bool hasStuck = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasStuck) return;

        if (collision.relativeVelocity.magnitude > stickThreshold)
        {
            hasStuck = true;
            rb.isKinematic = true;
            // Optionally parent to hit object
            transform.parent = collision.transform;
        }
    }
}
```

---

## Tuning Guide

### Throws Feel Weak
- Increase **Velocity Multiplier** (try 1.5-2.0)
- Decrease Rigidbody **Mass**
- Decrease Rigidbody **Drag**

### Throws Feel Too Strong
- Decrease **Velocity Multiplier** (try 0.7-0.9)
- Increase Rigidbody **Mass**
- Increase Rigidbody **Drag**

### Objects Spin Too Much
- Decrease **Angular Velocity Multiplier**
- Increase Rigidbody **Angular Drag**

### Throws Feel Laggy/Delayed
- Decrease **Velocity Samples** (try 3)
- Ensure **Release Delay** is 0

### Throws Feel Jittery/Unpredictable
- Increase **Velocity Samples** (try 8-10)
- May indicate tracking issues

### Objects Drop Straight Down
- Check Throwable component is enabled
- Verify Rigidbody is not Kinematic
- Ensure hand is actually moving when releasing

---

## Tips & Best Practices

💡 **Test with actual throws**
Desktop simulation doesn't replicate real VR throwing. Always test in headset.

💡 **Consider game balance**
Realistic throws might be too powerful (or weak) for your game. Tune for fun, not realism.

💡 **Add sound effects**
Wire Grabable's On Deselected to play a "whoosh" sound for satisfying throws.

💡 **Use physics materials**
Unity's PhysicsMaterial on colliders affects bounce. A rubber ball should have high bounciness.

💡 **Layer-based collision**
Use physics layers to control what thrown objects can hit.

⚠️ **Common Mistake:** Missing Rigidbody
Throwable requires a Rigidbody. Without it, velocity can't be applied.

⚠️ **Common Mistake:** Kinematic Rigidbody
If Rigidbody is Kinematic, physics won't affect it. Grabable should handle this automatically.

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Objects don't throw, just drop | Throwable disabled or missing | Ensure Throwable is added and enabled |
| Objects stick to hand | Release not being called | Check Grabable events, verify input setup |
| Throws go wrong direction | Hand tracking issues | Check controller calibration |
| Objects pass through walls | Moving too fast | Enable Continuous collision detection on Rigidbody |
| No trail appears | Trail not configured | Check Use Trail is enabled and Trail Renderer assigned |

---

## Scripting API

### Properties

```csharp
// Velocity multiplier
throwable.VelocityMultiplier = 1.5f;

// Angular velocity multiplier
throwable.AngularVelocityMultiplier = 0.5f;
```

### Manual Throw

```csharp
// Apply throw velocity manually
public void ManualThrow(Vector3 velocity)
{
    Rigidbody rb = GetComponent<Rigidbody>();
    rb.isKinematic = false;
    rb.velocity = velocity;
}
```

### Tracking Throw Stats

```csharp
public class ThrowTracker : MonoBehaviour
{
    [SerializeField] private Grabable grabable;
    private Vector3 lastPosition;
    private float grabTime;

    void Start()
    {
        grabable.OnSelected
            .Subscribe(_ => OnGrab())
            .AddTo(this);

        grabable.OnDeselected
            .Subscribe(_ => OnRelease())
            .AddTo(this);
    }

    void OnGrab()
    {
        lastPosition = transform.position;
        grabTime = Time.time;
    }

    void OnRelease()
    {
        float holdTime = Time.time - grabTime;
        float throwSpeed = GetComponent<Rigidbody>().velocity.magnitude;
        Debug.Log($"Held for {holdTime}s, thrown at {throwSpeed} m/s");
    }
}
```

---

## Related Documentation

- [Grabable](Grabable.md) — Required for Throwable to work
- [PoseConstrainer](../PoseSystem/PoseConstrainer.md) — Hand positioning
- [Feedback System](../Systems/FeedbackSystem.md) — Add throw sound effects
- [Socket System](../SocketSystem/SocketSystem.md) — Catch thrown objects

---

**Last Updated:** January 2026
**Component Version:** 1.0.0
