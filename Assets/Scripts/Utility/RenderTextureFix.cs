using System.Collections.Generic;
using UnityEngine;

public class RenderTextureFix : MonoBehaviour
{
    private static List<RenderTexture> trackedTextures = new List<RenderTexture>();

    /// <summary>
    /// Custom wrapper for RenderTexture.GetTemporary to track usage.
    /// </summary>
    public static RenderTexture GetTemporary(int width, int height, int depthBuffer = 0, RenderTextureFormat format = RenderTextureFormat.Default)
    {
        RenderTexture tempRT = RenderTexture.GetTemporary(width, height, depthBuffer, format);
        trackedTextures.Add(tempRT);
        //Debug.Log($"[RenderTextureTracker] Allocated Temporary RenderTexture: {tempRT}");
        return tempRT;
    }

    /// <summary>
    /// Custom method to release a temporary RenderTexture and remove it from the tracking list.
    /// </summary>
    public static void ReleaseTemporary(RenderTexture renderTexture)
    {
        if (renderTexture != null && trackedTextures.Contains(renderTexture))
        {
            RenderTexture.ReleaseTemporary(renderTexture);
            trackedTextures.Remove(renderTexture);
            //Debug.Log($"[RenderTextureTracker] Released Temporary RenderTexture: {renderTexture}");
        }
        else
        {
            //Debug.LogWarning($"[RenderTextureTracker] Attempted to release untracked or null RenderTexture: {renderTexture}");
        }
    }

    /// <summary>
    /// Releases all tracked temporary RenderTextures.
    /// </summary>
    public static void ReleaseAllTemporaryTextures()
    {
        foreach (var rt in trackedTextures)
        {
            if (rt != null)
            {
                RenderTexture.ReleaseTemporary(rt);
                //Debug.Log($"[RenderTextureTracker] Released Temporary RenderTexture: {rt}");
            }
        }
        trackedTextures.Clear();
        //Debug.Log("[RenderTextureTracker] All Temporary RenderTextures Released!");
    }

    /// <summary>
    /// Automatically release unused assets and clear VRAM when unloading a scene.
    /// </summary>
    private void OnDestroy()
    {
        ReleaseAllTemporaryTextures();
        Resources.UnloadUnusedAssets();
    }
}
