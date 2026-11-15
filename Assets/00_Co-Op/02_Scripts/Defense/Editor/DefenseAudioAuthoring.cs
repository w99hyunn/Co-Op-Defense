using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OpenFrame.Editor
{
    public static class DefenseAudioAuthoring
    {
        public static void Install()
        {
            var view = Object.FindAnyObjectByType<DefenseView>();
            if (!view.TryGetComponent<AudioSource>(out var source)) source = view.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.volume = 0.35f;
            view.feedbackSource = source;
            view.placementSound = Tone("Place", 0.09f, 520, false);
            view.attackSound = Tone("Shot", 0.065f, 230, false);
            view.clearSound = Tone("Clear", 0.44f, 440, true);
            EditorUtility.SetDirty(view);
            EditorSceneManager.MarkSceneDirty(view.gameObject.scene);
            EditorSceneManager.SaveScene(view.gameObject.scene);
        }

        private static AudioClip Tone(string name, float duration, float frequency, bool chord)
        {
            string folder = "Assets/00_Co-Op/08_Audio";
            Directory.CreateDirectory(folder);
            string path = folder + "/Defense" + name + ".wav";
            int sampleRate = 22050;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + samples * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
                writer.Write((short)1); writer.Write((short)1); writer.Write(sampleRate);
                writer.Write(sampleRate * 2); writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(samples * 2);
                for (int i = 0; i < samples; i++)
                {
                    float t = i / (float)sampleRate;
                    float note = chord ? (t < 0.13f ? 1 : t < 0.26f ? 1.25f : 1.5f) : 1;
                    float signal = Mathf.Sin(2 * Mathf.PI * frequency * note * t);
                    float envelope = Mathf.Min(1, t / 0.005f) * Mathf.Pow(1 - t / duration, 1.6f);
                    writer.Write((short)(signal * envelope * 16000));
                }
            }
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
    }
}
