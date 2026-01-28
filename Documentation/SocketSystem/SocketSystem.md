# Socket System — Object Placement and Snapping

> **Quick Reference**
> **Socket Component:** Component > Shababeek > Interactions > Socket
> **Socketable Component:** Component > Shababeek > Interactions > Socketable
> **Use For:** Objects that snap into specific positions/holders

---

## What It Does

The **Socket System** enables objects to snap into designated holders or positions. A **Socket** is a receptacle that accepts objects, while a **Socketable** is an object that can be placed into sockets.

**Perfect for:**
- ✅ Tool holders and racks
- ✅ Puzzle piece placement
- ✅ Inventory slots
- ✅ Key-in-lock mechanics
- ✅ Battery/magazine insertion
- ✅ Crafting stations

---

## Core Concepts

| Component | Role | Analogy |
|-----------|------|---------|
| **Socket** | The receiver/holder | A cup holder |
| **Socketable** | The object that snaps in | A cup |

**Key Interactions:**
- Socketable enters Socket trigger → Snapping attempt
- Socket accepts → Object snaps to position
- Grab socketed object → Object is removed

---

## Quick Example

> **Goal:** Create a flashlight holder

![Socket Snap](../Images/socket-snap.gif)

1. Create mount → Add Socket component + Trigger collider
2. Create flashlight → Add Socketable + Grabable
3. Move flashlight near mount → It snaps in
4. Grab flashlight → It pops out

---

## Socket Component

The receiver that accepts Socketable objects.

### Inspector Reference

![Socket Inspector](../Images/socket-inspector.png)

### Settings

#### Pivot
Transform where socketed objects will position themselves.

**If not assigned:** Uses the Socket's transform.

**Tip:** Create a child empty GameObject positioned exactly where objects should snap, assign it as Pivot.

#### Current (Read-Only)
Shows the currently socketed object (if any).

### Required Setup

1. Add **Socket** component
2. Add **Collider** (Box, Sphere, etc.)
3. Check **Is Trigger** on collider
4. Optionally assign a **Pivot** transform

### How It Works

```
Socketable enters trigger
    ↓
Socket.CanSocket() checked
    ↓
If true → Socket.Insert() called
    ↓
Socketable positions to Pivot
    ↓
Object is now socketed
```

### Methods

| Method | Description |
|--------|-------------|
| `CanSocket()` | Returns true if socket can accept an object (empty) |
| `Insert(Socketable)` | Places socketable into this socket |
| `Remove(Socketable)` | Removes socketable from socket |
| `Pivot` | Property returning the snap position |

---

## Socketable Component

Objects that can be placed into sockets.

### Inspector Reference

![Socketable Inspector](../Images/socketable-inspector.png)

### Required Setup

1. Add **Socketable** component to your object
2. Ensure object also has **Grabable** (so you can pick it up)
3. Object should have a **Collider**

### Properties

| Property | Description |
|----------|-------------|
| `IsSocketed` | True if currently in a socket |
| `CurrentSocket` | Reference to containing socket (or null) |

### Events

Socketable fires events via the Grabable component's event system when socketed/unsocketed.

---

## Adding to Your Scene

### Step 1: Create a Socket (Holder)

1. Create an empty GameObject, name it "ToolHolder"
2. Add visual model as child (the mount/bracket)
3. Create empty child for snap point, name it "SnapPoint"
4. Position SnapPoint where objects should land

```
ToolHolder (empty)
├── MountModel (visual)
└── SnapPoint (empty - positioned for object)
```

5. Select ToolHolder
6. **Add Component > Socket**
7. **Add Component > Box Collider** (or appropriate shape)
8. Check **Is Trigger**
9. Size collider to detection area
10. Assign SnapPoint to **Pivot** field

### Step 2: Create a Socketable (Object)

1. Create your object (e.g., tool, key, battery)
2. Ensure it has a Collider
3. **Add Component > Grabable** (if not already)
4. **Add Component > Socketable**

### Step 3: Configure Matching (Optional)

For type-specific sockets (key only fits specific lock), see "Socket Filtering" section below.

### Step 4: Test

1. Enter Play mode
2. Grab the socketable object
3. Move it near the socket's trigger area
4. Release — object should snap to position
5. Grab again — object pops out

---

## Common Workflows

### How To: Tool Rack

> **Goal:** Multiple tools snap to wall-mounted holders
> **Time:** ~10 minutes

#### Setup Structure
```
ToolRack (parent)
├── WrenchHolder (Socket)
│   └── SnapPoint
├── HammerHolder (Socket)
│   └── SnapPoint
└── ScrewdriverHolder (Socket)
    └── SnapPoint
```

