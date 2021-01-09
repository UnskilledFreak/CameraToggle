using System;
using System.Collections.Generic;
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

            // todo :: reflection via fields
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

            // todo :: reflection via fields
            var linesToWrite = new List<string>();

            linesToWrite.Add(LockScreen.GetSaveStr());
            linesToWrite.Add(Fov.GetSaveStr());
            linesToWrite.Add(AntiAliasing.GetSaveStr());
            linesToWrite.Add(RenderScale.GetSaveStr());
            linesToWrite.Add(PositionSmooth.GetSaveStr());
            linesToWrite.Add(RotationSmooth.GetSaveStr());
            linesToWrite.Add(Cam360Smoothness.GetSaveStr());
            linesToWrite.Add(Cam360RotateControlNew.GetSaveStr());
            linesToWrite.Add(ThirdPerson.GetSaveStr());
            linesToWrite.Add(ShowThirdPersonCamera.GetSaveStr());
            linesToWrite.Add(Use360Camera.GetSaveStr());
            linesToWrite.Add(PosX.GetSaveStr());
            linesToWrite.Add(PosY.GetSaveStr());
            linesToWrite.Add(PosZ.GetSaveStr());
            linesToWrite.Add(AngX.GetSaveStr());
            linesToWrite.Add(AngY.GetSaveStr());
            linesToWrite.Add(AngZ.GetSaveStr());
            linesToWrite.Add(FirstPersonPosOffsetX.GetSaveStr());
            linesToWrite.Add(FirstPersonPosOffsetY.GetSaveStr());
            linesToWrite.Add(FirstPersonPosOffsetZ.GetSaveStr());
            linesToWrite.Add(FirstPersonRotOffsetX.GetSaveStr());
            linesToWrite.Add(FirstPersonRotOffsetY.GetSaveStr());
            linesToWrite.Add(FirstPersonRotOffsetZ.GetSaveStr());
            linesToWrite.Add(Cam360ForwardOffset.GetSaveStr());
            linesToWrite.Add(Cam360XTilt.GetSaveStr());
            linesToWrite.Add(Cam360ZTilt.GetSaveStr());
            linesToWrite.Add(Cam360YTilt.GetSaveStr());
            linesToWrite.Add(Cam360UpOffset.GetSaveStr());
            linesToWrite.Add(Cam360RightOffset.GetSaveStr());
            linesToWrite.Add(ScreenWidth.GetSaveStr());
            linesToWrite.Add(ScreenHeight.GetSaveStr());
            linesToWrite.Add(ScreenPosX.GetSaveStr());
            linesToWrite.Add(ScreenPosY.GetSaveStr());
            linesToWrite.Add(MultiPlayerNumber.GetSaveStr());
            linesToWrite.Add(DisplayMultiPlayerNameInfo.GetSaveStr());
            linesToWrite.Add(Layer.GetSaveStr());
            linesToWrite.Add(FitToCanvas.GetSaveStr());
            linesToWrite.Add(TransparentWalls.GetSaveStr());
            linesToWrite.Add(ForceFirstPersonUpRight.GetSaveStr());
            linesToWrite.Add(Avatar.GetSaveStr());
            linesToWrite.Add(Debris.GetSaveStr());
            linesToWrite.Add(HideUi.GetSaveStr());
            linesToWrite.Add(MovementScriptPath.GetSaveStr());
            linesToWrite.Add(MovementAudioSync.GetSaveStr());

            Factory.WaitUntilFileIsNotLocked(_filePath);
            File.WriteAllText(_filePath, string.Join(Environment.NewLine, linesToWrite));
        }

        public void RestoreFromBackup()
        {
            if (_backup == null)
            {
                return;
            }

            LockScreen.Value = _backup.LockScreen.Value;
            Fov.Value = _backup.Fov.Value;
            AntiAliasing.Value = _backup.AntiAliasing.Value;
            RenderScale.Value = _backup.RenderScale.Value;
            PositionSmooth.Value = _backup.PositionSmooth.Value;
            RotationSmooth.Value = _backup.RotationSmooth.Value;
            Cam360Smoothness.Value = _backup.Cam360Smoothness.Value;
            Cam360RotateControlNew.Value = _backup.Cam360RotateControlNew.Value;
            ThirdPerson.Value = _backup.ThirdPerson.Value;
            ShowThirdPersonCamera.Value = _backup.ShowThirdPersonCamera.Value;
            Use360Camera.Value = _backup.Use360Camera.Value;
            PosX.Value = _backup.PosX.Value;
            PosY.Value = _backup.PosY.Value;
            PosZ.Value = _backup.PosZ.Value;
            AngX.Value = _backup.AngX.Value;
            AngY.Value = _backup.AngY.Value;
            AngZ.Value = _backup.AngZ.Value;
            FirstPersonPosOffsetX.Value = _backup.FirstPersonPosOffsetX.Value;
            FirstPersonPosOffsetY.Value = _backup.FirstPersonPosOffsetY.Value;
            FirstPersonPosOffsetZ.Value = _backup.FirstPersonPosOffsetZ.Value;
            FirstPersonRotOffsetX.Value = _backup.FirstPersonRotOffsetX.Value;
            FirstPersonRotOffsetY.Value = _backup.FirstPersonRotOffsetY.Value;
            FirstPersonRotOffsetZ.Value = _backup.FirstPersonRotOffsetZ.Value;
            Cam360ForwardOffset.Value = _backup.Cam360ForwardOffset.Value;
            Cam360XTilt.Value = _backup.Cam360XTilt.Value;
            Cam360ZTilt.Value = _backup.Cam360ZTilt.Value;
            Cam360YTilt.Value = _backup.Cam360YTilt.Value;
            Cam360UpOffset.Value = _backup.Cam360UpOffset.Value;
            Cam360RightOffset.Value = _backup.Cam360RightOffset.Value;
            ScreenWidth.Value = _backup.ScreenWidth.Value;
            ScreenHeight.Value = _backup.ScreenHeight.Value;
            ScreenPosX.Value = _backup.ScreenPosX.Value;
            ScreenPosY.Value = _backup.ScreenPosY.Value;
            MultiPlayerNumber.Value = _backup.MultiPlayerNumber.Value;
            DisplayMultiPlayerNameInfo.Value = _backup.DisplayMultiPlayerNameInfo.Value;
            Layer.Value = _backup.Layer.Value;
            FitToCanvas.Value = _backup.FitToCanvas.Value;
            TransparentWalls.Value = _backup.TransparentWalls.Value;
            ForceFirstPersonUpRight.Value = _backup.ForceFirstPersonUpRight.Value;
            Avatar.Value = _backup.Avatar.Value;
            Debris.Value = _backup.Debris.Value;
            HideUi.Value = _backup.HideUi.Value;
            MovementScriptPath.Value = _backup.MovementScriptPath.Value;
            MovementAudioSync.Value = _backup.MovementAudioSync.Value;

            Changed = true;
        }

        private static bool ToBoolValue(string value)
        {
            return value.ToLower() == "true";
        }
    }
}