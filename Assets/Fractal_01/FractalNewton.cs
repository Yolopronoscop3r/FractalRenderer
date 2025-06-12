using UnityEngine;

[ExecuteAlways]
public class FractalNewton : MonoBehaviour
{
    [SerializeField] private ComputeShader shader;
    [SerializeField] private Gradient gradient;
    [SerializeField] private bool useBasins = true;

    [SerializeField] private double img_real = 0.0;
    [SerializeField] private double img_imag = 0.0;
    [SerializeField] private double pixel_size = 4.0 / Screen.height;

    [Range(1, 256)]
    [SerializeField] private int maxIterations = 64;

    [SerializeField] private float epsilon = 0.001f;

    private Texture2D gradientTexture;
    private RenderTexture renderTexture;
    private bool needsUpdate = true;


    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (needsUpdate || renderTexture == null || renderTexture.width != src.width || renderTexture.height != src.height)
        {
            UpdateRenderTexture(src.width, src.height);
            needsUpdate = false;
        }

        Graphics.Blit(renderTexture, dest);
    }

    private void UpdateGradient()
    {
        if (gradientTexture == null || gradientTexture.width != 256)
            gradientTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false, true);

        for (int i = 0; i < 256; i++)
        {
            float t = i / 255f;
            gradientTexture.SetPixel(i, 0, gradient.Evaluate(t));
        }
        gradientTexture.Apply();
    }

    private void UpdateRenderTexture(int width, int height)
    {
        if (renderTexture != null) renderTexture.Release();

        renderTexture = new RenderTexture(width, height, 0) { enableRandomWrite = true };
        renderTexture.Create();

        UpdateGradient();

        int kernel = shader.FindKernel("Newton");
        shader.SetTexture(kernel, "Result", renderTexture);
        shader.SetTexture(kernel, "gradientTexture", gradientTexture);

        shader.SetInt("width", width);
        shader.SetInt("height", height);
        shader.SetFloat("epsilon", epsilon);
        shader.SetInt("maxIterations", maxIterations);
        shader.SetBool("useBasins", useBasins);

        Vector2 realParts = new(
            (float)img_real, 
            (float)(img_real - (float)img_real));
        Vector2 imagParts = new(
            (float)img_imag, 
            (float)(img_imag - (float)img_imag));
        Vector2 pixelSizeParts = new(
            (float)pixel_size, 
            (float)(pixel_size - (float)pixel_size));

        shader.SetVector("img_real_parts", realParts);
        shader.SetVector("img_imag_parts", imagParts);
        shader.SetVector("pixel_size_parts", pixelSizeParts);

        shader.Dispatch(kernel, width / 8, height / 8, 1);
    }


    private void OnValidate() => needsUpdate = true;

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) { Move(-1, 0); }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) { Move(1, 0); }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) { Move(0, 1); }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) { Move(0, -1); }

        if (Input.GetKey(KeyCode.Q)) { Zoom(-1); }
        if (Input.GetKey(KeyCode.E)) { Zoom(1); }

        if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.KeypadPlus)) { MaxIterations(1); }
        if (Input.GetKey(KeyCode.V) || Input.GetKey(KeyCode.KeypadMinus)) { MaxIterations(-1); }

        if (Input.GetKeyDown(KeyCode.R))
        {
            img_real = 0.0;
            img_imag = 0.0;
            pixel_size = 4.0 / Screen.height;
            maxIterations = 64;
            epsilon = 0.01f;
            needsUpdate = true;
        }
    }

    private bool IsShift() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    private void MaxIterations(int value)
    {
        int modifire = IsShift() ? 1 : 4;
        float newValue = maxIterations + value * modifire * 100f * Time.deltaTime;
        maxIterations = Mathf.Clamp(Mathf.RoundToInt(newValue), 1, 256);
        needsUpdate = true;
    }

    private void Move(double real, double imag)
    {
        double moveSpeed = Screen.height / 2;
        double modifier = IsShift() ? 0.2f : 1.0f;
        double moveAmount = moveSpeed * pixel_size * Time.deltaTime * 3 * modifier;

        img_real += real * moveAmount;
        img_imag += imag * moveAmount;

        needsUpdate = true;
    }

    private void Zoom(int direction)
    {
        float modifier = IsShift() ? 0.15f : 1f;
        double delta = direction * pixel_size * Time.deltaTime * 4 * modifier;

        pixel_size += delta;

        needsUpdate = true;
    }
}
