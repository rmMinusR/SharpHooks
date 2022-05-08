using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

using Events;
using Event = Events.Event;

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
        //Build VisualElement tree

        TwoPaneSplitView splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

        TwoPaneSplitView splitView2 = new TwoPaneSplitView(1, 400, TwoPaneSplitViewOrientation.Horizontal);

        {
            Box logPanelMain = new Box();
            logPanelMain.Add(BuildHeader("Log"));
            
            BuildLogBox(logPanelMain);
            splitView.Add(logPanelMain);

            Button clearButton = new Button(ClearHistory);
            clearButton.Add(new Label("Clear"));
            logPanelMain.Add(clearButton);
        }

        {
            Box eventDataPanelMain = new Box();
            Label detailsTitle = BuildHeader("Data");
            eventDataPanelMain.Add(detailsTitle);
            BuildEventDataBox(eventDataPanelMain);
            splitView2.Add(eventDataPanelMain);
        }

        {
            Box listenersPanelMain = new Box();
            Label listenersTitle = BuildHeader("Listeners");
            listenersPanelMain.Add(listenersTitle);
            BuildListenersBox(listenersPanelMain);
            splitView2.Add(listenersPanelMain);
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

    #region Log data structures

    public struct EventFireRecord
    {
        public System.DateTime timestamp;
        public Event @event;
        public List<EventCallback> listeners;
    }

    private List<EventFireRecord> history = new List<EventFireRecord>();

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

    [EventHandler(Priority.Highest)]
    private void RecordEvent(Event e)
    {
        history.Add(new EventFireRecord
        {
            @event = e,
            timestamp = System.DateTime.Now,
            listeners = new List<EventCallback>() //TODO fill!
        });

    }

    #endregion

    #region Log box

    private ScrollView logBox;
    private void BuildLogBox(VisualElement root)
    {
        root.Add(BuildDivider());

        logBox = new ScrollView(ScrollViewMode.Vertical);
        logBox.style.flexGrow = 9999;
        root.Add(logBox);
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
            if(this.record.@event != record.@event)
            {
                this.record = record;
                UpdateTimestampLabel();
                UpdateEventTypeLabel();
            }
        }

        private void UpdateTimestampLabel()
        {
            timestampLabel.text = record.timestamp.ToLocalTime().ToString("HH:mm:ss");
        }

        private void UpdateEventTypeLabel()
        {
            eventTypeLabel.text = record.@event?.GetType()?.Name ?? "null";
        }
    }

    private LogEntryDisplay selectedLogEntry = null;
    private void SelectLogEntry(LogEntryDisplay which)
    {
        foreach (LogEntryDisplay i in logBox.Children()) i.SetSelected(i == which);
        selectedLogEntry = which;
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

    private void BuildEventDataBox(VisualElement root)
    {
        root.Add(BuildDivider());

        
    }

    private void BuildListenersBox(VisualElement root)
    {
        root.Add(BuildDivider());

        
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
