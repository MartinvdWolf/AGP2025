using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class Bloom : MonoBehaviour
{
    [Range(1, 5)]
    const int iterations = 4;
    [Range(0, 10)]
    public float threshold = 0.9f;
    [Range(0, 1)]
    public float softThreshold = 0.6f;
    [Range(0, 10)]
    public float intensity = 1.8f;
    public bool debug;

    public Shader bloomShader;
    [NonSerialized]
    Material bloom;

    const int PreFilterBoxDownPass = 0;
    const int BoxDownPass = 1;
    const int BoxUpPass = 2;
    const int BloomPass = 3;
    const int DebugBloomPass = 4;

    RenderTexture[] textures = new RenderTexture[16];

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!bloom)
        {
            bloom = new Material(bloomShader);
            bloom.hideFlags = HideFlags.HideAndDontSave;
        }

        float knee = threshold * softThreshold;
        Vector4 filter;
        filter.x = threshold;
        filter.y = filter.x - knee;
        filter.z = 2f * knee;
        filter.w = 0.25f / (knee + 0.00001f);

        bloom.SetVector("_Filter", filter);
        bloom.SetFloat("_Intensity", intensity);

        int width = source.width / 2;
        int height = source.height / 2;
        RenderTextureFormat format = source.format;

        RenderTexture curDestination = textures[0] = RenderTextureFix.GetTemporary(width, height, 0, format);

        Graphics.Blit(source, curDestination, bloom, PreFilterBoxDownPass);
        RenderTexture curSource = curDestination;

        // First loop to downsample, use BoxDownPass for the correct subshader pass
        int i = 0;

            for (; i < iterations; i++)
            {
                width /= 2;
                height /= 2;

                if (height < 2 || width < 2)
                    break;

                curDestination = textures[i] =
                    RenderTextureFix.GetTemporary(width, height, 0, format);
                Graphics.Blit(curSource, curDestination, bloom, BoxDownPass);
                curSource = curDestination;
            }

            // Second loop to upsample, use BoxUpPass for the correct subshader pass
            for (i -= 2; i >= 0; i--)
            {
                curDestination = textures[i];
                textures[i] = null;
                Graphics.Blit(curSource, curDestination, bloom, BoxUpPass);
                RenderTextureFix.ReleaseTemporary(curSource);
                curSource = curDestination;
            }

            if (debug)
            {
                Graphics.Blit(curSource, destination, bloom, DebugBloomPass);
            }
            else
            {
                bloom.SetTexture("_SourceTex", source);
                Graphics.Blit(curSource, destination, bloom, BloomPass);
            }
        

            // Memory leak fix
            if (curSource != null)
                RenderTextureFix.ReleaseTemporary(curSource);

            for (int j = 0; j < textures.Length; j++)
            {
                if (textures[j] != null)
                {
                    RenderTextureFix.ReleaseTemporary(textures[j]);
                    textures[j] = null;
                }
            }
       
    }

    private void OnDestroy()
    {
        RenderTextureFix.ReleaseAllTemporaryTextures();
    }
}