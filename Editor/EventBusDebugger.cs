using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

namespace rmMinusR.EventBus.Editor
{

    public class EventBusDebugger : EditorWindow, IListener
    {
        #region Window setup and status

        [MenuItem("Window/Analysis/EventBus Debugger")]
        private static void Init()
        {
            EventBusDebugger window = GetWindow<EventBusDebugger>();
            window.Show();
        }

        static EventBusDebugger ActiveInstance;

        public static bool IsOpen => ActiveInstance != null;

        #endregion

        private void OnEnable()
        {
            ClearHistory();

            //Build VisualElement tree

            TwoPaneSplitView splitViewRoot = new TwoPaneSplitView(1, 250, TwoPaneSplitViewOrientation.Horizontal);

            TwoPaneSplitView splitViewLeftHalf = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);

            TwoPaneSplitView splitViewRightHalf = new TwoPaneSplitView(1, 150, TwoPaneSplitViewOrientation.Vertical);

            //Horizontally
            splitViewLeftHalf.Add(BuildLogBox(out logBox));
            splitViewLeftHalf.Add(BuildEventDataBox(out eventDataBox));
            
            //Vertically
            splitViewRightHalf.Add(BuildListenersBox());
            splitViewRightHalf.Add(BuildStacktraceBox());

            //Composite
            splitViewRoot.Add(splitViewLeftHalf);
            splitViewRoot.Add(splitViewRightHalf);
            rootVisualElement.Add(splitViewRoot);

            //Setup instance
            ActiveInstance = this;

            //Setup listening
            ListenTo(EventBus.Main);

            titleContent = new GUIContent("EventBus Debugger");
        }

        private void OnDisable()
        {
            EventBus.Main.UnregisterAllHandlers(this);
            ActiveInstance = null;
        }

        private EventBus listenedBus = null;
        public void ListenTo(EventBus which)
        {
            //Stop listening
            if (listenedBus != null)
            {
                Debug.Log("Unregistering static events for " + this + " on bus '"+listenedBus.Name+"'");
                listenedBus.UnregisterAllHandlers(this);
            }

            listenedBus = which;

            //Start listening
            if (listenedBus != null)
            {
                Debug.Log("Registering static events for " + this + " on bus '" + listenedBus.Name + "'");
                listenedBus.RegisterStaticHandlers(this);
            }
        }

        #region Event record I/O

        [Serializable]
        public struct EventFireRecord
        {
            public DateTime timestamp;
            [SerializeReference] public Event @event;
            public List<EventCallback> listeners;
            public string stacktrace;

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

        [SerializeField] private List<LogEntryDisplay> logBoxEntries = new List<LogEntryDisplay>();

        private void ClearHistory()
        {
            logBoxEntries.Clear();
            if(logBox != null) logBox.Clear();
            selectedLogEntry = null;
            if (eventDataInspector != null) eventDataInspector.visible = false;
            if (stacktraceLabel != null) stacktraceLabel.text = "";
            if (logHeader != null) logHeader.text = "Log - 0 entries";
        }

        [EventHandler(Priority.Final)]
        private void RecordEvent(Event e)
        {
            EventFireRecord record = new EventFireRecord
            {
                @event = e,
                timestamp = DateTime.Now,
                //listeners = new List<EventCallback>(), //TODO fill!
                stacktrace = StackTraceUtility.ExtractStackTrace()
            };

            LogEntryDisplay display = new LogEntryDisplay(record, logBoxEntries.Count, this);
            logBoxEntries.Add(display);
            logBox.Add(display);

            logHeader.text = "Log - " + logBoxEntries.Count + " entries";
        }

        #endregion

        #region Log panel UI