Each holder gets its own Socket component and trigger collider.

Tools each get Grabable + Socketable.

---

### How To: Key and Lock

> **Goal:** Only specific key opens specific door
> **Time:** ~5 minutes

#### Using Tags for Filtering

1. Create custom Socket subclass:

```csharp
public class TaggedSocket : Socket
{
    [SerializeField] private string acceptedTag = "GoldKey";

    public override bool CanSocket()
    {
        // Will be overridden when checking specific socketable
        return base.CanSocket();
    }

    public override Transform Insert(Socketable socketable)
    {
        // Check tag before accepting
        if (!socketable.CompareTag(acceptedTag))
        {
            return null; // Reject
        }
        return base.Insert(socketable);
    }
}
```

2. Tag your key objects appropriately
3. Set acceptedTag on each lock

---

### How To: Inventory Slots

> **Goal:** Grid of slots for storing items
> **Time:** ~10 minutes

1. Create UI-style layout of socket objects
2. Each socket is a small trigger area
3. Items with Socketable snap into any empty slot

#### Inventory Manager
```csharp
public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Socket[] slots;

    public Socket GetEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot.CanSocket()) return slot;
        }
        return null;
    }

    public int GetFilledCount()
    {
        int count = 0;
        foreach (var slot in slots)
        {
            if (!slot.CanSocket()) count++;
        }
        return count;
    }
}
```

---

### How To: Snap With Audio Feedback

> **Goal:** Click sound when object snaps in
> **Time:** ~3 minutes

1. Add **AudioSource** to socket
2. Create script to play on insertion:

```csharp
public class SocketAudio : MonoBehaviour
{
    [SerializeField] private Socket socket;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip snapSound;

    void OnTriggerEnter(Collider other)
    {
        var socketable = other.GetComponent<Socketable>();
        if (socketable != null && socket.CanSocket())
        {
            // About to socket, play sound
            audioSource.PlayOneShot(snapSound);
        }
    }
}
```

---

### How To: Snap With Visual Preview

> **Goal:** Ghost outline shows where object will snap
> **Time:** ~5 minutes

1. Create a semi-transparent "ghost" version of your object
2. Parent it to the socket's Pivot
3. Hide/show based on proximity:

```csharp
public class SocketPreview : MonoBehaviour
{
    [SerializeField] private Socket socket;
    [SerializeField] private GameObject previewGhost;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Socketable>() && socket.CanSocket())
        {
            previewGhost.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        previewGhost.SetActive(false);
    }
}
```

![Socket Ghost Preview](../Images/socket-ghost-preview.png)

---

## Socket Filtering

### By Tag
Most common approach. See "Key and Lock" example above.

### By Component Type
Check for specific component:

```csharp
public override Transform Insert(Socketable socketable)
{
    if (!socketable.GetComponent<Battery>())
    {
        return null; // Only accept batteries
    }
    return base.Insert(socketable);
}
```

### By ScriptableObject ID
For complex item systems:

```csharp
[SerializeField] private ItemData[] acceptedItems;

public override Transform Insert(Socketable socketable)
{
    var item = socketable.GetComponent<Item>();
    if (item == null || !acceptedItems.Contains(item.Data))
    {
        return null;
    }
    return base.Insert(socketable);
}
```

---

## Tips & Best Practices

💡 **Size triggers generously**
Players need forgiving snap zones in VR. Make triggers larger than the visual holder.

💡 **Position pivot precisely**
Test that objects look correct when socketed. Adjust pivot position/rotation as needed.

💡 **Add feedback**
Sound, haptics, and visual effects make snapping satisfying. See Feedback System.

💡 **Consider removal**
How does the player retrieve socketed objects? Ensure Grabable works when socketed.

💡 **Use preview ghosts**
Semi-transparent previews help players understand where objects will land.

⚠️ **Common Mistake:** Collider not trigger
Socket detection uses OnTriggerEnter. Ensure **Is Trigger** is checked.

⚠️ **Common Mistake:** Missing Grabable
Socketable objects usually need Grabable to be picked up and placed.

⚠️ **Common Mistake:** Wrong layer setup
If objects don't detect sockets, check physics layer collision settings.

---

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| Object doesn't snap | Collider not a trigger | Check **Is Trigger** on socket collider |
| Object snaps to wrong position | Pivot wrong | Adjust Pivot transform position/rotation |
| Can't grab socketed object | Grabable issues | Ensure Grabable component is configured |
| Multiple objects snap to one socket | CanSocket not checking | Verify socket logic returns false when full |
| Object falls through socket | Collider too small | Enlarge socket trigger area |

---

## Scripting API

### Socket

