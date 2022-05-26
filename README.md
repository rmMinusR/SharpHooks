
# Event Bus System for Unity

Provides support for indirectly passing custom messages between objects, making event-driven architecture easy, safe, and decoupled. Minimal setup, similar in syntax to the builtin `OnPointerClick` or `OnCollisionEnter`.

# Getting Started

1. Create a data type for your custom message
2. Receiver must inherit from `ScopedListener`, which will automatically manage registration in the MonoBehaviour lifecycle
3. Create a function in the receiver to receive the event
4. Fire the event!

```
//Step 1
public class InteractEvent : Event
{
	public Player player;
	public Interactable interactedObject;
}

//Step 2
public class LightSwitch : ScopedListener
{
	//Step 3
	[EventHandler]
	void HandleLightsToggle(InteractEvent e) //This function can be named anything
	{
		if(e.interactedObject == this) {
			connectedLight.enabled = !connectedLight.enabled;
			e.Consume();
		}
	}
}

//Step 4 - Player.cs
InteractEvent e = new InteractEvent { player = this, interactedObject = objectUnderCursor };
EventBus.Main.Dispatch(e);
```

# More advanced use

Event handling functions can be named anything, so long as the parameter type matches and the function is marked `EventHandler`. This also means that multiple EventHandlers can be defined in the same script for the same type, allowing easy processing of events.

`ScopedListener` is a convenience, but the `IListener` interface allows for finer control over registration, or a listener that isn't a unity object. By default `ScopedListener` registers the whole class using `EventBus.Main.RegisterStaticHandlers` / `EventBus.Main.UnregisterAllHandlers`. Managing specifically callbacks dynamically (not on enable/disable) is also possible, and supports lambdas and local functions as well.

# Uses

No matter where an event is fired from, it will reach all valid listeners. This makes it useful for decoupling. For example, an achievement for "Deal at least 100 damage in a single hit" or "Harvest 30 rutabaga" would listen for a DamageEvent or CropHarvestEvent. This also helps maintainability, as putting achievement code in the combat system or interaction controller would make reading difficult. Robert Nystrom explains this application in detail: https://gameprogrammingpatterns.com/event-queue.html

This implementation can also be used like a query, or like the Builder pattern, with the message object treated as an accumulator. To implement an equippable trinket that grants +25 fire damage on attacks and -12% resistance to water damage, the item can listen for a DamageEvent and modify the damage value before it is applied--Priority.Final is equivalent to DamageBuilder.Build(). This can also be useful for status effects and abilities, such as blocking the first projectile from a direction and reducing all further damage from that direction by 10%.

# Inspiration

Messaging use inspired by patterns such as Observer, Event Bus, and Pub/Sub.
Query use with mutable events inspired by the event buses of [MinecraftForge](https://mcforge.readthedocs.io/en/1.14.x/events/intro/) and [Bukkit](https://www.spigotmc.org/wiki/using-the-event-api/).
Unintentionally similar to [Guava's EventBus](https://guava.dev/releases/22.0/api/docs/com/google/common/eventbus/EventBus.html).