using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;

namespace Mover
{
    public class Factory
    {
        public const string CommandBack = "Back";
        public const string CommandFront = "Front";
        public const string CommandRestore = "Restore";
        public const string CommandFirstPerson = "FirstPerson";
        public const string CommandFirstPersonSmallCams = "FirstPersonSmallCams";
        public const string CommandToggle360 = "Toggle360";

        private const string DirectoryName = "commands";
        private readonly List<CameraPlusConfig> _cameras = new List<CameraPlusConfig>();
        private readonly ILogger _logger;
        
        private readonly Size _screenBig = new Size(650, 340);
        private readonly Size _screenSmall = new Size(510, 300);
        
        private FileSystemWatcher _watcher;
        private string _beatSaberDirectory;
        private bool _toggle360State = false;

        public bool IsLoaded { get; private set; }

        public Factory(ILogger logger)
        {
            _logger = logger;

            _logger.Log("Initializing...");

            // create command directory if not exists
            if (!Directory.Exists(DirectoryName))
            {
                Directory.CreateDirectory(DirectoryName);
            }

            // delete old - non triggered commands if any
            foreach (var file in Directory.GetFiles(DirectoryName))
            {
                File.Delete(file);
            }
            
            //SetBeatSaberPath(File.ReadAllText("beatsaberpath.txt"));
        }

        public void SetBeatSaberPath(string path)
        {
            _beatSaberDirectory = Path.GetFullPath(path);
            LoadData();
        }

        public void Destroy()
        {
            if (!_cameras.Any())
            {
                return;
            }
            
            _logger.Log("destroying...");
            foreach (var config in _cameras)
            {
                config.Destroy();
            }

            if (_watcher != null)
            {
                _watcher.Created -= FileCreatedEvent;   
            }
            
            _logger.Log("done");
        }

        private bool AreCamsIn360(CameraPlusConfig front = null, CameraPlusConfig back = null)
        {
            if (!IsLoaded)
            {
                return false;
            }
            
            if (front == null)
            {
                front = GetFromView(View.Front);
            }

            if (back == null)
            {
                back = GetFromView(View.Back);
            }

            return front.Use360Camera.Value && back.Use360Camera.Value;
        }
        
        private void LoadData()
        {
            try
            {
                _cameras.Clear();
                Destroy();
                
                // read directory file and check if everything is right
                _logger.Log("Checking Beat Saber Data...");
                _logger.Log(_beatSaberDirectory);
                if (!Directory.Exists(_beatSaberDirectory))
                {
                    throw new DirectoryNotFoundException(_beatSaberDirectory);
                }

                _logger.Log($"Beat Saber is installed to: {_beatSaberDirectory}");

                var beatSaberDirectory = _beatSaberDirectory + "/UserData/CameraPlus/";
                if (!Directory.Exists(beatSaberDirectory))
                {
                    throw new DirectoryNotFoundException(beatSaberDirectory);
                }

                _logger.Log($"found camera path: {beatSaberDirectory}");

                // load cameras
                _logger.Log("Loading Cameras...");
                _cameras.Add(CameraPlusConfig.FromFile(beatSaberDirectory + "cameraplus.cfg", View.Back, _logger));
                _cameras.Add(CameraPlusConfig.FromFile(beatSaberDirectory + "customcamera1.cfg", View.FirstPerson, _logger));
                _cameras.Add(CameraPlusConfig.FromFile(beatSaberDirectory + "customcamera2.cfg", View.Front, _logger));

                _logger.Log($"loaded {_cameras.Count} cameras");

                // create a watcher
                _watcher = new FileSystemWatcher(DirectoryName)
                {
                    IncludeSubdirectories = false,
                    EnableRaisingEvents = true,
                    //NotifyFilter = NotifyFilters.LastWrite
                };
                _watcher.Created += FileCreatedEvent;
                _watcher.Error += (sender, args) =>
                {
                    _logger.Log("could not watch directory!");
                };

                _logger.Log("done, go ahead :3");
                _logger.Log("-----------------");

                IsLoaded = true;
                File.WriteAllText("beatsaberpath.txt", _beatSaberDirectory);

                _toggle360State = AreCamsIn360();
            }
            catch (Exception e)
            {
                IsLoaded = false;
                _logger.LogException(e);
                _logger.Log("disabled mover...");
            }
        }

