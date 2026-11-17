# Advanced Sockets — Categories, Multi-Slot Sockets, Inventories, Fasteners

The [Socket System](SocketSystem.md) page covers the basics: a **Socket** that holds one **Socketable**. This page covers everything built on top of it.

| Component | Use for |
|---|---|
| [Socket categories](#socket-categories) | Deciding which objects fit which sockets |
| [Multi Socket](#multi-socket) | Dropping any number of objects onto a surface |
| [Grid Multi Socket](#grid-multi-socket) | Rows and columns of identical slots |
| [Transform Array Multi Socket](#transform-array-multi-socket) | Hand-placed slots in any arrangement |
| [Shifting Rack Socket](#shifting-rack-socket) | A row where items slide along to make room |
| [Inventory Grid Socket](#inventory-grid-socket) | A grid inventory where items take up several cells |
| [Fastener and Fastener Tool](#fastener-and-fastener-tool) | Nuts, bolts, wrenches and drills |

All sockets share the four events from the base socket: **On Socket Connected**, **On Socket Disconnected**, **On Hover Start**, **On Hover End**.

---

## Socket Categories

Categories decide what fits where, independently of physics layers. There are up to 32, named in a **Socket Mask Registry** asset (Create > Shababeek > Interactions > Socket Mask Registry), in the same way tags and layers are named in Unity.

- On a **Socketable**, **Socketable Mask** lists the categories the object belongs to. Default: everything.
- On a socket, **Accepted Categories** lists what it takes. Leave it empty to accept any Socketable.
- An object fits when the two share at least one category.

**Socket Layer Mask** on the Socketable is separate: it limits which physics layers are scanned when looking for sockets. Both must match.

**Example:** name two categories *Battery* and *Fuse*. Give battery objects the Battery category and the battery compartment's socket Accepted Categories = Battery. Fuses will no longer snap into the battery compartment.

---

## Multi Socket

Accepts many objects on a flat surface. Each dropped object is projected onto the plane and parked where it landed.

| Field | What it does |
|---|---|
| **Local Offset** | Centre of the surface, in local space. |
| **Projection Direction** | Which way the surface faces. |
| **Pivot Rotation Offset** | Rotation applied to every parked object. |
| **Initial Pivot Count** | Slots created up front. More are created as needed. |

Use for table tops, trays and pin boards.

---

## Grid Multi Socket

Arranges objects in rows and columns. A dropped object goes to the nearest free slot.

| Field | What it does |
|---|---|
| **Grid Size** | Columns (X) and rows (Y). |
| **Spacing** | Distance between slots. |
| **Grid Plane** | Which way the grid faces. |
| **Grid Scale** | Uniform scale for the whole grid. |
| **Local Offset / Center Grid** | Where the grid sits, and whether it is centred on that point. |
| **Pivot Rotation Offset** | Rotation applied to every slot. |

Use for shelves, display cases and tool walls.

---

## Transform Array Multi Socket

You place the slots yourself as child transforms. The most flexible option.

| Field | What it does |
|---|---|
| **Socket Points** | The transforms that mark each slot. |
| **Use Closest Socket** | Pick the nearest free slot to where the object is released. |
| **Max Socket Distance** | How close an object must be to snap to a slot. |
| **Show Socket Gizmos**, colours | Scene view markers: free, occupied, hovered. |

```csharp
int free = socket.GetAvailableSocketCount();
bool taken = socket.IsSocketOccupied(2);
socket.ForceSocketToSlot(socketable, 0);
```

---

## Shifting Rack Socket

A single row of slots where items make room for each other, like books on a shelf or cartridges in a rack.

### Layout

| Field | What it does |
|---|---|
| **Required Category** | The one category this rack accepts. Empty accepts anything. |
| **Slot Count** | Number of slots. |
| **Spacing** | Distance between slot centres. |
| **Axis** | Direction the rack runs, in local space. |
| **Local Offset / Center Line** | Where the rack sits, and whether it is centred there. |
| **Pivot Rotation Offset** | Rotation applied to every slot. |
| **Placement Position / Rotation Offset** | Offset applied to items in a slot, for example to sit them on the surface. |

### Behaviour

| Field | What it does |
|---|---|
| **Allow Gaps** | Off (default): items pack from the start and close up when one is removed. On: any slot can be filled; dropping onto an occupied slot pushes neighbours toward the nearest gap. |
| **Shift Speed** | How fast items slide. 10 to 16 feels light. |
| **Highlight Prefab** | Optional visual moved to the slot a held item would land in. |
| **On Slot Highlighted / Unhighlighted** | Fired as the preview slot changes. |
| **Haptics** | Slot change, insert and reject vibration. Assign [Haptic Patterns](../Systems/Haptics.md) or use the amplitude and duration values. |

---

## Inventory Grid Socket

> **Menu Path:** Component > Shababeek > Interactions > Interactables > Inventory Grid Socket

A grid inventory where each item takes up a rectangle of cells (1×1, 2×1, 2×3, and so on). Items are shrunk to fit their cells when stored and restored to full size when taken out.

### Pieces

| Piece | Where it goes | Job |
|---|---|---|
| **Inventory Grid Socket** | The container | The grid itself |
| **Item Footprint** (asset) | Project window > Create > Shababeek > Interactions > Item Footprint | How many cells an item type takes, plus its name, icon and placement offsets |
| **Grid Item** | Each storable object (needs a Socketable) | Links the object to its Item Footprint. Without one an item takes 1×1. |
| **Inventory Grid Canvas Controller** | The container | Draws the cells: free, occupied, and where the held item would go |
| **Backpack Item Grabber** or **Inventory Backpack Grabable** | The container | Lets a hand take an item back out |

### Inventory Grid Socket — Inspector

| Field | What it does |
|---|---|
| **Grid Size** | Columns (X) and rows (Y). |
| **Cell World Size** | Size of one cell in metres. |
| **Grid Plane / Local Offset / Center Grid** | Orientation and position of the grid. |
| **Pivot Rotation Offset** | Rotation applied to all cells. |
| **Placement Position / Rotation Offset** | Offset applied to stored items. |
| **Scale To Fit** | Shrink items to fit their cells. |
| **Scale Padding** | Fraction of the cell area an item fills (0 to 1), so items do not touch the edges. |
| **On Grid Changed** | Fired after any insert or remove. |

### Taking items out

- **Backpack Item Grabber** on a fixed container: a grip press within **Grab Radius** of an occupied cell takes that item.
- **Inventory Backpack Grabable** for a backpack the player can also carry: gripping over an occupied cell takes the item; gripping anywhere else picks up the backpack.

---

## Fastener and Fastener Tool

Screw-in fasteners turned by a matching tool.

A **Fastener** (nut, bolt, screw) turns about its own axis and travels along it by its thread pitch, between fully tight and fully loose. A **Fastener Tool** goes on the tip of a grabbable tool (the wrench head, the drill bit) and turns fasteners it matches.

### Fastener — Inspector

| Field | What it does |
|---|---|
| **Fastener Mask** | Which tools can turn it. Uses [socket categories](#socket-categories). |
| **Axis** | Local thread axis. |
| **Pitch** | Metres travelled per full turn. |
| **Turns To Remove** | Full turns from tight to loose. |
| **Detach When Fully Loose** | Drop with physics once fully loosened. |
| **On Tightened / On Fully Loosened / On Progress** | Events. Progress runs 0 (tight) to 1 (loose). |
| **Verbose Logs** | Console logging. Turn off outside debugging. |

### Fastener Tool — Inspector

| Field | What it does |
|---|---|
| **Tool Mask** | Categories this tool can turn. |
| **Tip** | Point that looks for fasteners. Defaults to this transform. |
| **Detection Radius / Fastener Layers** | How close and on which layers fasteners are found. |
| **Drive Axis Local** | The tool's bit axis. On engaging, the tool snaps so this lines up with the fastener's axis. |
| **Mode** | **Manual**: a wrench; turn it by moving your hand around the fastener. **Motor**: a drill; turns at a fixed rate while a button is held. |
| **Motor Degrees Per Second** | Motor mode speed. |
| **Disengage Grace** | Stay engaged this long after the tip briefly leaves the fastener. |
| **Spin Visual** | Optional part that spins with the fastener, such as a drill bit. |
| **Use Fake Hand** | Show a posed hand on the tool while operating. |
| **On Engaged / On Disengaged** | Events. |
| **Verbose Logs** | Console logging. Turn off outside debugging. |

### Setup

1. Name two categories, for example *Nut* and *Bolt*.
2. Add **Fastener** to each nut and bolt and set its mask.
3. Make the wrench a [Grabable](../Interactables/Grabable.md). Add a child at the head with **Fastener Tool**, Tool Mask = Nut, Mode = Manual.
4. Make the drill a Grabable. Add a child at the bit with **Fastener Tool**, Tool Mask = Bolt, Mode = Motor.

---

## Related Documentation

- [Socket System](SocketSystem.md) — Socket and Socketable basics
- [Interaction Drivers](../ScriptableSystem/Drivers.md#socket-drivers) — Write socket state to variables and events
- [Interaction Sequences](../Systems/SequencingSystem.md) — Socket steps in tutorials
- [Haptic Patterns](../Systems/Haptics.md)

---

**Last Updated:** November 2026
