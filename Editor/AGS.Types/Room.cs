using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace AGS.Types
{
    [PropertyTab(typeof(PropertyTabInteractions), PropertyTabScope.Component)]
    [DefaultProperty("Description")]
    public class Room : UnloadedRoom, IChangeNotification, ILoadedRoom
    {
        // These constants are also defined in the native code, if you change
        // them you need to update the Native DLL as well
        public const int MAX_BACKGROUNDS = 5;
        public const int MAX_OBJECTS = 40;
        public const int MAX_HOTSPOTS = 50;
        public const int MAX_WALKABLE_AREAS = 16;
        public const int MAX_WALK_BEHINDS = 16;
        public const int MAX_REGIONS = 16;

        public const string EVENT_SUFFIX_ROOM_LOAD = "Load";

        public const string PROPERTY_NAME_MASKRESOLUTION = "MaskResolution";

        private static InteractionSchema _interactionSchema;

        public delegate void RoomModifiedChangedHandler(bool isModified);
        public event RoomModifiedChangedHandler RoomModifiedChanged;

        private int _leftEdgeX;
        private int _rightEdgeX;
        private int _topEdgeY;
        private int _bottomEdgeY;
        private int _playMusicOnRoomLoad;
        private bool _saveLoadEnabled = true;
        private bool _showPlayerCharacter = true;
        private int _playerCharacterView;
        private RoomVolumeAdjustment _musicVolumeAdjustment;
        private int _maskResolution = 1;
        private int _colorDepth;
        private int _width;
        private int _height;
        private int _backgroundAnimationDelay;
        private int _backgroundCount;
        private int _gameId;
        private bool _modified;
        private CustomProperties _properties = new CustomProperties();
        private Interactions _interactions = new Interactions(_interactionSchema);
        private readonly List<RoomMessage> _messages = new List<RoomMessage>();
        private readonly List<RoomObject> _objects = new List<RoomObject>();
        private readonly List<RoomHotspot> _hotspots = new List<RoomHotspot>();
        private readonly List<RoomWalkableArea> _walkableAreas = new List<RoomWalkableArea>();
        private readonly List<RoomWalkBehind> _walkBehinds = new List<RoomWalkBehind>();
        private readonly List<RoomRegion> _regions = new List<RoomRegion>();
        private IList<OldInteractionVariable> _oldInteractionVariables = new List<OldInteractionVariable>();
        public IntPtr _roomStructPtr;

        static Room()
        {
            _interactionSchema = new InteractionSchema(new string[] {
                "Walks off left edge",
                "Walks off right edge",
                "Walks off bottom edge",
                "Walks off top edge",
                "First time enters room",
                "Enters room before fade-in",
                "Repeatedly execute",
                "Enters room after fade-in",
                "Leaves room",
            },
                new string[] { "LeaveLeft", "LeaveRight", "LeaveBottom", "LeaveTop", 
                    "FirstLoad", EVENT_SUFFIX_ROOM_LOAD, "RepExec", "AfterFadeIn", "Leave" });
        }

        public Room(int roomNumber) : base(roomNumber)
        {
            _script = new Script("room" + roomNumber + ".asc", "// Room script file", false);

            for (int i = 0; i < MAX_HOTSPOTS; i++)
            {
                RoomHotspot hotspot = new RoomHotspot(this);
                hotspot.ID = i;
                if (i == 0) hotspot.Description = "No hotspot";
                else hotspot.Description = "Hotspot " + i;
                _hotspots.Add(hotspot);
            }

            for (int i = 0; i < MAX_WALKABLE_AREAS; i++)
            {
                RoomWalkableArea area = new RoomWalkableArea();
                area.ID = i;
                _walkableAreas.Add(area);
            }

            for (int i = 0; i < MAX_WALK_BEHINDS; i++)
            {
                RoomWalkBehind area = new RoomWalkBehind();
                area.ID = i;
                _walkBehinds.Add(area);
            }

            for (int i = 0; i < MAX_REGIONS; i++)
            {
                RoomRegion area = new RoomRegion();
                area.ID = i;
                _regions.Add(area);
            }
        }

        public Room(XmlNode node) : base(node)
        {
            LoadScript();
            _messages.AddRange(GetXmlChildren(node, "/Room/Messages", int.MaxValue).Select((xml, i) => new RoomMessage(i, xml)));
            _objects.AddRange(GetXmlChildren(node, "/Room/Objects", MAX_OBJECTS).Select((xml, i) => new RoomObject(this, xml) { ID = i }));
            _hotspots.AddRange(GetXmlChildren(node, "/Room/Hotspots", MAX_HOTSPOTS).Select((xml, i) => new RoomHotspot(this, xml) { ID = i }));
            _walkableAreas.AddRange(GetXmlChildren(node, "/Room/WalkableAreas", MAX_WALKABLE_AREAS).Select((xml, i) => new RoomWalkableArea(xml) { ID = i }));
            _walkBehinds.AddRange(GetXmlChildren(node, "/Room/WalkBehinds", MAX_WALK_BEHINDS).Select((xml, i) => new RoomWalkBehind(xml) { ID = i }));
            _regions.AddRange(GetXmlChildren(node, "/Room/Regions", MAX_REGIONS).Select((xml, i) => new RoomRegion(xml) { ID = i }));
        }

        [Browsable(false)]
        public bool Modified
        {
            get { return _modified; }
            set 
            {
                if (value != _modified)
                {
                    _modified = value;
                    if (RoomModifiedChanged != null)
                    {
                        RoomModifiedChanged(_modified);
                    }
                }
            }
        }

        [Description("What resolution do room region masks have relative to the room size")]
        [Category("Regions")]
        [DefaultValue(1)]
        [TypeConverter(typeof(RoomMaskResolutionTypeConverter))]
        public int MaskResolution
        {
            get { return _maskResolution; }
            set { _maskResolution = value; }
        }

        [Browsable(false)]
        public int BackgroundCount
        {
            get { return _backgroundCount; }
            set { _backgroundCount = value; }
        }

        [Browsable(false)]
        public int GameID
        {
            get { return _gameId; }
            set { _gameId = value; }
        }

        [Description("Colour depth of the room background, in bits per pixel")]
        [Category("Visual")]
        [ReadOnly(true)]
        public int ColorDepth
        {
            get { return _colorDepth; }
            set { _colorDepth = value; }
        }

        [Description("Width of the room, in game units")]
        //[DisplayName("WidthInGameUnits")]
        [Category("Visual")]
        [ReadOnly(true)]
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        [Description("Height of the room, in game units")]
        //[DisplayName("HeightInGameUnits")]
        [Category("Visual")]
        [ReadOnly(true)]
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }
        /*
        [Description("Width of the background image, in real pixels")]
        [DisplayName("ImageWidth")]
        [Category("Visual")]
        [ReadOnly(true)]
        public int ImageWidth
        {
            get { return _width * (int)_resolution; }
        }

        [Description("Height of the background image, in real pixels")]
        [DisplayName("ImageHeight")]
        [Category("Visual")]
        [ReadOnly(true)]
        public int ImageHeight
        {
            get { return _height * (int)_resolution; }
        }*/

        [Description("Room-specific messages")]
        [Category("Messages")]
        [EditorAttribute(typeof(RoomMessagesUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public IList<RoomMessage> Messages
        {
            get { return _messages; }
        }

        [Browsable(false)]
        public IList<RoomObject> Objects
        {
            get { return _objects; }
        }

        [Browsable(false)]
        public IList<RoomHotspot> Hotspots
        {
            get { return _hotspots; }
        }

        [Browsable(false)]
        public IList<RoomWalkableArea> WalkableAreas
        {
            get { return _walkableAreas; }
        }

        [Browsable(false)]
        public IList<RoomWalkBehind> WalkBehinds
        {
            get { return _walkBehinds; }
        }

        [Browsable(false)]
        public IList<RoomRegion> Regions
        {
            get { return _regions; }
        }

        [Description("The number of hotspots in the room")]
        [Category("Regions")]
        [Browsable(false)]
        public int HotspotCount
        {
            get { return _hotspots.Count; }
        }

        [Description("The number of walkable areas in the room")]
        [Category("Regions")]
        [Browsable(false)]
        public int WalkableAreaCount
        {
            get { return _walkableAreas.Count; }
        }

        [Description("The number of walk-behinds in the room")]
        [Category("Regions")]
        [Browsable(false)]
        public int WalkBehindCount
        {
            get { return _walkBehinds.Count; }
        }

        [Description("The number of regions in the room")]
        [Category("Regions")]
        [Browsable(false)]
        public int RegionCount
        {
            get { return _regions.Count; }
        }
       
        [Browsable(false)]
        public IList<OldInteractionVariable> OldInteractionVariables
        {
            get { return _oldInteractionVariables; }
        }

        [Browsable(false)]
        public string PropertyGridTitle
        {
            get { return _description + " (Room; number " + _number + ")"; }
        }

        [Description("The speed at which the backgrounds will rotate (only applicable if you have imported more than one)")]
        [DefaultValue(5)]
        [Category("Settings")]
        public int BackgroundAnimationDelay
        {
            get { return _backgroundAnimationDelay; }
            set { _backgroundAnimationDelay = value; }
        }

        [Browsable(false)]
        public int PlayMusicOnRoomLoad
        {
            get { return _playMusicOnRoomLoad; }
            set { _playMusicOnRoomLoad = value; }
        }

        [Browsable(false)]
        public bool SaveLoadEnabled
        {
            get { return _saveLoadEnabled; }
            set { _saveLoadEnabled = value; }
        }

        [Description("Whether the player character is visible on this screen")]
        [DefaultValue(true)]
        [Category("Settings")]
        public bool ShowPlayerCharacter
        {
            get { return _showPlayerCharacter; }
            set { _showPlayerCharacter = value; }
        }

        [Description("Override the player character's walking view for this room only")]
        [DefaultValue(0)]
        [Category("Settings")]
        [EditorAttribute(typeof(ViewUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public int PlayerCharacterView
        {
            get { return _playerCharacterView; }
            set { _playerCharacterView = value; }
        }

        [Description("LEGACY PlayMusic COMMANDS ONLY: The music volume will be adjusted when in this room")]
        [DefaultValue(RoomVolumeAdjustment.Normal)]
        [Category("Settings")]
        [DisplayName("LegacyMusicVolumeAdjustment")]
        public RoomVolumeAdjustment MusicVolumeAdjustment
        {
            get { return _musicVolumeAdjustment; }
            set { _musicVolumeAdjustment = value; }
        }

        [Description("The X co-ordinate of the left room edge")]
        [Category("Edges")]
        public int LeftEdgeX
        {
            get { return _leftEdgeX; }
            set { _leftEdgeX = value; }
        }

        [Description("The X co-ordinate of the right room edge")]
        [Category("Edges")]
        public int RightEdgeX
        {
            get { return _rightEdgeX; }
            set { _rightEdgeX = value; }
        }

        [Description("The Y co-ordinate of the top room edge")]
        [Category("Edges")]
        public int TopEdgeY
        {
            get { return _topEdgeY; }
            set { _topEdgeY = value; }
        }

        [Description("The Y co-ordinate of the bottom room edge")]
        [Category("Edges")]
        public int BottomEdgeY
        {
            get { return _bottomEdgeY; }
            set { _bottomEdgeY = value; }
        }

        [AGSSerializeClass()]
        [Description("Custom properties for this room")]
        [Category("Properties")]
        [EditorAttribute(typeof(CustomPropertiesUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public CustomProperties Properties
        {
            get { return _properties; }
            protected set { _properties = value; }
        }

        [AGSNoSerialize()]
        [Browsable(false)]
        public Interactions Interactions
        {
            get { return _interactions; }
        }

        public double GetMaskScale(RoomAreaMaskType mask)
        {
            switch (mask)
            {
                case RoomAreaMaskType.WalkBehinds:
                    return 1.0; // walk-behinds always 1:1 with room size
                case RoomAreaMaskType.Hotspots:
                case RoomAreaMaskType.WalkableAreas:
                case RoomAreaMaskType.Regions:
                    return 1.0 / MaskResolution;
                default:
                    throw new ArgumentException($"Illegal mask type, mask {mask} cannot be scaled.");
            }
        }

        public string GetBackgroundFileName(int background)
        {
            if (background < 0 && background >= MAX_BACKGROUNDS)
                throw new ArgumentException($"Must be positive number, but less than {MAX_BACKGROUNDS}", nameof(background));

            return Path.Combine(ROOM_DIRECTORY, $"{Number}", $"background{background}.png");
        }

        void IChangeNotification.ItemModified()
		{
			this.Modified = true;
		}

        public override void ToXml(XmlTextWriter writer)
        {
            SerializeUtils.SerializeToXML(this, writer, false);
            Interactions.ToXml(writer);
            SerializeUtils.SerializeToXML(writer, "Messages", Messages);
            SerializeUtils.SerializeToXML(writer, "Objects", Objects);
            SerializeUtils.SerializeToXML(writer, "Hotspots", Hotspots);
            SerializeUtils.SerializeToXML(writer, "WalkableAreas", WalkableAreas);
            SerializeUtils.SerializeToXML(writer, "WalkBehinds", WalkBehinds);
            SerializeUtils.SerializeToXML(writer, "Regions", Regions);
            writer.WriteEndElement();
        }

        private static IEnumerable<XmlNode> GetXmlChildren(XmlNode node, string xpath, int maxLimit)
        {
            return node.SelectSingleNode(xpath).ChildNodes.Cast<XmlNode>().Take(maxLimit);
        }
    }
}