        private void FileCreatedEvent(object sender, FileSystemEventArgs args)
        {
            WaitUntilFileIsNotLocked(args.FullPath);
            
            try
            {
                var command = File.ReadAllText(args.FullPath);
                ParseCommands(command.Trim('\n','\r'));
                File.Delete(args.FullPath);
            }
            catch (IOException)
            {
                _logger.Log($"cant access {args.Name} yet.. waiting for 200 msecs");
                Thread.Sleep(2000);
                FileCreatedEvent(sender, args);
            }
        }

        public static void WaitUntilFileIsNotLocked(string filePath)
        {
            // can we access it? if not wait a few m sec
            while (IsFileLocked(filePath))
            {
                Thread.Sleep(200);
            }
        }

        public static bool IsFileLocked(string filePath)
        {
            var file = new FileInfo(filePath);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
            }

            return false;
        }

        public void ParseCommands(string commandString)
        {
            if (!IsLoaded)
            {
                _logger.Log($"NOT LOADED! ignoring command {commandString}");
                return;
            }
            
            _logger.Log($"received command: {commandString}");

            var parsing = commandString.Split(' ');
            if (parsing.Length < 1)
            {
                _logger.Log("no command given :(");
                return;
            }

            RestoreAllCams();
            
            var command = parsing[0].Trim();
            _logger.Log($"invoking \"{command}\"");

            switch (command)
            {
                case CommandBack:
                case CommandRestore:
                    // ignore, its just restore and saving but must be here so it wont get trapped in default case
                    break;
                
                case CommandFirstPerson:
                    CmdFirstPerson(false);
                    break;

                case CommandFirstPersonSmallCams:
                    CmdFirstPerson(true);
                    break;

                case CommandFront:
                    CmdFront();
                    break;

                case CommandToggle360:
                    CmdToggle360();
                    break;

                default:
                    _logger.Log($"unrecognised command: {command}");
                    return;
            }
            /*
            GetFromView(View.FirstPerson).Avatar.Value = false;
            GetFromView(View.Back).Avatar.Value = true;
            GetFromView(View.Front).Avatar.Value = true;
            */
            SaveAllCams();
        }
        
        public void RestoreAllCams()
        {
            foreach (var camera in _cameras)
            {
                camera.RestoreFromBackup();
            }
            
            SetCamDimensions(GetFromView(View.FirstPerson), GetFromView(View.Front), _screenBig);
        }

        public void CmdFirstPerson(bool smallCams)
        {
            Toggle(GetFromView(View.Back), GetFromView(View.FirstPerson));
            if (smallCams)
            {
                SetCamDimensions(GetFromView(View.FirstPerson), GetFromView(View.Front), _screenSmall);
            }
        }

        public void CmdFront()
        {
            Toggle(GetFromView(View.Back), GetFromView(View.Front));
        }

        public void CmdToggle360()
        {
            _toggle360State = !_toggle360State;
            var back = GetFromView(View.Back);
            var front = GetFromView(View.Front);
            var state = _toggle360State;

            front.Use360Camera.Value = state;
            back.Use360Camera.Value = state;

            front.Changed = true;
            back.Changed = true;
        }

        public void SaveAllCams()
        {
            _logger.Log("saving cams");
            foreach (var camera in _cameras)
            {
                camera.Save();
            }
        }

