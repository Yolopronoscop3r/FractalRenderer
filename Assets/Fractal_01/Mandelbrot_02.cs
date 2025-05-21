using UnityEngine;

public class Mandelbrot_02 : MonoBehaviour
{
    [SerializeField] private ComputeShader shader;

    private RenderTexture renderTexture;
    private bool needsUpdate = true;
    
    [SerializeField] private double img_real = 0.0;
    [SerializeField] private double img_imag = 0.0;
    [SerializeField] private double pixel_size = 4.0 / Screen.height;


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
        if (renderTexture == null)
        {
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

        shader.SetFloat("width", width);
        shader.SetFloat("height", height);

        shader.SetFloat("img_real", (float)img_real);
        shader.SetFloat("img_imag", (float)img_imag);
        shader.SetFloat("pixel_size", (float)pixel_size);
        
        shader.SetInt("iterations_per_group", iterationsPerGroup);
        shader.SetInt("num_groups", numGroups);
        shader.SetTexture(kernelHandle, "gradient_texture", gradientTexture);

        shader.Dispatch(kernelHandle, width / 32, height / 32, 1);
    }


    private void OnValidate()
    {
        needsUpdate = true;
    }


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
        if (Input.GetKey(KeyCode.LeftArrow)) { Move(-1, 0); }
        if (Input.GetKey(KeyCode.RightArrow)) { Move(1, 0); }
        if (Input.GetKey(KeyCode.UpArrow)) { Move(0, 1); }
        if (Input.GetKey(KeyCode.DownArrow)) { Move(0, -1); }

        if (Input.GetKey(KeyCode.LeftShift)) { Zoom(-1); }
        if (Input.GetKey(KeyCode.LeftControl)) { Zoom(1); }

        if (Input.GetMouseButtonDown(0)) { CenterOnMouse(); }

    }

    void CenterOnMouse()
    {
        double mouseX = Input.mousePosition.x - (Screen.width / 2);
        double mouseY = Input.mousePosition.y - (Screen.height / 2);

        img_real += mouseX * pixel_size;
        img_imag += mouseY * pixel_size;

        needsUpdate = true;
    }


    private void Move(double real, double imag)
    {
        double moveSpeed = Screen.height / 8;
        double moveAmout = moveSpeed * pixel_size * Time.deltaTime * 2;

        img_real += real * moveAmout;
        img_imag += imag * moveAmout;

        needsUpdate = true;
    }

    private void Zoom(int direction)
    {
        pixel_size += direction * pixel_size * Mathf.Min(Time.deltaTime);

        needsUpdate = true;
    }
}