```csharp
// Check if socket can accept
bool canAccept = socket.CanSocket();

// Get pivot position
Transform snapPoint = socket.Pivot;

// Manually insert (usually automatic)
socket.Insert(socketable);

// Manually remove
socket.Remove(socketable);
```

### Socketable

```csharp
// Check if currently socketed
bool isIn = socketable.IsSocketed;

// Get current socket
AbstractSocket currentSocket = socketable.CurrentSocket;
```

### Custom Socket Example

```csharp
public class CountingSocket : Socket
{
    public int insertCount = 0;

    public override Transform Insert(Socketable socketable)
    {
        insertCount++;
        Debug.Log($"Total insertions: {insertCount}");
        return base.Insert(socketable);
    }
}
```

### Socket Events via Code

```csharp
public class SocketMonitor : MonoBehaviour
{
    [SerializeField] private Socket socket;

    void OnTriggerEnter(Collider other)
    {
        var socketable = other.GetComponent<Socketable>();
        if (socketable != null && socket.CanSocket())
        {
            OnItemSocketed(socketable);
        }
    }

    void OnItemSocketed(Socketable item)
    {
        Debug.Log($"{item.name} was socketed!");
        // Trigger events, update UI, etc.
    }
}
```

---

## Abstract Socket

The Socket component inherits from **AbstractSocket**, which provides the base interface. You can create custom socket types by extending AbstractSocket:

```csharp
public abstract class AbstractSocket : MonoBehaviour
{
    public abstract Transform Pivot { get; }
    public abstract bool CanSocket();
    public virtual Transform Insert(Socketable socketable) { ... }
    public virtual void Remove(Socketable socketable) { ... }
}
```

### Example: Multi-Slot Socket

A socket that can hold multiple objects:

```csharp
public class MultiSocket : AbstractSocket
{
    [SerializeField] private Transform[] pivots;
    [SerializeField] private Socketable[] socketed;

    public override Transform Pivot => GetNextPivot();

    public override bool CanSocket()
    {
        return GetNextPivot() != null;
    }

    private Transform GetNextPivot()
    {
        for (int i = 0; i < pivots.Length; i++)
        {
            if (socketed[i] == null) return pivots[i];
        }
        return null;
    }

    // ... implement Insert/Remove to track which pivot used
}
```

---

---

## Socket Binders (Scriptable System Integration)

The Socket System integrates with the Scriptable System through dedicated binders. These allow socket events to drive variables and trigger GameEvents without custom code.

### Available Binders

| Binder | Attached To | Purpose |
|--------|-------------|---------|
| **SocketToBoolBinder** | Socket | Track if socket has an object |
| **SocketToEventBinder** | Socket | Fire events on insert/remove |
| **SocketableToBoolBinder** | Socketable | Track if object is socketed |
| **SocketableToEventBinder** | Socketable | Fire events on socket/unsocket |

### Socket To Bool Binder

Attach to a Socket to expose its state as a BoolVariable.

```
Socket (with SocketToBoolBinder)
├── Has Object Variable → true when filled
├── On Inserted Event → fires when object inserted
└── On Removed Event → fires when object removed
```

**Use Case:** Light indicator that turns on when slot is filled.

### Socketable To Bool Binder

Attach to a Socketable object to track its socketed state.

```
Key (with SocketableToBoolBinder)
├── Is Socketed Variable → true when in socket
├── On Socketed Event → fires when inserted
└── On Unsocketed Event → fires when removed
```

**Use Case:** Key that triggers door unlock when inserted.

### Example: Puzzle with All Slots Filled

1. Create 3 sockets, each with `SocketToBoolBinder`
2. Assign each to a different `BoolVariable` (Slot1Filled, Slot2Filled, Slot3Filled)
3. Create a `BoolComposite` that ANDs all three
4. Use result to trigger puzzle completion

### Example: Battery Installation Feedback

1. Add `SocketableToBoolBinder` to battery
2. Assign `IsInstalledVariable` BoolVariable
3. Use `BoolToggleBinder` to enable device when `IsInstalledVariable = true`

See [Binders Documentation](../ScriptableSystem/Binders.md) for full binder reference.

---

## Related Documentation

- [Grabable](../Interactables/Grabable.md) — Making objects grabbable
- [Feedback System](../Systems/FeedbackSystem.md) — Adding snap feedback
- [Sequencing System](../Systems/SequencingSystem.md) — Validating socket sequences
- [Binders](../ScriptableSystem/Binders.md) — Socket binder details
- [Quick Start Guide](../GettingStarted/QuickStart.md) — Basic setup

---

**Last Updated:** January 2026
**Component Version:** 1.1.0
