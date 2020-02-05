using System;
using System.Collections.Generic;
using System.IO;

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

        public float Fov { get; set; } = 90;
        public int AntiAliasing { get; set; } = 2;
        public float RenderScale { get; set; } = 1;
        public float PositionSmooth { get; set; } = 10;
        public float RotationSmooth { get; set; } = 5;
        public float Cam360Smoothness { get; set; } = 2;
        public bool ThirdPerson { get; set; } = false;
        public bool ShowThirdPersonCamera { get; set; } = true;
        public bool Use360Camera { get; set; } = false;
        public float PosX { get; set; }
        public float PosY { get; set; } = 2;
        public float PosZ { get; set; } = -1.2f;
        public float AngX { get; set; } = 15;
        public float AngY { get; set; }
        public float AngZ { get; set; }
        public float FirstPersonPosOffsetX { get; set; }
        public float FirstPersonPosOffsetY { get; set; }
        public float FirstPersonPosOffsetZ { get; set; }
        public float FirstPersonRotOffsetX { get; set; }
        public float FirstPersonRotOffsetY { get; set; }
        public float FirstPersonRotOffsetZ { get; set; }
        public float Cam360ForwardOffset { get; set; } = -2;
        public float Cam360XTilt { get; set; } = 10;
        public float Cam360ZTilt { get; set; }
        public float Cam360YTilt { get; set; }
        public float Cam360UpOffset { get; set; } = 2.2f;
        public float Cam360RightOffset { get; set; }
        public int ScreenWidth { get; set; } = 1920;
        public int ScreenHeight { get; set; } = 1080;
        public int ScreenPosX { get; set; }
        public int ScreenPosY { get; set; }
        public int Layer { get; set; } = -1000;
        public bool FitToCanvas { get; set; } = false;
        public bool TransparentWalls { get; set; } = false;
        public bool ForceFirstPersonUpRight { get; set; } = false;
        public string MovementScriptPath { get; set; } = string.Empty;

        public static CameraPlusConfig FromFile(string filePath, View view, ILogger logger)
        {
            return new CameraPlusConfig(filePath, view, logger);
        }

        private CameraPlusConfig(string filePath, View view, ILogger logger, bool createRestoreBackup = true)
        {
            _filePath = filePath;
            View = view;
            _logger = logger;
            Changed = false;
            
            LoadFile(createRestoreBackup);
        }

        private void LoadFile(bool createRestoreBackup = true) {
            foreach (var line in File.ReadAllLines(_filePath))
            {
                var split = line.Split('=');
                var value = split[1];
                switch (split[0])
                {
                    case "fov":
                        Fov = float.Parse(value);
                        break;
                    case "antiAliasing":
                        AntiAliasing = int.Parse(value);
                        break;
                    case "renderScale":
                        RenderScale = float.Parse(value);
                        break;
                    case "positionSmooth":
                        PositionSmooth = float.Parse(value);
                        break;
                    case "rotationSmooth":
                        RotationSmooth = float.Parse(value);
                        break;
                    case "cam360Smoothness":
                        Cam360Smoothness = float.Parse(value);
                        break;
                    case "thirdPerson":
                        ThirdPerson = value.ToLower() == "true";
                        break;
                    case "showThirdPersonCamera":
                        ShowThirdPersonCamera = value.ToLower() == "true";
                        break;
                    case "use360Camera":
                        Use360Camera = value.ToLower() == "true";
                        break;
                    case "posx":
                        PosX = float.Parse(value);
                        break;
                    case "posy":
                        PosY = float.Parse(value);
                        break;
                    case "posz":
                        PosZ = float.Parse(value);
                        break;
                    case "angx":
                        AngX = float.Parse(value);
                        break;
                    case "angy":
                        AngY = float.Parse(value);
                        break;
                    case "angz":
                        AngZ = float.Parse(value);
                        break;
                    case "firstPersonPosOffsetX":
                        FirstPersonPosOffsetX = float.Parse(value);
                        break;
                    case "firstPersonPosOffsetY":
                        FirstPersonPosOffsetY = float.Parse(value);
                        break;
                    case "firstPersonPosOffsetZ":
                        FirstPersonPosOffsetZ = float.Parse(value);
                        break;
                    case "firstPersonRotOffsetX":
                        FirstPersonRotOffsetX = float.Parse(value);
                        break;
                    case "firstPersonRotOffsetY":
                        FirstPersonRotOffsetY = float.Parse(value);
                        break;
                    case "firstPersonRotOffsetZ":
                        FirstPersonRotOffsetZ = float.Parse(value);
                        break;
                    case "cam360ForwardOffset":
                        Cam360ForwardOffset = float.Parse(value);
                        break;
                    case "cam360XTilt":
                        Cam360XTilt = float.Parse(value);
                        break;
                    case "cam360YTilt":
                        Cam360YTilt = float.Parse(value);
                        break;
                    case "cam360ZTilt":
                        Cam360ZTilt = float.Parse(value);
                        break;
                    case "cam360UpOffset":
                        Cam360UpOffset = float.Parse(value);
                        break;
                    case "cam360RightOffset":
                        Cam360RightOffset = float.Parse(value);
                        break;
                    case "screenWidth":
                        ScreenWidth = int.Parse(value);
                        break;
                    case "screenHeight":
                        ScreenHeight = int.Parse(value);
                        break;
                    case "screenPosX":
                        ScreenPosX = int.Parse(value);
                        break;
                    case "screenPosY":
                        ScreenPosY = int.Parse(value);
                        break;
                    case "layer":
                        Layer = int.Parse(value);
                        break;
                    case "fitToCanvas":
                        FitToCanvas = value.ToLower() == "true";
                        break;
                    case "transparentWalls":
                        TransparentWalls = value.ToLower() == "true";
                        break;
                    case "forceFirstPersonUpRight":
                        ForceFirstPersonUpRight = value.ToLower() == "true";
                        break;
                    case "movementScriptPath":
                        MovementScriptPath = value;
                        break;
                }
            }

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
            _logger.Log($"loaded {createRestoreBackupString}{Path.GetFileName(_filePath)}");
        }

        public void Destroy()
        {
            _watcher.Changed -= FileChangeEvent;
        }

        private void FileChangeEvent(object sender, FileSystemEventArgs args)
        {
            //var conf = _cameras.First(x => x.FilePath == args.FullPath);
            //_logger.Log($"{conf.View:g} config file written!");
            _logger.Log($"{args.Name} config written");
            LoadFile(false);
        }

        public void Save()
        {
            if (!Changed)
            {
                return;
            }
            
            var linesToWrite = new List<string>
            {
                "fov=" + Fov,
                "antiAliasing=" + AntiAliasing,
                "renderScale=" + RenderScale,
                "positionSmooth=" + PositionSmooth,
                "rotationSmooth=" + RotationSmooth,
                "cam360Smoothness=" + Cam360Smoothness,
                "thirdPerson=" + ToBool(ThirdPerson),
                "showThirdPersonCamera=" + ToBool(ShowThirdPersonCamera),
                "use360Camera=" + ToBool(Use360Camera),
                "posx=" + PosX,
                "posy=" + PosY,
                "posz=" + PosZ,
                "angx=" + AngX,
                "angy=" + AngY,
                "angz=" + AngZ,
                "firstPersonPosOffsetX=" + FirstPersonPosOffsetX,
                "firstPersonPosOffsetY=" + FirstPersonPosOffsetY,
                "firstPersonPosOffsetZ=" + FirstPersonPosOffsetZ,
                "firstPersonRotOffsetX=" + FirstPersonRotOffsetX,
                "firstPersonRotOffsetY=" + FirstPersonRotOffsetY,
                "firstPersonRotOffsetZ=" + FirstPersonRotOffsetZ,
                "cam360ForwardOffset=" + Cam360ForwardOffset,
                "cam360XTilt=" + Cam360XTilt,
                "cam360ZTilt=" + Cam360ZTilt,
                "cam360YTilt=" + Cam360YTilt,
                "cam360UpOffset=" + Cam360UpOffset,
                "cam360RightOffset=" + Cam360RightOffset,
                "screenWidth=" + ScreenWidth,
                "screenHeight=" + ScreenHeight,
                "screenPosX=" + ScreenPosX,
                "screenPosY=" + ScreenPosY,
                "layer=" + Layer,
                "fitToCanvas=" + ToBool(FitToCanvas),
                "transparentWalls=" + ToBool(TransparentWalls),
                "forceFirstPersonUpRight=" + ToBool(ForceFirstPersonUpRight),
                "movementScriptPath=" + MovementScriptPath,
            };

            File.WriteAllText(_filePath, string.Join(Environment.NewLine, linesToWrite));
        }

        public void RestoreFromBackup()
        {
            if (_backup == null)
            {
                return;
            }

            Fov = _backup.Fov;
            AntiAliasing = _backup.AntiAliasing;
            RenderScale = _backup.RenderScale;
            PositionSmooth = _backup.PositionSmooth;
            RotationSmooth = _backup.RotationSmooth;
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
            Layer = _backup.Layer;
            FitToCanvas = _backup.FitToCanvas;
            TransparentWalls = _backup.TransparentWalls;
            ForceFirstPersonUpRight = _backup.ForceFirstPersonUpRight;
            MovementScriptPath = _backup.MovementScriptPath;
        }

        private string ToBool(bool input) => input ? "True" : "False";
    }
}