        private static void SetCamDimensions(CameraPlusConfig config1, CameraPlusConfig config2, Size toSize)
        {
            config1.ScreenPosX.Value = 0;
            config1.ScreenPosY.Value = 1080 - toSize.Height;
            config1.ScreenWidth.Value = toSize.Width;
            config1.ScreenHeight.Value = toSize.Height;

            config2.ScreenPosX.Value = 1920 - toSize.Width;
            config2.ScreenPosY.Value = config1.ScreenPosY;
            config2.ScreenWidth.Value = toSize.Width;
            config2.ScreenHeight.Value = toSize.Height;

            config1.Changed = true;
            config2.Changed = true;
        }

        private static void ToggleProperty<T>(ConfigProperty<T> from, ConfigProperty<T> to)
        {
            T tmp = from.Value;
            from.Value = to.Value;
            to.Value = tmp;
        }

        private static void Toggle(CameraPlusConfig fromCam, CameraPlusConfig toCam)
        {
            // swap settings
            ToggleProperty(fromCam.Fov, toCam.Fov);
            ToggleProperty(fromCam.PositionSmooth, toCam.PositionSmooth);
            ToggleProperty(fromCam.RotationSmooth, toCam.RotationSmooth);
            ToggleProperty(fromCam.Cam360Smoothness, toCam.Cam360Smoothness);
            ToggleProperty(fromCam.Cam360RotateControlNew, toCam.Cam360RotateControlNew);
            ToggleProperty(fromCam.ThirdPerson, toCam.ThirdPerson);
            ToggleProperty(fromCam.ShowThirdPersonCamera, toCam.ShowThirdPersonCamera);
            ToggleProperty(fromCam.Use360Camera, toCam.Use360Camera);
            ToggleProperty(fromCam.PosX, toCam.PosX);
            ToggleProperty(fromCam.PosY, toCam.PosY);
            ToggleProperty(fromCam.PosZ, toCam.PosZ);
            ToggleProperty(fromCam.AngX, toCam.AngX);
            ToggleProperty(fromCam.AngY, toCam.AngY);
            ToggleProperty(fromCam.AngZ, toCam.AngZ);
            ToggleProperty(fromCam.FirstPersonPosOffsetX, toCam.FirstPersonPosOffsetX);
            ToggleProperty(fromCam.FirstPersonPosOffsetY, toCam.FirstPersonPosOffsetY);
            ToggleProperty(fromCam.FirstPersonPosOffsetZ, toCam.FirstPersonPosOffsetZ);
            ToggleProperty(fromCam.FirstPersonRotOffsetX, toCam.FirstPersonRotOffsetX);
            ToggleProperty(fromCam.FirstPersonRotOffsetY, toCam.FirstPersonRotOffsetY);
            ToggleProperty(fromCam.FirstPersonRotOffsetZ, toCam.FirstPersonRotOffsetZ);
            ToggleProperty(fromCam.Cam360ForwardOffset, toCam.Cam360ForwardOffset);
            ToggleProperty(fromCam.Cam360XTilt, toCam.Cam360XTilt);
            ToggleProperty(fromCam.Cam360ZTilt, toCam.Cam360ZTilt);
            ToggleProperty(fromCam.Cam360YTilt, toCam.Cam360YTilt);
            ToggleProperty(fromCam.Cam360UpOffset, toCam.Cam360UpOffset);
            ToggleProperty(fromCam.Cam360RightOffset, toCam.Cam360RightOffset);
            ToggleProperty(fromCam.MultiPlayerNumber, toCam.MultiPlayerNumber);
            ToggleProperty(fromCam.DisplayMultiPlayerNameInfo, toCam.DisplayMultiPlayerNameInfo);
            ToggleProperty(fromCam.TransparentWalls, toCam.TransparentWalls);
            ToggleProperty(fromCam.Avatar, toCam.Avatar);
            ToggleProperty(fromCam.Debris, toCam.Debris);
            ToggleProperty(fromCam.HideUi, toCam.HideUi);

            fromCam.Changed = true;
            toCam.Changed = true;
        }

        private CameraPlusConfig GetFromView(View view) => _cameras.First(cam => cam.View == view);

        public bool Cam360State() => _toggle360State;
    }
}