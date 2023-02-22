using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Rendering;


[System.Serializable]
[RequireComponent(typeof(Camera))]
public class SelectionOutline : MonoBehaviour
{
    public Camera camera;
    public bool isInitialized = false;
    public bool isSelected = false;

    [Header("Selection / outline Modes")]
    public SelectionMode selectionMode = SelectionMode.PARENT_AND_CHILDREN;
    public OutlineMode outlineType = OutlineMode.COLORIZE_OCCLUDED;
    public AlphaType alphaType = AlphaType.KEEP_HOLES;

    [Header("outline Properties")] 
    [ColorUsageAttribute(true, true)]
    public Color outlineColor = new Color(1f, 0.55f, 0f);
    [ColorUsageAttribute(true, true)]
    public Color occludedColor = new Color(0.5f, 0.9f, 0.3f);
    [Range(0, 1)]
    public float outlineWidth = 0.2f;
    [Range(0, 1)]
    public float outlineHardness = 0.85f;

    [Header("Renderer Properties")]
    public Renderer targetRenderer;
    public Renderer lastTarget;
    public Renderer[] childRenderers;

    private Material outlineMaterial;
    private Shader outlineShader;
    private Shader targetShader;
    private RenderTexture mask;
    private RenderTexture outline;
    private CommandBuffer commandBuffer;

    void OnEnable()
    {
        Initialize();
    }
    void Initialize()
    {
        //outlineShader = Shader.Find("outline/PostprocessOutline");
        outlineShader = Resources.Load<Shader>("Shaders/PostprocessOutline");
        //targetShader = Shader.Find("outline/Target");
        targetShader = Resources.Load<Shader>("Shaders/Target");
        if (outlineShader == null || targetShader == null)
        {
            Debug.LogError("Can't find the outline shaders,please check the Always Included Shaders in Graphics settings.");
            return;
        }
        camera = GetComponent<Camera>();
        camera.depthTextureMode = outlineType > 0 ? DepthTextureMode.None : DepthTextureMode.Depth;
        outlineMaterial = new Material(outlineShader);
        if (outlineType > 0)
        {
            Shader.EnableKeyword("_COLORIZE");
            mask = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.RFloat);
            outline = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.RG16);
            if (outlineType == OutlineMode.ONLY_VISIBLE)
            {
                Shader.EnableKeyword("_OCCLUDED");
            }
            else
            {
                Shader.DisableKeyword("_OCCLUDED");
            }
        }
        else
        {
            Shader.DisableKeyword("_OCCLUDED");
            Shader.DisableKeyword("_COLORIZE");
            mask = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.R8);
            outline = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.R8);
        }
        camera.RemoveAllCommandBuffers();
        commandBuffer = new CommandBuffer { name = "outline Command Buffer" };
        commandBuffer.SetRenderTarget(mask);
        camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, commandBuffer);
        isInitialized = true;
    }
    private void OnValidate()
    {
        if (!isInitialized)
        {
            Initialize();
        }
        camera.depthTextureMode = outlineType > 0 ? DepthTextureMode.Depth : DepthTextureMode.None;
        if (outlineType > 0)
        {
            Shader.EnableKeyword("_COLORIZE");

            if (outlineType == OutlineMode.ONLY_VISIBLE)
            {
                Shader.EnableKeyword("_OCCLUDED");
            }
            else
            {
                Shader.DisableKeyword("_OCCLUDED");
            }
        }
        else
        {
            Shader.DisableKeyword("_OCCLUDED");
            Shader.DisableKeyword("_COLORIZE");
        }
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (outlineMaterial == null)
        {
            Initialize();
            if (!isInitialized)
            {
                return;
            }
        }
        outlineMaterial.SetFloat("_OutlineWidth", outlineWidth * 10f);
        outlineMaterial.SetFloat("_OutlineHardness", 8.99f * (1f - outlineHardness) + 0.01f);
        outlineMaterial.SetColor("_OutlineColor", outlineColor);
        outlineMaterial.SetColor("_OccludedColor", occludedColor);

        outlineMaterial.SetTexture("_Mask", mask);
        Graphics.Blit(source, outline, outlineMaterial, 0);
        outlineMaterial.SetTexture("_Outline", outline);
        Graphics.Blit(source, destination, outlineMaterial, 1);
    }
    void RenderTarget(Renderer target)
    {
        Material targetMaterial = new Material(targetShader);
        bool mainTextureFlag = false;
        string[] attributes = target.sharedMaterial.GetTexturePropertyNames();
        foreach (var c in attributes)
        {
            if (c == "_MainTex")
            {
                mainTextureFlag = true;
                break;
            }
        }
        if (mainTextureFlag && target.sharedMaterial.mainTexture != null && alphaType == AlphaType.KEEP_HOLES)
        {
            targetMaterial.mainTexture = target.sharedMaterial.mainTexture;
        }

        commandBuffer.DrawRenderer(target, targetMaterial);
        Graphics.ExecuteCommandBuffer(commandBuffer);
    }
    public void SetTarget()
    {
        commandBuffer.SetRenderTarget(mask);
        commandBuffer.ClearRenderTarget(true, true, Color.black);
        isSelected = true;
        if (targetRenderer != null)
        {
            RenderTarget(targetRenderer);
            if (selectionMode == SelectionMode.PARENT_AND_CHILDREN && childRenderers != null)
            {
                foreach (Renderer c in childRenderers)
                {
                    if (c == targetRenderer) continue;
                    RenderTarget(c);

                }
            }
        }
        else
        {
            Debug.LogWarning("No renderer provided for outline.");
        }
    }
    public void ClearTarget()
    {
        isSelected = false;
        commandBuffer.ClearRenderTarget(true, true, Color.black);

        Graphics.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Clear();
    }
}
