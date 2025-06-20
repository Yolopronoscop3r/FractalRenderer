using UnityEngine;
using UnityEngine.SceneManagement;

public class Mandelbrot_02 : MonoBehaviour
{
    [SerializeField] private ComputeShader shader;

    private RenderTexture renderTexture;
    private bool needsUpdate = true;


    [Header("Image position")]
    [SerializeField] private double img_real = 0.0;
    [SerializeField] private double img_imag = 0.0;
    [SerializeField] private double pixel_size = 4.0 / Screen.height;

    [Header("Fractal power")]
    [SerializeField] private float power = 3.5f;

    [Header("Colour setup")]
    [Range(1, 256)]
    [SerializeField] private int iterationsPerGroup = 256;
    [Range(1, 10)]
    [SerializeField] private int numGroups = 1;

    [SerializeField] private Gradient gradient;
    private Texture2D gradientTexture;


    private void UpdateGradient()
    {
        for (int i = 0; i < iterationsPerGroup; i++)
        {
            float percent = (float)i / (iterationsPerGroup - 1);
            gradientTexture.SetPixel(i, 1, (percent > 0) ? gradient.Evaluate(percent) : gradient.Evaluate(1));
        }
        gradientTexture.Apply();
    }


    private void UpdateRenderTexture(int width, int height)
    {
        if (renderTexture == null || renderTexture.width != width || renderTexture.height != height)
        {
            if (renderTexture != null)
                renderTexture.Release();

            renderTexture = new(width, height, 24)
            {
                enableRandomWrite = true
            };
            renderTexture.Create();
        }

        if (gradientTexture == null || (iterationsPerGroup != gradientTexture.width))
        {
            gradientTexture = new(iterationsPerGroup, 1);
        }
        UpdateGradient();

        int kernelHandle = shader.FindKernel("Mandelbrot");
        shader.SetTexture(kernelHandle, "Result", renderTexture);

        Vector2 realParts = new(
            (float)img_real,
            (float)(img_real - (float)img_real));
        Vector2 imagParts = new(
            (float)img_imag,
            (float)(img_imag - (float)img_imag));
        Vector2 widthParts = new(
            (float)Screen.width,
            (float)(Screen.width - (float)Screen.width));
        Vector2 heightParts = new(
            (float)Screen.height,
            (float)(Screen.height - (float)Screen.height));
        Vector2 pixelSizeParts = new(
            (float)pixel_size,
            (float)(pixel_size - (float)pixel_size));
        Vector2 powerParts = new(
            (float)power,
            (float)(power - (float)power));

        shader.SetVector("img_real_parts", realParts);
        shader.SetVector("img_imag_parts", imagParts);
        shader.SetVector("screen_width_parts", widthParts);
        shader.SetVector("screen_height_parts", heightParts);
        shader.SetVector("pixel_size_parts", pixelSizeParts);
        shader.SetVector("power_parts", powerParts);


        shader.SetInt("iterations_per_group", iterationsPerGroup);
        shader.SetInt("num_groups", numGroups);
        shader.SetTexture(kernelHandle, "gradient_texture", gradientTexture);

        shader.Dispatch(kernelHandle, width / 32, height / 32, 1);
    }


    private void OnValidate() => needsUpdate = true;


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (needsUpdate)
        {
            UpdateRenderTexture(source.width, source.height);
            needsUpdate = false;
        }
        Graphics.Blit(renderTexture, destination);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) { Move(-1, 0); }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) { Move(1, 0); }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) { Move(0, 1); }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) { Move(0, -1); }

        if (Input.GetKey(KeyCode.Q)) { Zoom(-1); }
        if (Input.GetKey(KeyCode.E)) { Zoom(1); }

        if (Input.GetKey(KeyCode.Z)) { Power(-1); }
        if (Input.GetKey(KeyCode.X)) { Power(1); }

        if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.KeypadPlus)) { IterationPerGroup(1); }
        if (Input.GetKey(KeyCode.V) || Input.GetKey(KeyCode.KeypadMinus)) { IterationPerGroup(-1); }

        if (Input.GetKeyDown(KeyCode.B) || Input.GetKeyDown(KeyCode.KeypadMultiply)) { NumGroups(1); }
        if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.KeypadDivide)) { NumGroups(-1); }

        if (Input.GetKeyDown(KeyCode.R))
        {
            img_real = 0.0;
            img_imag = 0.0;
            pixel_size = 4.0 / Screen.height;
            power = 3.5f;
            iterationsPerGroup = 64;
            numGroups = 1;
            needsUpdate = true;
        }
    }

    private bool IsShift() => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    private void IterationPerGroup(int value)
    {
        int modifire = IsShift() ? 1 : 4;
        float newValue = iterationsPerGroup + value * modifire * 100f * Time.deltaTime;
        iterationsPerGroup = Mathf.Clamp(Mathf.RoundToInt(newValue), 1, 256);
        needsUpdate = true;
    }
    private void NumGroups(int value)
    {
        int modifire = IsShift() ? 1 : 2;
        int newValue = numGroups + value * modifire;
        numGroups = Mathf.Clamp(newValue, 1, 10);
        needsUpdate = true;
    }
    private void Power(float pow)
    {
        float modifier = IsShift() ? 0.125f : 1f;
        float delta = pow * Time.deltaTime * modifier * 2;

        power += delta;

        needsUpdate = true;
    }
    private void Move(double real, double imag)
    {
        double moveSpeed = Screen.height / 2;
        double modifier = IsShift() ? 0.2 : 1.0;
        double moveAmount = moveSpeed * pixel_size * Time.deltaTime * 3 * modifier;

        img_real += real * moveAmount;
        img_imag += imag * moveAmount;

        needsUpdate = true;
    }
    private void Zoom(int direction)
    {
        float modifier = IsShift() ? 0.2f : 1f;
        double delta = direction * pixel_size * Time.deltaTime * 4 * modifier;

        pixel_size += delta;

        needsUpdate = true;
    }

}
