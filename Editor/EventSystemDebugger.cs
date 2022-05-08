using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

using Events;
using Event = Events.Event;
using UnityEditor.UIElements;
using System;

public class EventSystemDebugger : EditorWindow, IListener
{
    #region Window setup and status

    [MenuItem("Window/Analysis/Event Debugger")]
    private static void Init()
    {
        GetWindow(typeof(EventSystemDebugger)).Show();
    }

    static EventSystemDebugger ActiveInstance;

    public static bool IsOpen => ActiveInstance != null;

    #endregion

    private void OnEnable()
    {
        ClearHistory();

        //Build VisualElement tree

        TwoPaneSplitView splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

        TwoPaneSplitView splitView2 = new TwoPaneSplitView(1, 400, TwoPaneSplitViewOrientation.Horizontal);

        splitView.Add(BuildLogBox(out logBox));
        {
            splitView2.Add(BuildEventDataBox(out eventDataBox));
            splitView2.Add(BuildListenersBox());
        }
        splitView.Add(splitView2);
        rootVisualElement.Add(splitView);

        ActiveInstance = this;
        DoEventRegistration();
    }

    private void OnDisable()
    {
        EventAPI.UnregisterAllHandlers(this);
        ActiveInstance = null;
    }

    [Serializable]
    public struct EventFireRecord
    {
        public System.DateTime timestamp;
        public Event @event;
        public List<EventCallback> listeners;

        public static bool operator==(EventFireRecord a, EventFireRecord b)
        {
            return a.@event == b.@event
                && a.timestamp == b.timestamp;
        }

        public static bool operator !=(EventFireRecord a, EventFireRecord b) => !(a == b);

        public override bool Equals(object obj)
        {
            return obj is EventFireRecord && this == (EventFireRecord)obj;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ timestamp.GetHashCode() ^ @event.GetHashCode() ^ listeners.GetHashCode();
        }
    }

    #region Log data structures

    [SerializeField] private List<EventFireRecord> history = new List<EventFireRecord>();

    private void ClearHistory()
    {
        history.Clear();
        selectedLogEntry = null;
    }

    protected internal virtual void DoEventRegistration()
    {
        Debug.Log("Registering static events for " + this);
        EventAPI.RegisterStaticHandlers(this);
    }

    [EventHandler(Priority.Final)]
    private void RecordEvent(Event e)
    {
        history.Add(new EventFireRecord
        {
            @event = e,
            timestamp = DateTime.Now,
            listeners = new List<EventCallback>() //TODO fill!
        });

    }

    #endregion

    #region Log box

    private ScrollView logBox;
    private VisualElement BuildLogBox(out ScrollView logBox)
    {
        Box root = new Box();
        root.Add(BuildHeader("Log"));

        root.Add(BuildDivider());

        logBox = new ScrollView(ScrollViewMode.Vertical);
        logBox.style.flexGrow = 9999;
        root.Add(logBox);

        Button clearButton = new Button(ClearHistory);
        clearButton.Add(new Label("Clear"));
        root.Add(clearButton);

        return root;
    }

    private sealed class LogEntryDisplay : VisualElement, IEventHandler
    {
        private EventSystemDebugger owner;
        private EventFireRecord record;
        private Label timestampLabel;
        private Label eventTypeLabel;

        public LogEntryDisplay(EventFireRecord record, EventSystemDebugger owner)
        {
            this.owner = owner;
            style.alignSelf = Align.Stretch;
            style.flexDirection = FlexDirection.Row;
            style.paddingTop = style.paddingBottom = 2.5f;

            //RegisterCallback<ClickEvent>(e => EventSystemDebugger.ActiveInstance.SelectLogEntry(this));
            RegisterCallback<ClickEvent>(e => owner.SelectLogEntry(this));
            focusable = true;

            Add(timestampLabel = new Label("[HH:MM:SS]"));
            timestampLabel.style.width = 80;

            Add(eventTypeLabel = new Label("MyEvent"));

            SetDisplayedRecord(record);
        }

        internal void SetSelected(bool sel)
        {
            if (sel) style.backgroundColor = Color.Lerp(Color.black, Color.white, 0.9f);
            else     style.backgroundColor = Color.Lerp(Color.black, Color.white, 0.72f);

            Focus();
        }

        internal void SetDisplayedRecord(EventFireRecord record)
        {
            if(this.record != record)
            {
                this.record = record;
                UpdateTimestampLabel();
                UpdateEventTypeLabel();
            }
        }

        internal EventFireRecord GetDisplayedRecord() => record;

        private void UpdateTimestampLabel()
        {
            timestampLabel.text = record.timestamp.ToLocalTime().ToString("HH:mm:ss");
        }

        private void UpdateEventTypeLabel()
        {
            eventTypeLabel.text = record.@event?.GetType()?.Name ?? "null";
        }
    }

    private void Update()
    {
        //Ensure same child count
        while (logBox.childCount > history.Count) logBox.RemoveAt(history.Count); //Remove excess
        while (logBox.childCount < history.Count) logBox.Add(new LogEntryDisplay(default, this)); //Add to fill

        //Write displayed records
        int index = 0;
        foreach(LogEntryDisplay i in logBox.Children())
        {
            i.SetDisplayedRecord(history[index]);
            ++index;
        }
    }

    #endregion

    private LogEntryDisplay selectedLogEntry = null;
    private void SelectLogEntry(LogEntryDisplay which)
    {
        //Update selection visual state
        selectedLogEntry = which;
        foreach (LogEntryDisplay i in logBox.Children()) i.SetSelected(i == selectedLogEntry);

        //Update event data panel
        if (serializedRepr != null) serializedRepr.Update();
        else serializedRepr = new SerializedObject(this);
        
        if(eventDataInspector != null) eventDataBox.Remove(eventDataInspector);
        eventDataInspector = null;

        if (selectedLogEntry != null)
        {
            int selectedIndex = history.FindIndex(r => r == which.GetDisplayedRecord());
            eventDataInspector = new PropertyField(serializedRepr.FindProperty("history").GetArrayElementAtIndex(selectedIndex).FindPropertyRelative("event"));
            eventDataInspector.style.flexGrow = 1;
            eventDataBox.Add(eventDataInspector);
        }

        //serializedRepr.ApplyModifiedProperties();
    }

    #region Event data

    private VisualElement eventDataBox;

    private SerializedObject serializedRepr;
    private PropertyField eventDataInspector;
    private VisualElement BuildEventDataBox(out VisualElement eventDataBox)
    {
        Box root = new Box();
        root.Add(BuildHeader("Data"));
        root.Add(BuildDivider());

        eventDataBox = root;
        //root.Add(eventInspectorElement = new PropertyField());
        //root.Add(new PropertyField());

        return root;
    }

    #endregion

    private VisualElement BuildListenersBox()
    {
        Box root = new Box();
        root.Add(BuildHeader("Listeners"));
        root.Add(BuildDivider());

        //TODO

        return root;
    }


    #region Utils

    private Label BuildHeader(string text)
    {
        Label headerLabel = new Label(text);
        headerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        headerLabel.style.height = headerLabel.style.maxHeight = headerLabel.style.minHeight = 30;
        headerLabel.style.alignSelf = Align.Center;
        headerLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
        headerLabel.style.flexGrow = 1;
        headerLabel.style.flexDirection = FlexDirection.Row;
        return headerLabel;
    }

    private VisualElement BuildDivider()
    {
        Box divider = new Box();
        divider.style.backgroundColor = Color.black;
        return divider;
    }

    #endregion
}
