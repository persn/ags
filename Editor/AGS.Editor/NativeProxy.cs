#if !NO_GUI
using AGS.Native;
#endif
using AGS.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using AGS.Editor.Preferences;
using System.Linq;

namespace AGS.Editor
{
    class NativeProxy : IDisposable
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string dllToLoad);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("kernel32.dll")]
		public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
		[DllImport("kernel32.dll")]
		public static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);

        public const uint WM_MOUSEACTIVATE = 0x21;
        public const uint MA_ACTIVATE = 1;
        public const uint MA_ACTIVATEANDEAT = 2; 

        // IMPORTANT: These lengths reflect the resource file in ACWIN.EXE
        private const int AUTHOR_NAME_LEN = 30;
        private const int FILE_DESCRIPTION_LEN = 40;

        private static NativeProxy _instance;

        public static NativeProxy Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NativeProxy();
                }
                return _instance;
            }
        }

#if !NO_GUI
        private NativeMethods _native;
#endif

		// The sprite set lock is used to prevent a crash if the window
		// thread tries to redraw while the game is being saved/loaded
		private object _spriteSetLock = new object();

        private NativeProxy()
        {
#if !NO_GUI
			_native = new NativeMethods(AGS.Types.Version.AGS_EDITOR_VERSION);
            _native.Initialize();
#endif
        }

        public void NewWorkingDirSet(String workingDir)
        {
#if !NO_GUI
            _native.NewWorkingDirSet(workingDir);
#endif
        }

        public void NewGameLoaded(Game game, List<string> errors)
        {
#if !NO_GUI
            _native.NewGameLoaded(game, errors);
#endif
        }

        public void GameSettingsChanged(Game game)
        {
#if !NO_GUI
            _native.GameSettingsChanged(game);
#endif
        }

        public void PaletteColoursChanged(Game game)
        {
#if !NO_GUI
            _native.PaletteColoursUpdated(game);
#endif
        }

        public void SaveGame(Game game)
        {
#if !NO_GUI
			lock (_spriteSetLock)
			{
				_native.SaveGame(game);
			}
#endif
        }

        public void DrawGUI(IntPtr hdc, int x, int y, GUI gui, int resolutionFactor, float scale, int selectedControl)
        {
#if !NO_GUI
            _native.DrawGUI((int)hdc, x, y, gui, resolutionFactor, scale, selectedControl);
#endif
        }

        public void DrawSprite(IntPtr hdc, int x, int y, int spriteNum)
        {
#if !NO_GUI
			lock (_spriteSetLock)
			{
				_native.DrawSprite((int)hdc, x, y, spriteNum, false);
			}
#endif
        }

        public void DrawSprite(IntPtr hdc, int x, int y, int spriteNum, bool flipImage)
        {
#if !NO_GUI
			lock (_spriteSetLock)
			{
				_native.DrawSprite((int)hdc, x, y, spriteNum, flipImage);
			}
#endif
        }

        public int DrawFont(IntPtr hdc, int x, int y, int width, int fontNum)
        {
#if !NO_GUI
            return _native.DrawFont((int)hdc, x, y, width, fontNum);
#else
            return -1;
#endif
        }

        public void DrawSprite(IntPtr hdc, int x, int y, int width, int height, int spriteNum, bool flipImage = false)
        {
#if !NO_GUI
			lock (_spriteSetLock)
			{
				_native.DrawSprite((int)hdc, x, y, width, height, spriteNum, flipImage);
			}
#endif
        }

        public void DrawBlockOfColour(IntPtr hdc, int x, int y, int width, int height, int colourNum)
        {
#if !NO_GUI
            _native.DrawBlockOfColour((int)hdc, x, y, width, height, colourNum);
#endif
        }

        public void DrawViewLoop(IntPtr hdc, ViewLoop loop, int x, int y, int sizeInPixels, int selectedFrame)
        {
#if !NO_GUI
			lock (_spriteSetLock)
			{
				_native.DrawViewLoop((int)hdc, loop, x, y, sizeInPixels, selectedFrame);
			}
#endif
        }

        public Sprite CreateSpriteFromBitmap(Bitmap bmp, SpriteImportTransparency transparency, bool remapColours, bool useRoomBackgroundColours, bool alphaChannel)
        {
#if NO_GUI
            return new Sprite(0, 0, 0);
#else
            int spriteSlot = _native.GetFreeSpriteSlot();
            return _native.SetSpriteFromBitmap(spriteSlot, bmp, (int)transparency, remapColours, useRoomBackgroundColours, alphaChannel);
#endif
        }

        public void ReplaceSpriteWithBitmap(Sprite spr, Bitmap bmp, SpriteImportTransparency transparency, bool remapColours, bool useRoomBackgroundColours, bool alphaChannel)
        {
#if !NO_GUI
            _native.ReplaceSpriteWithBitmap(spr, bmp, (int)transparency, remapColours, useRoomBackgroundColours, alphaChannel);
#endif
        }

        public bool CropSpriteEdges(IList<Sprite> sprites, bool symettric)
        {
#if NO_GUI
            return false;
#else
            return _native.CropSpriteEdges(sprites, symettric);
#endif
        }

        public bool DoesSpriteExist(int spriteNumber)
        {
#if NO_GUI
            return false;
#else
            lock (_spriteSetLock)
			{
				return _native.DoesSpriteExist(spriteNumber);
			}
#endif
        }

        public void ChangeSpriteNumber(Sprite sprite, int newNumber)
        {
#if !NO_GUI
            _native.ChangeSpriteNumber(sprite, newNumber);
#endif
        }

        public void SpriteResolutionsChanged(Sprite[] sprites)
        {
#if !NO_GUI
            _native.SpriteResolutionsChanged(sprites);
#endif
        }

        public Bitmap GetBitmapForSprite(int spriteSlot, int width, int height)
        {
#if NO_GUI
            return new Bitmap(width, height);
#else
            lock (_spriteSetLock)
			{
				return _native.GetBitmapForSprite(spriteSlot, width, height);
			}
#endif
        }

        public Bitmap GetBitmapForSprite(int spriteSlot)
        {
#if NO_GUI
            return new Bitmap(0, 0);
#else
            lock (_spriteSetLock)
			{
				return _native.GetBitmapForSpritePreserveColDepth(spriteSlot);
			}
#endif
        }

        public void DeleteSprite(Sprite sprite)
        {
#if !NO_GUI
            lock (_spriteSetLock)
			{
				_native.DeleteSprite(sprite.Number);
			}
#endif
        }

        public Game ImportOldGame(string fileName)
        {
#if NO_GUI
            return new Game();
#else
            return _native.ImportOldGameFile(fileName);
#endif
        }

        public void ImportSCIFont(string fileName, int fontSlot)
        {
#if !NO_GUI
            _native.ImportSCIFont(fileName, fontSlot);
#endif
        }

        public void ReloadFont(int fontSlot)
        {
#if !NO_GUI
            _native.ReloadFont(fontSlot);
#endif
        }

        public void OnFontUpdated(Game game, int fontSlot, bool forceUpdate)
        {
#if !NO_GUI
            _native.OnGameFontUpdated(game, fontSlot, forceUpdate);
#endif
        }

        public Dictionary<int, Sprite> LoadSpriteDimensions()
        {
#if NO_GUI
            return new Dictionary<int, Sprite>();
#else
            return _native.LoadAllSpriteDimensions();
#endif
        }

        public void LoadNewSpriteFile()
        {
#if !NO_GUI
			lock (_spriteSetLock)
			{
				_native.LoadNewSpriteFile();
			}
#endif
        }

        public SpriteInfo GetSpriteInfo(int spriteSlot)
        {
#if NO_GUI
            return new SpriteInfo(0, 0, SpriteImportResolution.LowRes);
#else
            lock (_spriteSetLock)
            {
                return _native.GetSpriteInfo(spriteSlot);
            }
#endif
        }

        public int GetSpriteWidth(int spriteSlot)
        {
#if NO_GUI
            return 0;
#else
            lock (_spriteSetLock)
            {
                return _native.GetSpriteWidth(spriteSlot);
            }
#endif
        }

        public int GetSpriteHeight(int spriteSlot)
        {
#if NO_GUI
            return 0;
#else
            lock (_spriteSetLock)
            {
                return _native.GetSpriteHeight(spriteSlot);
            }
#endif
        }

        public Room LoadRoom(UnloadedRoom roomToLoad)
        {
#if NO_GUI
            return new Room(0);
#else
            return _native.LoadRoomFile(roomToLoad);
#endif
        }

        public void SaveRoom(Room roomToSave)
        {
#if !NO_GUI
            _native.SaveRoomFile(roomToSave);
#endif
        }

        public void DrawRoomBackground(IntPtr hDC, Room room, int x, int y, int backgroundNumber, float scaleFactor, RoomAreaMaskType maskType, int selectedArea, int maskTransparency)
        {
#if !NO_GUI
            _native.DrawRoomBackground((int)hDC, room, x, y, backgroundNumber, scaleFactor, maskType, selectedArea, maskTransparency);
#endif
        }

        public void ImportBackground(Room room, int backgroundNumber, Bitmap bmp, bool useExactPalette, bool sharePalette)
        {
#if !NO_GUI
            _native.ImportBackground(room, backgroundNumber, bmp, useExactPalette, sharePalette);
#endif
        }

        public void DeleteBackground(Room room, int backgroundNumber)
        {
#if !NO_GUI
            _native.DeleteBackground(room, backgroundNumber);
#endif
        }

        public Bitmap GetBitmapForBackground(Room room, int backgroundNumber)
        {
#if NO_GUI
            return new Bitmap(0, 0);
#else
            return _native.GetBitmapForBackground(room, backgroundNumber);
#endif
        }

        public void AdjustRoomMaskResolution(Room room)
        {
#if !NO_GUI
            _native.AdjustRoomMaskResolution(room);
#endif
        }

        public void CreateBuffer(int width, int height)
        {
#if !NO_GUI
            _native.CreateBuffer(width, height);
#endif
        }

        public void RenderBufferToHDC(IntPtr hDC)
        {
#if !NO_GUI
            _native.RenderBufferToHDC((int)hDC);
#endif
        }

        public void DrawSpriteToBuffer(int spriteNum, int x, int y, float scale)
        {
#if !NO_GUI
            _native.DrawSpriteToBuffer(spriteNum, x, y, scale);
#endif
        }

        public void DrawLineOntoMask(Room room, RoomAreaMaskType mask, int x1, int y1, int x2, int y2, int color)
        {
#if !NO_GUI
            _native.DrawLineOntoMask(room, mask, x1, y1, x2, y2, color);
#endif
        }

		public void DrawFilledRectOntoMask(Room room, RoomAreaMaskType mask, int x1, int y1, int x2, int y2, int color)
		{
#if !NO_GUI
			_native.DrawFilledRectOntoMask(room, mask, x1, y1, x2, y2, color);
#endif
		}

        public void DrawFillOntoMask(Room room, RoomAreaMaskType mask, int x1, int y1, int color)
        {
#if !NO_GUI
            _native.DrawFillOntoMask(room, mask, x1, y1, color);
#endif
        }

        public void CopyWalkableAreaMaskToRegions(Room room)
        {
#if !NO_GUI
            _native.CopyWalkableMaskToRegions(room);
#endif
        }

		public bool GreyOutNonSelectedMasks
		{
#if NO_GUI
            set { }
#else
            set { _native.SetGreyedOutMasksEnabled(value); }
#endif
		}

        public int GetAreaMaskPixel(Room room, RoomAreaMaskType mask, int x, int y)
        {
#if !NO_GUI
            int pixel = _native.GetAreaMaskPixel(room, mask, x, y);
#else
            int pixel = 0;
#endif
            // if it lies outside the bitmap, just return 0
            if (pixel < 0)
            {
                pixel = 0;
            }
            return pixel;
        }

        public void CreateUndoBuffer(Room room, RoomAreaMaskType mask)
        {
#if !NO_GUI
            _native.CreateUndoBuffer(room, mask);
#endif
        }

        public bool DoesUndoBufferExist()
        {
#if NO_GUI
            return false;
#else
            return _native.DoesUndoBufferExist();
#endif
        }

        public void ClearUndoBuffer()
        {
#if !NO_GUI
            _native.ClearUndoBuffer();
#endif
        }

        public void RestoreFromUndoBuffer(Room room, RoomAreaMaskType mask)
        {
#if !NO_GUI
            _native.RestoreFromUndoBuffer(room, mask);
#endif
        }

        public void ImportAreaMask(Room room, RoomAreaMaskType mask, Bitmap bmp)
        {
#if !NO_GUI
            _native.ImportAreaMask(room, mask, bmp);
#endif
        }

        public Bitmap ExportAreaMask(Room room, RoomAreaMaskType mask)
        {
#if NO_GUI
            return new Bitmap(0, 0);
#else
            return _native.ExportAreaMask(room, mask);
#endif
        }

        public string LoadRoomScript(string roomFileName)
        {
#if NO_GUI
            return string.Empty;
#else
            return _native.LoadRoomScript(roomFileName);
#endif
        }

        public void CompileScript(Script script, string[] preProcessedData, Game game)
        {
#if !NO_GUI
            _native.CompileScript(script, preProcessedData, game);
#endif
        }

        public void CreateDataFile(string[] fileList, int splitSize, string baseFileName, bool isGameEXE)
        {
#if !NO_GUI
            _native.CreateDataFile(fileList, splitSize, baseFileName, isGameEXE);
#endif
        }

        public void CreateDebugMiniEXE(string[] fileList, string exeFileName)
        {
            CreateDataFile(fileList, 0, exeFileName, false);
        }

        public void CreateVOXFile(string fileName, string[] fileList)
        {
#if !NO_GUI
            _native.CreateVOXFile(fileName, fileList);
#endif
        }

        public void CreateTemplateFile(string templateFileName, string[] fileList)
        {
            CreateDataFile(fileList, 0, templateFileName, false);
        }

        public GameTemplate LoadTemplateFile(string fileName)
        {
#if NO_GUI
            return null;
#else
            return _native.LoadTemplateFile(fileName);
#endif
        }

		public RoomTemplate LoadRoomTemplateFile(string fileName)

        {
#if NO_GUI
            return null;
#else
            return _native.LoadRoomTemplateFile(fileName);
#endif
		}

        public void ExtractTemplateFiles(string templateFileName)
        {
#if !NO_GUI
            _native.ExtractTemplateFiles(templateFileName);
#endif
        }

		public void ExtractRoomTemplateFiles(string templateFileName, int newRoomNumber)
		{
#if !NO_GUI
			_native.ExtractRoomTemplateFiles(templateFileName, newRoomNumber);
#endif
		}

        public void UpdateFileIcon(string fileToUpdate, string newIconToUse)
        {
#if !NO_GUI
            _native.UpdateFileIcon(fileToUpdate, newIconToUse);
#endif
        }

		public void UpdateGameExplorerXML(string fileToUpdate, byte[] newData)
		{
#if !NO_GUI
			_native.UpdateGameExplorerXML(fileToUpdate, newData);
#endif
		}

		public void UpdateGameExplorerThumbnail(string fileToUpdate, byte[] newData)
		{
#if !NO_GUI
			_native.UpdateGameExplorerThumbnail(fileToUpdate, newData);
#endif
		}

        public void UpdateFileVersionInfo(string fileToUpdate, string authorName, string gameName)
        {
#if !NO_GUI
            _native.UpdateFileVersionInfo(fileToUpdate, 
                System.Text.Encoding.Unicode.GetBytes(authorName.PadRight(AUTHOR_NAME_LEN, ' ')), 
                System.Text.Encoding.Unicode.GetBytes(gameName.PadRight(FILE_DESCRIPTION_LEN, ' ')));
#endif
        }

        public bool AreSpritesModified
        {
#if NO_GUI
            get { return true; }
#else
            get { return _native.HaveSpritesBeenModified(); }
#endif
        }

        public byte[] TransformStringToBytes(string text)
        {
#if NO_GUI
            return Enumerable.Empty<byte>().ToArray();
#else
            return _native.TransformStringToBytes(text);
#endif
        }

        /// <summary>
        /// Allows the Editor to reuse constants from the native code. If a constant required by the Editor
        /// is not also required by the Engine, then it should instead by moved into AGS.Types (AGS.Native
        /// references the AGS.Types assembly). Note that this method returns only System::Int32 and
        /// System::String objects -- it is up to the user to determine if the value should be used as a
        /// smaller integral type (additional casting may be required to cast to a non-int integral type).
        /// </summary>
        public object GetNativeConstant(string name)
        {
#if NO_GUI
            return string.Empty;
#else
            return _native.GetNativeConstant(name);
#endif
        }

        public void Dispose()
        {
#if !NO_GUI
            _native.Shutdown();
#endif
        }

    }
}
