using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Mover
{
    public class Factory
    {
        public const string DirectoryName = "commands";

        public const string CommandBack = "Back";
        public const string CommandFront = "Front";
        public const string CommandRestore = "Restore";
        public const string CommandFirstPerson = "FirstPerson";
        public const string CommandFirstPersonSmallCams = "FirstPersonSmallCams";
        public const string CommandToggle360 = "Toggle360";

        private FileSystemWatcher _watcher;
        private List<CameraPlusConfig> _cameras = new List<CameraPlusConfig>();
        private ILogger _logger;

        private string _beatSaberDirectory;

        public bool IsLoaded { get; private set; }
        public Action Callback360Toggle;

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
            _beatSaberDirectory = path;
            LoadData();
        }

        public string GetBeatSaberPath() => _beatSaberDirectory;

        public void Destroy()
        {
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

        public bool AreCamsIn360(CameraPlusConfig front = null, CameraPlusConfig back = null)
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

            return front.Use360Camera && back.Use360Camera;
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

                _logger.Log($"camera path: {beatSaberDirectory}");

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
            // can we access it? if not wait a few m sec
            while (IsFileLocked(args.FullPath))
            {
                Thread.Sleep(200);
            }

            try
            {
                var command = File.ReadAllText(args.FullPath);
                ParseCommands(command);
                File.Delete(args.FullPath);
            }
            catch (IOException)
            {
                _logger.Log($"cant access {args.Name} yet.. waiting for 200 msecs");
                Thread.Sleep(2000);
                FileCreatedEvent(sender, args);
            }
        }

        private bool IsFileLocked(string filePath)
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

        private void ParseCommands(string commandString)
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
            
            var back = GetFromView(View.Back);
            var front = GetFromView(View.Front);
            var fp = GetFromView(View.FirstPerson);
            
            switch (command)
            {
                case CommandFirstPerson:
                    Toggle(back, fp);
                    break;

                case CommandFirstPersonSmallCams:
                    Toggle(back, fp);
                    SetCamDimensions(GetFromView(View.FirstPerson), GetFromView(View.Front));
                    break;

                case CommandFront:
                    Toggle(back, front);
                    break;

                case CommandBack:
                case CommandRestore:
                    //RestoreAllCams();
                    break;

                case CommandToggle360:
                    var state = !AreCamsIn360(front, back);

                    front.Use360Camera = state;
                    back.Use360Camera = state;

                    front.Changed = true;
                    back.Changed = true;
                    break;

                default:
                    _logger.Log($"unrecognised command: {command}");
                    return;
            }

            SaveAllCams();

            if (command == CommandToggle360)
            {
                Callback360Toggle?.Invoke();
            }
        }

        private void SetCamDimensions(CameraPlusConfig config1, CameraPlusConfig config2)
        {
            const int width = 510;
            const int height = 300;

            config1.ScreenPosX = 0;
            config1.ScreenPosY = 1080 - height;
            config1.ScreenWidth = width;
            config1.ScreenHeight = height;

            config2.ScreenPosX = 1920 - width;
            config2.ScreenPosY = config1.ScreenPosY;
            config2.ScreenWidth = width;
            config2.ScreenHeight = height;

            config1.Changed = true;
            config2.Changed = true;
        }
        private void RestoreAllCams()
        {
            foreach (var camera in _cameras)
            {
                camera.RestoreFromBackup();
                camera.Changed = false;
            }
        }

        private void SaveAllCams()
        {
            _logger.Log("saving cams");
            foreach (var camera in _cameras)
            {
                camera.Save();
            }
        }

        private void Toggle(CameraPlusConfig fromCam, CameraPlusConfig toCam)
        {
            /*
            if (fromCam.Use360Camera != toCam.Use360Camera)
            {
                return;
            }
            */


            var tmpTransparentWalls = fromCam.TransparentWalls;
            fromCam.TransparentWalls = toCam.TransparentWalls;
            toCam.TransparentWalls = tmpTransparentWalls;

            /*
            if (fromCam.Use360Camera)
            {
            */
            // swap 360 settings
            var tmpCam360Smoothness = fromCam.Cam360Smoothness;
            var tmpUse360Camera = fromCam.Use360Camera;
            var tmpCam360ForwardOffset = fromCam.Cam360ForwardOffset;
            var tmpCam360XTilt = fromCam.Cam360XTilt;
            var tmpCam360ZTilt = fromCam.Cam360ZTilt;
            var tmpCam360YTilt = fromCam.Cam360YTilt;
            var tmpCam360UpOffset = fromCam.Cam360UpOffset;
            var tmpCam360RightOffset = fromCam.Cam360RightOffset;

            fromCam.Cam360Smoothness = toCam.Cam360Smoothness;
            fromCam.Use360Camera = toCam.Use360Camera;
            fromCam.Cam360ForwardOffset = toCam.Cam360ForwardOffset;
            fromCam.Cam360XTilt = toCam.Cam360XTilt;
            fromCam.Cam360ZTilt = toCam.Cam360ZTilt;
            fromCam.Cam360YTilt = toCam.Cam360YTilt;
            fromCam.Cam360UpOffset = toCam.Cam360UpOffset;
            fromCam.Cam360RightOffset = toCam.Cam360RightOffset;

            toCam.Cam360Smoothness = tmpCam360Smoothness;
            toCam.Use360Camera = tmpUse360Camera;
            toCam.Cam360ForwardOffset = tmpCam360ForwardOffset;
            toCam.Cam360XTilt = tmpCam360XTilt;
            toCam.Cam360ZTilt = tmpCam360ZTilt;
            toCam.Cam360YTilt = tmpCam360YTilt;
            toCam.Cam360UpOffset = tmpCam360UpOffset;
            toCam.Cam360RightOffset = tmpCam360RightOffset;
            /*
        }
        else
        {
        */
            // swap global position
            var tmpPositionSmooth = fromCam.PositionSmooth;
            var tmpRotationSmooth = fromCam.RotationSmooth;
            var tmpThirdPerson = fromCam.ThirdPerson;
            var tmpShowThirdPersonCamera = fromCam.ShowThirdPersonCamera;
            var tmpPosX = fromCam.PosX;
            var tmpPosY = fromCam.PosY;
            var tmpPosZ = fromCam.PosZ;
            var tmpAngX = fromCam.AngX;
            var tmpAngY = fromCam.AngY;
            var tmpAngZ = fromCam.AngZ;

            fromCam.PositionSmooth = toCam.PositionSmooth;
            fromCam.RotationSmooth = toCam.RotationSmooth;
            fromCam.ThirdPerson = toCam.ThirdPerson;
            fromCam.ShowThirdPersonCamera = toCam.ShowThirdPersonCamera;
            fromCam.PosX = toCam.PosX;
            fromCam.PosY = toCam.PosY;
            fromCam.PosZ = toCam.PosZ;
            fromCam.AngX = toCam.AngX;
            fromCam.AngY = toCam.AngY;
            fromCam.AngZ = toCam.AngZ;

            toCam.PositionSmooth = tmpPositionSmooth;
            toCam.RotationSmooth = tmpRotationSmooth;
            toCam.ThirdPerson = tmpThirdPerson;
            toCam.ShowThirdPersonCamera = tmpShowThirdPersonCamera;
            toCam.PosX = tmpPosX;
            toCam.PosY = tmpPosY;
            toCam.PosZ = tmpPosZ;
            toCam.AngX = tmpAngX;
            toCam.AngY = tmpAngY;
            toCam.AngZ = tmpAngZ;
            //}

            fromCam.Changed = true;
            toCam.Changed = true;
        }

        private CameraPlusConfig GetFromView(View view) => _cameras.First(cam => cam.View == view);
    }
}