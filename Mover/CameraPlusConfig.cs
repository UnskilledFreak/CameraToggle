using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Mover
{
    public class CameraPlusConfig
    {
        private readonly string _filePath;
        private readonly ILogger _logger;
        private CameraPlusConfig _backup;
        private FileSystemWatcher _watcher;

        public bool Changed { get; set; }
        public readonly View View;

        public ConfigProperty<bool> LockScreen = new ConfigProperty<bool>("LockScreen", true);
        public ConfigProperty<float> Fov = new ConfigProperty<float>("fov", 90);
        public ConfigProperty<int> AntiAliasing = new ConfigProperty<int>("antiAliasing", 2);
        public ConfigProperty<float> RenderScale = new ConfigProperty<float>("renderScale", 1);
        public ConfigProperty<float> PositionSmooth = new ConfigProperty<float>("positionSmooth", 10);
        public ConfigProperty<float> RotationSmooth = new ConfigProperty<float>("rotationSmooth", 5);
        public ConfigProperty<float> Cam360Smoothness = new ConfigProperty<float>("cam360Smoothness", 2);
        public ConfigProperty<bool> Cam360RotateControlNew = new ConfigProperty<bool>("cam360RotateControlNew", true);
        public ConfigProperty<bool> ThirdPerson = new ConfigProperty<bool>("thirdPerson", true);
        public ConfigProperty<bool> ShowThirdPersonCamera = new ConfigProperty<bool>("showThirdPersonCamera", true);
        public ConfigProperty<bool> Use360Camera = new ConfigProperty<bool>("use360Camera", false);
        public ConfigProperty<float> PosX = new ConfigProperty<float>("posx", 0);
        public ConfigProperty<float> PosY = new ConfigProperty<float>("posy", 2.5f);
        public ConfigProperty<float> PosZ = new ConfigProperty<float>("posz", -2);
        public ConfigProperty<float> AngX = new ConfigProperty<float>("angx", 15);
        public ConfigProperty<float> AngY = new ConfigProperty<float>("angy", 0);
        public ConfigProperty<float> AngZ = new ConfigProperty<float>("angz", 0);
        public ConfigProperty<float> FirstPersonPosOffsetX = new ConfigProperty<float>("firstPersonPosOffsetX", 0);
        public ConfigProperty<float> FirstPersonPosOffsetY = new ConfigProperty<float>("firstPersonPosOffsetY", 0);
        public ConfigProperty<float> FirstPersonPosOffsetZ = new ConfigProperty<float>("firstPersonPosOffsetZ", 0);
        public ConfigProperty<float> FirstPersonRotOffsetX = new ConfigProperty<float>("firstPersonRotOffsetX", 0);
        public ConfigProperty<float> FirstPersonRotOffsetY = new ConfigProperty<float>("firstPersonRotOffsetY", 0);
        public ConfigProperty<float> FirstPersonRotOffsetZ = new ConfigProperty<float>("firstPersonRotOffsetZ", 0);
        public ConfigProperty<float> Cam360ForwardOffset = new ConfigProperty<float>("cam360ForwardOffset", -2);
        public ConfigProperty<float> Cam360XTilt = new ConfigProperty<float>("cam360XTilt", 10);
        public ConfigProperty<float> Cam360ZTilt = new ConfigProperty<float>("cam360ZTilt", 0);
        public ConfigProperty<float> Cam360YTilt = new ConfigProperty<float>("cam360YTilt", 0);
        public ConfigProperty<float> Cam360UpOffset = new ConfigProperty<float>("cam360UpOffset", 2.2f);
        public ConfigProperty<float> Cam360RightOffset = new ConfigProperty<float>("cam360RightOffset", 0);
        public ConfigProperty<int> ScreenWidth = new ConfigProperty<int>("screenWidth", 1920);
        public ConfigProperty<int> ScreenHeight = new ConfigProperty<int>("screenHeight", 1080);
        public ConfigProperty<int> ScreenPosX = new ConfigProperty<int>("screenPosX", 0);
        public ConfigProperty<int> ScreenPosY = new ConfigProperty<int>("screenPosY", 0);
        public ConfigProperty<int> MultiPlayerNumber = new ConfigProperty<int>("MultiPlayerNumber", 1);
        public ConfigProperty<bool> DisplayMultiPlayerNameInfo = new ConfigProperty<bool>("DisplayMultiPlayerNameInfo", true);
        public ConfigProperty<int> Layer = new ConfigProperty<int>("layer", -1000);
        public ConfigProperty<bool> FitToCanvas = new ConfigProperty<bool>("fitToCanvas", false);
        public ConfigProperty<bool> TransparentWalls = new ConfigProperty<bool>("transparentWalls", true);
        public ConfigProperty<bool> ForceFirstPersonUpRight = new ConfigProperty<bool>("forceFirstPersonUpRight", false);
        public ConfigProperty<bool> Avatar = new ConfigProperty<bool>("avatar", true);
        public ConfigProperty<string> Debris = new ConfigProperty<string>("HideUI", "link");
        public ConfigProperty<bool> HideUi = new ConfigProperty<bool>("avatar", false);
        public ConfigProperty<string> MovementScriptPath = new ConfigProperty<string>("movementScriptPath", "");
        public ConfigProperty<bool> MovementAudioSync = new ConfigProperty<bool>("movementAudioSync", true);

        public static CameraPlusConfig FromFile(string filePath, View view, ILogger logger)
        {
            return new CameraPlusConfig(filePath, view, logger);
        }
        public void Destroy()
        {
            _watcher.Changed -= FileChangeEvent;
        }

        private CameraPlusConfig(string filePath, View view, ILogger logger, bool createRestoreBackup = true)
        {
            _filePath = filePath;
            View = view;
            _logger = logger;
            Changed = false;
            
            LoadFile(createRestoreBackup);
        }

        private void LoadFile(bool createRestoreBackup = true)
        {
            Factory.WaitUntilFileIsNotLocked(_filePath);
            var array = File.ReadAllLines(_filePath).Select(x => x.Split('=')).ToList();
            
            LockScreen.LoadFromStr(array);
            Fov.LoadFromStr(array);
            AntiAliasing.LoadFromStr(array);
            RenderScale.LoadFromStr(array);
            PositionSmooth.LoadFromStr(array);
            RotationSmooth.LoadFromStr(array);
            Cam360Smoothness.LoadFromStr(array);
            Cam360RotateControlNew.LoadFromStr(array);
            ThirdPerson.LoadFromStr(array);
            ShowThirdPersonCamera.LoadFromStr(array);
            Use360Camera.LoadFromStr(array);
            PosX.LoadFromStr(array);
            PosY.LoadFromStr(array);
            PosZ.LoadFromStr(array);
            AngX.LoadFromStr(array);
            AngY.LoadFromStr(array);
            AngZ.LoadFromStr(array);
            FirstPersonPosOffsetX.LoadFromStr(array);
            FirstPersonPosOffsetY.LoadFromStr(array);
            FirstPersonPosOffsetZ.LoadFromStr(array);
            FirstPersonRotOffsetX.LoadFromStr(array);
            FirstPersonRotOffsetY.LoadFromStr(array);
            FirstPersonRotOffsetZ.LoadFromStr(array);
            Cam360ForwardOffset.LoadFromStr(array);
            Cam360XTilt.LoadFromStr(array);
            Cam360ZTilt.LoadFromStr(array);
            Cam360YTilt.LoadFromStr(array);
            Cam360UpOffset.LoadFromStr(array);
            Cam360RightOffset.LoadFromStr(array);
            ScreenWidth.LoadFromStr(array);
            ScreenHeight.LoadFromStr(array);
            ScreenPosX.LoadFromStr(array);
            ScreenPosY.LoadFromStr(array);
            MultiPlayerNumber.LoadFromStr(array);
            DisplayMultiPlayerNameInfo.LoadFromStr(array);
            Layer.LoadFromStr(array);
            FitToCanvas.LoadFromStr(array);
            TransparentWalls.LoadFromStr(array);
            ForceFirstPersonUpRight.LoadFromStr(array);
            Avatar.LoadFromStr(array);
            Debris.LoadFromStr(array);
            HideUi.LoadFromStr(array);
            MovementScriptPath.LoadFromStr(array);
            MovementAudioSync.LoadFromStr(array);
            
            if (createRestoreBackup)
            {
                _backup = new CameraPlusConfig(_filePath, View, _logger, false);
                _watcher = new FileSystemWatcher(Path.GetDirectoryName(_filePath))
                {
                    NotifyFilter = NotifyFilters.LastWrite,
                    Filter = Path.GetFileName(_filePath),
                    EnableRaisingEvents = true
                };
                _watcher.Changed += FileChangeEvent;
            }

            var createRestoreBackupString = createRestoreBackup ? "backup " : "";
            _logger.Log($"(re)loaded {createRestoreBackupString}{Path.GetFileName(_filePath)}");
        }

        private void FileChangeEvent(object sender, FileSystemEventArgs args)
        {
            //var conf = _cameras.First(x => x.FilePath == args.FullPath);
            //_logger.Log($"{conf.View:g} config file written!");
            //_logger.Log($"{args.Name} config written");
            LoadFile(false);
        }

        public void Save()
        {
            if (!Changed)
            {
                return;
            }

            var linesToWrite = GetType()
                .GetProperties(BindingFlags.Public)
                .Where(x => x.PropertyType == typeof(ConfigProperty<>))
                .Select(x => x.PropertyType.GetMethod("GetSaveStr")?.Invoke(x.GetValue(null), null));

            Factory.WaitUntilFileIsNotLocked(_filePath);
            File.WriteAllText(_filePath, string.Join(Environment.NewLine, linesToWrite));
        }

        public void RestoreFromBackup()
        {
            if (_backup == null)
            {
                return;
            }

            LockScreen = _backup.LockScreen;
            Fov = _backup.Fov;
            AntiAliasing = _backup.AntiAliasing;
            RenderScale = _backup.RenderScale;
            PositionSmooth = _backup.PositionSmooth;
            RotationSmooth = _backup.RotationSmooth;
            Cam360Smoothness = _backup.Cam360Smoothness;
            Cam360RotateControlNew = _backup.Cam360RotateControlNew;
            ThirdPerson = _backup.ThirdPerson;
            ShowThirdPersonCamera = _backup.ShowThirdPersonCamera;
            Use360Camera = _backup.Use360Camera;
            PosX = _backup.PosX;
            PosY = _backup.PosY;
            PosZ = _backup.PosZ;
            AngX = _backup.AngX;
            AngY = _backup.AngY;
            AngZ = _backup.AngZ;
            FirstPersonPosOffsetX = _backup.FirstPersonPosOffsetX;
            FirstPersonPosOffsetY = _backup.FirstPersonPosOffsetY;
            FirstPersonPosOffsetZ = _backup.FirstPersonPosOffsetZ;
            FirstPersonRotOffsetX = _backup.FirstPersonRotOffsetX;
            FirstPersonRotOffsetY = _backup.FirstPersonRotOffsetY;
            FirstPersonRotOffsetZ = _backup.FirstPersonRotOffsetZ;
            Cam360ForwardOffset = _backup.Cam360ForwardOffset;
            Cam360XTilt = _backup.Cam360XTilt;
            Cam360ZTilt = _backup.Cam360ZTilt;
            Cam360YTilt = _backup.Cam360YTilt;
            Cam360UpOffset = _backup.Cam360UpOffset;
            Cam360RightOffset = _backup.Cam360RightOffset;
            ScreenWidth = _backup.ScreenWidth;
            ScreenHeight = _backup.ScreenHeight;
            ScreenPosX = _backup.ScreenPosX;
            ScreenPosY = _backup.ScreenPosY;
            MultiPlayerNumber = _backup.MultiPlayerNumber;
            DisplayMultiPlayerNameInfo = _backup.DisplayMultiPlayerNameInfo;
            Layer = _backup.Layer;
            FitToCanvas = _backup.FitToCanvas;
            TransparentWalls = _backup.TransparentWalls;
            ForceFirstPersonUpRight = _backup.ForceFirstPersonUpRight;
            Avatar = _backup.Avatar;
            Debris = _backup.Debris;
            HideUi = _backup.HideUi;
            MovementScriptPath = _backup.MovementScriptPath;
            MovementAudioSync = _backup.MovementAudioSync;

            Changed = true;
        }
        
        private static bool ToBoolValue(string value)
        {
            return value.ToLower() == "true";
        }
    }
}