        private ScrollView logBox;
        private Label logHeader;
        private VisualElement BuildLogBox(out ScrollView logBox)
        {
            Box root = new Box();
            root.style.minWidth = 150;
            root.Add(logHeader = BuildHeader("Log"));

            root.Add(BuildDivider());

            logBox = new ScrollView(ScrollViewMode.Vertical);
            logBox.style.flexGrow = 9999;
            root.Add(logBox);

            Button clearButton = new Button(ClearHistory);
            clearButton.Add(new Label("Clear"));
            root.Add(clearButton);

            return root;
        }

        [Serializable]
        private sealed class LogEntryDisplay : VisualElement, IEventHandler
        {
            private EventBusDebugger owner;
            [SerializeField] internal EventFireRecord record;
            private Label timestampLabel;
            private Label eventTypeLabel;
            private int id;

            public LogEntryDisplay(EventFireRecord record, int id, EventBusDebugger owner)
            {
                this.owner = owner;
                style.alignSelf = Align.Stretch;
                style.flexDirection = FlexDirection.Row;
                style.paddingTop = style.paddingBottom = 2.5f;

                this.id = id;
                RegisterCallback<ClickEvent>(e => owner.SelectLogEntry(id));
                focusable = true;

                Add(timestampLabel = new Label("[HH:MM:SS]"));
                timestampLabel.style.width = 60;

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

            private void UpdateTimestampLabel() => timestampLabel.text = record.timestamp.ToLocalTime().ToString("HH:mm:ss");

            private void UpdateEventTypeLabel() => eventTypeLabel.text = record.@event?.GetType()?.Name ?? "null";
        }

        private int? selectedLogEntryID;
        [SerializeField] private LogEntryDisplay selectedLogEntry = null;
        private void SelectLogEntry(int whichID)
        {
            //Update selection visual state
            if (selectedLogEntry != null) selectedLogEntry.SetSelected(false);
            selectedLogEntryID = whichID;
            selectedLogEntry = logBoxEntries[whichID];
            selectedLogEntry.SetSelected(true);

            //Update event data panel
            if (serializedRepr != null) serializedRepr.Update();
            else serializedRepr = new SerializedObject(this);

            eventDataInspector.visible = selectedLogEntry!=null;

            if (selectedLogEntry != null)
            {
                SerializedProperty prop = serializedRepr.FindProperty("selectedLogEntry").FindPropertyRelative("record").FindPropertyRelative("event");
                eventDataInspector.BindProperty(prop);

                //Try to open foldout, if it exists
                Foldout foldout = eventDataInspector.ElementAt(0) as Foldout;
                if (foldout != null) foldout.value = true;
            }

            //Update stacktrace label
            stacktraceLabel.text = selectedLogEntry != null ? selectedLogEntry.record.stacktrace : "";
        }

        #endregion

        #region Event data panel UI

        private VisualElement eventDataBox;

        private SerializedObject serializedRepr;
        private PropertyField eventDataInspector;

        private Label stacktraceLabel;
        private VisualElement BuildEventDataBox(out VisualElement eventDataBox)
        {
            Box root = new Box();
            root.style.minWidth = 150;
            root.Add(BuildHeader("Data"));
            root.Add(BuildDivider());

            eventDataBox = root;
            root.Add(eventDataInspector = new PropertyField());
            eventDataInspector.style.flexGrow = 1;

            return root;
        }

        #endregion

        private VisualElement BuildListenersBox()
        {
            Box root = new Box();
            root.style.minHeight = 100;
            root.Add(BuildHeader("Listeners"));
            root.Add(BuildDivider());

            //TODO

            return root;
        }

        private VisualElement BuildStacktraceBox()
        {
            Box root = new Box();
            root.style.minHeight = 100;
            root.Add(BuildHeader("Stacktrace"));
            root.Add(BuildDivider());

            ScrollView stacktraceScrollview = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            stacktraceScrollview.style.flexGrow = 1;
            stacktraceScrollview.Add(stacktraceLabel = new Label());
            root.Add(stacktraceScrollview);

            return root;
        }

        #region Misc helper UI functions

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

}