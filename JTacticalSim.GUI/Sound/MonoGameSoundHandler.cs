using JTacticalSim.API;
using JTacticalSim.API.Component;
using JTacticalSim.API.Media.Sound;
using JTacticalSim.Utility;
using Microsoft.Xna.Framework.Audio;

namespace JTacticalSim.GUI.Sound;

internal sealed class MonoGameSoundHandler : ISoundHandler
{
    public event EventHandler FileLoaded;
    public event EventHandler PlayFinished;

    private SoundEffect _currentEffect;
    private SoundEffectInstance _currentInstance;

    public IResult<FileStream, FileStream> GetSound(string name)
    {
        var r = new OperationResult<FileStream, FileStream>();

        try
        {
            var mediaRoot = ConfigurationManager.AppSettings["mediafilepathDefault"]
                .Replace('\\', Path.DirectorySeparatorChar);
            if (!Path.IsPathRooted(mediaRoot))
                mediaRoot = Path.Combine(AppContext.BaseDirectory, mediaRoot);

            var fileDir = Path.Combine(mediaRoot,
                Game.Instance.LoadedScenario.ComponentSet.Name,
                ConfigurationManager.AppSettings["soundsourcetype"]);

            var fs = new FileStream(Path.Combine(fileDir, name + ".wav"), FileMode.Open, FileAccess.Read);

            r.Result = fs;
            r.SuccessfulObjects.Add(fs);
            r.Status = ResultStatus.SUCCESS;
        }
        catch (Exception ex)
        {
            r.Status = ResultStatus.EXCEPTION;
            r.Messages.Add("Could not load sound file '{0}'.".F(name));
            r.ex = ex;
        }

        return r;
    }

    public void PlaySoundAsync(FileStream fs)
    {
        PlayInternal(fs, loop: false);
        // MonoGame audio is inherently non-blocking; fire event immediately
        PlayFinished?.Invoke(this, EventArgs.Empty);
    }

    public void PlaySound(FileStream fs)
    {
        PlayInternal(fs, loop: false);

        // Poll until playback ends for the synchronous overload
        while (_currentInstance?.State == SoundState.Playing)
            Thread.Sleep(10);

        PlayFinished?.Invoke(this, EventArgs.Empty);
    }

    public void StopPlayback()
    {
        StopCurrent();
        PlayFinished?.Invoke(this, EventArgs.Empty);
    }

    public void StopPlaybackAsync()
    {
        StopCurrent();
        PlayFinished?.Invoke(this, EventArgs.Empty);
    }

    private void PlayInternal(FileStream fs, bool loop = false)
    {
        StopCurrent();

        try
        {
            _currentEffect = SoundEffect.FromStream(fs);
            fs.Dispose();
            FileLoaded?.Invoke(this, EventArgs.Empty);
            _currentInstance = _currentEffect.CreateInstance();
            _currentInstance.IsLooped = loop;
            _currentInstance.Play();
        }
        catch (Exception)
        {
            // Silently ignore audio failures — game must not crash on sound errors
            fs?.Dispose();
        }
    }

    private void StopCurrent()
    {
        _currentInstance?.Stop();
        _currentInstance?.Dispose();
        _currentInstance = null;
        _currentEffect?.Dispose();
        _currentEffect = null;
    }
}
