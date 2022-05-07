# Event Messaging System for Unity

Provides support for passing custom messages between objects, making event-driven architecture easy and safe. Minimal setup, similar in syntax to the builtin `OnPointerClick` or `OnCollisionEnter`.

# Getting Started

1. Create a data type for your custom message
2. Receiver must inherit from `ScopedListener`, which will automatically manage registration
3. Create a function in the receiver to receive the event
4. Fire the event!

```
//Step 1
public class InteractEvent : Event
{
	public Player player;
	public Interactable interactedObject;
}

//Step 2/3 - LightSwitch.cs
[EventHandler]
void HandleLightsToggle(InteractEvent e) //This function can be named anything
{
	if(e.interactedObject == this) connectedLight.enabled = !connectedLight.enabled;
}

//Step 4 - PlayerController.cs
EventAPI.Dispatch(new InteractEvent { player = this, interactedObject = objectUnderCursor });
```

If your MonoBehaviour needs to inherit from something else, instead implement the `IListener` interface and manually register using `EventAPI.RegisterStaticHandlers`.